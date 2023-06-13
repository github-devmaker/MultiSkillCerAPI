using System;
using System.Collections.Generic;
using LicenseAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseAPI.Contexts;

public partial class dbIoTFac2 : DbContext
{
    public dbIoTFac2()
    {
    }

    public dbIoTFac2(DbContextOptions<dbIoTFac2> options)
        : base(options)
    {
    }

    public virtual DbSet<BrazingCertDataLog> BrazingCertDataLogs { get; set; }

    public virtual DbSet<BrazingCertDatum> BrazingCertData { get; set; }

    public virtual DbSet<EtdLeakCheck> EtdLeakChecks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=192.168.226.145;Database=dbIoTFac2;TrustServerCertificate=True;uid=sa;password=decjapan");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BrazingCertDataLog>(entity =>
        {
            entity.ToTable("Brazing_Cert_Data_Log");

            entity.HasIndex(e => new { e.EmpCode, e.Pddate }, "IX_Brazing_Cert_Data_Log");

            entity.HasIndex(e => new { e.EmpCode, e.Pddate, e.Pdshift, e.Line }, "NonClusteredIndex-20210611-173326");

            entity.HasIndex(e => new { e.EmpCode, e.BrazingNo, e.Pddate, e.Pdshift, e.Line }, "NonClusteredIndex-20210617-233456");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BrazingNo)
                .HasMaxLength(50)
                .HasColumnName("Brazing_No");
            entity.Property(e => e.CountFg).HasColumnName("CountFG");
            entity.Property(e => e.CountNg).HasColumnName("CountNG");
            entity.Property(e => e.EmpCode).HasMaxLength(50);
            entity.Property(e => e.Expdate)
                .HasColumnType("datetime")
                .HasColumnName("EXPDate");
            entity.Property(e => e.Line).HasMaxLength(50);
            entity.Property(e => e.Pddate)
                .HasColumnType("date")
                .HasColumnName("PDDate");
            entity.Property(e => e.Pdshift)
                .HasMaxLength(50)
                .HasColumnName("PDShift");
            entity.Property(e => e.StationNo).HasMaxLength(5);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<BrazingCertDatum>(entity =>
        {
            entity.HasKey(e => e.Empcode);

            entity.ToTable("Brazing_Cert_Data");

            entity.Property(e => e.Empcode).HasMaxLength(50);
            entity.Property(e => e.BrazingNo)
                .HasMaxLength(50)
                .HasColumnName("Brazing_No");
            entity.Property(e => e.CountFg).HasColumnName("Count_FG");
            entity.Property(e => e.CountNg).HasColumnName("Count_NG");
            entity.Property(e => e.Expdate)
                .HasColumnType("datetime")
                .HasColumnName("EXPDate");
            entity.Property(e => e.LastInsertDate)
                .HasColumnType("datetime")
                .HasColumnName("Last_InsertDate");
            entity.Property(e => e.LeakPointNg)
                .HasMaxLength(50)
                .HasColumnName("LeakPoint_NG");
            entity.Property(e => e.Line).HasMaxLength(50);
            entity.Property(e => e.NgupdateDate)
                .HasColumnType("datetime")
                .HasColumnName("NGUpdateDate");
            entity.Property(e => e.Pddate)
                .HasColumnType("date")
                .HasColumnName("PDDate");
            entity.Property(e => e.Pdshift)
                .HasMaxLength(50)
                .HasColumnName("PDShift");
            entity.Property(e => e.SerialLastUpdate)
                .HasMaxLength(50)
                .HasColumnName("Serial_LastUpdate");
            entity.Property(e => e.SerialNgupdate)
                .HasMaxLength(50)
                .HasColumnName("Serial_NGUpdate");
        });

        modelBuilder.Entity<EtdLeakCheck>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("etd_leak_check", tb =>
                {
                    tb.HasTrigger("Data_Brazing_Cert");
                    tb.HasTrigger("Data_Brazing_Cert_New");
                    tb.HasTrigger("Main_Part_Stock_Out");
                });

            entity.HasIndex(e => new { e.Brazing, e.LineName }, "<Name of Missing Index, sysname,>");

            entity.HasIndex(e => new { e.LineName, e.StampTime }, "NonClusteredIndex-20200921-161500");

            entity.HasIndex(e => new { e.SerialNo, e.StampTime, e.LineName }, "NonClusteredIndex-20210426-104700");

            entity.HasIndex(e => e.StampTime, "NonClusteredIndex-20211216-094839");

            entity.Property(e => e.Brazing).HasMaxLength(50);
            entity.Property(e => e.EmpCode).HasMaxLength(50);
            entity.Property(e => e.LineName).HasMaxLength(50);
            entity.Property(e => e.SerialNo).HasMaxLength(50);
            entity.Property(e => e.StampTime).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
