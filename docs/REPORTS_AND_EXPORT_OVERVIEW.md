# Отчёты и выгрузка — текущая реализация

Краткий обзор: какие отчёты есть, как они генерируются и как реализована выгрузка (скачивание).

---

## 1. Типы отчётов

### 1.1. Отчёт о смене (Shift Report)

- **Назначение:** отчёт по завершённой смене (WorkReport) — производство, заявки на детали, ответственности.
- **Где:** сервис `ShiftReportService`, контроллер `ShiftReportsController`, модель `ShiftReport`.
- **Когда создаётся:**
  - **Автоматически** при завершении смены: `WorkReportsController.FinishWork` → `_shiftReportService.GenerateReportAsync(workReportId)` (после `FinishWorkAsync`). При ошибке генерации — только лог, ответ 200 всё равно возвращается.
  - **Вручную:** `POST api/shiftreports/generate/{workReportId}?requestingUserId=...` (доступ: свой сменный отчёт или Admin/Owner).
- **Содержимое Excel (ClosedXML):**
  - Лист «Отчёт о смене»: дата, ФИО, должность, смена, производство (артикул, продукт, станок, выпущено, брак, эко, ед.изм.), итоги, подписи.
  - Лист «Заявки на перемещение»: материал, кол-во, откуда/куда, статус (если есть заявки).
  - Лист «Ответственности»: тип, наименование, количество, ед.изм., дата назначения (если есть; данные из таблицы **Responsibility**, не ResponsibilityFilling).
- **Хранение:** файл сохраняется на диск в `reports/` (путь из `IWebHostEnvironment.ContentRootPath`), имя вида `report_Фамилия_Имя_yyyy-MM-dd_HH-mm.xlsx`. В БД создаётся запись `ShiftReport` (WorkReportId, UserId, FileName, FilePath, FileSize, ShiftStart, ShiftEnd, Summary).

### 1.2. Выгрузка отчёта о смене (Download)

- **API:** `GET api/shiftreports/{id}/download?requestingUserId=...`
- **Права:** `ShiftReportService.CanUserDownloadReportAsync` — скачивать может владелец отчёта (report.UserId == requestingUserId) или роль Owner/Admin.
- **Ответ:** `File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", report.FileName)` — бинарный xlsx.
- **Сервис:** `GetReportFileAsync(reportId)` читает файл с диска по `report.FilePath`; если файла нет — `FileNotFoundException`.
- **Клиент (MAUI):**
  - Страница «Рабочие отчёты» (`WorkReportsPage`): список WorkReport по пользователю; кнопка «Скачать отчёт» только у завершённых смен (`FinishWork != null`).
  - По нажатию: `WorkReportsViewModel.DownloadReportAsync(workReportId)`:
    1. `GetShiftReportByWorkReportIdAsync(workReportId)` — получить ShiftReport по WorkReport.
    2. `DownloadShiftReportAsync(shiftReport.Id)` — GET download, байты через `ReadAsByteArrayAsync()`.
    3. Сохранение файла: Android — `Environment.GetExternalStoragePublicDirectory(DirectoryDownloads)`; иначе — `MyDocuments`. Имя из `shiftReport.FileName`, при совпадении — суффикс `_1`, `_2` и т.д.
  - Успех/ошибка через `DisplayAlert` и `ErrorMessage`.

### 1.3. Отчёты по заявкам и переработке (ReportsController)

- **Маршрут:** `api/reports`.
- **Нет проверки прав** — любой авторизованный запрос может дергать эти эндпоинты (если перед ними нет глобального RequirePermission).
- **Два отчёта (генерация в память, без сохранения на диск):**

| Эндпоинт | Описание | Параметры | Формат |
|----------|----------|-----------|--------|
| `GET api/reports/part-requests` | Отчёт по заявкам на перемещение (PartRequest) | startDate, endDate, userId? | xlsx, имя `PartRequests_{start}_{end}.xlsx` |
| `GET api/reports/reprocessing` | Отчёт по переработке (Reprocessing) | startDate, endDate, userId? | xlsx, имя `Reprocessing_{start}_{end}.xlsx` |

- **Сервис:** `ReportGeneratorService` (ClosedXML):
  - Part Request: листы «Отправленные заявки», «Принятые заявки», «Сводка по материалам».
  - Reprocessing: листы «Операции переработки», «Сводка исходных материалов», «Сводка по выходу».
- **Клиент:** вызовов `api/reports/part-requests` и `api/reports/reprocessing` в приложении **нет** — эти отчёты доступны только через API (например, из браузера или другого клиента).

---

## 2. Сводка по выгрузке

| Что | Где генерируется | Где хранится | Как выгружается |
|-----|------------------|--------------|-----------------|
| Отчёт о смене | ShiftReportService.GenerateReportAsync | Диск `reports/` + запись ShiftReport в БД | GET shiftreports/{id}/download → файл с диска; в приложении — кнопка «Скачать отчёт» |
| Part Requests | ReportGeneratorService | Только в памяти | GET api/reports/part-requests → стрим xlsx (в приложении не используется) |
| Reprocessing | ReportGeneratorService | Только в памяти | GET api/reports/reprocessing → стрим xlsx (в приложении не используется) |

---

## 3. Важные файлы

**Сервер**

- `Controllers/ShiftReportsController.cs` — список/по id/по workReport/user/date-range, **download**, generate, delete.
- `Controllers/WorkReportsController.cs` — завершение смены и вызов `GenerateReportAsync` после FinishWork.
- `Controllers/ReportsController.cs` — part-requests и reprocessing (GET, отдача File).
- `Services/ShiftReportService.cs` — генерация Excel, сохранение в `reports/`, чтение файла для download, права скачивания.
- `Services/ReportGeneratorService.cs` — Part Request и Reprocessing отчёты (только генерация в память).
- `Models/ShiftReport.cs` — FileName, FilePath, FileSize, WorkReportId, UserId, ShiftStart, ShiftEnd, Summary.

**Клиент (MAUI)**

- `Pages/WorkReportsPage.xaml` — список WorkReport, кнопки «Завершить работу» и «Скачать отчёт».
- `Pages/WorkReportsPage.xaml.cs` — обработчики, вызов `DownloadReportAsync(report.Id)` (передаётся WorkReport.Id).
- `ViewModels/WorkReportsViewModel.cs` — `DownloadReportAsync(workReportId)`: получение ShiftReport по workReportId, затем DownloadShiftReportAsync(shiftReport.Id), сохранение в папку загрузок/документы.
- `Services/ApiService.cs` — `GetShiftReportByWorkReportIdAsync`, `DownloadShiftReportAsync(reportId)` (GET + ReadAsByteArrayAsync).

---

## 4. Замечания

- Отчёт о смене использует **Responsibility** (старая модель без склада), а не ResponsibilityFilling — при необходимости вывода ответственности по складам логику листов нужно расширить.
- Отчёт о смене создаётся при завершении смены; если генерация падает, пользователь всё равно видит «работа завершена», но при скачивании получит «Отчет смены не найден», пока не будет вызван `POST .../generate/{workReportId}` или не починят автоматическую генерацию.
- Отчёты `api/reports/part-requests` и `api/reports/reprocessing` не вызываются из MAUI — выгрузка по ним идёт только через прямой вызов API.

Можно переходить к доработке выгрузки отчётов с этой схемы.
