using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerEmail
{
    public int Id { get; set; }

    public string? ConsNo { get; set; }

    public string? Emailid { get; set; }

    public string? Name { get; set; }

    public string? Property { get; set; }

    public int? Status { get; set; }

    public int? Isactive { get; set; }
}
