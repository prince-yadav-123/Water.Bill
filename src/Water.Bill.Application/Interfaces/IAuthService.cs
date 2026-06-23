using Water.Bill.Application.DTOs.Auth;

namespace Water.Bill.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<AuthorityLoginValidationResult> ValidateAuthorityCredentialsAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<LoginResponseDto> CompleteAuthorityLoginAsync(int userId, CancellationToken ct = default);
}
