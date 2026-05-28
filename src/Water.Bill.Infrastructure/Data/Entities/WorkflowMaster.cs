namespace Water.Bill.Infrastructure.Data.Entities;

public partial class WorkflowMaster
{
    public int Id { get; set; }

    public string WorkflowName { get; set; } = null!;

    public string ApplicationType { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual ICollection<WorkflowStage> Stages { get; set; } = new List<WorkflowStage>();
}
