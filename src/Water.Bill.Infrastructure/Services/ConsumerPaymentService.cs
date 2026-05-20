using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Water.Bill.Application.DTOs.Payments;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Data.Entities;

namespace Water.Bill.Infrastructure.Services;

public class ConsumerPaymentService : IConsumerPaymentService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public ConsumerPaymentService(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<PaymentInitiationResultDto> InitiateBillPaymentAsync(
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

        var duplicate = await _db.JalnoidaBankpayMasters
            .AsNoTracking()
            .Where(x => x.Consid == TrimToLength(request.ConsumerNo, 8)
                && x.BillNo == TrimToLength(request.BillNo, 15)
                && x.Paymentstatus == "N")
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
                Consid = TrimToLength(request.ConsumerNo, 8),
                ConsName = TrimToLength(request.ConsumerName, 150),
                ConsProperty = TrimToLength(request.ConsumerProperty, 50),
                DateFrom = request.BillDateFrom,
                DateTo = request.BillDateTo,
                Payamount = Math.Round(request.Amount, 2),
                EmailId = TrimToLength(request.Email, 30),
                MobileNo = TrimToLength(request.MobileNo, 15),
                DepositBank = ResolveGatewayName(gatewayCode),
                Status = "1",
                EntryDate = DateTime.Now,
                Disclaimer = "Y",
                Paymentstatus = "N",
                BillNo = TrimToLength(request.BillNo, 15),
                ChallanNo = TrimToLength(request.ChallanNo, 20),
                DueDate = FormatOldDueDate(request.DueDate),
                BillNdc = TrimToLength(request.BillOrNdc, 4) ?? "Bill"
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
        string consumerNo,
        CancellationToken cancellationToken = default)
    {
        var normalizedReference = jalReferenceId.Trim().ToUpperInvariant();
        var normalizedConsumer = TrimToLength(consumerNo, 8);

        var entity = await _db.JalnoidaBankpayMasters
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Jalrefid == normalizedReference && x.Consid == normalizedConsumer, cancellationToken);

        if (entity is null)
            return null;

        var gatewayCode = normalizedReference.Length >= 2 ? normalizedReference[..2] : "AX";
        return BuildResult(entity, gatewayCode);
    }

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

    private PaymentInitiationResultDto BuildResult(JalnoidaBankpayMaster entity, string gatewayCode)
    {
        var enabled = _configuration.GetValue<bool>($"PaymentGateway:Gateways:{gatewayCode}:Enabled");
        var url = _configuration[$"PaymentGateway:Gateways:{gatewayCode}:PaymentUrl"];
        var payload = enabled && gatewayCode == "AX" ? BuildAxisBillDeskPayload(entity) : null;

        return new PaymentInitiationResultDto
        {
            Success = true,
            JalReferenceId = entity.Jalrefid,
            GatewayCode = gatewayCode,
            GatewayName = entity.DepositBank ?? ResolveGatewayName(gatewayCode),
            Amount = entity.Payamount ?? 0,
            IsLiveGatewayEnabled = enabled && !string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(payload),
            GatewayUrl = url,
            GatewayPayload = payload,
            Message = entity.Paymentstatus == "Y"
                ? "Payment is already marked successful."
                : "Payment reference has been created."
        };
    }

    private string? BuildAxisBillDeskPayload(JalnoidaBankpayMaster entity)
    {
        var merchantId = _configuration["PaymentGateway:Gateways:AX:MerchantId"];
        var securityId = _configuration["PaymentGateway:Gateways:AX:SecurityId"];
        var checksumKey = _configuration["PaymentGateway:Gateways:AX:ChecksumKey"];
        var returnUrl = _configuration["PaymentGateway:Gateways:AX:ReturnUrl"];

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

    private static string CreateHmacSha256(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(dataBytes)).ToUpperInvariant();
    }

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
}
