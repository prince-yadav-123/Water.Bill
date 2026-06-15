namespace Water.Bill.Application.DTOs.Payments;

public class PaymentProcessingResultDto
{
    public bool Success { get; set; }
    public bool IsAlreadyProcessed { get; set; }
    public bool IsGatewaySuccess { get; set; }
    public bool IsPending { get; set; }
    public bool IsCancelled { get; set; }
    public string? Message { get; set; }
    public string? JalReferenceId { get; set; }
    public string PaymentKind { get; set; } = PaymentReferenceKinds.Bill;
    public long? LocalEntityId { get; set; }
    public string? LocalReferenceNo { get; set; }
    public bool IsPublicFlow { get; set; }
    public string? GatewayStatusCode { get; set; }
    public string? TransactionReferenceNo { get; set; }
}
