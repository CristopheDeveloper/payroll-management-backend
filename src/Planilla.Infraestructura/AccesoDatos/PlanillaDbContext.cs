using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Planilla.Dominio.Entidades;

namespace Planilla.Infraestructura.AccesoDatos;

public partial class PlanillaDbContext : DbContext
{
    public PlanillaDbContext(DbContextOptions<PlanillaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bitacora> Bitacora { get; set; }

    public virtual DbSet<Departamento> Departamento { get; set; }

    public virtual DbSet<Empleado> Empleado { get; set; }

    public virtual DbSet<Parametro> Parametro { get; set; }

    public virtual DbSet<PeriodoLaboral> PeriodoLaboral { get; set; }

    public virtual DbSet<Puesto> Puesto { get; set; }

    public virtual DbSet<Rol> Rol { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bitacora>(entity =>
        {
            entity.Property(e => e.Fecha).HasDefaultValueSql("(sysdatetime())", "DF_Bitacora_Fecha");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Departamento_Activo");
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_Departamento_FechaIng");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Empleado_Activo");
            entity.Property(e => e.CodigoEmpleado).HasComputedColumnSql("(CONVERT([nvarchar](15),'EMP-'+case when [IdEmpleado]<(10000) then right('0000'+CONVERT([varchar](10),[IdEmpleado]),(4)) else CONVERT([varchar](10),[IdEmpleado]) end))", true);
            entity.Property(e => e.Dpi).IsFixedLength();
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_Empleado_FechaIng");
            entity.Property(e => e.Genero).IsFixedLength();
            entity.Property(e => e.TipoPago).HasDefaultValue("TRANSFERENCIA", "DF_Empleado_TipoPago");

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Empleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Empleado_Departamento");

            entity.HasOne(d => d.IdPuestoNavigation).WithMany(p => p.Empleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Empleado_Puesto");
        });

        modelBuilder.Entity<Parametro>(entity =>
        {
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_Parametro_FechaIng");
        });

        modelBuilder.Entity<PeriodoLaboral>(entity =>
        {
            entity.HasIndex(e => new { e.IdEmpleado, e.FechaBaja }, "UX_PeriodoLaboral_Abierto")
                .IsUnique()
                .HasFilter("([FechaBaja] IS NULL)");

            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_PeriodoLaboral_FechaIng");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.PeriodoLaboral)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PeriodoLaboral_Empleado");
        });

        modelBuilder.Entity<Puesto>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Puesto_Activo");
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_Puesto_FechaIng");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Rol_Activo");
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_Rol_FechaIng");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Usuario_Activo");
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_Usuario_FechaIng");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
