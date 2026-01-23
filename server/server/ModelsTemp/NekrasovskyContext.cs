using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace server.ModelsTemp;

public partial class NekrasovskyContext : DbContext
{
    public NekrasovskyContext(DbContextOptions<NekrasovskyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessibleMovement> AccessibleMovements { get; set; }

    public virtual DbSet<AlarmEvent> AlarmEvents { get; set; }

    public virtual DbSet<FillingWarehouse> FillingWarehouses { get; set; }

    public virtual DbSet<Material> Materials { get; set; }

    public virtual DbSet<PartRequest> PartRequests { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Recipe> Recipes { get; set; }

    public virtual DbSet<RequestLog> RequestLogs { get; set; }

    public virtual DbSet<ShiftTransfer> ShiftTransfers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    public virtual DbSet<WorkReport> WorkReports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessibleMovement>(entity =>
        {
            entity.HasKey(e => new { e.FromWarehouseId, e.ToWarehouseId, e.MaterialId });

            entity.ToTable("AccessibleMovement");

            entity.HasIndex(e => e.MaterialId, "IX_AccessibleMovement_MaterialId");

            entity.HasIndex(e => e.ToWarehouseId, "IX_AccessibleMovement_ToWarehouseId");

            entity.HasOne(d => d.FromWarehouse).WithMany(p => p.AccessibleMovementFromWarehouses)
                .HasForeignKey(d => d.FromWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Material).WithMany(p => p.AccessibleMovements).HasForeignKey(d => d.MaterialId);

            entity.HasOne(d => d.ToWarehouse).WithMany(p => p.AccessibleMovementToWarehouses)
                .HasForeignKey(d => d.ToWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AlarmEvent>(entity =>
        {
            entity.ToTable("AlarmEvent");

            entity.HasIndex(e => e.UserId, "IX_AlarmEvent_UserId");

            entity.Property(e => e.Location).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.AlarmEvents).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<FillingWarehouse>(entity =>
        {
            entity.HasKey(e => new { e.WarehouseId, e.MaterialId });

            entity.ToTable("FillingWarehouse");

            entity.HasIndex(e => e.MaterialId, "IX_FillingWarehouse_MaterialId");

            entity.Property(e => e.MeasuringType).HasMaxLength(20);

            entity.HasOne(d => d.Material).WithMany(p => p.FillingWarehouses).HasForeignKey(d => d.MaterialId);

            entity.HasOne(d => d.Warehouse).WithMany(p => p.FillingWarehouses).HasForeignKey(d => d.WarehouseId);
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.ToTable("Material");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.MeasuringUnit).HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<PartRequest>(entity =>
        {
            entity.ToTable("PartRequest");

            entity.HasIndex(e => e.FromUserId, "IX_PartRequest_FromUserId");

            entity.HasIndex(e => e.FromWarehouseId, "IX_PartRequest_FromWarehouseId");

            entity.HasIndex(e => e.MaterialId, "IX_PartRequest_MaterialId");

            entity.HasIndex(e => e.ToUserId, "IX_PartRequest_ToUserId");

            entity.HasIndex(e => e.ToWarehouseId, "IX_PartRequest_ToWarehouseId");

            entity.Property(e => e.MeasuringType).HasMaxLength(20);

            entity.HasOne(d => d.FromUser).WithMany(p => p.PartRequestFromUsers)
                .HasForeignKey(d => d.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.FromWarehouse).WithMany(p => p.PartRequestFromWarehouses)
                .HasForeignKey(d => d.FromWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Material).WithMany(p => p.PartRequests)
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.ToUser).WithMany(p => p.PartRequestToUsers)
                .HasForeignKey(d => d.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.ToWarehouse).WithMany(p => p.PartRequestToWarehouses)
                .HasForeignKey(d => d.ToWarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permission");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Product");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Product).HasForeignKey<Product>(d => d.Id);
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.MaterialId });

            entity.ToTable("Recipe");

            entity.HasIndex(e => e.MaterialId, "IX_Recipe_MaterialId");

            entity.Property(e => e.MeasuringType).HasMaxLength(20);

            entity.HasOne(d => d.Material).WithMany(p => p.Recipes).HasForeignKey(d => d.MaterialId);

            entity.HasOne(d => d.Product).WithMany(p => p.Recipes).HasForeignKey(d => d.ProductId);
        });

        modelBuilder.Entity<RequestLog>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_RequestLogs_UserId");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.ClientIp).HasMaxLength(50);
            entity.Property(e => e.Controller).HasMaxLength(100);
            entity.Property(e => e.HttpMethod).HasMaxLength(10);
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.UserAgent).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.RequestLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ShiftTransfer>(entity =>
        {
            entity.ToTable("ShiftTransfer");

            entity.HasIndex(e => e.FromUserId, "IX_ShiftTransfer_FromUserId");

            entity.HasIndex(e => e.ToUserId, "IX_ShiftTransfer_ToUserId");

            entity.HasOne(d => d.FromUser).WithMany(p => p.ShiftTransferFromUsers)
                .HasForeignKey(d => d.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.ToUser).WithMany(p => p.ShiftTransferToUsers)
                .HasForeignKey(d => d.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "IX_User_Email").IsUnique();

            entity.HasIndex(e => e.Login, "IX_User_Login").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.EncryptedPassword).HasMaxLength(255);
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Surname).HasMaxLength(100);

            entity.HasMany(d => d.Permissions).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserPermission",
                    r => r.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"),
                    l => l.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "PermissionId");
                        j.ToTable("UserPermissions");
                        j.HasIndex(new[] { "PermissionId" }, "IX_UserPermissions_PermissionId");
                    });
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("Warehouse");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<WorkReport>(entity =>
        {
            entity.ToTable("WorkReport");

            entity.HasIndex(e => e.UserId, "IX_WorkReport_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.WorkReports)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
