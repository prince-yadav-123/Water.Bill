using Water.Bill.Application.DTOs.Consumer;

namespace Water.Bill.Application.Interfaces;

public interface IConsumerAccountService
{
    Task<ConsumerAccountLoginResult> LoginAsync(string usernameOrEmail, string password, CancellationToken ct = default);
}
