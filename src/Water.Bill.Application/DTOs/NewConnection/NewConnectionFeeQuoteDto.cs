namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionFeeQuoteDto
{
    public int ConfigurationId { get; set; }

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
}
