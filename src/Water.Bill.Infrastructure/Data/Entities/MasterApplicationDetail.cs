using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class MasterApplicationDetail
{
    public string ApplicationId { get; set; } = null!;

    public string? ConsNo { get; set; }

    public string? ConName { get; set; }

    public string? ConAddress { get; set; }

    public string? ConPhoneMobile { get; set; }

    public string? SectorVill { get; set; }

    public string? Block { get; set; }

    public string? PlotNo { get; set; }

    public string? PlotArea { get; set; }

    public string? PipeSize { get; set; }

    public string? SeewarPurpose { get; set; }

    public string? ConnType { get; set; }

    public string? PropertyType { get; set; }

    public string? PrevConYesNo { get; set; }

    public string? PrevConDetail { get; set; }

    public int? Status { get; set; }

    public DateOnly? EnterDate { get; set; }

    public byte[]? PhotoId { get; set; }

    public byte[]? SingId { get; set; }

    public string? DivName { get; set; }

    public string? ApplicationStatus { get; set; }

    public DateOnly? StatusDate { get; set; }

    public string? ApplcationStatusDetail { get; set; }

    public int? Reg { get; set; }

    public string? CurrentHoldingPer { get; set; }

    public string? AppType { get; set; }
}
