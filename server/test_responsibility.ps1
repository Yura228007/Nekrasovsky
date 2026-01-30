# Скрипт для тестирования системы ответственности с количеством
# Использование: .\test_responsibility.ps1

$baseUrl = "http://localhost:9000"
$headers = @{
    "Content-Type" = "application/json"
    "X-User-Id" = "1"  # Замените на ID пользователя-админа
}

Write-Host "=== Тестирование системы ответственности с количеством ===" -ForegroundColor Green
Write-Host ""

# Тест 1: Назначение ответственности без количества (обратная совместимость)
Write-Host "Тест 1: Назначение ответственности без количества" -ForegroundColor Yellow
$materialId = 1  # Замените на реальный ID материала
$userId = 1      # Замените на реальный ID пользователя

$body = @{
    userId = $userId
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/responsibilities/material/$materialId/assign" `
        -Method Post `
        -Headers $headers `
        -Body $body
    
    Write-Host "✓ Успешно: Ответственность назначена" -ForegroundColor Green
    Write-Host "  ID: $($response.responsibility.id)" -ForegroundColor Gray
    Write-Host "  Quantity: $($response.responsibility.quantity)" -ForegroundColor Gray
    Write-Host "  IsActive: $($response.responsibility.isActive)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Ошибка: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Тест 2: Назначение ответственности с количеством
Write-Host "Тест 2: Назначение ответственности с количеством" -ForegroundColor Yellow
$body = @{
    userId = $userId
    quantity = 100
    measuringUnit = "кг"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/responsibilities/material/$materialId/assign" `
        -Method Post `
        -Headers $headers `
        -Body $body
    
    Write-Host "✓ Успешно: Ответственность назначена с количеством" -ForegroundColor Green
    Write-Host "  ID: $($response.responsibility.id)" -ForegroundColor Gray
    Write-Host "  Quantity: $($response.responsibility.quantity)" -ForegroundColor Gray
    Write-Host "  MeasuringUnit: $($response.responsibility.measuringUnit)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Ошибка: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Тест 3: Получение ответственностей пользователя
Write-Host "Тест 3: Получение ответственностей пользователя" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/responsibilities/user/$userId" `
        -Method Get `
        -Headers $headers
    
    Write-Host "✓ Успешно: Найдено ответственностей: $($response.Count)" -ForegroundColor Green
    foreach ($resp in $response) {
        Write-Host "  - MaterialId: $($resp.materialId), Quantity: $($resp.quantity), IsActive: $($resp.isActive)" -ForegroundColor Gray
    }
} catch {
    Write-Host "✗ Ошибка: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Тест 4: Проверка активных назначений
Write-Host "Тест 4: Получение активных назначений материалов" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/responsibilities/materials/active" `
        -Method Get `
        -Headers $headers
    
    Write-Host "✓ Успешно: Найдено активных назначений: $($response.Count)" -ForegroundColor Green
    foreach ($assignment in $response) {
        Write-Host "  - MaterialId: $($assignment.itemId), UserId: $($assignment.userId), UserName: $($assignment.userName)" -ForegroundColor Gray
    }
} catch {
    Write-Host "✗ Ошибка: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "=== Тестирование завершено ===" -ForegroundColor Green
Write-Host ""
Write-Host "Примечание: Для тестирования переработки нужно:" -ForegroundColor Cyan
Write-Host "1. Запустить сервер: cd server/server && dotnet run" -ForegroundColor Gray
Write-Host "2. Открыть Swagger: http://localhost:9000/swagger" -ForegroundColor Gray
Write-Host "3. Выполнить переработку через API и проверить уменьшение количества" -ForegroundColor Gray
