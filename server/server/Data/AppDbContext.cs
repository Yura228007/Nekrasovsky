using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<UserPermissions> UserPermissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<Warehouse> Warehouses { get; set; } = null!;
    public DbSet<Material> Materials { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Recipe> Recipes { get; set; } = null!;
    public DbSet<AccessibleMovement> AccessibleMovements { get; set; } = null!;
    public DbSet<PartRequest> PartRequests { get; set; } = null!;
    public DbSet<AlarmEvent> AlarmEvents { get; set; } = null!;
    public DbSet<ShiftTransfer> ShiftTransfers { get; set; } = null!;
    public DbSet<FillingWarehouse> FillingWarehouses { get; set; } = null!;
    public DbSet<WorkReport> WorkReports { get; set; } = null!;
    public DbSet<ResponsibilityFilling> ResponsibilityFillings { get; set; } = null!;
    public DbSet<Reprocessing> Reprocessings { get; set; } = null!;
    public DbSet<ReprocessingItem> ReprocessingItems { get; set; } = null!;
    public DbSet<ReprocessingSourceItem> ReprocessingSourceItems { get; set; } = null!;
    public DbSet<HistoryEvent> HistoryEvents { get; set; } = null!;
    public DbSet<ProductOutput> ProductOutputs { get; set; } = null!;
    public DbSet<Machine> Machines { get; set; } = null!;
    public DbSet<ShiftReport> ShiftReports { get; set; } = null!;
    public DbSet<ResponsibilityShiftSnapshot> ResponsibilityShiftSnapshots { get; set; } = null!;
    public DbSet<ResponsibilityShiftSnapshotItem> ResponsibilityShiftSnapshotItems { get; set; } = null!;
    public DbSet<ProductBatch> ProductBatches { get; set; } = null!;
    public DbSet<ProductMovementRequest> ProductMovementRequests { get; set; } = null!;
    public DbSet<DisposalRequest> DisposalRequests { get; set; } = null!;
    public DbSet<FinishedGoodsRequest> FinishedGoodsRequests { get; set; } = null!;
    public DbSet<ProductSale> ProductSales { get; set; } = null!;
    public DbSet<SDHRequest> SDHRequests { get; set; } = null!;
    public DbSet<MaterialSDHSale> MaterialSDHSales { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =============================
        // Составные ключи
        // =============================

        modelBuilder.Entity<AccessibleMovement>()
            .HasKey(am => new { am.FromWarehouseId, am.ToWarehouseId, am.MaterialId });

        modelBuilder.Entity<Recipe>()
            .HasKey(r => new { r.ProductId, r.MaterialId });

        modelBuilder.Entity<UserPermissions>()
            .HasKey(up => new { up.UserId, up.PermissionId });

        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        modelBuilder.Entity<FillingWarehouse>()
            .HasKey(fw => fw.Id);

        // =============================
        // User
        // =============================

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // =============================
        // Role и User-Role связь
        // =============================

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        // =============================
        // RolePermission
        // =============================

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // =============================
        // Связи AccessibleMovement
        // =============================

        modelBuilder.Entity<AccessibleMovement>()
            .HasOne(am => am.FromWarehouse)
            .WithMany(w => w.FromMovements)
            .HasForeignKey(am => am.FromWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccessibleMovement>()
            .HasOne(am => am.ToWarehouse)
            .WithMany(w => w.ToMovements)
            .HasForeignKey(am => am.ToWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccessibleMovement>()
            .HasOne(am => am.Material)
            .WithMany()
            .HasForeignKey(am => am.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        // =============================
        // Recipe
        // =============================

        modelBuilder.Entity<Recipe>()
            .HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Recipe>()
            .HasOne(r => r.Material)
            .WithMany()
            .HasForeignKey(r => r.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        // =============================
        // UserPermissions
        // =============================

        modelBuilder.Entity<UserPermissions>()
            .HasOne(up => up.User)
            .WithMany()
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserPermissions>()
            .HasOne(up => up.Permission)
            .WithMany()
            .HasForeignKey(up => up.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // =============================
        // PartRequest
        // =============================

        modelBuilder.Entity<PartRequest>()
            .HasOne(pr => pr.FromUser)
            .WithMany(u => u.SentPartRequests)
            .HasForeignKey(pr => pr.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PartRequest>()
            .HasOne(pr => pr.ToUser)
            .WithMany(u => u.ReceivedPartRequests)
            .HasForeignKey(pr => pr.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PartRequest>()
            .HasOne(pr => pr.FromWarehouse)
            .WithMany(w => w.FromRequests)
            .HasForeignKey(pr => pr.FromWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PartRequest>()
            .HasOne(pr => pr.ToWarehouse)
            .WithMany(w => w.ToRequests)
            .HasForeignKey(pr => pr.ToWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PartRequest>()
            .HasOne(pr => pr.Material)
            .WithMany()
            .HasForeignKey(pr => pr.MaterialId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        modelBuilder.Entity<PartRequest>()
            .HasOne(pr => pr.Product)
            .WithMany()
            .HasForeignKey(pr => pr.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // XOR constraint: только один из MaterialId или ProductId должен быть заполнен
        modelBuilder.Entity<PartRequest>()
            .HasCheckConstraint("CK_PartRequest_MaterialOrProduct", 
                @$"(""MaterialId"" IS NOT NULL AND ""ProductId"" IS NULL) OR (""MaterialId"" IS NULL AND ""ProductId"" IS NOT NULL)");

        // =============================
        // ShiftTransfer
        // =============================

        modelBuilder.Entity<ShiftTransfer>()
            .HasOne(st => st.FromUser)
            .WithMany(u => u.SentShiftTransfers)
            .HasForeignKey(st => st.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShiftTransfer>()
            .HasOne(st => st.ToUser)
            .WithMany(u => u.ReceivedShiftTransfers)
            .HasForeignKey(st => st.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // =============================
        // FillingWarehouse
        // =============================

        modelBuilder.Entity<FillingWarehouse>()
            .HasOne(fw => fw.Warehouse)
            .WithMany()
            .HasForeignKey(fw => fw.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FillingWarehouse>()
            .HasOne(fw => fw.Material)
            .WithMany()
            .HasForeignKey(fw => fw.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FillingWarehouse>()
            .HasOne(fw => fw.Product)
            .WithMany()
            .HasForeignKey(fw => fw.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FillingWarehouse>()
            .HasIndex(fw => new { fw.WarehouseId, fw.MaterialId })
            .IsUnique()
            .HasFilter("\"MaterialId\" IS NOT NULL");

        modelBuilder.Entity<FillingWarehouse>()
            .HasIndex(fw => new { fw.WarehouseId, fw.ProductId })
            .IsUnique()
            .HasFilter("\"ProductId\" IS NOT NULL");

        modelBuilder.Entity<FillingWarehouse>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_FillingWarehouse_MaterialOrProduct",
                "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)"));

        // =============================
        // ResponsibilityFilling
        // =============================

        modelBuilder.Entity<ResponsibilityFilling>()
            .HasOne(rf => rf.User)
            .WithMany()
            .HasForeignKey(rf => rf.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ResponsibilityFilling>()
            .HasOne(rf => rf.Warehouse)
            .WithMany()
            .HasForeignKey(rf => rf.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ResponsibilityFilling>()
            .HasOne(rf => rf.Material)
            .WithMany()
            .HasForeignKey(rf => rf.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ResponsibilityFilling>()
            .HasOne(rf => rf.Product)
            .WithMany()
            .HasForeignKey(rf => rf.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ResponsibilityFilling>()
            .HasIndex(rf => new { rf.UserId, rf.WarehouseId, rf.MaterialId })
            .HasFilter("\"IsActive\" = true AND \"MaterialId\" IS NOT NULL");

        modelBuilder.Entity<ResponsibilityFilling>()
            .HasIndex(rf => new { rf.UserId, rf.WarehouseId, rf.ProductId })
            .HasFilter("\"IsActive\" = true AND \"ProductId\" IS NOT NULL");

        modelBuilder.Entity<ResponsibilityFilling>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_ResponsibilityFilling_MaterialOrProduct",
                "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)"));

        // =============================
        // WorkReport
        // =============================

        modelBuilder.Entity<WorkReport>()
            .HasOne(wr => wr.User)
            .WithMany()
            .HasForeignKey(wr => wr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // =============================
        // Reprocessing
        // =============================

        modelBuilder.Entity<Reprocessing>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reprocessing>()
            .HasOne(r => r.Warehouse)
            .WithMany()
            .HasForeignKey(r => r.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reprocessing>()
            .HasOne(r => r.SourceMaterial)
            .WithMany()
            .HasForeignKey(r => r.SourceMaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReprocessingItem>()
            .HasOne(ri => ri.Reprocessing)
            .WithMany(r => r.Items)
            .HasForeignKey(ri => ri.ReprocessingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReprocessingItem>()
            .HasOne(ri => ri.Material)
            .WithMany()
            .HasForeignKey(ri => ri.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReprocessingItem>()
            .HasOne(ri => ri.Product)
            .WithMany()
            .HasForeignKey(ri => ri.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReprocessingItem>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_ReprocessingItem_MaterialOrProduct",
                "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)"));

        modelBuilder.Entity<ReprocessingSourceItem>()
            .HasOne(rsi => rsi.Reprocessing)
            .WithMany(r => r.Sources)
            .HasForeignKey(rsi => rsi.ReprocessingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReprocessingSourceItem>()
            .HasOne(rsi => rsi.Material)
            .WithMany()
            .HasForeignKey(rsi => rsi.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        // =============================
        // HistoryEvent
        // =============================

        modelBuilder.Entity<HistoryEvent>()
            .HasOne(he => he.User)
            .WithMany()
            .HasForeignKey(he => he.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HistoryEvent>()
            .HasOne(he => he.RelatedUser)
            .WithMany()
            .HasForeignKey(he => he.RelatedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<HistoryEvent>()
            .HasOne(he => he.Warehouse)
            .WithMany()
            .HasForeignKey(he => he.WarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<HistoryEvent>()
            .HasOne(he => he.Material)
            .WithMany()
            .HasForeignKey(he => he.MaterialId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<HistoryEvent>()
            .HasOne(he => he.Product)
            .WithMany()
            .HasForeignKey(he => he.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        // =============================
        // ProductOutput
        // =============================

        modelBuilder.Entity<ProductOutput>()
            .HasOne(po => po.User)
            .WithMany()
            .HasForeignKey(po => po.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductOutput>()
            .HasOne(po => po.WorkReport)
            .WithMany()
            .HasForeignKey(po => po.WorkReportId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ProductOutput>()
            .HasOne(po => po.Product)
            .WithMany()
            .HasForeignKey(po => po.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductOutput>()
            .HasOne(po => po.Warehouse)
            .WithMany()
            .HasForeignKey(po => po.WarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ProductOutput>()
            .HasOne(po => po.ProductBatch)
            .WithMany()
            .HasForeignKey(po => po.ProductBatchId)
            .OnDelete(DeleteBehavior.SetNull);


        modelBuilder.Entity<ProductOutput>()
            .HasOne(po => po.NormalWarehouse)
            .WithMany()
            .HasForeignKey(po => po.NormalWarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ProductOutput>()
            .HasOne(po => po.EcoWarehouse)
            .WithMany()
            .HasForeignKey(po => po.EcoWarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ProductOutput>()
            .HasOne(po => po.DefectWarehouse)
            .WithMany()
            .HasForeignKey(po => po.DefectWarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ProductOutput>()
            .HasOne(po => po.RewindWarehouse)
            .WithMany()
            .HasForeignKey(po => po.RewindWarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ProductOutput>()
            .HasOne(po => po.RewindToUser)
            .WithMany()
            .HasForeignKey(po => po.RewindToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // =============================
        // ShiftReport
        // =============================

        modelBuilder.Entity<ShiftReport>()
            .HasOne(sr => sr.User)
            .WithMany()
            .HasForeignKey(sr => sr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ShiftReport>()
            .HasOne(sr => sr.WorkReport)
            .WithMany()
            .HasForeignKey(sr => sr.WorkReportId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ShiftReport>()
            .HasIndex(sr => sr.WorkReportId)
            .IsUnique();

        // =============================
        // ResponsibilityShiftSnapshot
        // =============================

        modelBuilder.Entity<ResponsibilityShiftSnapshot>()
            .HasOne(s => s.WorkReport)
            .WithMany()
            .HasForeignKey(s => s.WorkReportId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ResponsibilityShiftSnapshot>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ResponsibilityShiftSnapshot>()
            .HasMany(s => s.Items)
            .WithOne(i => i.ResponsibilityShiftSnapshot)
            .HasForeignKey(i => i.ResponsibilityShiftSnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ResponsibilityShiftSnapshotItem>()
            .HasOne(i => i.Warehouse)
            .WithMany()
            .HasForeignKey(i => i.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ResponsibilityShiftSnapshotItem>()
            .HasOne(i => i.Material)
            .WithMany()
            .HasForeignKey(i => i.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ResponsibilityShiftSnapshotItem>()
            .HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // =============================
        // DisposalRequest
        // =============================

        modelBuilder.Entity<DisposalRequest>()
            .HasOne(dr => dr.FromUser)
            .WithMany()
            .HasForeignKey(dr => dr.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DisposalRequest>()
            .HasOne(dr => dr.ApprovedByUser)
            .WithMany()
            .HasForeignKey(dr => dr.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DisposalRequest>()
            .HasOne(dr => dr.FromWarehouse)
            .WithMany()
            .HasForeignKey(dr => dr.FromWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DisposalRequest>()
            .HasOne(dr => dr.ToWarehouse)
            .WithMany()
            .HasForeignKey(dr => dr.ToWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DisposalRequest>()
            .HasOne(dr => dr.Material)
            .WithMany()
            .HasForeignKey(dr => dr.MaterialId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        modelBuilder.Entity<DisposalRequest>()
            .HasOne(dr => dr.Product)
            .WithMany()
            .HasForeignKey(dr => dr.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // XOR constraint: только один из MaterialId или ProductId должен быть заполнен
        modelBuilder.Entity<DisposalRequest>()
            .HasCheckConstraint("CK_DisposalRequest_MaterialOrProduct", 
                @$"(""MaterialId"" IS NOT NULL AND ""ProductId"" IS NULL) OR (""MaterialId"" IS NULL AND ""ProductId"" IS NOT NULL)");

        // =============================
        // FinishedGoodsRequest
        // =============================

        modelBuilder.Entity<FinishedGoodsRequest>()
            .HasOne(fgr => fgr.FromUser)
            .WithMany()
            .HasForeignKey(fgr => fgr.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FinishedGoodsRequest>()
            .HasOne(fgr => fgr.ApprovedByUser)
            .WithMany()
            .HasForeignKey(fgr => fgr.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FinishedGoodsRequest>()
            .HasOne(fgr => fgr.FromWarehouse)
            .WithMany()
            .HasForeignKey(fgr => fgr.FromWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FinishedGoodsRequest>()
            .HasOne(fgr => fgr.ToWarehouse)
            .WithMany()
            .HasForeignKey(fgr => fgr.ToWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FinishedGoodsRequest>()
            .HasOne(fgr => fgr.Product)
            .WithMany()
            .HasForeignKey(fgr => fgr.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FinishedGoodsRequest>()
            .HasOne(fgr => fgr.ProductOutput)
            .WithMany()
            .HasForeignKey(fgr => fgr.ProductOutputId)
            .OnDelete(DeleteBehavior.SetNull);

        // =============================
        // ProductSale
        // =============================

        modelBuilder.Entity<ProductSale>()
            .HasOne(ps => ps.User)
            .WithMany()
            .HasForeignKey(ps => ps.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductSale>()
            .HasOne(ps => ps.Warehouse)
            .WithMany()
            .HasForeignKey(ps => ps.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductSale>()
            .HasOne(ps => ps.Product)
            .WithMany()
            .HasForeignKey(ps => ps.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // =============================
        // SDHRequest
        // =============================

        modelBuilder.Entity<SDHRequest>()
            .HasOne(sr => sr.FromUser)
            .WithMany()
            .HasForeignKey(sr => sr.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SDHRequest>()
            .HasOne(sr => sr.ApprovedByUser)
            .WithMany()
            .HasForeignKey(sr => sr.ApprovedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<SDHRequest>()
            .HasOne(sr => sr.FromWarehouse)
            .WithMany()
            .HasForeignKey(sr => sr.FromWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SDHRequest>()
            .HasOne(sr => sr.ToWarehouse)
            .WithMany()
            .HasForeignKey(sr => sr.ToWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SDHRequest>()
            .HasOne(sr => sr.Material)
            .WithMany()
            .HasForeignKey(sr => sr.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SDHRequest>()
            .HasOne(sr => sr.Product)
            .WithMany()
            .HasForeignKey(sr => sr.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SDHRequest>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_SDHRequest_MaterialOrProduct",
                "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)"));

        // =============================
        // MaterialSDHSale
        // =============================

        modelBuilder.Entity<MaterialSDHSale>()
            .HasOne(ms => ms.User)
            .WithMany()
            .HasForeignKey(ms => ms.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MaterialSDHSale>()
            .HasOne(ms => ms.Warehouse)
            .WithMany()
            .HasForeignKey(ms => ms.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MaterialSDHSale>()
            .HasOne(ms => ms.Material)
            .WithMany()
            .HasForeignKey(ms => ms.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MaterialSDHSale>()
            .HasOne(ms => ms.Product)
            .WithMany()
            .HasForeignKey(ms => ms.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MaterialSDHSale>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_MaterialSDHSale_MaterialOrProduct",
                "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)"));
    }
}