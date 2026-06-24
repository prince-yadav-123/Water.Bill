namespace Water.Bill.Application.DTOs.Auth;

public class EncryptedAuthorityLoginRequestDto
{
    public string KeyId { get; set; } = string.Empty;
    public string RequestToken { get; set; } = string.Empty;
    public string EncryptedKey { get; set; } = string.Empty;
    public string Iv { get; set; } = string.Empty;
    public string CipherText { get; set; } = string.Empty;
}
