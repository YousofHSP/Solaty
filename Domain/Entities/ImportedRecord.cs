using System.ComponentModel.DataAnnotations;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities;

public class ImportedRecord : BaseEntity
{
    public string Title { get; set; }
    public int ImportedFileId { get; set; }
    public ImportedRecordStatus Status { get; set; }
    public string Data { get; set; }
    public ImportedFile ImportedFile { get; set; }
}

public class ImportedRecordConfiguration : IEntityTypeConfiguration<ImportedRecord>
{
    public void Configure(EntityTypeBuilder<ImportedRecord> builder)
    {
        builder.HasOne(i => i.CreatorUser)
            .WithMany(i => i.CreatedImportedRecords)
            .HasForeignKey(i => i.CreatorUserId);
        builder.HasOne(i => i.ImportedFile)
            .WithMany(i => i.Records)
            .HasForeignKey(i => i.ImportedFileId);

    }
}

public enum ImportedFileType
{
}

public enum ImportedRecordStatus
{
    [Display(Name = "پیدا شده")] Founded = 1,
    [Display(Name = "پیدا نشده")] NotFounded,
    [Display(Name = "تطبیق شده")] Adapted,
    [Display(Name = "نادیده گرفته شده")] Ignored,
    [Display(Name = "تداخل")] Conflict,
    [Display(Name = "تکراری")] Duplicate,
    [Display(Name = "اطلاعات ناقص")] Imperfect
}