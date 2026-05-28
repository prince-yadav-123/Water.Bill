namespace Water.Bill.Application.DTOs.NewConnection;

public class NewConnectionFeeRequestDto
{
    public string? ConnectionCategory { get; set; }

    public string? ConnectionType { get; set; }

    public decimal? PipeSize { get; set; }

    public decimal? PlotSize { get; set; }
}
