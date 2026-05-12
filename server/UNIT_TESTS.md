# Юнит-тесты сервера Nekrasovsky

Кратко: в проекте **`server/ControllersTests`** добавлены автоматические тесты (xUnit) для части бизнес-логики backend. Проект подключён к **`server.sln`**.

## Что покрыто тестами

| Файл | Что проверяется |
|------|------------------|
| `PasswordServiceTests.cs` | Хэширование и проверка пароля (BCrypt), пустой ввод, legacy plain-текст и флаг обновления хэша, `IsLegacyPassword`. |
| `UserPermissionsServiceTests.cs` | Назначение и снятие прав пользователю, дубликаты, ошибки при несуществующих пользователе/праве, полная замена набора прав (`UpdateUserPermissionsAsync`). |
| `RecipeServiceTests.cs` | Создание рецепта (продукт + материал), ошибки если сущности нет, запрет дубликата рецепта, выборка рецептов по продукту. |

Инфраструктура: **`TestDbContextFactory.cs`** — создание **in-memory** `AppDbContext` (отдельная БД на каждый тест) и простые сиды тестовых данных.

Тесты **не** поднимают PostgreSQL и **не** запускают Kestrel; для сервисов, завязанных на EF, используется **Microsoft.EntityFrameworkCore.InMemory** (версия совпадает с основным проектом: **8.0.10**).

## Требования

- Установлен [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0) (`dotnet --version` показывает 8.x).

## Как запустить тесты

Из каталога **`server`** (где лежит `server.sln`):

**Только тестовый проект:**

```bash
dotnet test server/ControllersTests/ControllersTests.csproj
```

**Все проекты в solution (сейчас сервер + тесты):**

```bash
dotnet test server.sln
```

Полезные опции:

```bash
# подробный вывод
dotnet test server/ControllersTests/ControllersTests.csproj --verbosity normal

# с покрытием (пакет coverlet уже указан в тестовом csproj)
dotnet test server/ControllersTests/ControllersTests.csproj --collect:"XPlat Code Coverage"
```

При успехе в конце будет строка вроде: **Passed!  - Failed: 0, Passed: N**.

## Где лежат файлы

```
server/
  server.sln
  UNIT_TESTS.md                    ← этот файл
  server/
    ControllersTests/
      ControllersTests.csproj
      TestDbContextFactory.cs
      PasswordServiceTests.cs
      UserPermissionsServiceTests.cs
      RecipeServiceTests.cs
    server.csproj
    ...
```

## Добавление новых тестов

1. Создайте класс `*Tests.cs` в `server/ControllersTests/`.
2. Используйте `TestDbContextFactory.CreateContext()` для БД или тестируйте сервисы без БД (как `PasswordService`).
3. Запустите `dotnet test` из каталога `server`.

При расширении сценариев помните: in-memory EF **не** повторяет все особенности PostgreSQL; для интеграционных проверок миграций и SQL лучше отдельный проект с тестовой БД.
