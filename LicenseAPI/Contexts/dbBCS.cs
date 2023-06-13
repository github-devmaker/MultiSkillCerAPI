using System;
using System.Collections.Generic;
using LicenseAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseAPI.Contexts;

public partial class dbBCS : DbContext
{
    public dbBCS()
    {
    }

    public dbBCS(DbContextOptions<dbBCS> options)
        : base(options)
    {
    }

    public virtual DbSet<PositMstr> PositMstrs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=192.168.226.86;Database=dbBCS;TrustServerCertificate=True;uid=sa;password=decjapan");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Thai_CI_AS");

        modelBuilder.Entity<PositMstr>(entity =>
        {
            entity.HasKey(e => e.PositId).HasName("PK_MP_POSIT");

            entity.ToTable("POSIT_Mstr");

            entity.HasIndex(e => new { e.RangeOrder, e.PositId, e.PositName }, "IX_POSIT_Mstr");

            entity.Property(e => e.PositId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Posit_Id");
            entity.Property(e => e.AddBy)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("add_by");
            entity.Property(e => e.AddDt)
                .HasColumnType("datetime")
                .HasColumnName("add_dt");
            entity.Property(e => e.Comtype)
                .HasMaxLength(50)
                .HasColumnName("COMTYPE");
            entity.Property(e => e.EnableBgtype)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Enable_BGType");
            entity.Property(e => e.EnableOt1)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("Enable_OT1");
            entity.Property(e => e.EnableOt15)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("Enable_OT15");
            entity.Property(e => e.EnableOt2)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("Enable_OT2");
            entity.Property(e => e.EnableOt3)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("Enable_OT3");
            entity.Property(e => e.PositName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Posit_Name");
            entity.Property(e => e.RangeOrder).HasColumnName("RANGE_ORDER");
            entity.Property(e => e.Remark).HasColumnType("text");
            entity.Property(e => e.SalAvg)
                .HasColumnType("numeric(18, 0)")
                .HasColumnName("SAL_AVG");
            entity.Property(e => e.UpdBy)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("upd_by");
            entity.Property(e => e.UpdDt)
                .HasColumnType("datetime")
                .HasColumnName("upd_dt");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
