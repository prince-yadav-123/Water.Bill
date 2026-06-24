using Water.Bill.Application.DTOs.Auth;

namespace Water.Bill.Application.Interfaces;

public interface IAuthorityLoginEncryptionService
{
    AuthorityLoginEncryptionKeyDto GetPublicKey();
    LoginRequestDto DecryptLoginRequest(EncryptedAuthorityLoginRequestDto request);
}
