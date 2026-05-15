using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class PaymentCollection
{
    public int AutoId { get; set; }

    public string OrderId { get; set; } = null!;

    public string? TransactionNo { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? ConsNo { get; set; }

    public string? ChallanNo { get; set; }

    public string? Rid { get; set; }

    public DateOnly? BillDate { get; set; }

    public decimal? TotalBillAmt { get; set; }

    public string? SuccessStatus { get; set; }

    public string? Message { get; set; }

    public string? Signature { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? LastUpdatedOn { get; set; }

    public int? CreatedBy { get; set; }

    public string? PaymentId { get; set; }

    public int? LastUpdatedBy { get; set; }

    public string? MobileNo { get; set; }

    public string? HashCode { get; set; }

    public string? Securetoken { get; set; }

    public bool? IsErpSend { get; set; }

    public DateTime? ErpRequestTime { get; set; }

    public int? AttemptCount { get; set; }

    public DateTime? ErpResponseTime { get; set; }

    public string? ErpResponseMassege { get; set; }

    public DateTime? TransDate { get; set; }

    public string? TrackingId { get; set; }

    public bool? IsRetry { get; set; }

    public string? Att1 { get; set; }

    public string? AppFlag { get; set; }

    public string? PaymentType { get; set; }
}
