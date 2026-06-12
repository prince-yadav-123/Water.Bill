namespace Water.Bill.Application.DTOs.Consumer;

public class ConsumerMobileRegistrationEligibilityResult
{
    public string ConsumerNo { get; set; } = string.Empty;
    public bool ConsumerExists { get; set; }
    public bool IsActiveConsumer { get; set; }
    public bool HasRegisteredMobile { get; set; }
    public bool CanRegisterMobile => ConsumerExists && IsActiveConsumer && !HasRegisteredMobile;
}
