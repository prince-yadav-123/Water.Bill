namespace Water.Bill.Infrastructure.Data.Entities;

public partial class NewConnectionFeeConfiguration
{
    public int Id { get; set; }

    public string? ConnectionCategory { get; set; }

    public string? ConnectionType { get; set; }

    public decimal? PipeSize { get; set; }

    public decimal? PlotSizeFrom { get; set; }

    public decimal? PlotSizeTo { get; set; }

    public decimal ApplicationFee { get; set; }

    public decimal ProcessingFee { get; set; }

    public decimal SecurityAmount { get; set; }

    public decimal MeterInstallationFee { get; set; }

    public decimal OtherCharges { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual ICollection<NewConnectionApplicationFee> ApplicationFees { get; set; } = new List<NewConnectionApplicationFee>();
}
