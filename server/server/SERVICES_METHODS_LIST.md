# Список методов для сервисов и контроллеров

## 1. UserService / UsersController ✅ (Уже реализовано)

### IUserService
- `GetAllUsersAsync()` - получить всех пользователей
- `SearchUsersAsync(string? name, string? surname)` - поиск по имени/фамилии
- `GetUserByIdAsync(int id)` - получить по ID
- `CreateUserAsync(User user)` - создать пользователя
- `UpdateUserAsync(int id, User updatedUser)` - обновить пользователя
- `DeleteUserAsync(int id)` - удалить пользователя

### UsersController
- `GET /api/users` - GetAll()
- `GET /api/users/search?name=&surname=` - GetByNameOrSurname()
- `GET /api/users/{id}` - GetById()
- `POST /api/users/add` - AddUser()
- `POST /api/users/edit/{id}` - EditUser()
- `POST /api/users/delete/{id}` - DeleteUser()

---

## 2. PermissionService / PermissionsController

### IPermissionService
- `GetAllPermissionsAsync()` - получить все разрешения
- `GetPermissionByIdAsync(int id)` - получить по ID
- `GetPermissionByCodeAsync(string code)` - получить по коду
- `CreatePermissionAsync(Permission permission)` - создать разрешение
- `UpdatePermissionAsync(int id, Permission updatedPermission)` - обновить разрешение
- `DeletePermissionAsync(int id)` - удалить разрешение

### PermissionsController
- `GET /api/permissions` - GetAll()
- `GET /api/permissions/{id}` - GetById()
- `GET /api/permissions/code/{code}` - GetByCode()
- `POST /api/permissions/add` - AddPermission()
- `POST /api/permissions/edit/{id}` - EditPermission()
- `POST /api/permissions/delete/{id}` - DeletePermission()

---

## 3. UserPermissionsService / UserPermissionsController

### IUserPermissionsService
- `GetUserPermissionsAsync(int userId)` - получить разрешения пользователя
- `GetUsersWithPermissionAsync(int permissionId)` - получить пользователей с разрешением
- `AddPermissionToUserAsync(int userId, int permissionId)` - добавить разрешение пользователю
- `RemovePermissionFromUserAsync(int userId, int permissionId)` - удалить разрешение у пользователя
- `HasPermissionAsync(int userId, string permissionCode)` - проверить наличие разрешения
- `UpdateUserPermissionsAsync(int userId, IEnumerable<int> permissionIds)` - обновить все разрешения пользователя

### UserPermissionsController
- `GET /api/user-permissions/user/{userId}` - GetUserPermissions()
- `GET /api/user-permissions/permission/{permissionId}` - GetUsersWithPermission()
- `POST /api/user-permissions/add` - AddPermissionToUser()
- `POST /api/user-permissions/remove` - RemovePermissionFromUser()
- `GET /api/user-permissions/check?userId=&permissionCode=` - CheckPermission()
- `POST /api/user-permissions/update/{userId}` - UpdateUserPermissions()

---

## 4. WarehouseService / WarehousesController

### IWarehouseService
- `GetAllWarehousesAsync()` - получить все склады
- `GetWarehouseByIdAsync(int id)` - получить по ID
- `GetWarehousesByTypeAsync(string type)` - получить по типу
- `SearchWarehousesAsync(string? name, string? type)` - поиск складов
- `CreateWarehouseAsync(Warehouse warehouse)` - создать склад
- `UpdateWarehouseAsync(int id, Warehouse updatedWarehouse)` - обновить склад
- `DeleteWarehouseAsync(int id)` - удалить склад
- `GetWarehouseMovementsAsync(int warehouseId)` - получить перемещения склада
- `GetWarehouseRequestsAsync(int warehouseId)` - получить запросы склада

### WarehousesController
- `GET /api/warehouses` - GetAll()
- `GET /api/warehouses/{id}` - GetById()
- `GET /api/warehouses/type/{type}` - GetByType()
- `GET /api/warehouses/search?name=&type=` - Search()
- `POST /api/warehouses/add` - AddWarehouse()
- `POST /api/warehouses/edit/{id}` - EditWarehouse()
- `POST /api/warehouses/delete/{id}` - DeleteWarehouse()
- `GET /api/warehouses/{id}/movements` - GetMovements()
- `GET /api/warehouses/{id}/requests` - GetRequests()

---

## 5. MaterialService / MaterialsController

### IMaterialService
- `GetAllMaterialsAsync()` - получить все материалы
- `GetMaterialByIdAsync(int id)` - получить по ID
- `SearchMaterialsAsync(string? name, string? code)` - поиск материалов
- `GetMaterialsByMeasuringUnitAsync(string unit)` - получить по единице измерения
- `CreateMaterialAsync(Material material)` - создать материал
- `UpdateMaterialAsync(int id, Material updatedMaterial)` - обновить материал
- `DeleteMaterialAsync(int id)` - удалить материал
- `GetMaterialMovementsAsync(int materialId)` - получить перемещения материала
- `GetMaterialUsageAsync(int materialId)` - получить использование материала (в рецептах, складах)

### MaterialsController
- `GET /api/materials` - GetAll()
- `GET /api/materials/{id}` - GetById()
- `GET /api/materials/search?name=&code=` - Search()
- `GET /api/materials/unit/{unit}` - GetByMeasuringUnit()
- `POST /api/materials/add` - AddMaterial()
- `POST /api/materials/edit/{id}` - EditMaterial()
- `POST /api/materials/delete/{id}` - DeleteMaterial()
- `GET /api/materials/{id}/movements` - GetMovements()
- `GET /api/materials/{id}/usage` - GetUsage()

---

## 6. ProductService / ProductsController

### IProductService
- `GetAllProductsAsync()` - получить все продукты
- `GetProductByIdAsync(int id)` - получить по ID
- `SearchProductsAsync(string? name, string? code)` - поиск продуктов
- `CreateProductAsync(Product product)` - создать продукт
- `UpdateProductAsync(int id, Product updatedProduct)` - обновить продукт
- `DeleteProductAsync(int id)` - удалить продукт
- `GetProductRecipeAsync(int productId)` - получить рецепт продукта
- `GetProductsUsingMaterialAsync(int materialId)` - получить продукты, использующие материал

### ProductsController
- `GET /api/products` - GetAll()
- `GET /api/products/{id}` - GetById()
- `GET /api/products/search?name=&code=` - Search()
- `POST /api/products/add` - AddProduct()
- `POST /api/products/edit/{id}` - EditProduct()
- `POST /api/products/delete/{id}` - DeleteProduct()
- `GET /api/products/{id}/recipe` - GetRecipe()
- `GET /api/products/material/{materialId}` - GetProductsUsingMaterial()

---

## 7. RecipeService / RecipesController

### IRecipeService
- `GetAllRecipesAsync()` - получить все рецепты
- `GetRecipeByProductAsync(int productId)` - получить рецепт продукта
- `GetRecipesByMaterialAsync(int materialId)` - получить рецепты, использующие материал
- `GetRecipeAsync(int productId, int materialId)` - получить конкретный рецепт
- `CreateRecipeAsync(Recipe recipe)` - создать рецепт
- `UpdateRecipeAsync(int productId, int materialId, Recipe updatedRecipe)` - обновить рецепт
- `DeleteRecipeAsync(int productId, int materialId)` - удалить рецепт
- `AddMaterialToRecipeAsync(int productId, int materialId, int quantity, string? measuringType)` - добавить материал в рецепт
- `RemoveMaterialFromRecipeAsync(int productId, int materialId)` - удалить материал из рецепта

### RecipesController
- `GET /api/recipes` - GetAll()
- `GET /api/recipes/product/{productId}` - GetByProduct()
- `GET /api/recipes/material/{materialId}` - GetByMaterial()
- `GET /api/recipes/{productId}/{materialId}` - GetRecipe()
- `POST /api/recipes/add` - AddRecipe()
- `POST /api/recipes/edit/{productId}/{materialId}` - EditRecipe()
- `POST /api/recipes/delete/{productId}/{materialId}` - DeleteRecipe()
- `POST /api/recipes/add-material` - AddMaterialToRecipe()
- `POST /api/recipes/remove-material` - RemoveMaterialFromRecipe()

---

## 8. AccessibleMovementService / AccessibleMovementsController

### IAccessibleMovementService
- `GetAllMovementsAsync()` - получить все разрешенные перемещения
- `GetMovementAsync(int fromWarehouseId, int toWarehouseId, int materialId)` - получить конкретное перемещение
- `GetMovementsFromWarehouseAsync(int warehouseId)` - получить перемещения из склада
- `GetMovementsToWarehouseAsync(int warehouseId)` - получить перемещения в склад
- `GetMovementsByMaterialAsync(int materialId)` - получить перемещения материала
- `CreateMovementAsync(AccessibleMovement movement)` - создать разрешенное перемещение
- `UpdateMovementAsync(int fromWarehouseId, int toWarehouseId, int materialId, AccessibleMovement updatedMovement)` - обновить перемещение
- `DeleteMovementAsync(int fromWarehouseId, int toWarehouseId, int materialId)` - удалить перемещение
- `IsMovementAllowedAsync(int fromWarehouseId, int toWarehouseId, int materialId)` - проверить разрешено ли перемещение

### AccessibleMovementsController
- `GET /api/accessible-movements` - GetAll()
- `GET /api/accessible-movements/{fromWarehouseId}/{toWarehouseId}/{materialId}` - GetMovement()
- `GET /api/accessible-movements/from/{warehouseId}` - GetFromWarehouse()
- `GET /api/accessible-movements/to/{warehouseId}` - GetToWarehouse()
- `GET /api/accessible-movements/material/{materialId}` - GetByMaterial()
- `POST /api/accessible-movements/add` - AddMovement()
- `POST /api/accessible-movements/edit/{fromWarehouseId}/{toWarehouseId}/{materialId}` - EditMovement()
- `POST /api/accessible-movements/delete/{fromWarehouseId}/{toWarehouseId}/{materialId}` - DeleteMovement()
- `GET /api/accessible-movements/check?fromWarehouseId=&toWarehouseId=&materialId=` - CheckAllowed()

---

## 9. PartRequestService / PartRequestsController

### IPartRequestService
- `GetAllPartRequestsAsync()` - получить все запросы
- `GetPartRequestByIdAsync(int id)` - получить по ID
- `GetPartRequestsByStatusAsync(PartRequestStatus status)` - получить по статусу
- `GetPartRequestsByUserAsync(int userId, bool sent = true)` - получить запросы пользователя (отправленные/полученные)
- `GetPartRequestsByWarehouseAsync(int warehouseId, bool from = true)` - получить запросы склада
- `GetPartRequestsByMaterialAsync(int materialId)` - получить запросы по материалу
- `CreatePartRequestAsync(PartRequest request)` - создать запрос
- `UpdatePartRequestAsync(int id, PartRequest updatedRequest)` - обновить запрос
- `ApprovePartRequestAsync(int id)` - одобрить запрос
- `RejectPartRequestAsync(int id, string? reason)` - отклонить запрос
- `DeletePartRequestAsync(int id)` - удалить запрос

### PartRequestsController
- `GET /api/part-requests` - GetAll()
- `GET /api/part-requests/{id}` - GetById()
- `GET /api/part-requests/status/{status}` - GetByStatus()
- `GET /api/part-requests/user/{userId}?sent=true` - GetByUser()
- `GET /api/part-requests/warehouse/{warehouseId}?from=true` - GetByWarehouse()
- `GET /api/part-requests/material/{materialId}` - GetByMaterial()
- `POST /api/part-requests/add` - AddPartRequest()
- `POST /api/part-requests/edit/{id}` - EditPartRequest()
- `POST /api/part-requests/{id}/approve` - Approve()
- `POST /api/part-requests/{id}/reject` - Reject()
- `POST /api/part-requests/delete/{id}` - DeletePartRequest()

---

## 10. AlarmEventService / AlarmEventsController

### IAlarmEventService
- `GetAllAlarmEventsAsync()` - получить все события
- `GetAlarmEventByIdAsync(int id)` - получить по ID
- `GetAlarmEventsByUserAsync(int userId)` - получить события пользователя
- `GetAlarmEventsByDateRangeAsync(DateTime startDate, DateTime endDate)` - получить события за период
- `GetAlarmEventsByLocationAsync(string location)` - получить события по локации
- `CreateAlarmEventAsync(AlarmEvent alarmEvent)` - создать событие
- `UpdateAlarmEventAsync(int id, AlarmEvent updatedAlarmEvent)` - обновить событие
- `DeleteAlarmEventAsync(int id)` - удалить событие

### AlarmEventsController
- `GET /api/alarm-events` - GetAll()
- `GET /api/alarm-events/{id}` - GetById()
- `GET /api/alarm-events/user/{userId}` - GetByUser()
- `GET /api/alarm-events/date-range?startDate=&endDate=` - GetByDateRange()
- `GET /api/alarm-events/location/{location}` - GetByLocation()
- `POST /api/alarm-events/add` - AddAlarmEvent()
- `POST /api/alarm-events/edit/{id}` - EditAlarmEvent()
- `POST /api/alarm-events/delete/{id}` - DeleteAlarmEvent()

---

## 11. ShiftTransferService / ShiftTransfersController

### IShiftTransferService
- `GetAllShiftTransfersAsync()` - получить все передачи смен
- `GetShiftTransferByIdAsync(int id)` - получить по ID
- `GetShiftTransfersByUserAsync(int userId, bool sent = true)` - получить передачи пользователя
- `GetShiftTransfersByDateAsync(DateTime date)` - получить передачи за дату
- `GetPendingShiftTransfersAsync(int userId)` - получить ожидающие подтверждения передачи
- `CreateShiftTransferAsync(ShiftTransfer transfer)` - создать передачу смены
- `UpdateShiftTransferAsync(int id, ShiftTransfer updatedTransfer)` - обновить передачу
- `ConfirmShiftTransferAsync(int id)` - подтвердить передачу смены
- `DeleteShiftTransferAsync(int id)` - удалить передачу

### ShiftTransfersController
- `GET /api/shift-transfers` - GetAll()
- `GET /api/shift-transfers/{id}` - GetById()
- `GET /api/shift-transfers/user/{userId}?sent=true` - GetByUser()
- `GET /api/shift-transfers/date/{date}` - GetByDate()
- `GET /api/shift-transfers/pending/{userId}` - GetPending()
- `POST /api/shift-transfers/add` - AddShiftTransfer()
- `POST /api/shift-transfers/edit/{id}` - EditShiftTransfer()
- `POST /api/shift-transfers/{id}/confirm` - Confirm()
- `POST /api/shift-transfers/delete/{id}` - DeleteShiftTransfer()

---

## 12. FillingWarehouseService / FillingWarehousesController

### IFillingWarehouseService
- `GetAllFillingWarehousesAsync()` - получить все заполнения складов
- `GetFillingWarehouseAsync(int warehouseId, int materialId)` - получить конкретное заполнение
- `GetFillingByWarehouseAsync(int warehouseId)` - получить заполнения склада
- `GetFillingByMaterialAsync(int materialId)` - получить заполнения по материалу
- `CreateFillingWarehouseAsync(FillingWarehouse filling)` - создать заполнение
- `UpdateFillingWarehouseAsync(int warehouseId, int materialId, FillingWarehouse updatedFilling)` - обновить заполнение
- `DeleteFillingWarehouseAsync(int warehouseId, int materialId)` - удалить заполнение
- `UpdateQuantityAsync(int warehouseId, int materialId, int quantity)` - обновить количество
- `GetWarehouseStockAsync(int warehouseId)` - получить остатки на складе

### FillingWarehousesController
- `GET /api/filling-warehouses` - GetAll()
- `GET /api/filling-warehouses/{warehouseId}/{materialId}` - GetFilling()
- `GET /api/filling-warehouses/warehouse/{warehouseId}` - GetByWarehouse()
- `GET /api/filling-warehouses/material/{materialId}` - GetByMaterial()
- `POST /api/filling-warehouses/add` - AddFilling()
- `POST /api/filling-warehouses/edit/{warehouseId}/{materialId}` - EditFilling()
- `POST /api/filling-warehouses/delete/{warehouseId}/{materialId}` - DeleteFilling()
- `POST /api/filling-warehouses/update-quantity` - UpdateQuantity()
- `GET /api/filling-warehouses/warehouse/{warehouseId}/stock` - GetStock()

---

## 13. WorkReportService / WorkReportsController

### IWorkReportService
- `GetAllWorkReportsAsync()` - получить все отчеты
- `GetWorkReportByIdAsync(int id)` - получить по ID
- `GetWorkReportsByUserAsync(int userId)` - получить отчеты пользователя
- `GetWorkReportsByDateAsync(DateTime date)` - получить отчеты за дату
- `GetWorkReportsByDateRangeAsync(DateTime startDate, DateTime endDate)` - получить отчеты за период
- `GetActiveWorkReportsAsync(int userId)` - получить активные отчеты (без FinishWork)
- `CreateWorkReportAsync(WorkReport report)` - создать отчет
- `UpdateWorkReportAsync(int id, WorkReport updatedReport)` - обновить отчет
- `StartWorkAsync(int userId, DateTime? startTime = null)` - начать работу
- `FinishWorkAsync(int reportId, DateTime? finishTime = null)` - закончить работу
- `DeleteWorkReportAsync(int id)` - удалить отчет

### WorkReportsController
- `GET /api/work-reports` - GetAll()
- `GET /api/work-reports/{id}` - GetById()
- `GET /api/work-reports/user/{userId}` - GetByUser()
- `GET /api/work-reports/date/{date}` - GetByDate()
- `GET /api/work-reports/date-range?startDate=&endDate=` - GetByDateRange()
- `GET /api/work-reports/user/{userId}/active` - GetActive()
- `POST /api/work-reports/add` - AddWorkReport()
- `POST /api/work-reports/edit/{id}` - EditWorkReport()
- `POST /api/work-reports/start` - StartWork()
- `POST /api/work-reports/{id}/finish` - FinishWork()
- `POST /api/work-reports/delete/{id}` - DeleteWorkReport()

---

## 14. RequestLogService / RequestLogsController (Аудит)

### IRequestLogService
- `GetAllRequestLogsAsync()` - получить все логи
- `GetRequestLogByIdAsync(int id)` - получить по ID
- `GetRequestLogsByUserAsync(int userId)` - получить логи пользователя
- `GetRequestLogsByDateRangeAsync(DateTime startDate, DateTime endDate)` - получить логи за период
- `GetRequestLogsByStatusCodeAsync(int statusCode)` - получить логи по статусу
- `GetRequestLogsByControllerAsync(string controller)` - получить логи контроллера
- `GetRequestLogsByActionAsync(string controller, string action)` - получить логи действия
- `SearchRequestLogsAsync(string? url, string? httpMethod, int? statusCode, DateTime? startDate, DateTime? endDate)` - поиск логов
- `DeleteOldRequestLogsAsync(DateTime beforeDate)` - удалить старые логи

### RequestLogsController
- `GET /api/request-logs` - GetAll()
- `GET /api/request-logs/{id}` - GetById()
- `GET /api/request-logs/user/{userId}` - GetByUser()
- `GET /api/request-logs/date-range?startDate=&endDate=` - GetByDateRange()
- `GET /api/request-logs/status/{statusCode}` - GetByStatusCode()
- `GET /api/request-logs/controller/{controller}` - GetByController()
- `GET /api/request-logs/action/{controller}/{action}` - GetByAction()
- `GET /api/request-logs/search?url=&httpMethod=&statusCode=&startDate=&endDate=` - Search()
- `POST /api/request-logs/cleanup` - DeleteOldLogs()

---

## Общие паттерны для всех сервисов:

1. **CRUD операции**: GetAll, GetById, Create, Update, Delete
2. **Поиск/Фильтрация**: Search, GetBy*, GetByDateRange
3. **Специфичные бизнес-методы**: в зависимости от сущности
4. **Валидация**: проверка уникальности, существования связанных сущностей
5. **Логирование**: все операции логируются через ILogger

## Примечания:

- Все методы асинхронные (async/await)
- Все сервисы используют AppDbContext через DI
- Все сервисы имеют интерфейсы для тестирования
- Контроллеры возвращают стандартные HTTP статусы
- Обработка ошибок через try-catch с соответствующими HTTP кодами

