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
