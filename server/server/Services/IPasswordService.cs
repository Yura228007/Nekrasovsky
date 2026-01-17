namespace server.Services
{
    public interface IPasswordService
    {
        string HashPassword(string plainText);
        bool VerifyPassword(string plainText, string hashedPassword);
        
        // Deprecated methods for backward compatibility during migration
        [Obsolete("Use HashPassword instead. This method is for backward compatibility only.")]
        string Encrypt(string plainText);
        [Obsolete("Password hashing is one-way. Use VerifyPassword instead. This method is for backward compatibility only.")]
        string Decrypt(string cipherText);
    }
}

