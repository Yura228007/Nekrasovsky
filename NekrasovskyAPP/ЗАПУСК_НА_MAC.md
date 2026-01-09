# Инструкция по запуску приложения на MacBook

## Требования

1. **macOS 13.1 или выше** (для MacCatalyst)
2. **.NET 8.0 SDK** - установлен
3. **Xcode** (для сборки MacCatalyst приложений)
4. **PostgreSQL сервер** должен быть запущен и доступен

## Шаги для запуска

### 1. Убедитесь, что сервер запущен

```bash
cd server/server
dotnet run
```

Сервер должен быть доступен по адресу `http://localhost:5000`

### 2. Запустите приложение на MacCatalyst

#### Через Visual Studio / Visual Studio Code:

1. Откройте проект `NekrasovskyAPP.sln`
2. В списке платформ выберите **MacCatalyst**
3. Нажмите **Run** или **F5**

#### Через командную строку:

```bash
cd NekrasovskyAPP/NekrasovskyAPP
dotnet build -f net8.0-maccatalyst
dotnet run -f net8.0-maccatalyst
```

### 3. Настройка подключения к серверу

Приложение автоматически использует `http://localhost:5000/` для MacCatalyst.

Если сервер запущен на другом порту или хосте, измените в файле:
`NekrasovskyAPP/NekrasovskyAPP/Services/ApiService.cs`

```csharp
return "http://localhost:5000/"; // Измените на нужный адрес
```

## Решение проблем

### Ошибка: "No suitable framework found"

Убедитесь, что установлен .NET 8.0 SDK:
```bash
dotnet --version
```

Должно быть: `8.0.x` или выше

### Ошибка: "Xcode not found"

Установите Xcode из App Store или через:
```bash
xcode-select --install
```

### Ошибка подключения к серверу

1. Проверьте, что сервер запущен:
   ```bash
   curl http://localhost:5000/api/users
   ```

2. Проверьте настройки брандмауэра macOS

3. Убедитесь, что PostgreSQL доступен

## Примечания

- Приложение будет работать как нативное macOS приложение
- Поддерживаются обе архитектуры: arm64 (Apple Silicon) и x86_64 (Intel)
- Для первого запуска может потребоваться разрешить запуск приложения в настройках безопасности macOS
