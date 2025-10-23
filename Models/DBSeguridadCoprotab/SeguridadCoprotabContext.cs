using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace inventario_coprotab.Models.DBSeguridadCoprotab;

public partial class SeguridadCoprotabContext : DbContext
{
    public SeguridadCoprotabContext()
    {
    }

    public SeguridadCoprotabContext(DbContextOptions<SeguridadCoprotabContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<PermisosInRole> PermisosInRoles { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UsersInApplication> UsersInApplications { get; set; }

    public virtual DbSet<UsersInRole> UsersInRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=SeguridadConection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.ApplicationId)
                .HasName("PK__Applicat__C93A4C997F60ED59")
                .HasFillFactor(90);

            entity.Property(e => e.ApplicationName)
                .HasMaxLength(235)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Image)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Platform)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Url)
                .HasMaxLength(800)
                .IsUnicode(false)
                .HasColumnName("URL");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.PermisoId)
                .HasName("PK__Permisos__96E0C72352359D76")
                .HasFillFactor(90);

            entity.Property(e => e.CodigoArbol)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.PermisoName)
                .HasMaxLength(256)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PermisosInRole>(entity =>
        {
            entity.HasKey(e => e.PermisosInRoleId)
                .HasName("PK__Permisos__4047E9B6F5ABC7B7")
                .HasFillFactor(90);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId)
                .HasName("PK__Roles__8AFACE1A03317E3D")
                .HasFillFactor(90);

            entity.Property(e => e.Description)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.RoleName)
                .HasMaxLength(256)
                .IsUnicode(false);

            entity.HasOne(d => d.Application).WithMany(p => p.Roles)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("RoleEntity_Application");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId)
                .HasName("PK__Users__1788CC4C07F6335A")
                .HasFillFactor(90);

            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Image)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("image");
            entity.Property(e => e.LastActivityDate).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(8000);
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UsersInApplication>(entity =>
        {
            entity.HasKey(e => e.UsersInApplicationId)
                .HasName("PK__UsersInA__251C8622882C64F7")
                .HasFillFactor(90);

            entity.HasOne(d => d.Application).WithMany(p => p.UsersInApplications)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UsersInApplication_Application");

            entity.HasOne(d => d.User).WithMany(p => p.UsersInApplications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UsersInApplication_User");
        });

        modelBuilder.Entity<UsersInRole>(entity =>
        {
            entity.HasKey(e => e.UsersInRoleId)
                .HasName("PK__UsersInR__33BD70550BC6C43E")
                .HasFillFactor(90);

            entity.HasOne(d => d.Role).WithMany(p => p.UsersInRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UsersInRole_Role");

            entity.HasOne(d => d.User).WithMany(p => p.UsersInRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UsersInRole_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
