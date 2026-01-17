using BCrypt.Net;
using System.Security.Cryptography;

namespace server.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(ILogger<PasswordService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Хэширует пароль с помощью BCrypt
        /// </summary>
        public string HashPassword(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                // BCrypt автоматически добавляет соль и использует work factor 10
                return BCrypt.Net.BCrypt.HashPassword(plainText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw;
            }
        }

        /// <summary>
        /// Проверяет пароль с помощью BCrypt
        /// </summary>
        public bool VerifyPassword(string plainText, string hashedPassword)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                // Проверяем, является ли хэш валидным BCrypt хэшем
                // BCrypt хэши начинаются с $2a$, $2b$, $2y$ или $2x$
                if (!hashedPassword.StartsWith("$2"))
                {
                    // Старый формат - возможно, это зашифрованный пароль (для обратной совместимости)
                    _logger.LogWarning("Password hash does not appear to be BCrypt format. Attempting legacy verification.");
                    return VerifyLegacyPassword(plainText, hashedPassword);
                }

                return BCrypt.Net.BCrypt.Verify(plainText, hashedPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return false;
            }
        }

        /// <summary>
        /// Проверяет пароль в старом формате (AES зашифрованный) для обратной совместимости
        /// </summary>
        private bool VerifyLegacyPassword(string plainText, string encryptedPassword)
        {
            try
            {
                var decrypted = Decrypt(encryptedPassword);
                return decrypted.Equals(plainText, StringComparison.Ordinal);
            }
            catch
            {
                // Если расшифровка не удалась, пробуем прямое сравнение (на случай, если пароль не зашифрован)
                return encryptedPassword.Equals(plainText, StringComparison.Ordinal);
            }
        }

        // Ключ должен быть длиной 32 байта (AES-256) - для обратной совместимости
        private static readonly byte[] Key = new byte[32]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F
        };

        // Вектор инициализации (IV) должен быть длиной 16 байт - для обратной совместимости
        private static readonly byte[] IV = new byte[16]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
        };

        /// <summary>
        /// Старый метод шифрования (deprecated) - для обратной совместимости
        /// </summary>
        [Obsolete("Use HashPassword instead. This method is for backward compatibility only.")]
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = IV;

                    var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (var sw = new StreamWriter(cs))
                            {
                                sw.Write(plainText);
                            }
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting password");
                throw;
            }
        }

        /// <summary>
        /// Старый метод расшифровки (deprecated) - для обратной совместимости
        /// </summary>
        [Obsolete("Password hashing is one-way. Use VerifyPassword instead. This method is for backward compatibility only.")]
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            // Проверяем, является ли строка валидным Base64
            // Зашифрованные пароли всегда в формате Base64
            if (!IsValidBase64(cipherText))
            {
                _logger.LogWarning("Password is not encrypted (not a valid Base64 string). Returning as plain text for backward compatibility.");
                return cipherText; // Возвращаем как есть для обратной совместимости
            }

            try
            {
                using (System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = IV;

                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (FormatException)
            {
                // Если Base64 валиден, но расшифровка не удалась - возможно, пароль не зашифрован
                _logger.LogWarning("Failed to decrypt password. Password may not be encrypted. Returning as plain text for backward compatibility.");
                return cipherText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting password");
                // В случае другой ошибки возвращаем исходную строку для обратной совместимости
                return cipherText;
            }
        }

        private static bool IsValidBase64(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;

            // Base64 строка должна содержать только символы A-Z, a-z, 0-9, +, /, = и пробелы
            // Зашифрованные пароли обычно длиннее 20 символов
            if (str.Length < 20)
                return false;

            try
            {
                // Пытаемся преобразовать в байты
                Convert.FromBase64String(str);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}