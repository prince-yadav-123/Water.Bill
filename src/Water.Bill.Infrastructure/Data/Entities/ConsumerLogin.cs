using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class ConsumerLogin
{
    public int AutoId { get; set; }

    public string? MobileNo { get; set; }

    public string? Photo { get; set; }

    public string? FirebaseToken { get; set; }

    public string? OtpNo { get; set; }

    public DateTime? OtpDatetime { get; set; }

    public DateTime? OtpExpiredTime { get; set; }

    public int? OtpVerified { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? LastUpdatedBy { get; set; }

    public DateTime? LastUpdatedOn { get; set; }

    public string? ConsNo { get; set; }

    public int? DevType { get; set; }

    public string? ConsumerName { get; set; }
}
