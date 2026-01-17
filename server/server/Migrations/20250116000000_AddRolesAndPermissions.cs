using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Создаем таблицу Role
            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Code = table.Column<string>(type: "varchar(50)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Rank = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            // Добавляем поле RoleId в User
            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "User",
                type: "integer",
                nullable: true);

            // Создаем таблицу RolePermission
            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermission_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermission_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_PermissionId",
                table: "RolePermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Code",
                table: "Role",
                column: "Code",
                unique: true);

            // Добавляем внешний ключ для User.RoleId
            migrationBuilder.AddForeignKey(
                name: "FK_User_Role_RoleId",
                table: "User",
                column: "RoleId",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Заполняем роли (используем WHERE NOT EXISTS для совместимости)
            migrationBuilder.Sql(@"
                INSERT INTO ""Role"" (""Name"", ""Code"", ""Description"", ""Rank"")
                SELECT * FROM (VALUES
                    ('Владелец', 'Owner', 'Владелец предприятия', 0),
                    ('Администратор', 'Admin', 'Администратор системы', 1),
                    ('Старший экструзионщик', 'SeniorExtruder', 'Старший экструзионщик', 2),
                    ('Экструзионщик', 'Extruder', 'Экструзионщик', 3),
                    ('Старший кладовщик', 'SeniorStorekeeper', 'Старший кладовщик', 2),
                    ('Кладовщик', 'Storekeeper', 'Кладовщик', 3),
                    ('Покрасочник', 'Painter', 'Покрасочник', 3),
                    ('Старшая упаковщица', 'SeniorPacker', 'Старшая упаковщица', 2),
                    ('Кладовщик готовой продукции', 'FinishedProductStorekeeper', 'Кладовщик готовой продукции', 3),
                    ('ТПА', 'TPA', 'ТПА', 3),
                    ('Утиль', 'Waste', 'Утиль', 4)
                ) AS v(""Name"", ""Code"", ""Description"", ""Rank"")
                WHERE NOT EXISTS (SELECT 1 FROM ""Role"" WHERE ""Role"".""Code"" = v.""Code"");
            ");

            // Заполняем права, если их еще нет
            migrationBuilder.Sql(@"
                INSERT INTO ""Permission"" (""Code"", ""Name"", ""Description"")
                SELECT * FROM (VALUES
                    ('AddUsersAllRanks', 'Добавление пользователей всех рангов', 'Позволяет добавлять пользователей всех рангов'),
                    ('AddUsersLowerRanks', 'Добавление пользователей ниже рангом', 'Позволяет добавлять пользователей ниже рангом'),
                    ('Reception', 'Приемка', 'Приемка сырья, станков и т.д. на производство'),
                    ('AssignBarcode', 'Назначение штрихкода', 'Назначение штрихкода позиции, таре и т.д.'),
                    ('SendToWaste', 'Отправка на Утиль', 'Отправка товара на утиль'),
                    ('SendToSDH', 'Отправка на СДХ', 'Отправка товара на СДХ'),
                    ('SendToSale', 'Отправка на реализацию', 'Отправка товара на реализацию'),
                    ('WriteOff', 'Списание', 'Списание (выбросить)'),
                    ('MoveFromMainToWorkshop', 'Перенос ТМЦ из Основного склада в цеха', 'Перенос ТМЦ из Основного склада в цеха'),
                    ('MoveFromWorkshopToMain', 'Перенос ТМЦ из Цеха на Основной склад', 'Перенос ТМЦ из Цеха на Основной склад'),
                    ('ShiftTransfer', 'Передача смены', 'Передача смены'),
                    ('ManageRecipes', 'Создание и добавление рецептур, единиц хранения', 'Создание и добавление рецептур, единиц хранения'),
                    ('Inventory', 'Инвентаризация', 'Инвентаризация')
                ) AS v(""Code"", ""Name"", ""Description"")
                WHERE NOT EXISTS (SELECT 1 FROM ""Permission"" WHERE ""Permission"".""Code"" = v.""Code"");
            ");

            // Базовое распределение прав (Владелец и Администратор имеют все права)
            // Примечание: После получения данных из файла "Список прав" нужно будет обновить этот блок
            migrationBuilder.Sql(@"
                -- Владелец имеет все права
                INSERT INTO ""RolePermission"" (""RoleId"", ""PermissionId"")
                SELECT r.""Id"", p.""Id""
                FROM ""Role"" r
                CROSS JOIN ""Permission"" p
                WHERE r.""Code"" = 'Owner'
                AND NOT EXISTS (
                    SELECT 1 FROM ""RolePermission"" rp 
                    WHERE rp.""RoleId"" = r.""Id"" AND rp.""PermissionId"" = p.""Id""
                );

                -- Администратор имеет все права (можно будет скорректировать позже)
                INSERT INTO ""RolePermission"" (""RoleId"", ""PermissionId"")
                SELECT r.""Id"", p.""Id""
                FROM ""Role"" r
                CROSS JOIN ""Permission"" p
                WHERE r.""Code"" = 'Admin'
                AND NOT EXISTS (
                    SELECT 1 FROM ""RolePermission"" rp 
                    WHERE rp.""RoleId"" = r.""Id"" AND rp.""PermissionId"" = p.""Id""
                );
            ");

            // Добавляем базовые цеха (можно будет дополнить после получения данных из файла)
            migrationBuilder.Sql(@"
                INSERT INTO ""Warehouse"" (""Name"", ""Type"", ""IsActive"")
                SELECT * FROM (VALUES
                    ('Основной склад', 'Склад', true),
                    ('Экструзионный цех', 'Цех', true),
                    ('Покрасочный цех', 'Цех', true),
                    ('Упаковочный цех', 'Цех', true),
                    ('Цех готовой продукции', 'Цех', true),
                    ('ТПА цех', 'Цех', true),
                    ('Утиль', 'Склад', true)
                ) AS v(""Name"", ""Type"", ""IsActive"")
                WHERE NOT EXISTS (SELECT 1 FROM ""Warehouse"" WHERE ""Warehouse"".""Name"" = v.""Name"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Role_RoleId",
                table: "User");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropIndex(
                name: "IX_User_RoleId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "User");
        }
    }
}

