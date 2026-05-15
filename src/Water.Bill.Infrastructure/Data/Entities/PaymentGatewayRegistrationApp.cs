using System;
using System.Collections.Generic;

namespace Water.Bill.Infrastructure.Data.Entities;

public partial class PaymentGatewayRegistrationApp
{
    public int AutoId { get; set; }

    public string? MerchantId { get; set; }

    public string? KeyId { get; set; }

    public string? KeySecret { get; set; }

    public string? Currency { get; set; }

    public bool? IsLive { get; set; }

    public string? SecureToken { get; set; }

    public DateTime? TokenExpireOn { get; set; }

    public string? ApiUrl { get; set; }

    public string? WebUrl { get; set; }
}
