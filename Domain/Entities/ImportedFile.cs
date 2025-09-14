using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities;

public class ImportedFile : BaseEntity
{
    public string Title { get; set; }
    public ImportedFileType Type { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }

    public List<ImportedRecord> Records { get; set; }
}

public class ImportedFileConfiguration : IEntityTypeConfiguration<ImportedFile>
{
    public void Configure(EntityTypeBuilder<ImportedFile> builder)
    {
        builder.HasOne(i => i.CreatorUser)
            .WithMany(i => i.CreatedImportedFiles)
            .HasForeignKey(i => i.CreatorUserId);
        builder.HasMany(i => i.Records)
            .WithOne(i => i.ImportedFile)
            .HasForeignKey(i => i.ImportedFileId);

    }
}