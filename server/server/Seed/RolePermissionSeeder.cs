using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using server.Data;
using server.Models;

namespace server.Seed
{
    public static class RolePermissionSeeder
    {
        private class PermissionSeed
        {
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public HashSet<string> RoleNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private class RoleSeed
        {
            public string Name { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int Rank { get; set; }
        }

        public static void SeedRolesAndPermissions(AppDbContext dbContext, ILogger logger, string contentRootPath)
        {
            var excelPath = Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "..", "..", "Technical_Files", "This_file.xlsx"));
            List<RoleSeed> roles;
            List<PermissionSeed> permissions;

            if (File.Exists(excelPath) && TryReadRolesAndPermissionsFromExcel(excelPath, out roles, out permissions))
            {
                logger.LogInformation("Roles/permissions loaded from Excel: {Path}", excelPath);
                ApplyDefaultRanksAndDescriptions(roles);
            }
            else
            {
                logger.LogWarning("Failed to read Excel roles. Falling back to defaults.");
                BuildDefaultRolesAndPermissions(out roles, out permissions);
            }

            SeedRoles(dbContext, roles);
            SeedPermissions(dbContext, permissions);
            SeedRolePermissions(dbContext, roles, permissions);
        }

        private static void SeedRoles(AppDbContext dbContext, List<RoleSeed> roles)
        {
            foreach (var role in roles)
            {
                if (!dbContext.Roles.Any(r => r.Code == role.Code))
                {
                    dbContext.Roles.Add(new Role
                    {
                        Name = role.Name,
                        Code = role.Code,
                        Description = role.Description,
                        Rank = role.Rank
                    });
                }
            }

            dbContext.SaveChanges();
        }

        private static void SeedPermissions(AppDbContext dbContext, List<PermissionSeed> permissions)
        {
            foreach (var permission in permissions)
            {
                if (!dbContext.Permissions.Any(p => p.Code == permission.Code))
                {
                    dbContext.Permissions.Add(new Permission
                    {
                        Code = permission.Code,
                        Name = permission.Name,
                        Description = permission.Description
                    });
                }
            }

            dbContext.SaveChanges();
        }

        private static void SeedRolePermissions(AppDbContext dbContext, List<RoleSeed> roles, List<PermissionSeed> permissions)
        {
            var roleByName = dbContext.Roles.ToDictionary(r => r.Name, StringComparer.OrdinalIgnoreCase);
            var permissionByCode = dbContext.Permissions.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);

            foreach (var permission in permissions)
            {
                if (!permissionByCode.TryGetValue(permission.Code, out var permEntity))
                {
                    continue;
                }

                foreach (var roleName in permission.RoleNames)
                {
                    if (!roleByName.TryGetValue(roleName, out var roleEntity))
                    {
                        continue;
                    }

                    if (!dbContext.RolePermissions.Any(rp => rp.RoleId == roleEntity.Id && rp.PermissionId == permEntity.Id))
                    {
                        dbContext.RolePermissions.Add(new RolePermission
                        {
                            RoleId = roleEntity.Id,
                            PermissionId = permEntity.Id
                        });
                    }
                }
            }

            dbContext.SaveChanges();
        }

        private static void ApplyDefaultRanksAndDescriptions(List<RoleSeed> roles)
        {
            var rankMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Владелец"] = 1,
                ["Администратор"] = 2,
                ["Старший экструзионщик"] = 3,
                ["Старший кладовщик"] = 3,
                ["Кладовщик готовой продукции"] = 3,
                ["Экструзионщик"] = 4,
                ["Кладовщик"] = 4,
                ["Покрасочник"] = 4,
                ["Старшая упаковщица"] = 4,
                ["ТПА"] = 4,
                ["Утиль"] = 4
            };

            var descMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Владелец"] = "Полный доступ ко всем функциям",
                ["Администратор"] = "Руководитель предприятия с ограничениями",
                ["Старший экструзионщик"] = "Замещение администратора с ограничениями",
                ["Старший кладовщик"] = "Ответственный за все склады и цеха",
                ["Кладовщик готовой продукции"] = "Ответственный за склад готовой продукции",
                ["Экструзионщик"] = "Производство",
                ["Кладовщик"] = "Склад",
                ["Покрасочник"] = "Цех покраски",
                ["Старшая упаковщица"] = "Упаковка и склад готовой продукции",
                ["ТПА"] = "ТПА",
                ["Утиль"] = "Цех утилизации"
            };

            foreach (var role in roles)
            {
                if (rankMap.TryGetValue(role.Name, out var rank))
                {
                    role.Rank = rank;
                }
                if (descMap.TryGetValue(role.Name, out var desc))
                {
                    role.Description = desc;
                }

                if (string.IsNullOrWhiteSpace(role.Code))
                {
                    role.Code = GenerateRoleCode(role.Name);
                }
            }
        }

        private static void BuildDefaultRolesAndPermissions(out List<RoleSeed> roles, out List<PermissionSeed> permissions)
        {
            roles = new List<RoleSeed>
            {
                new() { Name = "Владелец", Code = "Owner", Description = "Полный доступ", Rank = 1 },
                new() { Name = "Администратор", Code = "Admin", Description = "Руководитель предприятия", Rank = 2 },
                new() { Name = "Старший экструзионщик", Code = "SeniorExtruder", Description = "Замещение администратора", Rank = 3 },
                new() { Name = "Старший кладовщик", Code = "SeniorWarehouseman", Description = "Склады и цеха", Rank = 3 },
                new() { Name = "Кладовщик готовой продукции", Code = "FinishedGoodsWarehouseman", Description = "Склад готовой продукции", Rank = 3 },
                new() { Name = "Экструзионщик", Code = "Extruder", Description = "Производство", Rank = 4 },
                new() { Name = "Кладовщик", Code = "Warehouseman", Description = "Склад", Rank = 4 },
                new() { Name = "Покрасочник", Code = "Painter", Description = "Цех покраски", Rank = 4 },
                new() { Name = "Старшая упаковщица", Code = "SeniorPacker", Description = "Упаковка", Rank = 4 },
                new() { Name = "ТПА", Code = "TPA", Description = "ТПА", Rank = 4 },
                new() { Name = "Утиль", Code = "Utilization", Description = "Цех утилизации", Rank = 4 }
            };

            permissions = BuildDefaultPermissions();
        }

        private static List<PermissionSeed> BuildDefaultPermissions()
        {
            var permissions = new List<PermissionSeed>
            {
                new() { Code = "AddUsersAll", Name = "Добавление пользователей всех рангов", Description = "Создание пользователей любых рангов" },
                new() { Code = "AddUsersLower", Name = "Добавление пользователей ниже рангом", Description = "Создание пользователей ниже рангом" },
                new() { Code = "ReceiveGoods", Name = "Приемка", Description = "Приемка сырья/станков/товаров" },
                new() { Code = "AssignBarcode", Name = "Назначение штрихкода", Description = "Назначение штрихкодов позициям" },
                new() { Code = "SendToScrap", Name = "Отправка на Утиль", Description = "Отправка на утилизацию" },
                new() { Code = "SendToSDH", Name = "Отправка на СДХ", Description = "Отправка на СДХ" },
                new() { Code = "SendToSale", Name = "Отправка на реализацию", Description = "Отправка на реализацию" },
                new() { Code = "WriteOff", Name = "Списание", Description = "Списание/выброс" },
                new() { Code = "TransferMainToWorkshops", Name = "Перенос ТМЦ из Основного склада в цеха", Description = "Перенос со склада в цеха" },
                new() { Code = "TransferWorkshopsToMain", Name = "Перенос ТМЦ из Цеха на Основной склад", Description = "Перенос из цеха на склад" },
                new() { Code = "ShiftTransfer", Name = "Передача смены", Description = "Передача смены" },
                new() { Code = "ManageRecipes", Name = "Создание и добавление рецептур, едениц храннения.", Description = "Управление рецептурами" },
                new() { Code = "Inventory", Name = "Инвентаризация", Description = "Инвентаризация" },
                new() { Code = "ManageUsers", Name = "Управление пользователями", Description = "Добавление/редактирование/удаление пользователей" },
                new() { Code = "ManageResponsibility", Name = "Управление ответственностью", Description = "Назначение и изменение ответственных" }
            };

            // По умолчанию: владелец имеет все права
            var allRoles = new[] { "Владелец" };
            foreach (var perm in permissions)
            {
                perm.RoleNames.UnionWith(allRoles);
            }

            // Администратор: все кроме списания и удаления/рецептур
            var adminRoles = new[] { "Администратор" };
            foreach (var perm in permissions)
            {
                if (perm.Code is "WriteOff" or "SendToScrap" or "ManageRecipes")
                {
                    continue;
                }
                perm.RoleNames.UnionWith(adminRoles);
            }

            // Старший экструзионщик: как админ, но без пользователей и новых позиций
            foreach (var perm in permissions)
            {
                if (perm.Code is "AddUsersAll" or "AddUsersLower" or "ManageUsers" or "AssignBarcode")
                {
                    continue;
                }
                if (perm.Code is "WriteOff" or "SendToScrap" or "ManageRecipes")
                {
                    continue;
                }
                perm.RoleNames.Add("Старший экструзионщик");
            }

            // Старший кладовщик: склады/цеха
            foreach (var perm in permissions)
            {
                if (perm.Code is "ReceiveGoods" or "AssignBarcode" or "TransferMainToWorkshops" or "TransferWorkshopsToMain" or "Inventory")
                {
                    perm.RoleNames.Add("Старший кладовщик");
                }
            }

            // Утиль: утилизация
            foreach (var perm in permissions)
            {
                if (perm.Code is "SendToScrap" or "WriteOff")
                {
                    perm.RoleNames.Add("Утиль");
                }
            }

            return permissions;
        }

        private static bool TryReadRolesAndPermissionsFromExcel(string excelPath, out List<RoleSeed> roles, out List<PermissionSeed> permissions)
        {
            roles = new List<RoleSeed>();
            permissions = new List<PermissionSeed>();

            try
            {
                using var archive = ZipFile.OpenRead(excelPath);
                var sharedStrings = ReadSharedStrings(archive);
                var sheetPath = GetFirstSheetPath(archive);
                if (sheetPath == null)
                {
                    return false;
                }

                var rows = ReadSheetRows(archive, sheetPath, sharedStrings);
                if (rows.Count == 0)
                {
                    return false;
                }

                var header = rows[0];
                var roleNames = header.Skip(1)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => v.Trim())
                    .ToList();

                foreach (var roleName in roleNames)
                {
                    roles.Add(new RoleSeed { Name = roleName });
                }

                var usedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (var i = 1; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (row.Count == 0 || string.IsNullOrWhiteSpace(row[0]))
                    {
                        continue;
                    }

                    var permName = row[0].Trim();
                    if (permName.Equals("Права", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var permCode = GeneratePermissionCode(permName, usedCodes);
                    usedCodes.Add(permCode);

                    var permission = new PermissionSeed
                    {
                        Code = permCode,
                        Name = permName,
                        Description = permName
                    };

                    for (var col = 1; col < row.Count && col <= roleNames.Count; col++)
                    {
                        var flag = row[col]?.Trim();
                        if (!string.IsNullOrEmpty(flag) && flag.Equals("Да", StringComparison.OrdinalIgnoreCase))
                        {
                            permission.RoleNames.Add(roleNames[col - 1]);
                        }
                    }

                    permissions.Add(permission);
                }

                // Add ManageUsers permission for existing API checks
                if (permissions.All(p => !p.Code.Equals("ManageUsers", StringComparison.OrdinalIgnoreCase)))
                {
                    var manageUsers = new PermissionSeed
                    {
                        Code = "ManageUsers",
                        Name = "Управление пользователями",
                        Description = "Добавление/редактирование/удаление пользователей"
                    };
                    manageUsers.RoleNames.Add("Владелец");
                    manageUsers.RoleNames.Add("Администратор");
                    permissions.Add(manageUsers);
                }

                if (permissions.All(p => !p.Code.Equals("ManageResponsibility", StringComparison.OrdinalIgnoreCase)))
                {
                    var manageResponsibility = new PermissionSeed
                    {
                        Code = "ManageResponsibility",
                        Name = "Управление ответственностью",
                        Description = "Назначение и изменение ответственных"
                    };
                    manageResponsibility.RoleNames.Add("Владелец");
                    manageResponsibility.RoleNames.Add("Администратор");
                    permissions.Add(manageResponsibility);
                }

                ApplyDefaultRanksAndDescriptions(roles);
                return roles.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null)
            {
                return new List<string>();
            }

            using var stream = entry.Open();
            var doc = new XmlDocument();
            doc.Load(stream);
            var nsm = new XmlNamespaceManager(doc.NameTable);
            nsm.AddNamespace("s", doc.DocumentElement?.NamespaceURI ?? string.Empty);

            var nodes = doc.SelectNodes("//s:si", nsm);
            var list = new List<string>();
            if (nodes == null)
            {
                return list;
            }

            foreach (XmlNode node in nodes)
            {
                var t = node.SelectSingleNode(".//s:t", nsm);
                list.Add(t?.InnerText ?? string.Empty);
            }

            return list;
        }

        private static string? GetFirstSheetPath(ZipArchive archive)
        {
            var wbEntry = archive.GetEntry("xl/workbook.xml");
            var relEntry = archive.GetEntry("xl/_rels/workbook.xml.rels");
            if (wbEntry == null || relEntry == null)
            {
                return null;
            }

            var wbDoc = new XmlDocument();
            using (var stream = wbEntry.Open())
            {
                wbDoc.Load(stream);
            }

            var relDoc = new XmlDocument();
            using (var stream = relEntry.Open())
            {
                relDoc.Load(stream);
            }

            var wbNsm = new XmlNamespaceManager(wbDoc.NameTable);
            wbNsm.AddNamespace("w", wbDoc.DocumentElement?.NamespaceURI ?? string.Empty);
            var relNsm = new XmlNamespaceManager(relDoc.NameTable);
            relNsm.AddNamespace("r", relDoc.DocumentElement?.NamespaceURI ?? string.Empty);

            var sheetNode = wbDoc.SelectSingleNode("//w:sheets/w:sheet", wbNsm) as XmlElement;
            if (sheetNode == null)
            {
                return null;
            }

            var rid = sheetNode.GetAttribute("r:id");
            if (string.IsNullOrWhiteSpace(rid))
            {
                return null;
            }

            var relNode = relDoc.SelectSingleNode($"//r:Relationship[@Id='{rid}']", relNsm) as XmlElement;
            if (relNode == null)
            {
                return null;
            }

            var target = relNode.GetAttribute("Target");
            if (string.IsNullOrWhiteSpace(target))
            {
                return null;
            }

            return target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase) ? target : $"xl/{target}";
        }

        private static List<List<string>> ReadSheetRows(ZipArchive archive, string sheetPath, List<string> sharedStrings)
        {
            var entry = archive.GetEntry(sheetPath);
            if (entry == null)
            {
                return new List<List<string>>();
            }

            var sheetDoc = new XmlDocument();
            using (var stream = entry.Open())
            {
                sheetDoc.Load(stream);
            }

            var nsm = new XmlNamespaceManager(sheetDoc.NameTable);
            nsm.AddNamespace("w", sheetDoc.DocumentElement?.NamespaceURI ?? string.Empty);

            var rows = sheetDoc.SelectNodes("//w:sheetData/w:row", nsm);
            var result = new List<List<string>>();
            if (rows == null)
            {
                return result;
            }

            foreach (XmlNode row in rows)
            {
                var cells = row.SelectNodes("w:c", nsm);
                if (cells == null)
                {
                    continue;
                }

                var rowValues = new Dictionary<int, string>();
                foreach (XmlNode cell in cells)
                {
                    var refAttr = cell.Attributes?["r"]?.Value;
                    if (string.IsNullOrWhiteSpace(refAttr))
                    {
                        continue;
                    }

                    var col = Regex.Replace(refAttr, "\\d", string.Empty);
                    var colIndex = ColumnToIndex(col);

                    var valueNode = cell.SelectSingleNode("w:v", nsm);
                    var value = valueNode?.InnerText ?? string.Empty;
                    var type = cell.Attributes?["t"]?.Value;
                    if (type == "s" && int.TryParse(value, out var sharedIndex) && sharedIndex < sharedStrings.Count)
                    {
                        value = sharedStrings[sharedIndex];
                    }

                    rowValues[colIndex] = value;
                }

                if (rowValues.Count == 0)
                {
                    continue;
                }

                var maxIndex = rowValues.Keys.Max();
                var rowList = new List<string>(maxIndex + 1);
                for (var i = 0; i <= maxIndex; i++)
                {
                    rowList.Add(rowValues.TryGetValue(i, out var v) ? v : string.Empty);
                }

                result.Add(rowList);
            }

            return result;
        }

        private static int ColumnToIndex(string column)
        {
            var index = 0;
            foreach (var ch in column.ToUpperInvariant())
            {
                index = index * 26 + (ch - 'A' + 1);
            }
            return index - 1;
        }

        private static string GenerateRoleCode(string name)
        {
            var code = Transliterate(name);
            if (string.IsNullOrWhiteSpace(code))
            {
                return $"Role_{Guid.NewGuid():N}".Substring(0, 10);
            }
            return code.Length > 50 ? code.Substring(0, 50) : code;
        }

        private static string GeneratePermissionCode(string name, HashSet<string> usedCodes)
        {
            var baseCode = Transliterate(name);
            if (string.IsNullOrWhiteSpace(baseCode))
            {
                baseCode = "Permission";
            }

            baseCode = baseCode.Length > 45 ? baseCode.Substring(0, 45) : baseCode;
            var code = baseCode;
            var i = 1;
            while (usedCodes.Contains(code))
            {
                code = $"{baseCode}_{i}";
                i++;
            }

            return code;
        }

        private static string Transliterate(string input)
        {
            var map = new Dictionary<char, string>
            {
                ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d",
                ['е'] = "e", ['ё'] = "e", ['ж'] = "zh", ['з'] = "z", ['и'] = "i",
                ['й'] = "y", ['к'] = "k", ['л'] = "l", ['м'] = "m", ['н'] = "n",
                ['о'] = "o", ['п'] = "p", ['р'] = "r", ['с'] = "s", ['т'] = "t",
                ['у'] = "u", ['ф'] = "f", ['х'] = "kh", ['ц'] = "ts", ['ч'] = "ch",
                ['ш'] = "sh", ['щ'] = "shch", ['ы'] = "y", ['э'] = "e", ['ю'] = "yu",
                ['я'] = "ya"
            };

            var sb = new StringBuilder();
            foreach (var ch in input.ToLowerInvariant())
            {
                if (map.TryGetValue(ch, out var repl))
                {
                    sb.Append(repl);
                }
                else if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                }
                else
                {
                    sb.Append('_');
                }
            }

            var cleaned = Regex.Replace(sb.ToString(), "_+", "_").Trim('_');
            return cleaned;
        }
    }
}
