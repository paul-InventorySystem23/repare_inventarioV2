using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace inventario_coprotab.Models.DBInventario;

public partial class SistemaInventarioContext : DbContext
{
    public SistemaInventarioContext()
    {
    }

    public SistemaInventarioContext(DbContextOptions<SistemaInventarioContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Dispositivo> Dispositivos { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<Movimiento> Movimientos { get; set; }

    public virtual DbSet<ReparacionConsumible> ReparacionConsumibles { get; set; }

    public virtual DbSet<Reparacione> Reparaciones { get; set; }

    public virtual DbSet<Responsable> Responsables { get; set; }

    public virtual DbSet<TipoHardware> TipoHardwares { get; set; }

    public virtual DbSet<Ubicacione> Ubicaciones { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=MiConexion");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dispositivo>(entity =>
        {
            entity.HasKey(e => e.IdDispositivo).HasName("PK__disposit__FD7B94E590FBF907");

            entity.ToTable("dispositivos");

            entity.HasIndex(e => e.CodigoInventario, "UQ__disposit__2C4D9A17616ADCA7").IsUnique();

            entity.HasIndex(e => e.NroSerie, "UQ__disposit__AD64A161577217F7").IsUnique();

            entity.Property(e => e.IdDispositivo).HasColumnName("id_dispositivo");
            entity.Property(e => e.CodigoInventario)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("codigo_inventario");
            entity.Property(e => e.Descripcion)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.EstadoRegistro)
                .HasDefaultValue(true)
                .HasColumnName("estado_registro");
            entity.Property(e => e.FechaAlta).HasColumnName("fecha_alta");
            entity.Property(e => e.FechaBaja).HasColumnName("fecha_baja");
            entity.Property(e => e.IdMarca).HasColumnName("id_marca");
            entity.Property(e => e.IdTipo).HasColumnName("id_tipo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.NroSerie)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nro_serie");
            entity.Property(e => e.StockActual)
                .HasDefaultValue(0)
                .HasColumnName("stock_actual");
            entity.Property(e => e.StockMinimo)
                .HasDefaultValue(0)
                .HasColumnName("stock_minimo");

            entity.HasOne(d => d.IdMarcaNavigation).WithMany(p => p.Dispositivos)
                .HasForeignKey(d => d.IdMarca)
                .HasConstraintName("FK_Dispositivo_Marca");

            entity.HasOne(d => d.IdTipoNavigation).WithMany(p => p.Dispositivos)
                .HasForeignKey(d => d.IdTipo)
                .HasConstraintName("FK_Dispositivo_Tipo");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.IdMarca).HasName("PK__marca__7E43E99E8233471D");

            entity.ToTable("marca");

            entity.HasIndex(e => e.Nombre, "UQ__marca__72AFBCC69C3F315A").IsUnique();

            entity.Property(e => e.IdMarca).HasColumnName("id_marca");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Movimiento>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento).HasName("PK__movimien__2A071C24D8B3F3C8");

            entity.ToTable("movimientos");

            entity.Property(e => e.IdMovimiento).HasColumnName("id_movimiento");
            entity.Property(e => e.Cantidad)
                .HasDefaultValue(1)
                .HasColumnName("cantidad");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha");
            entity.Property(e => e.IdDispositivo).HasColumnName("id_dispositivo");
            entity.Property(e => e.IdResponsable).HasColumnName("id_responsable");
            entity.Property(e => e.IdUbicacion).HasColumnName("id_ubicacion");
            entity.Property(e => e.Observaciones)
                .IsUnicode(false)
                .HasColumnName("observaciones");
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tipo_movimiento");

            entity.HasOne(d => d.IdDispositivoNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdDispositivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mov_Dispositivo");

            entity.HasOne(d => d.IdResponsableNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdResponsable)
                .HasConstraintName("FK_Mov_Responsable");

            entity.HasOne(d => d.IdUbicacionNavigation).WithMany(p => p.Movimientos)
                .HasForeignKey(d => d.IdUbicacion)
                .HasConstraintName("FK_Mov_Ubicacion");
        });

        modelBuilder.Entity<ReparacionConsumible>(entity =>
        {
            entity.HasKey(e => e.IdReparacionConsumible).HasName("PK__reparaci__38AB6E9C54D7FEC9");

            entity.ToTable("reparacion_consumibles");

            entity.HasIndex(e => new { e.IdReparacion, e.IdDispositivo }, "UQ_Rep_Cons").IsUnique();

            entity.Property(e => e.IdReparacionConsumible).HasColumnName("id_reparacion_consumible");
            entity.Property(e => e.Cantidad)
                .HasDefaultValue(1)
                .HasColumnName("cantidad");
            entity.Property(e => e.IdDispositivo).HasColumnName("id_dispositivo");
            entity.Property(e => e.IdReparacion).HasColumnName("id_reparacion");

            entity.HasOne(d => d.IdDispositivoNavigation).WithMany(p => p.ReparacionConsumibles)
                .HasForeignKey(d => d.IdDispositivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RepCons_Dispositivo");

            entity.HasOne(d => d.IdReparacionNavigation).WithMany(p => p.ReparacionConsumibles)
                .HasForeignKey(d => d.IdReparacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RepCons_Reparacion");
        });

        modelBuilder.Entity<Reparacione>(entity =>
        {
            entity.HasKey(e => e.IdReparacion).HasName("PK__reparaci__5253371F11A05803");

            entity.ToTable("reparaciones");

            entity.Property(e => e.IdReparacion).HasColumnName("id_reparacion");
            entity.Property(e => e.Descripcion)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaFinalizacion).HasColumnName("fecha_finalizacion");
            entity.Property(e => e.FechaInicio)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.IdDispositivo).HasColumnName("id_dispositivo");
            entity.Property(e => e.IdResponsable).HasColumnName("id_responsable");
            entity.Property(e => e.Observaciones)
                .IsUnicode(false)
                .HasColumnName("observaciones");
            entity.Property(e => e.TipoReparacion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tipo_reparacion");

            entity.HasOne(d => d.IdDispositivoNavigation).WithMany(p => p.Reparaciones)
                .HasForeignKey(d => d.IdDispositivo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rep_Dispositivo");

            entity.HasOne(d => d.IdResponsableNavigation).WithMany(p => p.Reparaciones)
                .HasForeignKey(d => d.IdResponsable)
                .HasConstraintName("FK_Rep_Responsable");
        });

        modelBuilder.Entity<Responsable>(entity =>
        {
            entity.HasKey(e => e.IdResponsable).HasName("PK__responsa__99B1C6CE817A8DAE");

            entity.ToTable("responsables");

            entity.Property(e => e.IdResponsable).HasColumnName("id_responsable");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Sector)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("sector");
            entity.Property(e => e.Telefono)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<TipoHardware>(entity =>
        {
            entity.HasKey(e => e.IdTipo).HasName("PK__tipo_har__CF9010894B91E3DE");

            entity.ToTable("tipo_hardware");

            entity.HasIndex(e => e.Descripcion, "UQ__tipo_har__298336B6B070E509").IsUnique();

            entity.Property(e => e.IdTipo).HasColumnName("id_tipo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<Ubicacione>(entity =>
        {
            entity.HasKey(e => e.IdUbicacion).HasName("PK__ubicacio__81BAA59149A5D88A");

            entity.ToTable("ubicaciones");

            entity.Property(e => e.IdUbicacion).HasColumnName("id_ubicacion");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
