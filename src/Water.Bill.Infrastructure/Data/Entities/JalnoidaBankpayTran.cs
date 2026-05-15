using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class JalnoidaBankpayTran
{
    public string? Jalrefid { get; set; }

    public string? MerchantId { get; set; }

    public string? TrxReferenceNo { get; set; }

    public string? BankReferenceNo { get; set; }

    public string? TxnAmount { get; set; }

    public string? BankId { get; set; }

    public string? BankMerchantId { get; set; }

    public string? Tcntype { get; set; }

    public string? CurrencyName { get; set; }

    public string? ItemCode { get; set; }

    public string? SecurityType { get; set; }

    public string? SecurityId { get; set; }

    public string? SecurityPassword { get; set; }

    public string? TxnDate { get; set; }

    public string? AuthStatus { get; set; }

    public string? SettlementType { get; set; }

    public string? AdditionalInfo1 { get; set; }

    public string? AdditionalInfo2 { get; set; }

    public string? AdditionalInfo3 { get; set; }

    public string? AdditionalInfo4 { get; set; }

    public string? AdditionalInfo5 { get; set; }

    public string? AdditionalInfo6 { get; set; }

    public string? AdditionalInfo7 { get; set; }

    public string? ErrorStatus { get; set; }

    public string? ErrorDescription { get; set; }

    public string? CheckSum1 { get; set; }

    public string? Status { get; set; }

    public DateTime? EntryDate { get; set; }
}
