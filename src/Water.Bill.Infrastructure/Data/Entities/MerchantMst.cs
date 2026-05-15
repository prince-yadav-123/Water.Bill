using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class MerchantMst
{
    public int MerchantId { get; set; }

    public string? MerchantKey { get; set; }

    public string? MerchantSecreateKey { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? SecureToken { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? LastUpdatedBy { get; set; }

    public DateTime? LastUpdatedOn { get; set; }
}
