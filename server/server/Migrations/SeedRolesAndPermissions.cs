using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using server.Data;
using server.Models;

namespace server.Migrations
{
    /// <summary>
    /// Скрипт для заполнения ролей и прав из ТЗ
    /// Этот скрипт нужно будет вызвать вручную или через миграцию после получения информации о правах для ролей
    /// </summary>
    public static class SeedRolesAndPermissions
    {
        public static void SeedData(AppDbContext context)
        {
            // Список ролей из ТЗ (раздел 5.3)
            var roles = new List<Role>
            {
                new Role { Name = "Владелец", Code = "Owner", Rank = 0, Description = "Владелец предприятия" },
                new Role { Name = "Администратор", Code = "Admin", Rank = 1, Description = "Администратор системы" },
                new Role { Name = "Старший экструзионщик", Code = "SeniorExtruder", Rank = 2, Description = "Старший экструзионщик" },
                new Role { Name = "Экструзионщик", Code = "Extruder", Rank = 3, Description = "Экструзионщик" },
                new Role { Name = "Старший кладовщик", Code = "SeniorStorekeeper", Rank = 2, Description = "Старший кладовщик" },
                new Role { Name = "Кладовщик", Code = "Storekeeper", Rank = 3, Description = "Кладовщик" },
                new Role { Name = "Покрасочник", Code = "Painter", Rank = 3, Description = "Покрасочник" },
                new Role { Name = "Старшая упаковщица", Code = "SeniorPacker", Rank = 2, Description = "Старшая упаковщица" },
                new Role { Name = "Кладовщик готовой продукции", Code = "FinishedProductStorekeeper", Rank = 3, Description = "Кладовщик готовой продукции" },
                new Role { Name = "ТПА", Code = "TPA", Rank = 3, Description = "ТПА" },
                new Role { Name = "Утиль", Code = "Waste", Rank = 4, Description = "Утиль" }
            };

            // Список прав из ТЗ (раздел 5.4)
            var permissions = new List<Permission>
            {
                new Permission { Code = "AddUsersAllRanks", Name = "Добавление пользователей всех рангов", Description = "Позволяет добавлять пользователей всех рангов" },
                new Permission { Code = "AddUsersLowerRanks", Name = "Добавление пользователей ниже рангом", Description = "Позволяет добавлять пользователей ниже рангом" },
                new Permission { Code = "Reception", Name = "Приемка", Description = "Приемка сырья, станков и т.д. на производство" },
                new Permission { Code = "AssignBarcode", Name = "Назначение штрихкода", Description = "Назначение штрихкода позиции, таре и т.д." },
                new Permission { Code = "SendToWaste", Name = "Отправка на Утиль", Description = "Отправка товара на утиль" },
                new Permission { Code = "SendToSDH", Name = "Отправка на СДХ", Description = "Отправка товара на СДХ" },
                new Permission { Code = "SendToSale", Name = "Отправка на реализацию", Description = "Отправка товара на реализацию" },
                new Permission { Code = "WriteOff", Name = "Списание", Description = "Списание (выбросить)" },
                new Permission { Code = "MoveFromMainToWorkshop", Name = "Перенос ТМЦ из Основного склада в цеха", Description = "Перенос ТМЦ из Основного склада в цеха" },
                new Permission { Code = "MoveFromWorkshopToMain", Name = "Перенос ТМЦ из Цеха на Основной склад", Description = "Перенос ТМЦ из Цеха на Основной склад" },
                new Permission { Code = "ShiftTransfer", Name = "Передача смены", Description = "Передача смены" },
                new Permission { Code = "ManageRecipes", Name = "Создание и добавление рецептур, единиц хранения", Description = "Создание и добавление рецептур, единиц хранения" },
                new Permission { Code = "Inventory", Name = "Инвентаризация", Description = "Инвентаризация" }
            };

            // Добавляем роли, если их еще нет
            foreach (var role in roles)
            {
                if (!context.Roles.Any(r => r.Code == role.Code))
                {
                    context.Roles.Add(role);
                }
            }

            // Добавляем права, если их еще нет
            foreach (var permission in permissions)
            {
                if (!context.Permissions.Any(p => p.Code == permission.Code))
                {
                    context.Permissions.Add(permission);
                }
            }

            context.SaveChanges();

            // Базовое распределение прав
            // Владелец и Администратор имеют все права
            // Остальные роли получат права позже после получения данных из файла "Список прав"
            
            var ownerRole = context.Roles.FirstOrDefault(r => r.Code == "Owner");
            var adminRole = context.Roles.FirstOrDefault(r => r.Code == "Admin");
            var allPermissions = context.Permissions.ToList();

            if (ownerRole != null && allPermissions.Any())
            {
                foreach (var permission in allPermissions)
                {
                    if (!context.RolePermissions.Any(rp => rp.RoleId == ownerRole.Id && rp.PermissionId == permission.Id))
                    {
                        context.RolePermissions.Add(new RolePermission 
                        { 
                            RoleId = ownerRole.Id, 
                            PermissionId = permission.Id 
                        });
                    }
                }
            }

            if (adminRole != null && allPermissions.Any())
            {
                foreach (var permission in allPermissions)
                {
                    if (!context.RolePermissions.Any(rp => rp.RoleId == adminRole.Id && rp.PermissionId == permission.Id))
                    {
                        context.RolePermissions.Add(new RolePermission 
                        { 
                            RoleId = adminRole.Id, 
                            PermissionId = permission.Id 
                        });
                    }
                }
            }

            // TODO: Добавить права для остальных ролей после получения данных из файла "Список прав"
            // Пример:
            // var seniorExtruderRole = context.Roles.FirstOrDefault(r => r.Code == "SeniorExtruder");
            // var receptionPermission = context.Permissions.FirstOrDefault(p => p.Code == "Reception");
            // if (seniorExtruderRole != null && receptionPermission != null)
            // {
            //     context.RolePermissions.Add(new RolePermission { RoleId = seniorExtruderRole.Id, PermissionId = receptionPermission.Id });
            // }

            context.SaveChanges();
        }
    }
}

