namespace Water.Bill.Infrastructure.Data.Entities;

public partial class NewConnectionApplicationFee
{
    public long Id { get; set; }

    public long ApplicationId { get; set; }

    public string ApplicationNo { get; set; } = null!;

    public int FeeConfigurationId { get; set; }

    public decimal ApplicationFee { get; set; }

    public decimal ProcessingFee { get; set; }

    public decimal SecurityAmount { get; set; }

    public decimal MeterInstallationFee { get; set; }

    public decimal OtherCharges { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentStatus { get; set; } = "Pending";

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual NewConnectionApplication Application { get; set; } = null!;

    public virtual NewConnectionFeeConfiguration FeeConfiguration { get; set; } = null!;
}
