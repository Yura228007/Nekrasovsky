# Настройка SSL сертификата для сервера

## Вариант 1: Использование встроенного dev сертификата (для разработки)

.NET SDK автоматически создает и использует dev сертификат для HTTPS. Это самый простой способ для разработки.

### Установка/проверка dev сертификата

```bash
# Проверить наличие сертификата
dotnet dev-certs https --check

# Если сертификата нет, создать его
dotnet dev-certs https --trust

# На macOS может потребоваться:
dotnet dev-certs https --trust -v
```

### Запуск сервера с HTTPS

```bash
cd server/server
dotnet run
```

Сервер будет доступен по адресам:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

---

## Вариант 2: Использование кастомного SSL сертификата (для production)

### Шаг 1: Создание самоподписанного сертификата (для тестирования)

#### На macOS/Linux:

```bash
# Создать приватный ключ
openssl genrsa -out server.key 2048

# Создать запрос на сертификат (CSR)
openssl req -new -key server.key -out server.csr

# Создать самоподписанный сертификат (действителен 365 дней)
openssl x509 -req -days 365 -in server.csr -signkey server.key -out server.crt

# Создать PFX файл (для .NET)
openssl pkcs12 -export -out server.pfx -inkey server.key -in server.crt

# При запросе пароля введите пароль (запомните его!)
```

#### На Windows (PowerShell):

```powershell
# Создать самоподписанный сертификат
$cert = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "cert:\LocalMachine\My" -KeyExportPolicy Exportable -KeySpec Signature -KeyLength 2048 -KeyAlgorithm RSA -HashAlgorithm SHA256

# Экспортировать в PFX
$pwd = ConvertTo-SecureString -String "YourPassword123!" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "server.pfx" -Password $pwd
```

### Шаг 2: Настройка appsettings.json

Скопируйте созданный `.pfx` файл в папку `server/server/certs/` (создайте папку, если её нет) и обновите `appsettings.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001"
      }
    },
    "Certificates": {
      "Default": {
        "Path": "certs/server.pfx",
        "Password": "YourPassword123!"
      }
    }
  }
}
```

**Важно:** 
- Не коммитьте файлы сертификатов и пароли в git!
- Добавьте `certs/` в `.gitignore`
- Для production используйте сертификаты от доверенного CA (Let's Encrypt, и т.д.)

### Шаг 3: Запуск сервера

```bash
cd server/server
dotnet run
```

---

## Вариант 3: Использование Let's Encrypt (для production)

Для production окружения рекомендуется использовать бесплатные сертификаты от Let's Encrypt.

### Установка certbot

```bash
# macOS
brew install certbot

# Ubuntu/Debian
sudo apt-get update
sudo apt-get install certbot
```

### Получение сертификата

```bash
# Получить сертификат для домена
sudo certbot certonly --standalone -d yourdomain.com

# Сертификаты будут в:
# /etc/letsencrypt/live/yourdomain.com/fullchain.pem
# /etc/letsencrypt/live/yourdomain.com/privkey.pem
```

### Конвертация в PFX

```bash
# Конвертировать в PFX формат
openssl pkcs12 -export -out server.pfx \
  -inkey /etc/letsencrypt/live/yourdomain.com/privkey.pem \
  -in /etc/letsencrypt/live/yourdomain.com/fullchain.pem
```

Затем настройте путь в `appsettings.json` как в Варианте 2.

---

## Проверка работы SSL

### Проверка через браузер

1. Откройте `https://localhost:5001/swagger`
2. Если используется dev сертификат, браузер может показать предупреждение о безопасности
3. Нажмите "Дополнительно" → "Перейти на сайт" (для dev сертификата это нормально)

### Проверка через curl

```bash
# Проверка HTTPS соединения
curl -k https://localhost:5001/api/database/check

# С проверкой сертификата (для production)
curl https://yourdomain.com/api/database/check
```

---

## Устранение проблем

### Ошибка: "The certificate is invalid"

**Решение:**
1. Убедитесь, что путь к сертификату правильный
2. Проверьте, что пароль правильный
3. Убедитесь, что сертификат не истек

### Ошибка: "Permission denied" при чтении сертификата

**Решение:**
```bash
# Убедитесь, что файл доступен для чтения
chmod 644 certs/server.pfx
```

### Dev сертификат не работает

**Решение:**
```bash
# Удалить старый сертификат
dotnet dev-certs https --clean

# Создать новый
dotnet dev-certs https --trust
```

---

## Безопасность

⚠️ **Важно для production:**

1. **Никогда не коммитьте:**
   - Файлы сертификатов (`.pfx`, `.key`, `.crt`)
   - Пароли от сертификатов
   - Приватные ключи

2. **Добавьте в `.gitignore`:**
   ```
   certs/
   *.pfx
   *.key
   *.crt
   *.pem
   ```

3. **Используйте переменные окружения для паролей:**
   ```json
   "Certificates": {
     "Default": {
       "Path": "certs/server.pfx",
       "Password": "${CERTIFICATE_PASSWORD}"
     }
   }
   ```

4. **Ограничьте доступ к файлам сертификатов:**
   ```bash
   chmod 600 certs/server.pfx
   ```

---

## Обновление сертификата Let's Encrypt

Сертификаты Let's Encrypt действительны 90 дней. Настройте автоматическое обновление:

```bash
# Добавить в crontab
sudo crontab -e

# Добавить строку (обновление каждые 60 дней)
0 0 1 */2 * certbot renew --quiet && systemctl reload nginx
```

---

## Дополнительные ресурсы

- [Документация ASP.NET Core Kestrel](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel)
- [Let's Encrypt](https://letsencrypt.org/)
- [OpenSSL документация](https://www.openssl.org/docs/)
