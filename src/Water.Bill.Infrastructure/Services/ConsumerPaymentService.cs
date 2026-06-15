using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Water.Bill.Application.DTOs.NewConnection;
using Water.Bill.Application.DTOs.Payments;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class ConsumerPaymentService : IConsumerPaymentService
{
    private const string AxisSuccessCode = "0300";
    private const string AxisFailureCode = "0399";
    private const string AxisPendingCode = "0002";
    private const string AxisGatewayErrorCode = "0001";

    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly INewConnectionApplicationService _newConnectionApplicationService;

    public ConsumerPaymentService(
        ApplicationDbContext db,
        IConfiguration configuration,
        INewConnectionApplicationService newConnectionApplicationService)
    {
        _db = db;
        _configuration = configuration;
        _newConnectionApplicationService = newConnectionApplicationService;
    }

    public bool IsDevelopmentMode()
    {
        var value = _configuration["PaymentGateway:IsDevelopment"];
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return value.Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase)
               || value.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase)
               || bool.TryParse(value, out var enabled) && enabled;
    }

    public Task<PaymentInitiationResultDto> InitiateBillPaymentAsync(
        PaymentInitiationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        request.BillOrNdc = string.IsNullOrWhiteSpace(request.BillOrNdc)
            ? PaymentReferenceKinds.Bill
            : request.BillOrNdc;
        request.ContextReferenceNo ??= request.BillNo;
        return InitiatePaymentAsync(request, cancellationToken);
    }

    public async Task<PaymentInitiationResultDto> InitiatePaymentAsync(
        PaymentInitiationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var gatewayCode = NormalizeGatewayCode(request.GatewayCode);
        if (request.Amount <= 0)
        {
            return new PaymentInitiationResultDto
            {
                Success = false,
                Message = "Payment amount must be greater than zero.",
                GatewayCode = gatewayCode,
                GatewayName = ResolveGatewayName(gatewayCode)
            };
        }

        var normalizedKind = NormalizePaymentKind(request.BillOrNdc);
        var normalizedConsumerId = ResolveConsumerIdentifier(request, normalizedKind);
        var normalizedDisplayRef = ResolveDisplayReference(request, normalizedKind);
        var normalizedContextId = TrimToLength(request.ContextId, 20);
        var normalizedBillNo = ResolveBillNo(request, normalizedKind);

        var duplicateQuery = _db.JalnoidaBankpayMasters
            .AsNoTracking()
            .Where(x => x.BillNdc == normalizedKind
                && x.Paymentstatus == "N");

        if (normalizedKind == PaymentReferenceKinds.Challan)
        {
            duplicateQuery = duplicateQuery.Where(x =>
                !string.IsNullOrWhiteSpace(normalizedContextId)
                && x.ChallanNo == normalizedContextId);
        }
        else
        {
            duplicateQuery = duplicateQuery.Where(x =>
                (!string.IsNullOrWhiteSpace(normalizedContextId) && x.ChallanNo == normalizedContextId)
                || (!string.IsNullOrWhiteSpace(normalizedBillNo) && x.BillNo == normalizedBillNo));
        }

        var duplicate = await duplicateQuery
            .OrderByDescending(x => x.EntryDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (duplicate is not null)
        {
            var duplicateGatewayCode = duplicate.Jalrefid.Length >= 2 ? duplicate.Jalrefid[..2] : gatewayCode;
            return BuildResult(duplicate, duplicateGatewayCode);
        }

        for (var attempt = 0; attempt < 3; attempt++)
        {
            var referenceId = await GenerateReferenceIdAsync(gatewayCode, cancellationToken);
            var entity = new JalnoidaBankpayMaster
            {
                Jalrefid = referenceId,
                Consid = normalizedConsumerId,
                ConsName = TrimToLength(request.ConsumerName, 150) ?? normalizedDisplayRef ?? normalizedKind,
                ConsProperty = TrimToLength(request.ConsumerProperty, 50) ?? normalizedDisplayRef,
                DateFrom = request.BillDateFrom,
                DateTo = request.BillDateTo,
                Payamount = Math.Round(request.Amount, 2),
                EmailId = TrimToLength(request.Email, 30),
                MobileNo = TrimToLength(request.MobileNo, 15),
                DepositBank = ResolveGatewayName(gatewayCode),
                Status = "1",
                EntryDate = DateTime.Now,
                Disclaimer = IsDevelopmentMode() ? "D" : "Y",
                Paymentstatus = "N",
                BillNo = normalizedBillNo,
                ChallanNo = normalizedContextId,
                DueDate = FormatOldDueDate(request.DueDate),
                BillNdc = normalizedKind
            };

            _db.JalnoidaBankpayMasters.Add(entity);

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                return BuildResult(entity, gatewayCode);
            }
            catch (DbUpdateException) when (attempt < 2)
            {
                _db.Entry(entity).State = EntityState.Detached;
            }
        }

        return new PaymentInitiationResultDto
        {
            Success = false,
            Message = "Unable to create payment reference. Please try again.",
            GatewayCode = gatewayCode,
            GatewayName = ResolveGatewayName(gatewayCode)
        };
    }

    public async Task<PaymentInitiationResultDto?> GetInitiatedPaymentAsync(
        string jalReferenceId,
        string? consumerNo,
        CancellationToken cancellationToken = default)
    {
        var normalizedReference = jalReferenceId.Trim().ToUpperInvariant();
        var normalizedConsumer = TrimToLength(consumerNo, 8);

        var query = _db.JalnoidaBankpayMasters
            .AsNoTracking()
            .Where(x => x.Jalrefid == normalizedReference);

        if (!string.IsNullOrWhiteSpace(normalizedConsumer))
            query = query.Where(x => x.Consid == normalizedConsumer);

        var entity = await query.FirstOrDefaultAsync(cancellationToken);
        if (entity is null)
            return null;

        var gatewayCode = normalizedReference.Length >= 2 ? normalizedReference[..2] : "AX";
        return BuildResult(entity, gatewayCode);
    }

    public async Task<PaymentProcessingResultDto> ProcessDevelopmentSuccessAsync(
        string jalReferenceId,
        PaymentActorContextDto actor,
        CancellationToken cancellationToken = default)
    {
        var master = await _db.JalnoidaBankpayMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Jalrefid == jalReferenceId, cancellationToken);

        if (master is null)
        {
            return new PaymentProcessingResultDto
            {
                Success = false,
                JalReferenceId = jalReferenceId,
                Message = "Payment reference was not found."
            };
        }

        var now = DateTime.Now;
        var response = new AxisPaymentResponse(
            master.DepositBank == "AXIS" ? ResolveMerchantId() ?? "NOIDAAUTH" : ResolveMerchantId() ?? "NOIDAAUTH",
            master.Jalrefid,
            $"SIM-{now:yyyyMMddHHmmssfff}",
            $"SIMBANK-{now:yyyyMMddHHmmss}",
            (master.Payamount ?? 0).ToString("0.00", CultureInfo.InvariantCulture),
            "SIM",
            ResolveGatewayName("AX"),
            "success",
            "INR",
            "NA",
            "HMACSHA256",
            ResolveSecurityId() ?? "noidaauth",
            "NA",
            now.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
            AxisSuccessCode,
            "NA",
            master.MobileNo,
            master.EmailId,
            master.Consid,
            master.ConsProperty,
            master.ConsName,
            FormatGatewayDate(master.DateFrom),
            FormatGatewayDate(master.DateTo),
            "SIMULATED",
            "Development payment simulated",
            null,
            []);

        return await ProcessResponseAsync(response, actor, skipChecksumValidation: true, cancellationToken);
    }

    public async Task<PaymentProcessingResultDto> ProcessAxisResponseAsync(
        string rawMessage,
        PaymentActorContextDto actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawMessage))
        {
            return new PaymentProcessingResultDto
            {
                Success = false,
                Message = "Payment gateway response was empty."
            };
        }

        var fields = rawMessage.Split('|');
        if (fields.Length < 15)
        {
            return new PaymentProcessingResultDto
            {
                Success = false,
                Message = "Payment gateway response was incomplete."
            };
        }

        var checksum = fields.Length > 0 ? fields[^1] : null;
        var response = new AxisPaymentResponse(
            GetField(fields, 0),
            GetField(fields, 1),
            GetField(fields, 2),
            GetField(fields, 3),
            GetField(fields, 4),
            GetField(fields, 5),
            GetField(fields, 6),
            GetField(fields, 7),
            GetField(fields, 8),
            GetField(fields, 9),
            GetField(fields, 10),
            GetField(fields, 11),
            GetField(fields, 12),
            GetField(fields, 13),
            GetField(fields, 14),
            GetField(fields, 15),
            GetField(fields, 16),
            GetField(fields, 17),
            GetField(fields, 18),
            GetField(fields, 19),
            GetField(fields, 20),
            GetField(fields, 21),
            GetField(fields, 22),
            GetField(fields, 23),
            GetField(fields, 24),
            checksum,
            fields);

        return await ProcessResponseAsync(response, actor, skipChecksumValidation: false, cancellationToken);
    }

    private async Task<PaymentProcessingResultDto> ProcessResponseAsync(
        AxisPaymentResponse response,
        PaymentActorContextDto actor,
        bool skipChecksumValidation,
        CancellationToken cancellationToken)
    {
        var master = await _db.JalnoidaBankpayMasters
            .FirstOrDefaultAsync(x => x.Jalrefid == response.JalReferenceId, cancellationToken);

        if (master is null)
        {
            return new PaymentProcessingResultDto
            {
                Success = false,
                JalReferenceId = response.JalReferenceId,
                Message = "Payment reference was not found in the current system."
            };
        }

        var kind = NormalizePaymentKind(master.BillNdc);
        var result = BuildBaseResult(master, kind, response);

        var merchantId = ResolveMerchantId();
        if (!string.IsNullOrWhiteSpace(merchantId)
            && !string.Equals(response.MerchantId, merchantId, StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Message = "Merchant validation failed for the gateway response.";
            return result;
        }

        var securityId = ResolveSecurityId();
        if (!string.IsNullOrWhiteSpace(securityId)
            && !string.IsNullOrWhiteSpace(response.SecurityId)
            && !string.Equals(response.SecurityId, securityId, StringComparison.OrdinalIgnoreCase))
        {
            result.Success = false;
            result.Message = "Security identifier validation failed for the gateway response.";
            return result;
        }

        if (!AmountsMatch(master.Payamount, response.TransactionAmount))
        {
            result.Success = false;
            result.Message = "Gateway amount does not match the pending payment amount.";
            return result;
        }

        if (!skipChecksumValidation && !ValidateAxisChecksum(response))
        {
            result.Success = false;
            result.Message = "Gateway checksum validation failed.";
            return result;
        }

        var gatewayOutcome = MapGatewayOutcome(response.AuthStatus, response.ErrorDescription);
        result.GatewayStatusCode = response.AuthStatus;
        result.IsGatewaySuccess = gatewayOutcome == GatewayOutcome.Success;
        result.IsPending = gatewayOutcome == GatewayOutcome.Pending;
        result.IsCancelled = gatewayOutcome == GatewayOutcome.Cancelled;
        result.Message = gatewayOutcome switch
        {
            GatewayOutcome.Success => "Payment was received successfully.",
            GatewayOutcome.Pending => "Payment is pending confirmation from the gateway.",
            GatewayOutcome.Cancelled => "Payment was cancelled.",
            _ => string.IsNullOrWhiteSpace(response.ErrorDescription)
                ? "Payment could not be completed."
                : response.ErrorDescription
        };

        await InsertGatewayTransactionAsync(response, gatewayOutcome, cancellationToken);

        if (!result.IsGatewaySuccess)
            return result;

        if (string.Equals(master.Paymentstatus, "Y", StringComparison.OrdinalIgnoreCase))
        {
            result.Success = true;
            result.IsAlreadyProcessed = true;
            result.Message = "Payment was already processed earlier.";
            return result;
        }

        var strategy = _db.Database.CreateExecutionStrategy();
        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
                var trackedMaster = await _db.JalnoidaBankpayMasters
                    .FirstOrDefaultAsync(x => x.Jalrefid == response.JalReferenceId, cancellationToken)
                    ?? throw new InvalidOperationException("Payment reference was not found.");

                if (string.Equals(trackedMaster.Paymentstatus, "Y", StringComparison.OrdinalIgnoreCase))
                    return;

                switch (kind)
                {
                    case PaymentReferenceKinds.Bill:
                        await FinalizeBillPaymentAsync(trackedMaster, response, cancellationToken);
                        break;
                    case PaymentReferenceKinds.Challan:
                        await FinalizeChallanPaymentAsync(trackedMaster, response, actor, cancellationToken);
                        break;
                    case PaymentReferenceKinds.NewConnection:
                        await FinalizeNewConnectionPaymentAsync(trackedMaster, response, actor, result, cancellationToken);
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported payment kind '{kind}'.");
                }

                trackedMaster.Paymentstatus = "Y";
                trackedMaster.Status = "1";
                trackedMaster.Disclaimer = IsDevelopmentMode() ? "D" : trackedMaster.Disclaimer;
                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.IsGatewaySuccess = true;
            result.Message = $"Gateway success received, but local update failed: {ex.Message}";
            return result;
        }

        result.Success = true;
        result.Message = result.IsAlreadyProcessed
            ? "Payment was already processed earlier."
            : "Payment completed successfully.";
        return result;
    }

    private async Task FinalizeBillPaymentAsync(
        JalnoidaBankpayMaster master,
        AxisPaymentResponse response,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(master.BillNo))
            throw new InvalidOperationException("Linked bill number is missing for this payment reference.");

        var bill = await _db.JalPrintBillMasters
            .FirstOrDefaultAsync(x => x.BillNo == master.BillNo && x.ConsNo == master.Consid, cancellationToken)
            ?? throw new InvalidOperationException("Linked bill record was not found.");

        if (bill.PaidDate.HasValue || string.Equals(bill.PaidStatus, "Y", StringComparison.OrdinalIgnoreCase))
            return;

        var amount = master.Payamount ?? ParseDouble(response.TransactionAmount) ?? 0;
        var paidOn = ParseGatewayDate(response.TransactionDate) ?? DateTime.Now;
        bill.PaidDate = paidOn;
        bill.PaidAmt = amount;
        bill.PaidStatus = "Y";
        bill.PaymentType ??= 1;
        bill.PaymentMode ??= 1;
        bill.BankCode = master.DepositBank ?? response.BankId ?? bill.BankCode;
        bill.Diff = Math.Round((bill.TotalBillAmt ?? bill.DueAmt ?? amount) - amount, 2);
        bill.UpdateRecord = TrimToLength($"Gateway {master.Jalrefid}", 20);
    }

    private async Task FinalizeChallanPaymentAsync(
        JalnoidaBankpayMaster master,
        AxisPaymentResponse response,
        PaymentActorContextDto actor,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(master.ChallanNo))
            throw new InvalidOperationException("Linked challan number is missing for this payment reference.");

        Challan? challan = null;

        if (long.TryParse(master.ChallanNo, out var challanId))
        {
            challan = await _db.Challans
                .FirstOrDefaultAsync(x => x.Id == challanId, cancellationToken);
        }

        challan ??= await _db.Challans
            .FirstOrDefaultAsync(x =>
                (string.IsNullOrWhiteSpace(master.Consid) || x.ConsNo == master.Consid)
                && (x.RecpNo == master.ChallanNo
                    || x.ReceiptId == master.ChallanNo
                    || x.ReceiptId1 == master.ChallanNo), cancellationToken);

        challan ??= await _db.Challans
            .FirstOrDefaultAsync(x => x.RecpNo == master.BillNo
                || x.ReceiptId == master.BillNo
                || x.ReceiptId1 == master.BillNo, cancellationToken);

        if (challan is null)
            throw new InvalidOperationException("Linked challan record was not found.");

        var currentStatus = ResolveChallanStatus(challan);
        if (currentStatus == ChallanStatus.Paid)
            return;

        var amount = ResolveChallanPayableAmount(challan);
        if (amount <= 0)
            amount = master.Payamount ?? ParseDouble(response.TransactionAmount) ?? 0;

        var paidOn = ParseGatewayDate(response.TransactionDate) ?? DateTime.Now;
        var paymentMode = NormalizeGatewayCode(master.DepositBank ?? response.BankMerchantId ?? "AX");
        var transactionReference = response.TransactionReferenceNo ?? response.BankReferenceNo ?? master.Jalrefid;

        challan.PayDate = paidOn;
        challan.PaidAmt = amount;
        challan.Status = "1";
        challan.ChallanStatus = 1;
        challan.Upd = "PAID";
        challan.BankId = challan.BankId ?? response.BankId ?? "ONLINE";
        challan.BnkCd = challan.BnkCd ?? response.BankId ?? "ONLINE";
        challan.BrNm = challan.BrNm ?? master.DepositBank ?? "Online Gateway Payment";

        var hasPaymentHistory = await _db.ChallanPaymentHistories
            .AnyAsync(x => x.ChallanId == challan.Id
                && x.TransactionReferenceNo == transactionReference
                && !x.IsDeleted, cancellationToken);

        if (!hasPaymentHistory)
        {
            _db.ChallanPaymentHistories.Add(new ChallanPaymentHistory
            {
                ChallanId = challan.Id,
                ChallanNo = challan.RecpNo ?? challan.ReceiptId,
                ConsumerNo = challan.ConsNo,
                SourceBillNo = string.Equals(challan.RevBilFr, "BILL", StringComparison.OrdinalIgnoreCase) ? challan.BillId : null,
                Amount = amount,
                PaymentDate = paidOn,
                PaymentMode = paymentMode,
                BankCode = challan.BnkCd,
                BankName = challan.BrNm,
                TransactionReferenceNo = transactionReference,
                Remarks = IsDevelopmentMode()
                    ? "Simulated gateway payment processed in development mode."
                    : "Gateway payment processed successfully.",
                PostedByUserId = actor.UserId,
                PostedByName = actor.UserName ?? "Payment Gateway",
                PostedOn = paidOn
            });
        }

        var hasHistory = await _db.ChallanHistories
            .AnyAsync(x => x.ChallanId == challan.Id
                && x.Action == "ConsumerPayment"
                && x.ActionOn == paidOn, cancellationToken);

        if (!hasHistory)
        {
            _db.ChallanHistories.Add(new ChallanHistory
            {
                ChallanId = challan.Id,
                ChallanNo = challan.RecpNo ?? challan.ReceiptId,
                ConsumerNo = challan.ConsNo,
                FromStatus = currentStatus,
                ToStatus = ChallanStatus.Paid,
                Action = "ConsumerPayment",
                Remarks = $"Gateway payment completed. Ref: {transactionReference}",
                ActionByUserId = actor.UserId,
                ActionByName = actor.UserName ?? "Payment Gateway",
                ActionOn = paidOn
            });
        }

        if (string.Equals(challan.RevBilFr, "BILL", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(challan.BillId)
            && !string.IsNullOrWhiteSpace(challan.ConsNo))
        {
            await MarkSourceBillPaidAsync(challan, amount, paidOn, paymentMode, cancellationToken);
        }
    }

    private async Task FinalizeNewConnectionPaymentAsync(
        JalnoidaBankpayMaster master,
        AxisPaymentResponse response,
        PaymentActorContextDto actor,
        PaymentProcessingResultDto result,
        CancellationToken cancellationToken)
    {
        var application = await ResolveNewConnectionApplicationAsync(master, cancellationToken)
            ?? throw new InvalidOperationException("Linked new connection application was not found.");

        result.LocalEntityId = application.Id;
        result.LocalReferenceNo = application.ApplicationNo;
        result.IsPublicFlow = application.IsPublicApplication;

        await _newConnectionApplicationService.FinalizeGatewayPaymentAsync(application.Id, new NewConnectionPaymentRequestDto
        {
            ActionBy = actor.UserId,
            ActionByName = actor.UserName ?? "Payment Gateway",
            ActionByRole = actor.UserRole ?? "PaymentGateway",
            IpAddress = actor.IpAddress,
            UserAgent = actor.UserAgent,
            PaymentMethod = master.DepositBank ?? response.BankMerchantId ?? "AX",
            PaymentIdentifier = response.TransactionReferenceNo ?? response.BankReferenceNo ?? master.Jalrefid,
            StartWorkflow = true
        }, cancellationToken);
    }

    private async Task<NewConnectionApplication?> ResolveNewConnectionApplicationAsync(
        JalnoidaBankpayMaster master,
        CancellationToken cancellationToken)
    {
        if (long.TryParse(master.ChallanNo, out var applicationId))
        {
            var byId = await _db.NewConnectionApplications
                .FirstOrDefaultAsync(x => x.Id == applicationId && !x.IsDeleted, cancellationToken);
            if (byId is not null)
                return byId;
        }

        if (!string.IsNullOrWhiteSpace(master.BillNo))
        {
            return await _db.NewConnectionApplications
                .FirstOrDefaultAsync(x => x.ApplicationNo == master.BillNo && !x.IsDeleted, cancellationToken);
        }

        return null;
    }

    private async Task MarkSourceBillPaidAsync(
        Challan challan,
        double amount,
        DateTime paymentDate,
        string? paymentMethod,
        CancellationToken cancellationToken)
    {
        var bill = await _db.JalPrintBillMasters
            .FirstOrDefaultAsync(x => x.BillNo == challan.BillId && x.ConsNo == challan.ConsNo, cancellationToken);

        if (bill is null)
            return;

        if (bill.PaidDate.HasValue || string.Equals(bill.PaidStatus, "Y", StringComparison.OrdinalIgnoreCase))
            return;

        bill.PaidDate = paymentDate;
        bill.PaidAmt = amount;
        bill.PaidStatus = "Y";
        bill.PaymentType ??= 1;
        bill.BankCode = paymentMethod ?? bill.BankCode;
        bill.PaymentMode ??= 1;
        bill.Diff = Math.Round((bill.TotalBillAmt ?? bill.DueAmt ?? amount) - amount, 2);
        bill.UpdateRecord = TrimToLength($"Challan {challan.RecpNo ?? challan.ReceiptId}", 20);
    }

    private async Task InsertGatewayTransactionAsync(
        AxisPaymentResponse response,
        GatewayOutcome outcome,
        CancellationToken cancellationToken)
    {
        var trxReference = TrimToLength(response.TransactionReferenceNo, 100) ?? "NA";
        var bankReference = TrimToLength(response.BankReferenceNo, 20) ?? "NA";
        var exists = await _db.JalnoidaBankpayTrans
            .AsNoTracking()
            .AnyAsync(x => x.Jalrefid == response.JalReferenceId
                && x.TrxReferenceNo == trxReference
                && x.BankReferenceNo == bankReference
                && x.AuthStatus == response.AuthStatus, cancellationToken);

        if (exists)
            return;

        var mappedTcnType = outcome switch
        {
            GatewayOutcome.Success => "success",
            GatewayOutcome.Pending => "Pend_Abon",
            GatewayOutcome.Cancelled => "failure",
            _ => string.Equals(response.AuthStatus, AxisGatewayErrorCode, StringComparison.OrdinalIgnoreCase)
                ? "Err_Billdesk"
                : "failure"
        };

        await _db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO jalnoida_bankpay_tran
(JALREFID, MerchantID, TrxReferenceNo, BankReferenceNo, TxnAmount, BankID, BankMerchantID, TCNType, CurrencyName, ItemCode, SecurityType, SecurityID, SecurityPassword, TxnDate, AuthStatus, SettlementType, AdditionalInfo1, AdditionalInfo2, AdditionalInfo3, AdditionalInfo4, AdditionalInfo5, AdditionalInfo6, AdditionalInfo7, ErrorStatus, ErrorDescription, CheckSum1, STATUS, ENTRY_DATE)
VALUES
({TrimToLength(response.JalReferenceId, 12)},
 {TrimToLength(response.MerchantId, 50)},
 {trxReference},
 {bankReference},
 {TrimToLength(response.TransactionAmount, 20)},
 {TrimToLength(response.BankId, 20)},
 {TrimToLength(response.BankMerchantId, 20)},
 {TrimToLength(mappedTcnType, 100)},
 {TrimToLength(response.CurrencyName, 20) ?? "INR"},
 {TrimToLength(response.ItemCode, 20)},
 {TrimToLength(response.SecurityType, 20)},
 {TrimToLength(response.SecurityId, 20)},
 {TrimToLength(response.SecurityPassword, 20)},
 {TrimToLength(response.TransactionDate, 20)},
 {TrimToLength(response.AuthStatus, 20)},
 {TrimToLength(response.SettlementType, 20)},
 {TrimToLength(response.AdditionalInfo1, 200)},
 {TrimToLength(response.AdditionalInfo2, 200)},
 {TrimToLength(response.AdditionalInfo3, 100)},
 {TrimToLength(response.AdditionalInfo4, 100)},
 {TrimToLength(response.AdditionalInfo5, 100)},
 {TrimToLength(response.AdditionalInfo6, 100)},
 {TrimToLength(response.AdditionalInfo7, 100)},
 {TrimToLength(response.ErrorStatus, 100)},
 {TrimToLength(response.ErrorDescription, 200)},
 {TrimToLength(response.Checksum, 50)},
 {"1"},
 {DateTime.Now});", cancellationToken);
    }

    private PaymentInitiationResultDto BuildResult(JalnoidaBankpayMaster entity, string gatewayCode)
    {
        var enabled = IsGatewayEnabled(gatewayCode);
        var url = _configuration[$"PaymentGateway:Gateways:{gatewayCode}:PaymentUrl"];
        var payload = gatewayCode == "AX" ? BuildAxisBillDeskPayload(entity) : null;

        return new PaymentInitiationResultDto
        {
            Success = true,
            JalReferenceId = entity.Jalrefid,
            GatewayCode = gatewayCode,
            GatewayName = entity.DepositBank ?? ResolveGatewayName(gatewayCode),
            Amount = entity.Payamount ?? 0,
            IsLiveGatewayEnabled = !IsDevelopmentMode() && enabled && !string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(payload),
            GatewayUrl = url,
            GatewayPayload = payload,
            Message = entity.Paymentstatus == "Y"
                ? "Payment is already marked successful."
                : IsDevelopmentMode()
                    ? "Payment reference created. Development mode will simulate success after confirmation."
                    : "Payment reference has been created."
        };
    }

    private string? BuildAxisBillDeskPayload(JalnoidaBankpayMaster entity)
    {
        var merchantId = ResolveMerchantId();
        var securityId = ResolveSecurityId();
        var checksumKey = ResolveChecksumKey();
        var returnUrl = ResolveReturnUrl();

        if (string.IsNullOrWhiteSpace(merchantId)
            || string.IsNullOrWhiteSpace(securityId)
            || string.IsNullOrWhiteSpace(checksumKey)
            || string.IsNullOrWhiteSpace(returnUrl))
            return null;

        var amount = (entity.Payamount ?? 0).ToString("0.00", CultureInfo.InvariantCulture);
        var data = string.Join("|", new[]
        {
            merchantId,
            entity.Jalrefid,
            "NA",
            amount,
            "NA",
            "NA",
            "NA",
            "INR",
            "NA",
            "R",
            securityId,
            "NA",
            "NA",
            "F",
            entity.MobileNo ?? "NA",
            entity.EmailId ?? "NA",
            entity.Consid ?? "NA",
            entity.ConsProperty ?? "NA",
            entity.ConsName ?? "NA",
            FormatGatewayDate(entity.DateFrom),
            FormatGatewayDate(entity.DateTo),
            returnUrl
        });

        return $"{data}|{CreateHmacSha256(data, checksumKey)}";
    }

    private bool ValidateAxisChecksum(AxisPaymentResponse response)
    {
        var checksumKey = ResolveChecksumKey();
        if (string.IsNullOrWhiteSpace(checksumKey) || string.IsNullOrWhiteSpace(response.Checksum))
            return false;

        if (response.RawFields.Length < 2)
            return false;

        var content = string.Join("|", response.RawFields.Take(response.RawFields.Length - 1));
        var expected = CreateHmacSha256(content, checksumKey);
        return string.Equals(expected, response.Checksum, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateHmacSha256(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(dataBytes)).ToUpperInvariant();
    }

    private bool IsGatewayEnabled(string gatewayCode)
        => !IsDevelopmentMode() && _configuration.GetValue<bool>($"PaymentGateway:Gateways:{gatewayCode}:Enabled");

    private string? ResolveMerchantId() => _configuration["PaymentGateway:Gateways:AX:MerchantId"];
    private string? ResolveSecurityId() => _configuration["PaymentGateway:Gateways:AX:SecurityId"];
    private string? ResolveChecksumKey() => _configuration["PaymentGateway:Gateways:AX:ChecksumKey"];
    private string? ResolveReturnUrl() => _configuration["PaymentGateway:Gateways:AX:ReturnUrl"];

    private async Task<string> GenerateReferenceIdAsync(string gatewayCode, CancellationToken cancellationToken)
    {
        var existingRefs = await _db.JalnoidaBankpayMasters
            .AsNoTracking()
            .Where(x => x.Jalrefid.StartsWith(gatewayCode))
            .Select(x => x.Jalrefid)
            .ToListAsync(cancellationToken);

        var max = existingRefs
            .Select(x => int.TryParse(x.Length > 2 ? x[2..] : null, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : 0)
            .DefaultIfEmpty(9999999)
            .Max();

        return $"{gatewayCode}{max + 1}";
    }

    private static PaymentProcessingResultDto BuildBaseResult(
        JalnoidaBankpayMaster master,
        string kind,
        AxisPaymentResponse response)
    {
        var result = new PaymentProcessingResultDto
        {
            JalReferenceId = master.Jalrefid,
            PaymentKind = kind,
            LocalReferenceNo = master.BillNo,
            TransactionReferenceNo = response.TransactionReferenceNo
        };

        if (kind == PaymentReferenceKinds.Challan)
        {
            result.LocalReferenceNo = master.BillNo ?? master.ChallanNo;
            if (long.TryParse(master.ChallanNo, out var challanId))
                result.LocalEntityId = challanId;
        }

        if (kind == PaymentReferenceKinds.NewConnection)
        {
            result.LocalReferenceNo = master.BillNo;
            if (long.TryParse(master.ChallanNo, out var applicationId))
                result.LocalEntityId = applicationId;
        }

        return result;
    }

    private static GatewayOutcome MapGatewayOutcome(string? authStatus, string? errorDescription)
    {
        var code = authStatus?.Trim();
        return code switch
        {
            AxisSuccessCode => GatewayOutcome.Success,
            AxisPendingCode => GatewayOutcome.Pending,
            AxisFailureCode => errorDescription?.Contains("cancel", StringComparison.OrdinalIgnoreCase) == true
                ? GatewayOutcome.Cancelled
                : GatewayOutcome.Failed,
            AxisGatewayErrorCode => GatewayOutcome.Failed,
            _ => GatewayOutcome.Failed
        };
    }

    private static string NormalizePaymentKind(string? value)
    {
        var normalized = value?.Trim();
        return normalized?.ToUpperInvariant() switch
        {
            "CHLN" or "CHALLAN" => PaymentReferenceKinds.Challan,
            "NC" or "NEWCONNECTION" => PaymentReferenceKinds.NewConnection,
            _ => PaymentReferenceKinds.Bill
        };
    }

    private static string ResolveConsumerIdentifier(PaymentInitiationRequestDto request, string paymentKind)
    {
        if (!string.IsNullOrWhiteSpace(request.ConsumerNo))
            return TrimToLength(request.ConsumerNo, 8) ?? "00000000";

        if (paymentKind == PaymentReferenceKinds.NewConnection && !string.IsNullOrWhiteSpace(request.ContextId))
            return TrimToLength($"NC{request.ContextId}", 8) ?? "NC";

        return TrimToLength(request.ContextReferenceNo, 8) ?? "00000000";
    }

    private static string? ResolveDisplayReference(PaymentInitiationRequestDto request, string paymentKind)
        => paymentKind switch
        {
            PaymentReferenceKinds.Challan => TrimToLength(request.ContextReferenceNo ?? request.ChallanNo, 50),
            PaymentReferenceKinds.NewConnection => TrimToLength(request.ContextReferenceNo ?? request.BillNo, 50),
            _ => TrimToLength(request.ConsumerProperty, 50)
        };

    private static string? ResolveBillNo(PaymentInitiationRequestDto request, string paymentKind)
        => paymentKind switch
        {
            PaymentReferenceKinds.NewConnection => TrimToLength(request.ContextReferenceNo ?? request.BillNo, 15),
            PaymentReferenceKinds.Challan => TrimToLength(request.ContextReferenceNo ?? request.ChallanNo ?? request.BillNo, 15),
            _ => TrimToLength(request.BillNo, 15)
        };

    private static string NormalizeGatewayCode(string? value) => value?.Trim().ToUpperInvariant() switch
    {
        "IC" or "ICICI" => "IC",
        "HD" or "HDFC" => "HD",
        "EV" or "EVM" => "EV",
        "KT" or "KOTAK" => "KT",
        "PT" or "PAYTM" or "UPI" or "WALLET" => "PT",
        "AX" or "AXIS" or "BILLDESK" or "CARD" or "NETBANKING" => "AX",
        _ => "AX"
    };

    private static string ResolveGatewayName(string gatewayCode) => gatewayCode switch
    {
        "IC" => "ICICI",
        "HD" => "HDFC",
        "EV" => "EVM",
        "KT" => "KOTAK",
        "PT" => "PAYTM",
        "AX" => "AXIS",
        _ => "AXIS"
    };

    private static bool AmountsMatch(double? expectedAmount, string? actualAmount)
    {
        var expected = Math.Round(expectedAmount ?? 0, 2);
        var actual = Math.Round(ParseDouble(actualAmount) ?? 0, 2);
        return expected == actual;
    }

    private static double ResolveChallanPayableAmount(Challan challan)
    {
        var values = new[]
        {
            challan.BillAmt ?? 0,
            challan.Surcharge ?? 0,
            challan.Noc ?? 0,
            challan.ConnCharge ?? 0,
            challan.PanalityCharges ?? 0,
            challan.Disconnection ?? 0,
            challan.Reconnection ?? 0
        };

        return values.Sum();
    }

    private static string ResolveChallanStatus(Challan challan)
    {
        if (challan.PayDate.HasValue || string.Equals(challan.Upd, "PAID", StringComparison.OrdinalIgnoreCase))
            return ChallanStatus.Paid;
        if (string.Equals(challan.Status, "C", StringComparison.OrdinalIgnoreCase)
            || string.Equals(challan.Upd, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            return ChallanStatus.Cancelled;
        return ChallanStatus.PendingPayment;
    }

    private static string? GetField(string[] fields, int index)
        => fields.Length > index ? fields[index] : null;

    private static double? ParseDouble(string? value)
        => double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;

    private static DateTime? ParseGatewayDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var formats = new[]
        {
            "dd-MM-yyyy HH:mm:ss",
            "dd-MM-yyyy",
            "dd/MM/yyyy",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.fff"
        };

        return DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)
                ? parsed
                : null;
    }

    private static string? TrimToLength(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }

    private static string? FormatOldDueDate(DateTime? value)
        => value.HasValue ? value.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : null;

    private static string FormatGatewayDate(DateTime? value)
        => value.HasValue ? value.Value.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) : "NA";

    private enum GatewayOutcome
    {
        Success,
        Failed,
        Pending,
        Cancelled
    }

    private sealed record AxisPaymentResponse(
        string? MerchantId,
        string? JalReferenceId,
        string? TransactionReferenceNo,
        string? BankReferenceNo,
        string? TransactionAmount,
        string? BankId,
        string? BankMerchantId,
        string? TcnType,
        string? CurrencyName,
        string? ItemCode,
        string? SecurityType,
        string? SecurityId,
        string? SecurityPassword,
        string? TransactionDate,
        string? AuthStatus,
        string? SettlementType,
        string? AdditionalInfo1,
        string? AdditionalInfo2,
        string? AdditionalInfo3,
        string? AdditionalInfo4,
        string? AdditionalInfo5,
        string? AdditionalInfo6,
        string? AdditionalInfo7,
        string? ErrorStatus,
        string? ErrorDescription,
        string? Checksum,
        string[] RawFields);

    private static class ChallanStatus
    {
        public const string PendingPayment = "PendingPayment";
        public const string Paid = "Paid";
        public const string Cancelled = "Cancelled";
    }
}
