using Microsoft.AspNetCore.DataProtection;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private readonly IDataProtector _protector;

    public EncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("NotificationSystem.ProviderConfiguration");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            return _protector.Unprotect(cipherText);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            // Se falhar ao descriptografar, assume que o dado já está descriptografado
            // Isso pode acontecer com dados antigos ou dados inseridos diretamente no banco
            return cipherText;
        }
    }
}
