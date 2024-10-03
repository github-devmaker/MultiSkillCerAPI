using System;
using System.Collections.Generic;
using LicenseAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseAPI.Contexts;

public partial class dbSCM : DbContext
{
    public dbSCM()
    {
    }

    public dbSCM(DbContextOptions<dbSCM> options)
        : base(options)
    {
    }

    public virtual DbSet<SkcCheckInOutLog> SkcCheckInOutLogs { get; set; }

    public virtual DbSet<SkcDictMstr> SkcDictMstrs { get; set; }

    public virtual DbSet<SkcLicenseTraining> SkcLicenseTrainings { get; set; }

    public virtual DbSet<SkcPrivilege> SkcPrivileges { get; set; }

    public virtual DbSet<SkcUserInRole> SkcUserInRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=192.168.226.86;Database=dbSCM;TrustServerCertificate=True;uid=sa;password=decjapan");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Thai_CI_AS");

        modelBuilder.Entity<SkcCheckInOutLog>(entity =>
        {
            entity.HasKey(e => e.ChkId);

            entity.ToTable("SKC_CheckInOutLog");

            entity.Property(e => e.ChkId).HasColumnName("CHK_ID");
            entity.Property(e => e.ChkDate)
                .HasColumnType("datetime")
                .HasColumnName("CHK_DATE");
            entity.Property(e => e.ChkEmpcode)
                .HasMaxLength(50)
                .HasColumnName("CHK_EMPCODE");
            entity.Property(e => e.ChkState)
                .HasMaxLength(50)
                .HasComment("in or out")
                .HasColumnName("CHK_STATE");
            entity.Property(e => e.DictCode)
                .HasMaxLength(50)
                .HasColumnName("DICT_CODE");
        });

        modelBuilder.Entity<SkcDictMstr>(entity =>
        {
            entity.HasKey(e => e.DictId);

            entity.ToTable("SKC_DictMstr");

            entity.HasIndex(e => e.DictType, "IX_SKC_DictMstr").IsDescending();

            entity.HasIndex(e => new { e.DictType, e.Code }, "IX_SKC_DictMstr_1").IsDescending();

            entity.HasIndex(e => new { e.DictType, e.Code, e.RefCode }, "IX_SKC_DictMstr_2");

            entity.Property(e => e.DictId).HasColumnName("DICT_ID");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CODE");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DATE");
            entity.Property(e => e.DictDesc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DICT_DESC");
            entity.Property(e => e.DictStatus).HasColumnName("DICT_STATUS");
            entity.Property(e => e.DictType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DICT_TYPE");
            entity.Property(e => e.Note)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NOTE");
            entity.Property(e => e.RefCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("REF_CODE");
            entity.Property(e => e.RefItem)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("REF_ITEM");
            entity.Property(e => e.UpdateDate)
                .HasColumnType("datetime")
                .HasColumnName("UPDATE_DATE");
        });

        modelBuilder.Entity<SkcLicenseTraining>(entity =>
        {
            entity.HasKey(e => e.TrId).HasName("PK_SKC_LicenseTraning");

            entity.ToTable("SKC_LicenseTraining");

            entity.Property(e => e.TrId).HasColumnName("TR_ID");
            entity.Property(e => e.AlertDate)
                .HasColumnType("datetime")
                .HasColumnName("ALERT_DATE");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(30)
                .HasColumnName("CREATE_BY");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DATE");
            entity.Property(e => e.DictCode)
                .HasMaxLength(30)
                .HasColumnName("DICT_CODE");
            entity.Property(e => e.EffectiveDate)
                .HasColumnType("datetime")
                .HasColumnName("EFFECTIVE_DATE");
            entity.Property(e => e.Empcode)
                .HasMaxLength(10)
                .HasColumnName("EMPCODE");
            entity.Property(e => e.ExpiredDate)
                .HasColumnType("datetime")
                .HasColumnName("EXPIRED_DATE");
            entity.Property(e => e.RefCode)
                .HasMaxLength(10)
                .HasColumnName("REF_CODE");
            entity.Property(e => e.TrStatus).HasColumnName("TR_STATUS");
        });

        modelBuilder.Entity<SkcPrivilege>(entity =>
        {
            entity.HasKey(e => new { e.PriRole, e.PriProgram }).HasName("PK_SKC_Privilege_1");

            entity.ToTable("SKC_Privilege");

            entity.Property(e => e.PriRole)
                .HasMaxLength(10)
                .HasColumnName("PRI_ROLE");
            entity.Property(e => e.PriProgram)
                .HasMaxLength(50)
                .HasColumnName("PRI_PROGRAM");
            entity.Property(e => e.PriAdd)
                .HasMaxLength(1)
                .HasColumnName("PRI_ADD");
            entity.Property(e => e.PriCreateBy)
                .HasMaxLength(20)
                .HasColumnName("PRI_CREATE_BY");
            entity.Property(e => e.PriCreateDate)
                .HasColumnType("datetime")
                .HasColumnName("PRI_CREATE_DATE");
            entity.Property(e => e.PriDelete)
                .HasMaxLength(1)
                .HasColumnName("PRI_DELETE");
            entity.Property(e => e.PriModify)
                .HasMaxLength(1)
                .HasColumnName("PRI_MODIFY");
            entity.Property(e => e.PriSearch)
                .HasMaxLength(1)
                .HasColumnName("PRI_SEARCH");
        });

        modelBuilder.Entity<SkcUserInRole>(entity =>
        {
            entity.HasKey(e => new { e.PriRole, e.PriEmpcode });

            entity.ToTable("SKC_UserInRole");

            entity.Property(e => e.PriRole)
                .HasMaxLength(50)
                .HasColumnName("PRI_ROLE");
            entity.Property(e => e.PriEmpcode)
                .HasMaxLength(20)
                .HasColumnName("PRI_EMPCODE");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("CREATE_DATE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
