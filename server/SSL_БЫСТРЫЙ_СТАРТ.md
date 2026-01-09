# SSL сертификат - Быстрый старт

## 🚀 Для разработки (самый простой способ)

### Шаг 1: Установить/проверить dev сертификат

```bash
# Проверить наличие
dotnet dev-certs https --check

# Если нужно, создать и доверить
dotnet dev-certs https --trust
```

### Шаг 2: Запустить сервер

```bash
cd server/server
dotnet run
```

Готово! Сервер доступен по адресам:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/swagger`

---

## 🔐 Для production (кастомный сертификат)

### Шаг 1: Создать сертификат

```bash
# Перейти в папку server
cd server

# Запустить скрипт создания сертификата
./create-ssl-cert.sh

# Ввести пароль для сертификата (запомните его!)
```

### Шаг 2: Настроить appsettings.json

Откройте `server/server/appsettings.json` и обновите:

```json
{
  "Kestrel": {
    "Certificates": {
      "Default": {
        "Path": "certs/server.pfx",
        "Password": "ваш_пароль_от_сертификата"
      }
    }
  }
}
```

### Шаг 3: Запустить сервер

```bash
cd server/server
dotnet run
```

---

## ⚠️ Важно

- **Не коммитьте** файлы сертификатов в git (они уже в .gitignore)
- **Не делитесь** паролями от сертификатов
- Для production используйте сертификаты от доверенного CA (Let's Encrypt)

Подробная инструкция: [НАСТРОЙКА_SSL.md](./НАСТРОЙКА_SSL.md)
