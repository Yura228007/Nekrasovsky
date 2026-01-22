namespace server.Services
{
    public interface IPasswordService
    {
        /// <summary>
        /// Хэширует пароль с помощью BCrypt
        /// </summary>
        string HashPassword(string plainText);

        /// <summary>
        /// Проверяет пароль (обратная совместимость)
        /// </summary>
        bool VerifyPassword(string plainText, string hashedPassword);

        /// <summary>
        /// Проверяет пароль и определяет, нужно ли обновить хэш
        /// </summary>
        PasswordVerificationResult VerifyPasswordWithUpgrade(string plainText, string storedPassword);

        /// <summary>
        /// Проверяет, является ли пароль в старом формате
        /// </summary>
        bool IsLegacyPassword(string storedPassword);

        /// <summary>
        /// Старый метод шифрования (deprecated)
        /// </summary>
        [Obsolete("Use HashPassword instead")]
        string Encrypt(string plainText);

        /// <summary>
        /// Старый метод расшифровки (deprecated)
        /// </summary>
        [Obsolete("Use VerifyPassword instead")]
        string Decrypt(string cipherText);
    }
}