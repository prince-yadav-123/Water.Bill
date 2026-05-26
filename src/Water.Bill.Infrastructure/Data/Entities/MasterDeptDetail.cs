using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class MasterDeptDetail
{
    public int Id { get; set; }

    public int DeptId { get; set; }

    public string? DeptName { get; set; }

    public string? Status { get; set; }

    public int? DevType { get; set; }
}
