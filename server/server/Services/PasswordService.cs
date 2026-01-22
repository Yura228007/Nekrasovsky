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
                return BCrypt.Net.BCrypt.HashPassword(plainText, workFactor: 12);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw;
            }
        }

        /// <summary>
        /// Проверяет пароль и определяет, нужно ли обновить хэш
        /// </summary>
        public PasswordVerificationResult VerifyPasswordWithUpgrade(string plainText, string storedPassword)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(storedPassword))
            {
                return new PasswordVerificationResult
                {
                    IsValid = false,
                    NeedsUpgrade = false
                };
            }

            try
            {
                // Проверяем, является ли это BCrypt хэшем
                if (storedPassword.StartsWith("$2"))
                {
                    bool isValid = BCrypt.Net.BCrypt.Verify(plainText, storedPassword);
                    return new PasswordVerificationResult
                    {
                        IsValid = isValid,
                        NeedsUpgrade = false,
                        NewHash = null
                    };
                }
                else
                {
                    // Это старый формат (AES-зашифрованный)
                    _logger.LogInformation("Legacy password format detected. Attempting verification and upgrade.");
                    
                    bool isValid = VerifyLegacyPassword(plainText, storedPassword);
                    
                    if (isValid)
                    {
                        // Создаем новый BCrypt хэш для обновления
                        string newHash = HashPassword(plainText);
                        return new PasswordVerificationResult
                        {
                            IsValid = true,
                            NeedsUpgrade = true,
                            NewHash = newHash
                        };
                    }
                    
                    return new PasswordVerificationResult
                    {
                        IsValid = false,
                        NeedsUpgrade = false
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return new PasswordVerificationResult
                {
                    IsValid = false,
                    NeedsUpgrade = false
                };
            }
        }

        /// <summary>
        /// Проверяет пароль с помощью BCrypt (обратная совместимость)
        /// </summary>
        public bool VerifyPassword(string plainText, string hashedPassword)
        {
            var result = VerifyPasswordWithUpgrade(plainText, hashedPassword);
            return result.IsValid;
        }

        /// <summary>
        /// Проверяет, является ли пароль в старом формате (нуждается в обновлении)
        /// </summary>
        public bool IsLegacyPassword(string storedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword))
                return false;

            return !storedPassword.StartsWith("$2");
        }

        /// <summary>
        /// Проверяет пароль в старом формате (AES зашифрованный)
        /// </summary>
        private bool VerifyLegacyPassword(string plainText, string encryptedPassword)
        {
            try
            {
                // Сначала пробуем расшифровать
                if (IsValidBase64(encryptedPassword))
                {
                    var decrypted = Decrypt(encryptedPassword);
                    bool matches = decrypted.Equals(plainText, StringComparison.Ordinal);
                    
                    if (matches)
                    {
                        _logger.LogInformation("Legacy password verified successfully (AES encrypted).");
                        return true;
                    }
                }
                
                // Если расшифровка не удалась, проверяем прямое сравнение
                // (на случай если в базе хранятся незашифрованные пароли)
                bool directMatch = encryptedPassword.Equals(plainText, StringComparison.Ordinal);
                
                if (directMatch)
                {
                    _logger.LogWarning("Legacy password verified with direct comparison (plain text). This is insecure!");
                }
                
                return directMatch;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying legacy password");
                return false;
            }
        }

        // Ключ AES-256 для обратной совместимости
        private static readonly byte[] Key = new byte[32]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
            0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F
        };

        // Вектор инициализации (IV) для обратной совместимости
        private static readonly byte[] IV = new byte[16]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
        };

        /// <summary>
        /// Старый метод шифрования (deprecated)
        /// </summary>
        [Obsolete("Use HashPassword instead. This method is for backward compatibility only.")]
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                using (var aes = Aes.Create())
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
        /// Старый метод расшифровки (deprecated)
        /// </summary>
        [Obsolete("Use VerifyPassword instead. This method is for backward compatibility only.")]
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            if (!IsValidBase64(cipherText))
            {
                _logger.LogWarning("Password is not in valid Base64 format.");
                return cipherText;
            }

            try
            {
                using (var aes = Aes.Create())
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting password");
                return cipherText;
            }
        }

        private static bool IsValidBase64(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;

            if (str.Length < 20)
                return false;

            try
            {
                Convert.FromBase64String(str);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Результат проверки пароля с информацией об обновлении
    /// </summary>
    public class PasswordVerificationResult
    {
        public bool IsValid { get; set; }
        public bool NeedsUpgrade { get; set; }
        public string? NewHash { get; set; }
    }
}