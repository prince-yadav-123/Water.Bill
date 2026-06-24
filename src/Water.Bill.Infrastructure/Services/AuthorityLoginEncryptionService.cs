using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Water.Bill.Application.DTOs.Auth;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.Infrastructure.Services;

public class AuthorityLoginEncryptionService : IAuthorityLoginEncryptionService, IDisposable
{
    private const int GcmNonceSize = 12;
    private const int GcmTagSize = 16;
    private static readonly TimeSpan PayloadLifetime = TimeSpan.FromMinutes(3);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<AuthorityLoginEncryptionService> _logger;
    private readonly RSA _rsa;
    private readonly string _keyId;
    private readonly string _publicKeySpkiBase64;
    private readonly IDataProtector _requestTokenProtector;

    public AuthorityLoginEncryptionService(
        ILogger<AuthorityLoginEncryptionService> logger,
        IDataProtectionProvider dataProtectionProvider)
    {
        _logger = logger;
        _rsa = RSA.Create(3072);
        _keyId = $"authority-login-{Guid.NewGuid():N}";
        _publicKeySpkiBase64 = Convert.ToBase64String(_rsa.ExportSubjectPublicKeyInfo());
        _requestTokenProtector = dataProtectionProvider.CreateProtector("Water.Bill.AuthorityLoginEncryption.RequestToken");
    }

    public AuthorityLoginEncryptionKeyDto GetPublicKey()
    {
        var issuedAt = DateTimeOffset.UtcNow;
        return new AuthorityLoginEncryptionKeyDto
        {
            KeyId = _keyId,
            PublicKeySpkiBase64 = _publicKeySpkiBase64,
            ExpiresAtUnixSeconds = issuedAt.Add(PayloadLifetime).ToUnixTimeSeconds(),
            RequestToken = _requestTokenProtector.Protect($"{_keyId}|{issuedAt.ToUnixTimeSeconds()}")
        };
    }

    public LoginRequestDto DecryptLoginRequest(EncryptedAuthorityLoginRequestDto request)
    {
        if (request is null
            || string.IsNullOrWhiteSpace(request.KeyId)
            || string.IsNullOrWhiteSpace(request.RequestToken)
            || string.IsNullOrWhiteSpace(request.EncryptedKey)
            || string.IsNullOrWhiteSpace(request.Iv)
            || string.IsNullOrWhiteSpace(request.CipherText))
        {
            throw new InvalidOperationException("Encrypted login payload is incomplete.");
        }

        if (!string.Equals(request.KeyId, _keyId, StringComparison.Ordinal))
            throw new InvalidOperationException("Encrypted login payload is no longer valid. Please try again.");

        try
        {
            ValidateRequestToken(request);

            var encryptedKey = Convert.FromBase64String(request.EncryptedKey);
            var iv = Convert.FromBase64String(request.Iv);
            var cipherBytes = Convert.FromBase64String(request.CipherText);

            if (iv.Length != GcmNonceSize)
                throw new InvalidOperationException("Encrypted login payload is invalid.");
            if (cipherBytes.Length <= GcmTagSize)
                throw new InvalidOperationException("Encrypted login payload is invalid.");

            var aesKey = _rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);
            var cipher = cipherBytes[..^GcmTagSize];
            var tag = cipherBytes[^GcmTagSize..];
            var plaintext = new byte[cipher.Length];

            using var aes = new AesGcm(aesKey, GcmTagSize);
            aes.Decrypt(iv, cipher, tag, plaintext);

            var payload = JsonSerializer.Deserialize<AuthorityLoginPlainPayload>(plaintext, JsonOptions)
                ?? throw new InvalidOperationException("Encrypted login payload is invalid.");

            return new LoginRequestDto
            {
                Username = payload.Username?.Trim() ?? string.Empty,
                Password = payload.Password ?? string.Empty,
                RememberMe = payload.RememberMe
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException or JsonException)
        {
            _logger.LogWarning(ex, "Authority login payload decryption failed for key {KeyId}.", request.KeyId);
            throw new InvalidOperationException("Login request could not be processed. Please try again.");
        }
    }

    private void ValidateRequestToken(EncryptedAuthorityLoginRequestDto request)
    {
        try
        {
            var tokenText = _requestTokenProtector.Unprotect(request.RequestToken);
            var parts = tokenText.Split('|', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2
                || !string.Equals(parts[0], request.KeyId, StringComparison.Ordinal)
                || !long.TryParse(parts[1], out var issuedUnixSeconds))
            {
                throw new InvalidOperationException("Encrypted login payload is invalid.");
            }

            var age = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(issuedUnixSeconds);
            if (age < TimeSpan.Zero || age > PayloadLifetime)
                throw new InvalidOperationException("Encrypted login payload has expired. Please login again.");
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Authority login request token validation failed for key {KeyId}.", request.KeyId);
            throw new InvalidOperationException("Encrypted login payload is invalid.");
        }
    }

    public void Dispose()
        => _rsa.Dispose();

    private sealed class AuthorityLoginPlainPayload
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public long TimestampUnixSeconds { get; set; }
    }
}
