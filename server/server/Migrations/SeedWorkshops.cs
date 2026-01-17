using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Migrations
{
    /// <summary>
    /// Скрипт для заполнения цехов из ТЗ
    /// Нужно будет дополнить список цехов после получения информации из файла "Список цехов"
    /// </summary>
    public static class SeedWorkshops
    {
        public static void SeedData(AppDbContext context)
        {
            // Базовый список цехов и складов (можно будет дополнить после получения данных из файла "Список цехов")
            var warehouses = new List<Warehouse>
            {
                new Warehouse { Name = "Основной склад", Type = "Склад", IsActive = true },
                new Warehouse { Name = "Экструзионный цех", Type = "Цех", IsActive = true },
                new Warehouse { Name = "Покрасочный цех", Type = "Цех", IsActive = true },
                new Warehouse { Name = "Упаковочный цех", Type = "Цех", IsActive = true },
                new Warehouse { Name = "Цех готовой продукции", Type = "Цех", IsActive = true },
                new Warehouse { Name = "ТПА цех", Type = "Цех", IsActive = true },
                new Warehouse { Name = "Утиль", Type = "Склад", IsActive = true }
            };

            foreach (var warehouse in warehouses)
            {
                if (!context.Warehouses.Any(w => w.Name == warehouse.Name))
                {
                    context.Warehouses.Add(warehouse);
                }
            }

            context.SaveChanges();

            // TODO: После получения данных из файла "Список цехов и их взаимодействие" нужно:
            // 1. Добавить все цеха из файла
            // 2. Настроить AccessibleMovement для разрешенных перемещений между цехами
        }
    }
}

