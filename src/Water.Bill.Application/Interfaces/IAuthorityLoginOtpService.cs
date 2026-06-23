using Water.Bill.Application.DTOs.Auth;

namespace Water.Bill.Application.Interfaces;

public interface IAuthorityLoginOtpService
{
    Task<AuthorityLoginOtpChallengeResult> RequestOtpAsync(AuthorityLoginValidationResult user, CancellationToken ct = default);
    Task<AuthorityLoginOtpChallengeResult?> GetChallengeAsync(string challengeToken, CancellationToken ct = default);
    Task<AuthorityLoginOtpChallengeResult> ResendOtpAsync(string challengeToken, CancellationToken ct = default);
    Task<AuthorityLoginOtpVerifyResult> VerifyOtpAsync(string challengeToken, string otp, CancellationToken ct = default);
}
