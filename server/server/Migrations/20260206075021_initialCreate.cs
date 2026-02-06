using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Machine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(200)", nullable: false),
                    Type = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machine", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Material",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "varchar(50)", nullable: true),
                    MeasuringUnit = table.Column<string>(type: "varchar(20)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Material", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "varchar(50)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "varchar(50)", nullable: true),
                    MeasuringUnit = table.Column<string>(type: "varchar(20)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouse", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recipe",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringType = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipe", x => new { x.ProductId, x.MaterialId });
                    table.ForeignKey(
                        name: "FK_Recipe_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recipe_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                        name: "FK_RolePermission_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermission_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Login = table.Column<string>(type: "varchar(50)", nullable: false),
                    EncryptedPassword = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    Surname = table.Column<string>(type: "varchar(100)", nullable: false),
                    Email = table.Column<string>(type: "varchar(150)", nullable: false),
                    Phone = table.Column<string>(type: "varchar(20)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AccessibleMovement",
                columns: table => new
                {
                    FromWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    ToWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessibleMovement", x => new { x.FromWarehouseId, x.ToWarehouseId, x.MaterialId });
                    table.ForeignKey(
                        name: "FK_AccessibleMovement_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessibleMovement_Warehouse_FromWarehouseId",
                        column: x => x.FromWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessibleMovement_Warehouse_ToWarehouseId",
                        column: x => x.ToWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FillingWarehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringType = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FillingWarehouse", x => x.Id);
                    table.CheckConstraint("CK_FillingWarehouse_MaterialOrProduct", "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_FillingWarehouse_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FillingWarehouse_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FillingWarehouse_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlarmEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlarmEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlarmEvent_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DisposalRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromUserId = table.Column<int>(type: "integer", nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "integer", nullable: true),
                    FromWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    ToWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringUnit = table.Column<string>(type: "varchar(20)", nullable: true),
                    RequestType = table.Column<string>(type: "varchar(20)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisposalRequest", x => x.Id);
                    table.CheckConstraint("CK_DisposalRequest_MaterialOrProduct", "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_DisposalRequest_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisposalRequest_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisposalRequest_User_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisposalRequest_User_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisposalRequest_Warehouse_FromWarehouseId",
                        column: x => x.FromWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DisposalRequest_Warehouse_ToWarehouseId",
                        column: x => x.ToWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistoryEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RelatedUserId = table.Column<int>(type: "integer", nullable: true),
                    Action = table.Column<string>(type: "varchar(100)", nullable: false),
                    EntityType = table.Column<string>(type: "varchar(50)", nullable: true),
                    EntityId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: true),
                    MaterialId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoryEvent_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HistoryEvent_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HistoryEvent_User_RelatedUserId",
                        column: x => x.RelatedUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HistoryEvent_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistoryEvent_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PartRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromUserId = table.Column<int>(type: "integer", nullable: false),
                    ToUserId = table.Column<int>(type: "integer", nullable: false),
                    FromWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    ToWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringType = table.Column<string>(type: "varchar(20)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartRequest", x => x.Id);
                    table.CheckConstraint("CK_PartRequest_MaterialOrProduct", "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_PartRequest_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartRequest_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartRequest_User_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartRequest_User_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartRequest_Warehouse_FromWarehouseId",
                        column: x => x.FromWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartRequest_Warehouse_ToWarehouseId",
                        column: x => x.ToWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductBatch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringUnit = table.Column<string>(type: "varchar(20)", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BatchNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductBatch_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductBatch_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductBatch_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reprocessing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    SourceMaterialId = table.Column<int>(type: "integer", nullable: false),
                    SourceQuantity = table.Column<int>(type: "integer", nullable: false),
                    DefectQuantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reprocessing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reprocessing_Material_SourceMaterialId",
                        column: x => x.SourceMaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reprocessing_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reprocessing_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftTransfer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromUserId = table.Column<int>(type: "integer", nullable: false),
                    ToUserId = table.Column<int>(type: "integer", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftTransfer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftTransfer_User_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftTransfer_User_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => new { x.UserId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_UserPermissions_Permission_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPermissions_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    StartWork = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishWork = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkReport_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductMovementRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromUserId = table.Column<int>(type: "integer", nullable: false),
                    ToUserId = table.Column<int>(type: "integer", nullable: false),
                    FromWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    ToWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    ProductBatchId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringType = table.Column<string>(type: "varchar(20)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMovementRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductMovementRequest_ProductBatch_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "ProductBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductMovementRequest_User_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductMovementRequest_User_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductMovementRequest_Warehouse_FromWarehouseId",
                        column: x => x.FromWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductMovementRequest_Warehouse_ToWarehouseId",
                        column: x => x.ToWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResponsibilityFilling",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    ProductBatchId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringUnit = table.Column<string>(type: "varchar(20)", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsibilityFilling", x => x.Id);
                    table.CheckConstraint("CK_ResponsibilityFilling_MaterialOrProduct", "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ResponsibilityFilling_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResponsibilityFilling_ProductBatch_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "ProductBatch",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResponsibilityFilling_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResponsibilityFilling_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResponsibilityFilling_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReprocessingItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReprocessingId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringType = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReprocessingItem", x => x.Id);
                    table.CheckConstraint("CK_ReprocessingItem_MaterialOrProduct", "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_ReprocessingItem_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReprocessingItem_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReprocessingItem_Reprocessing_ReprocessingId",
                        column: x => x.ReprocessingId,
                        principalTable: "Reprocessing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReprocessingSourceItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReprocessingId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringType = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReprocessingSourceItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReprocessingSourceItem_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReprocessingSourceItem_Reprocessing_ReprocessingId",
                        column: x => x.ReprocessingId,
                        principalTable: "Reprocessing",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductOutput",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    WorkReportId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: true),
                    ProductBatchId = table.Column<int>(type: "integer", nullable: true),
                    ProducedQuantity = table.Column<int>(type: "integer", nullable: false),
                    DefectQuantity = table.Column<int>(type: "integer", nullable: false),
                    EcoQuantity = table.Column<int>(type: "integer", nullable: false),
                    RewindQuantity = table.Column<int>(type: "integer", nullable: false),
                    NormalWarehouseId = table.Column<int>(type: "integer", nullable: true),
                    EcoWarehouseId = table.Column<int>(type: "integer", nullable: true),
                    DefectWarehouseId = table.Column<int>(type: "integer", nullable: true),
                    RewindWarehouseId = table.Column<int>(type: "integer", nullable: true),
                    RewindToUserId = table.Column<int>(type: "integer", nullable: true),
                    MeasuringUnit = table.Column<string>(type: "varchar(50)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOutput", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductOutput_ProductBatch_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "ProductBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductOutput_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductOutput_User_RewindToUserId",
                        column: x => x.RewindToUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductOutput_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductOutput_Warehouse_DefectWarehouseId",
                        column: x => x.DefectWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductOutput_Warehouse_EcoWarehouseId",
                        column: x => x.EcoWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductOutput_Warehouse_NormalWarehouseId",
                        column: x => x.NormalWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductOutput_Warehouse_RewindWarehouseId",
                        column: x => x.RewindWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductOutput_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductOutput_WorkReport_WorkReportId",
                        column: x => x.WorkReportId,
                        principalTable: "WorkReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ResponsibilityShiftSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkReportId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SnapshotAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsibilityShiftSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponsibilityShiftSnapshot_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResponsibilityShiftSnapshot_WorkReport_WorkReportId",
                        column: x => x.WorkReportId,
                        principalTable: "WorkReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShiftReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkReportId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "varchar(255)", nullable: false),
                    FilePath = table.Column<string>(type: "varchar(500)", nullable: true),
                    FileContent = table.Column<byte[]>(type: "bytea", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShiftStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShiftEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftReport_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftReport_WorkReport_WorkReportId",
                        column: x => x.WorkReportId,
                        principalTable: "WorkReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinishedGoodsRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromUserId = table.Column<int>(type: "integer", nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "integer", nullable: true),
                    FromWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    ToWarehouseId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringUnit = table.Column<string>(type: "varchar(20)", nullable: true),
                    RequestType = table.Column<string>(type: "varchar(20)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProductOutputId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinishedGoodsRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsRequest_ProductOutput_ProductOutputId",
                        column: x => x.ProductOutputId,
                        principalTable: "ProductOutput",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsRequest_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsRequest_User_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsRequest_User_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsRequest_Warehouse_FromWarehouseId",
                        column: x => x.FromWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinishedGoodsRequest_Warehouse_ToWarehouseId",
                        column: x => x.ToWarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResponsibilityShiftSnapshotItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ResponsibilityShiftSnapshotId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MeasuringUnit = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsibilityShiftSnapshotItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponsibilityShiftSnapshotItem_Material_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Material",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResponsibilityShiftSnapshotItem_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResponsibilityShiftSnapshotItem_ResponsibilityShiftSnapshot~",
                        column: x => x.ResponsibilityShiftSnapshotId,
                        principalTable: "ResponsibilityShiftSnapshot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResponsibilityShiftSnapshotItem_Warehouse_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessibleMovement_MaterialId",
                table: "AccessibleMovement",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessibleMovement_ToWarehouseId",
                table: "AccessibleMovement",
                column: "ToWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_AlarmEvent_UserId",
                table: "AlarmEvent",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DisposalRequest_ApprovedByUserId",
                table: "DisposalRequest",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DisposalRequest_FromUserId",
                table: "DisposalRequest",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DisposalRequest_FromWarehouseId",
                table: "DisposalRequest",
                column: "FromWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_DisposalRequest_MaterialId",
                table: "DisposalRequest",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_DisposalRequest_ProductId",
                table: "DisposalRequest",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DisposalRequest_ToWarehouseId",
                table: "DisposalRequest",
                column: "ToWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_FillingWarehouse_MaterialId",
                table: "FillingWarehouse",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_FillingWarehouse_ProductId",
                table: "FillingWarehouse",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_FillingWarehouse_WarehouseId_MaterialId",
                table: "FillingWarehouse",
                columns: new[] { "WarehouseId", "MaterialId" },
                unique: true,
                filter: "\"MaterialId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FillingWarehouse_WarehouseId_ProductId",
                table: "FillingWarehouse",
                columns: new[] { "WarehouseId", "ProductId" },
                unique: true,
                filter: "\"ProductId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsRequest_ApprovedByUserId",
                table: "FinishedGoodsRequest",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsRequest_FromUserId",
                table: "FinishedGoodsRequest",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsRequest_FromWarehouseId",
                table: "FinishedGoodsRequest",
                column: "FromWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsRequest_ProductId",
                table: "FinishedGoodsRequest",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsRequest_ProductOutputId",
                table: "FinishedGoodsRequest",
                column: "ProductOutputId");

            migrationBuilder.CreateIndex(
                name: "IX_FinishedGoodsRequest_ToWarehouseId",
                table: "FinishedGoodsRequest",
                column: "ToWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryEvent_MaterialId",
                table: "HistoryEvent",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryEvent_ProductId",
                table: "HistoryEvent",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryEvent_RelatedUserId",
                table: "HistoryEvent",
                column: "RelatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryEvent_UserId",
                table: "HistoryEvent",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryEvent_WarehouseId",
                table: "HistoryEvent",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PartRequest_FromUserId",
                table: "PartRequest",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartRequest_FromWarehouseId",
                table: "PartRequest",
                column: "FromWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PartRequest_MaterialId",
                table: "PartRequest",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_PartRequest_ProductId",
                table: "PartRequest",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PartRequest_ToUserId",
                table: "PartRequest",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PartRequest_ToWarehouseId",
                table: "PartRequest",
                column: "ToWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatch_CreatedByUserId",
                table: "ProductBatch",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatch_ProductId",
                table: "ProductBatch",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatch_WarehouseId",
                table: "ProductBatch",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMovementRequest_FromUserId",
                table: "ProductMovementRequest",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMovementRequest_FromWarehouseId",
                table: "ProductMovementRequest",
                column: "FromWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMovementRequest_ProductBatchId",
                table: "ProductMovementRequest",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMovementRequest_ToUserId",
                table: "ProductMovementRequest",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMovementRequest_ToWarehouseId",
                table: "ProductMovementRequest",
                column: "ToWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_DefectWarehouseId",
                table: "ProductOutput",
                column: "DefectWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_EcoWarehouseId",
                table: "ProductOutput",
                column: "EcoWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_NormalWarehouseId",
                table: "ProductOutput",
                column: "NormalWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_ProductBatchId",
                table: "ProductOutput",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_ProductId",
                table: "ProductOutput",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_RewindToUserId",
                table: "ProductOutput",
                column: "RewindToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_RewindWarehouseId",
                table: "ProductOutput",
                column: "RewindWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_UserId",
                table: "ProductOutput",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_WarehouseId",
                table: "ProductOutput",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOutput_WorkReportId",
                table: "ProductOutput",
                column: "WorkReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_MaterialId",
                table: "Recipe",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Reprocessing_SourceMaterialId",
                table: "Reprocessing",
                column: "SourceMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Reprocessing_UserId",
                table: "Reprocessing",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reprocessing_WarehouseId",
                table: "Reprocessing",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReprocessingItem_MaterialId",
                table: "ReprocessingItem",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ReprocessingItem_ProductId",
                table: "ReprocessingItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReprocessingItem_ReprocessingId",
                table: "ReprocessingItem",
                column: "ReprocessingId");

            migrationBuilder.CreateIndex(
                name: "IX_ReprocessingSourceItem_MaterialId",
                table: "ReprocessingSourceItem",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ReprocessingSourceItem_ReprocessingId",
                table: "ReprocessingSourceItem",
                column: "ReprocessingId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityFilling_MaterialId",
                table: "ResponsibilityFilling",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityFilling_ProductBatchId",
                table: "ResponsibilityFilling",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityFilling_ProductId",
                table: "ResponsibilityFilling",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityFilling_UserId_WarehouseId_MaterialId",
                table: "ResponsibilityFilling",
                columns: new[] { "UserId", "WarehouseId", "MaterialId" },
                filter: "\"IsActive\" = true AND \"MaterialId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityFilling_UserId_WarehouseId_ProductId",
                table: "ResponsibilityFilling",
                columns: new[] { "UserId", "WarehouseId", "ProductId" },
                filter: "\"IsActive\" = true AND \"ProductId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityFilling_WarehouseId",
                table: "ResponsibilityFilling",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityShiftSnapshot_UserId",
                table: "ResponsibilityShiftSnapshot",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityShiftSnapshot_WorkReportId",
                table: "ResponsibilityShiftSnapshot",
                column: "WorkReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityShiftSnapshotItem_MaterialId",
                table: "ResponsibilityShiftSnapshotItem",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityShiftSnapshotItem_ProductId",
                table: "ResponsibilityShiftSnapshotItem",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityShiftSnapshotItem_ResponsibilityShiftSnapshot~",
                table: "ResponsibilityShiftSnapshotItem",
                column: "ResponsibilityShiftSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityShiftSnapshotItem_WarehouseId",
                table: "ResponsibilityShiftSnapshotItem",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_PermissionId",
                table: "RolePermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftReport_UserId",
                table: "ShiftReport",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftReport_WorkReportId",
                table: "ShiftReport",
                column: "WorkReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShiftTransfer_FromUserId",
                table: "ShiftTransfer",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftTransfer_ToUserId",
                table: "ShiftTransfer",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Login",
                table: "User",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_PermissionId",
                table: "UserPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkReport_UserId",
                table: "WorkReport",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessibleMovement");

            migrationBuilder.DropTable(
                name: "AlarmEvent");

            migrationBuilder.DropTable(
                name: "DisposalRequest");

            migrationBuilder.DropTable(
                name: "FillingWarehouse");

            migrationBuilder.DropTable(
                name: "FinishedGoodsRequest");

            migrationBuilder.DropTable(
                name: "HistoryEvent");

            migrationBuilder.DropTable(
                name: "Machine");

            migrationBuilder.DropTable(
                name: "PartRequest");

            migrationBuilder.DropTable(
                name: "ProductMovementRequest");

            migrationBuilder.DropTable(
                name: "Recipe");

            migrationBuilder.DropTable(
                name: "ReprocessingItem");

            migrationBuilder.DropTable(
                name: "ReprocessingSourceItem");

            migrationBuilder.DropTable(
                name: "ResponsibilityFilling");

            migrationBuilder.DropTable(
                name: "ResponsibilityShiftSnapshotItem");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "ShiftReport");

            migrationBuilder.DropTable(
                name: "ShiftTransfer");

            migrationBuilder.DropTable(
                name: "UserPermissions");

            migrationBuilder.DropTable(
                name: "ProductOutput");

            migrationBuilder.DropTable(
                name: "Reprocessing");

            migrationBuilder.DropTable(
                name: "ResponsibilityShiftSnapshot");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "ProductBatch");

            migrationBuilder.DropTable(
                name: "Material");

            migrationBuilder.DropTable(
                name: "WorkReport");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Warehouse");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
