using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models.Entities;

namespace ProyectoSauna.Models;

public partial class SaunaDbContext : DbContext
{
    public SaunaDbContext()
    {
    }

    public SaunaDbContext(DbContextOptions<SaunaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CabEgreso> CabEgreso { get; set; }

    public virtual DbSet<CategoriaProducto> CategoriaProducto { get; set; }

    public virtual DbSet<Cliente> Cliente { get; set; }

    public virtual DbSet<Comprobante> Comprobante { get; set; }

    public virtual DbSet<Cuenta> Cuenta { get; set; }

    public virtual DbSet<DetEgreso> DetEgreso { get; set; }

    public virtual DbSet<DetalleConsumo> DetalleConsumo { get; set; }

    public virtual DbSet<EstadoCuenta> EstadoCuenta { get; set; }

    public virtual DbSet<MetodoPago> MetodoPago { get; set; }

    public virtual DbSet<MovimientoInventario> MovimientoInventario { get; set; }

    public virtual DbSet<Pago> Pago { get; set; }

    public virtual DbSet<Producto> Producto { get; set; }

    public virtual DbSet<ProgramaFidelizacion> ProgramaFidelizacion { get; set; }

    public virtual DbSet<Rol> Rol { get; set; }

    public virtual DbSet<TipoComprobante> TipoComprobante { get; set; }

    public virtual DbSet<TipoEgreso> TipoEgreso { get; set; }

    public virtual DbSet<TipoMovimiento> TipoMovimiento { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=ProyectoSauna;Trusted_Connection=true;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CabEgreso>(entity =>
        {
            entity.HasKey(e => e.idCabEgreso);

            entity.Property(e => e.fecha)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.montoTotal).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.idUsuarioNavigation).WithMany(p => p.CabEgreso)
                .HasForeignKey(d => d.idUsuario)
                .HasConstraintName("FK_CabEgreso_Usaurio");
        });

        modelBuilder.Entity<CategoriaProducto>(entity =>
        {
            entity.HasKey(e => e.idCategoriaProducto);

            entity.HasIndex(e => e.nombre, "UQ_CategoriaProducto_nombre").IsUnique();

            entity.Property(e => e.nombre).HasMaxLength(80);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.idCliente);

            entity.Property(e => e.activo).HasDefaultValue(true);
            entity.Property(e => e.apellidos).HasMaxLength(120);
            entity.Property(e => e.correo).HasMaxLength(150);
            entity.Property(e => e.direccion).HasMaxLength(200);
            entity.Property(e => e.fechaRegistro)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.nombre).HasMaxLength(80);
            entity.Property(e => e.numero_documento).HasMaxLength(20);
            entity.Property(e => e.telefono).HasMaxLength(30);

            entity.HasOne(d => d.idProgramaNavigation).WithMany(p => p.Cliente)
                .HasForeignKey(d => d.idPrograma)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cliente_ProgramaFidelizacion");
        });

        modelBuilder.Entity<Comprobante>(entity =>
        {
            entity.HasKey(e => e.idComprobante);

            entity.HasIndex(e => new { e.serie, e.numero }, "UQ_Comprobante_SerieNumero").IsUnique();

            entity.HasIndex(e => e.idCuenta, "UQ_Comprobante_idCuenta").IsUnique();

            entity.Property(e => e.fechaEmision)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.igv).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.numero).HasMaxLength(15);
            entity.Property(e => e.serie).HasMaxLength(10);
            entity.Property(e => e.subtotal).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.total).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.idCuentaNavigation).WithOne(p => p.Comprobante)
                .HasForeignKey<Comprobante>(d => d.idCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comprobante_Cuenta");

            entity.HasOne(d => d.idTipoComprobanteNavigation).WithMany(p => p.Comprobante)
                .HasForeignKey(d => d.idTipoComprobante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comprobante_TipoComprobante");
        });

        modelBuilder.Entity<Cuenta>(entity =>
        {
            entity.HasKey(e => e.idCuenta);

            entity.Property(e => e.descuento).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.fechaHoraCreacion)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.fechaHoraSalida).HasColumnType("datetime");
            entity.Property(e => e.montoPagado).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.precioEntrada).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.saldo).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.subtotalConsumos).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.total).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.idClienteNavigation).WithMany(p => p.Cuenta)
                .HasForeignKey(d => d.idCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cuenta__idClient__68487DD7");

            entity.HasOne(d => d.idEstadoCuentaNavigation).WithMany(p => p.Cuenta)
                .HasForeignKey(d => d.idEstadoCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cuenta__idEstado__66603565");

            entity.HasOne(d => d.idUsuarioCreadorNavigation).WithMany(p => p.Cuenta)
                .HasForeignKey(d => d.idUsuarioCreador)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cuenta__idUsuari__6754599E");
        });

        modelBuilder.Entity<DetEgreso>(entity =>
        {
            entity.HasKey(e => e.idDetEgreso);

            entity.Property(e => e.comprobanteRuta)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.concepto).HasMaxLength(200);
            entity.Property(e => e.monto).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.idCabEgresoNavigation).WithMany(p => p.DetEgreso)
                .HasForeignKey(d => d.idCabEgreso)
                .HasConstraintName("FK_CabEgreso_CabEgreso");

            entity.HasOne(d => d.idTipoEgresoNavigation).WithMany(p => p.DetEgreso)
                .HasForeignKey(d => d.idTipoEgreso)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Egreso_TipoEgreso");
        });

        modelBuilder.Entity<DetalleConsumo>(entity =>
        {
            entity.HasKey(e => e.idDetalle);

            entity.Property(e => e.precioUnitario).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.subtotal).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.idCuentaNavigation).WithMany(p => p.DetalleConsumo)
                .HasForeignKey(d => d.idCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DetalleCo__idCue__797309D9");

            entity.HasOne(d => d.idProductoNavigation).WithMany(p => p.DetalleConsumo)
                .HasForeignKey(d => d.idProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleConsumo_Producto");
        });

        modelBuilder.Entity<EstadoCuenta>(entity =>
        {
            entity.HasKey(e => e.idEstadoCuenta);

            entity.HasIndex(e => e.nombre, "UQ_EstadoCuenta_nombre").IsUnique();

            entity.Property(e => e.nombre).HasMaxLength(30);
        });

        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.HasKey(e => e.idMetodoPago);

            entity.HasIndex(e => e.nombre, "UQ_MetodoPago_nombre").IsUnique();

            entity.Property(e => e.nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.idMovimiento);

            entity.Property(e => e.costoTotal).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.costoUnitario).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.fecha)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.observaciones).HasMaxLength(300);

            entity.HasOne(d => d.idProductoNavigation).WithMany(p => p.MovimientoInventario)
                .HasForeignKey(d => d.idProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovInv_Producto");

            entity.HasOne(d => d.idTipoMovimientoNavigation).WithMany(p => p.MovimientoInventario)
                .HasForeignKey(d => d.idTipoMovimiento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovInv_TipoMovimiento");

            entity.HasOne(d => d.idUsuarioNavigation).WithMany(p => p.MovimientoInventario)
                .HasForeignKey(d => d.idUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovInv_Usuario");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.idPago);

            entity.Property(e => e.fechaHora)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.monto).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.numeroReferencia).HasMaxLength(100);

            entity.HasOne(d => d.idCuentaNavigation).WithMany(p => p.Pago)
                .HasForeignKey(d => d.idCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_Cuenta");

            entity.HasOne(d => d.idMetodoPagoNavigation).WithMany(p => p.Pago)
                .HasForeignKey(d => d.idMetodoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pago_MetodoPago");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.idProducto);

            entity.HasIndex(e => e.codigo, "UQ_Producto_codigo").IsUnique();

            entity.Property(e => e.activo).HasDefaultValue(true);
            entity.Property(e => e.codigo).HasMaxLength(50);
            entity.Property(e => e.descripcion).HasMaxLength(300);
            entity.Property(e => e.nombre).HasMaxLength(120);
            entity.Property(e => e.precioCompra).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.precioVenta).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.idCategoriaProductoNavigation).WithMany(p => p.Producto)
                .HasForeignKey(d => d.idCategoriaProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Producto_CategoriaProducto");
        });

        modelBuilder.Entity<ProgramaFidelizacion>(entity =>
        {
            entity.HasKey(e => e.idPrograma);

            entity.Property(e => e.montoDescuentoCumpleanos).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.porcentajeDescuento).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.idRol);

            entity.HasIndex(e => e.nombre, "UQ_Rol_nombre").IsUnique();

            entity.Property(e => e.nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TipoComprobante>(entity =>
        {
            entity.HasKey(e => e.idTipoComprobante);

            entity.HasIndex(e => e.nombre, "UQ_TipoComprobante_nombre").IsUnique();

            entity.Property(e => e.nombre).HasMaxLength(30);
        });

        modelBuilder.Entity<TipoEgreso>(entity =>
        {
            entity.HasKey(e => e.idTipoEgreso);

            entity.HasIndex(e => e.nombre, "UQ_TipoEgreso_nombre").IsUnique();

            entity.Property(e => e.nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TipoMovimiento>(entity =>
        {
            entity.HasKey(e => e.idTipoMovimiento);

            entity.Property(e => e.descripcion)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.tipo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("")
                .IsFixedLength();
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.idUsuario);

            entity.HasIndex(e => e.nombreUsuario, "UQ_Usuario_nombreUsuario").IsUnique();

            entity.Property(e => e.activo).HasDefaultValue(true);
            entity.Property(e => e.contraseniaHash).HasMaxLength(200);
            entity.Property(e => e.correo).HasMaxLength(150);
            entity.Property(e => e.fechaCreacion)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.nombreUsuario).HasMaxLength(50);

            entity.HasOne(d => d.idRolNavigation).WithMany(p => p.Usuario)
                .HasForeignKey(d => d.idRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
