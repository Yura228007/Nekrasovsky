# Восстановление истории миграций

Если в `__EFMigrationsHistory` оказались только `InitialCreate` и `ShiftReportFileContent`, три миграции между ними не применяются, и в БД не создаются столбцы `ProductBatchId`, `DefectQuantity`, `RewindQuantity`, `FileContent` и т.д.

## Что сделать (один раз)

1. **Выполнить SQL в вашей PostgreSQL-базе** (psql, pgAdmin, DBeaver и т.п.), подключённой к той же БД, что и приложение:
   - Откройте и выполните скрипт `FixMigrationHistory.sql` из этой папки.

2. **Применить миграции из папки проекта сервера:**
   ```powershell
   cd d:\Projects\Nekrasovsky\server\server
   dotnet ef database update --context AppDbContext
   ```

Миграции применятся по порядку и являются идемпотентными (добавляют столбцы только если их ещё нет), поэтому повторный запуск безопасен.

3. **Проверить список миграций:**
   ```powershell
   dotnet ef migrations list --context AppDbContext
   ```
   Должны быть отмечены как применённые все пять миграций:
   - 20260202054247_InitialCreate
   - 20260202120000_ProductOutputProductBatchId
   - 20260202130000_ReprocessingDefectQuantity
   - 20260202140000_ProductOutputRewindQuantity
   - 20260203050108_ShiftReportFileContent
