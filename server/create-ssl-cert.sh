#!/bin/bash

# Скрипт для создания самоподписанного SSL сертификата для разработки
# Использование: ./create-ssl-cert.sh

set -e

CERT_DIR="server/certs"
KEY_FILE="$CERT_DIR/server.key"
CSR_FILE="$CERT_DIR/server.csr"
CRT_FILE="$CERT_DIR/server.crt"
PFX_FILE="$CERT_DIR/server.pfx"

echo "🔐 Создание SSL сертификата для сервера..."

# Создать директорию для сертификатов
mkdir -p "$CERT_DIR"

# Проверить наличие OpenSSL
if ! command -v openssl &> /dev/null; then
    echo "❌ OpenSSL не установлен. Установите OpenSSL:"
    echo "   macOS: brew install openssl"
    echo "   Ubuntu: sudo apt-get install openssl"
    exit 1
fi

# Создать приватный ключ
echo "📝 Создание приватного ключа..."
openssl genrsa -out "$KEY_FILE" 2048

# Создать запрос на сертификат
echo "📝 Создание запроса на сертификат..."
openssl req -new -key "$KEY_FILE" -out "$CSR_FILE" -subj "/CN=localhost/O=Nekrasovsky/C=RU"

# Создать самоподписанный сертификат (действителен 365 дней)
echo "📝 Создание самоподписанного сертификата..."
openssl x509 -req -days 365 -in "$CSR_FILE" -signkey "$KEY_FILE" -out "$CRT_FILE"

# Создать PFX файл
echo "📝 Создание PFX файла..."
echo "⚠️  Введите пароль для сертификата (запомните его для appsettings.json!):"
openssl pkcs12 -export -out "$PFX_FILE" -inkey "$KEY_FILE" -in "$CRT_FILE" -name "Nekrasovsky Server"

# Установить права доступа
chmod 600 "$KEY_FILE"
chmod 644 "$CRT_FILE"
chmod 600 "$PFX_FILE"

# Удалить временные файлы
rm "$CSR_FILE"

echo ""
echo "✅ SSL сертификат успешно создан!"
echo ""
echo "📁 Файлы:"
echo "   - Приватный ключ: $KEY_FILE"
echo "   - Сертификат: $CRT_FILE"
echo "   - PFX файл: $PFX_FILE"
echo ""
echo "📝 Следующие шаги:"
echo "   1. Обновите appsettings.json:"
echo "      \"Path\": \"certs/server.pfx\""
echo "      \"Password\": \"<пароль, который вы ввели>\""
echo ""
echo "   2. Запустите сервер:"
echo "      cd server/server"
echo "      dotnet run"
echo ""
echo "   3. Откройте в браузере:"
echo "      https://localhost:5001/swagger"
echo ""
