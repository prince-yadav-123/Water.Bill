namespace Water.Bill.Infrastructure.Data.Entities;

public partial class WorkflowStageNotification
{
    public int Id { get; set; }

    public int WorkflowStageId { get; set; }

    public string EventType { get; set; } = null!;

    public bool SendEmail { get; set; }

    public bool SendSms { get; set; }

    public bool SendWhatsApp { get; set; }

    public bool SendInAppNotification { get; set; }

    public int? TemplateId { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public virtual WorkflowStage WorkflowStage { get; set; } = null!;
}
