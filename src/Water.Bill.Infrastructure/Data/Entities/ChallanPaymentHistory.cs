namespace Water.Bill.Infrastructure.Data.Entities;

public class ChallanPaymentHistory
{
    public long Id { get; set; }

    public long ChallanId { get; set; }

    public string? ChallanNo { get; set; }

    public string? ConsumerNo { get; set; }

    public string? SourceBillNo { get; set; }

    public double Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? PaymentMode { get; set; }

    public string? BankCode { get; set; }

    public string? BankName { get; set; }

    public string? TransactionReferenceNo { get; set; }

    public string? Remarks { get; set; }

    public int? PostedByUserId { get; set; }

    public string? PostedByName { get; set; }

    public DateTime PostedOn { get; set; }

    public bool IsDeleted { get; set; }
}
