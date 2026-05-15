using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class CcavenueTransactiponStatus
{
    public int AutoId { get; set; }

    public string? OrderId { get; set; }

    public string? TrackingId { get; set; }

    public string? BankRefNo { get; set; }

    public string? OrderStatus { get; set; }

    public string? FailureMessage { get; set; }

    public string? PaymentMode { get; set; }

    public string? CardName { get; set; }

    public string? StatusCode { get; set; }

    public string? StatusMessage { get; set; }

    public string? Currency { get; set; }

    public string? Amount { get; set; }

    public string? BillingName { get; set; }

    public string? BillingAddress { get; set; }

    public string? BillingCity { get; set; }

    public string? BillingState { get; set; }

    public string? BillingZip { get; set; }

    public string? BillingCountry { get; set; }

    public string? BillingTel { get; set; }

    public string? DeliveryName { get; set; }

    public string? DeliveryAddress { get; set; }

    public string? DeliveryCity { get; set; }

    public string? DeliveryState { get; set; }

    public string? DeliveryZip { get; set; }

    public string? DeliveryCountry { get; set; }

    public string? DeliveryTel { get; set; }

    public string? MerchantParam1 { get; set; }

    public string? MerchantParam2 { get; set; }

    public string? MerchantParam3 { get; set; }

    public string? MerchantParam4 { get; set; }

    public string? MerchantParam5 { get; set; }

    public string? Vault { get; set; }

    public string? OfferType { get; set; }

    public string? OfferCode { get; set; }

    public string? DiscountValue { get; set; }

    public string? MerAmount { get; set; }

    public string? EciValue { get; set; }

    public string? Retry { get; set; }

    public string? ResponseCode { get; set; }

    public string? BillingNotes { get; set; }

    public DateTime? TransDate { get; set; }

    public string? BinCountry { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public bool? Status { get; set; }

    public string? PaymentStatus { get; set; }

    public string? EncResp { get; set; }

    public string? ErrorMessage { get; set; }
}
