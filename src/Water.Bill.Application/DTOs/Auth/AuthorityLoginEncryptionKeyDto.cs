namespace Water.Bill.Application.DTOs.Auth;

public class AuthorityLoginEncryptionKeyDto
{
    public string KeyId { get; set; } = string.Empty;
    public string PublicKeySpkiBase64 { get; set; } = string.Empty;
    public string Algorithm { get; set; } = "RSA-OAEP-256/AES-GCM-256";
    public long ExpiresAtUnixSeconds { get; set; }
    public string RequestToken { get; set; } = string.Empty;
}
