using System.ComponentModel.DataAnnotations;
using Water.Bill.Application.DTOs.Auth;

namespace Water.Bill.API.ViewModels;

public class AuthorityLoginViewModel
{
    public string Username { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = true;

    public string KeyId { get; set; } = string.Empty;
    public string RequestToken { get; set; } = string.Empty;
    public string EncryptedKey { get; set; } = string.Empty;
    public string Iv { get; set; } = string.Empty;
    public string CipherText { get; set; } = string.Empty;

    public EncryptedAuthorityLoginRequestDto ToEncryptedRequest()
        => new()
        {
            KeyId = KeyId,
            RequestToken = RequestToken,
            EncryptedKey = EncryptedKey,
            Iv = Iv,
            CipherText = CipherText
        };
}
