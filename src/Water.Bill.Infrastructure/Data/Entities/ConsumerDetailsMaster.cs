using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerDetailsMaster
{
    public string ConsNo { get; set; } = null!;

    public string? ConsNm1 { get; set; }

    public string? ConsNm2 { get; set; }

    public string? ConTp { get; set; }

    public string? ConsCtg { get; set; }

    public string? FlatType { get; set; }

    public string? FlatNo { get; set; }

    public string? BlkNo { get; set; }

    public string? Sector { get; set; }

    public int? PlotSize { get; set; }

    public int? PipeSize { get; set; }

    public string? RegNo { get; set; }

    public DateTime? ConnDt { get; set; }

    public string? EstiNo { get; set; }

    public int? EstiAmt { get; set; }

    public int? Secu { get; set; }

    public DateTime? EstiDt { get; set; }

    public int? Esti1Amt { get; set; }

    public int? DevType { get; set; }

    public string? OldNewEn { get; set; }

    public string? OldNewUp { get; set; }

    public string? MobNo { get; set; }

    public string? EmailId { get; set; }

    public string? ConsAddress { get; set; }

    public string? OtherCon { get; set; }

    public string? IssueOfficer { get; set; }

    public string? PurposeCon { get; set; }

    public DateTime? NoduesDt { get; set; }

    public DateTime? TypeChangeDate { get; set; }

    public double? MonthlyRate { get; set; }

    public int? Status { get; set; }

    public string? Userid { get; set; }

    public DateTime? EntryDate { get; set; }

    public DateTime? ModifyDate { get; set; }

    public DateTime? DeleteDate { get; set; }

    public double? CessAmt { get; set; }

    public double? MaonthyCharges { get; set; }

    public DateTime? CalDate { get; set; }

    public DateTime? LedgerDate { get; set; }

    public string? KhasraNo { get; set; }

    public string? VillgaeName { get; set; }

    public int? VillgaeId { get; set; }

    public int? NewStatus { get; set; }

    public long? Rid { get; set; }

    public string? Narration { get; set; }

    public string? PlotMapId { get; set; }

    public string? KiloLitter { get; set; }

    public virtual ICollection<ConsumerDetailsTran> ConsumerDetailsTrans { get; set; } = new List<ConsumerDetailsTran>();

    public virtual ICollection<ConsumerNodue> ConsumerNodues { get; set; } = new List<ConsumerNodue>();

    public virtual ICollection<ConsumerTransfer> ConsumerTransfers { get; set; } = new List<ConsumerTransfer>();
}
