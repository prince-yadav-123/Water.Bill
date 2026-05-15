using Water.Bill.Application.DTOs.Auth;

namespace Water.Bill.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
}
