using System;
using System.Collections.Generic;
using Financiera.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Financiera.Backend.Data;

public partial class FinancieraContext : DbContext
{
    public FinancieraContext()
    {
    }

    public FinancieraContext(DbContextOptions<FinancieraContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Cuenta> Cuentas { get; set; }

    public virtual DbSet<Transaccione> Transacciones { get; set; }

    // --- 🚨 AQUÍ BORRAMOS EL MÉTODO OnConfiguring QUE DABA PROBLEMAS ---

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Esta línea es VITAL para que encuentre las tablas en Neon
        modelBuilder.HasDefaultSchema("financiera"); 

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.ClienteId).HasName("clientes_pkey");

            entity.ToTable("clientes", "financiera");

            entity.HasIndex(e => e.NumeroDocumento, "idx_clientes_documento");

            entity.HasIndex(e => new { e.TipoDocumento, e.NumeroDocumento }, "unq_documento").IsUnique();

            entity.Property(e => e.ClienteId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("cliente_id");
            entity.Property(e => e.Direccion)
                .HasMaxLength(250)
                .HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Estado)
                .HasDefaultValue(true)
                .HasColumnName("estado");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(20)
                .HasColumnName("numero_documento");
            entity.Property(e => e.RazonSocialNombre)
                .HasMaxLength(200)
                .HasColumnName("razon_social_nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(10)
                .HasColumnName("tipo_documento");
        });

        modelBuilder.Entity<Cuenta>(entity =>
        {
            entity.HasKey(e => e.CuentaId).HasName("cuentas_pkey");

            entity.ToTable("cuentas", "financiera");

            entity.HasIndex(e => e.NumeroCuenta, "cuentas_numero_cuenta_key").IsUnique();

            entity.HasIndex(e => e.ClienteId, "idx_cuentas_cliente");

            entity.Property(e => e.CuentaId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("cuenta_id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValueSql("'ACTIVA'::character varying")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Moneda)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("moneda");
            entity.Property(e => e.NumeroCuenta)
                .HasMaxLength(20)
                .HasColumnName("numero_cuenta");
            entity.Property(e => e.Saldo)
                .HasPrecision(20, 4)
                .HasDefaultValueSql("0.0000")
                .HasColumnName("saldo");
            entity.Property(e => e.TipoCuenta)
                .HasMaxLength(20)
                .HasDefaultValueSql("'AHORROS'::character varying")
                .HasColumnName("tipo_cuenta");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Cuenta)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cliente");
        });

        modelBuilder.Entity<Transaccione>(entity =>
        {
            entity.HasKey(e => e.TransaccionId).HasName("transacciones_pkey");

            entity.ToTable("transacciones", "financiera");

            entity.HasIndex(e => e.FechaOperacion, "idx_transacciones_fecha");

            entity.Property(e => e.TransaccionId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("transaccion_id");
            entity.Property(e => e.Canal)
                .HasMaxLength(20)
                .HasDefaultValueSql("'VENTANILLA'::character varying")
                .HasColumnName("canal");
            entity.Property(e => e.CuentaId).HasColumnName("cuenta_id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaOperacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_operacion");
            entity.Property(e => e.Monto)
                .HasPrecision(20, 4)
                .HasColumnName("monto");
            entity.Property(e => e.SaldoHistorico)
                .HasPrecision(20, 4)
                .HasColumnName("saldo_historico");
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(20)
                .HasColumnName("tipo_movimiento");
            entity.Property(e => e.UsuarioResponsable)
                .HasMaxLength(100)
                .HasColumnName("usuario_responsable");

            entity.HasOne(d => d.Cuenta).WithMany(p => p.Transacciones)
                .HasForeignKey(d => d.CuentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cuenta_transaccion");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}