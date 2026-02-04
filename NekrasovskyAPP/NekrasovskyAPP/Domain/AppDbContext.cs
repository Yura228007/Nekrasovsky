using NekrasovskyAPP.Models;
using Microsoft.EntityFrameworkCore;


namespace NekrasovskyAPP.Domain
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 🔹 Таблицы (DbSet)
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<UserPermissions> UserPermissions { get; set; } = null!;
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
        public DbSet<Reprocessing> Reprocessings { get; set; } = null!;
        public DbSet<ReprocessingItem> ReprocessingItems { get; set; } = null!;
        public DbSet<HistoryEvent> HistoryEvents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============================
            // 🔸 Составные ключи
            // =============================

            modelBuilder.Entity<AccessibleMovement>()
                .HasKey(am => new { am.FromWarehouseId, am.ToWarehouseId, am.MaterialId });

            modelBuilder.Entity<Recipe>()
                .HasKey(r => new { r.ProductId, r.MaterialId });

            modelBuilder.Entity<UserPermissions>()
                .HasKey(up => new { up.UserId, up.PermissionId });

            modelBuilder.Entity<FillingWarehouse>()
                .HasKey(fw => fw.Id);

            // =============================
            // 🔸 User
            // =============================

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // =============================
            // 🔸 Связи AccessibleMovement
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
            // 🔸 Recipe
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
            // 🔸 UserPermissions
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
            // 🔸 PartRequest
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
            // 🔸 ShiftTransfer
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
            // 🔸 FillingWarehouse
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
            // 🔸 WorkReport
            // =============================

            modelBuilder.Entity<WorkReport>()
                .HasOne(wr => wr.User)
                .WithMany()
                .HasForeignKey(wr => wr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // =============================
            // 🔸 Reprocessing
            // =============================

            modelBuilder.Entity<ReprocessingItem>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_ReprocessingItem_MaterialOrProduct",
                    "(\"MaterialId\" IS NOT NULL AND \"ProductId\" IS NULL) OR (\"MaterialId\" IS NULL AND \"ProductId\" IS NOT NULL)"));
        }
    }
}
