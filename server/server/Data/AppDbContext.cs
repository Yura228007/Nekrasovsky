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
    public DbSet<Responsibility> Responsibilities { get; set; } = null!;
    public DbSet<Reprocessing> Reprocessings { get; set; } = null!;
    public DbSet<ReprocessingItem> ReprocessingItems { get; set; } = null!;
    public DbSet<ReprocessingSourceItem> ReprocessingSourceItems { get; set; } = null!;
    public DbSet<HistoryEvent> HistoryEvents { get; set; } = null!;
    public DbSet<ProductOutput> ProductOutputs { get; set; } = null!;
    public DbSet<Machine> Machines { get; set; } = null!;
    public DbSet<ShiftReport> ShiftReports { get; set; } = null!;

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
            .OnDelete(DeleteBehavior.Restrict);

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
        // Responsibility
        // =============================

        modelBuilder.Entity<Responsibility>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Responsibility>()
            .HasOne(r => r.Material)
            .WithMany()
            .HasForeignKey(r => r.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Responsibility>()
            .HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Responsibility>()
            .HasIndex(r => new { r.MaterialId })
            .IsUnique()
            .HasFilter("\"IsActive\" = true AND \"MaterialId\" IS NOT NULL");

        modelBuilder.Entity<Responsibility>()
            .HasIndex(r => new { r.ProductId })
            .IsUnique()
            .HasFilter("\"IsActive\" = true AND \"ProductId\" IS NOT NULL");

        modelBuilder.Entity<Responsibility>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Responsibility_MaterialOrProduct",
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
            .HasOne(po => po.Machine)
            .WithMany()
            .HasForeignKey(po => po.MachineId)
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
    }
}