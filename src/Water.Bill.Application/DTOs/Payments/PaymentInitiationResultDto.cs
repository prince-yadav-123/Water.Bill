namespace Water.Bill.Application.DTOs.Payments;

public class PaymentInitiationResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? JalReferenceId { get; set; }
    public string GatewayCode { get; set; } = "AX";
    public string GatewayName { get; set; } = "AXIS";
    public double Amount { get; set; }
    public bool IsLiveGatewayEnabled { get; set; }
    public string? GatewayUrl { get; set; }
    public string? GatewayPayload { get; set; }
}
