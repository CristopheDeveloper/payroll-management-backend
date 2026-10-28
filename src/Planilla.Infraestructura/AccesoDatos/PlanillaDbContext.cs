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

    public virtual DbSet<Asistencia> Asistencia { get; set; }

    public virtual DbSet<Bitacora> Bitacora { get; set; }

    public virtual DbSet<Departamento> Departamento { get; set; }

    public virtual DbSet<Empleado> Empleado { get; set; }

    public virtual DbSet<EmpleadoFoto> EmpleadoFoto { get; set; }

    public virtual DbSet<Nomina> Nomina { get; set; }

    public virtual DbSet<NominaEmpleado> NominaEmpleado { get; set; }

    public virtual DbSet<NominaRubro> NominaRubro { get; set; }

    public virtual DbSet<NominaValor> NominaValor { get; set; }

    public virtual DbSet<Parametro> Parametro { get; set; }

    public virtual DbSet<PeriodoLaboral> PeriodoLaboral { get; set; }

    public virtual DbSet<Puesto> Puesto { get; set; }

    public virtual DbSet<Rol> Rol { get; set; }

    public virtual DbSet<Rubro> Rubro { get; set; }

    public virtual DbSet<TipoPlanilla> TipoPlanilla { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asistencia>(entity =>
        {
            entity.Property(e => e.Estado).HasDefaultValue("PRESENTE", "DF_Asistencia_Estado");
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_Asistencia_FechaIng");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Asistencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asistencia_Empleado");
        });

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

        modelBuilder.Entity<EmpleadoFoto>(entity =>
        {
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_EmpleadoFoto_FechaIng");
            entity.HasOne(d => d.Empleado).WithOne(p => p.Foto)
                .HasForeignKey<EmpleadoFoto>(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_EmpleadoFoto_Empleado");
        });

        modelBuilder.Entity<Nomina>(entity =>
        {
            entity.Property(e => e.Estado).HasDefaultValue("CREADA", "DF_Nomina_Estado");
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_Nomina_FechaIng");
            entity.Property(e => e.Version)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.IdTipoPlanillaNavigation).WithMany(p => p.Nomina)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Nomina_TipoPlanilla");
        });

        modelBuilder.Entity<NominaEmpleado>(entity =>
        {
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_NominaEmpleado_FechaIng");

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.NominaEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NominaEmpleado_Departamento");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.NominaEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NominaEmpleado_Empleado");

            entity.HasOne(d => d.IdNominaNavigation).WithMany(p => p.NominaEmpleado).HasConstraintName("FK_NominaEmpleado_Nomina");

            entity.HasOne(d => d.IdPuestoNavigation).WithMany(p => p.NominaEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NominaEmpleado_Puesto");
        });

        modelBuilder.Entity<NominaRubro>(entity =>
        {
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_NominaRubro_FechaIng");

            entity.HasOne(d => d.IdNominaEmpleadoNavigation).WithMany(p => p.NominaRubro).HasConstraintName("FK_NominaRubro_NominaEmpleado");

            entity.HasOne(d => d.IdRubroNavigation).WithMany(p => p.NominaRubro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NominaRubro_Rubro");
        });

        modelBuilder.Entity<NominaValor>(entity =>
        {
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_NominaValor_FechaIng");

            entity.HasOne(d => d.IdNominaEmpleadoNavigation).WithMany(p => p.NominaValor).HasConstraintName("FK_NominaValor_NominaEmpleado");

            entity.HasOne(d => d.IdRubroNavigation).WithMany(p => p.NominaValor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NominaValor_Rubro");
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

        modelBuilder.Entity<Rubro>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_Rubro_Activo");
            entity.Property(e => e.CuentaContable).IsFixedLength();
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_Rubro_FechaIng");
            entity.Property(e => e.Origen).HasDefaultValue("MANUAL", "DF_Rubro_Origen");
        });

        modelBuilder.Entity<TipoPlanilla>(entity =>
        {
            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_TipoPlanilla_Activo");
            entity.Property(e => e.FechaIng).HasDefaultValueSql("(sysdatetime())", "DF_TipoPlanilla_FechaIng");
            entity.Property(e => e.IncluyeIgss).HasDefaultValue(true, "DF_TipoPlanilla_IncluyeIgss");
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
