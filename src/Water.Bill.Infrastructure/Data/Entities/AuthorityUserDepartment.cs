namespace Water.Bill.Infrastructure.Data.Entities;

public partial class AuthorityUserDepartment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime CreatedOn { get; set; }
}
