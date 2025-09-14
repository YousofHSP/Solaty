using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities
{
    public class UploadedFile : BaseEntity, IHashedEntity
    {
        public string SavedName { get; set; }
        public string OriginalName { get; set; }
        public string MimeType { get; set; }
        public UploadedFileType Type { get; set; }
        public bool Enable{ get; set; }
        public string ModelType { get; set; }
        public int ModelId { get; set; }
        public string Hash { get; set; }
        public string SaltCode { get; set; }

    }

    public class UploadedFileConfiguration : IEntityTypeConfiguration<UploadedFile>
    {
        public void Configure(EntityTypeBuilder<UploadedFile> builder)
        {
            builder
                .HasOne(u => u.CreatorUser)
                .WithMany(u => u.CreatedUploadedFiles)
                .HasForeignKey(u => u.CreatorUserId);
        }
    }

    public enum UploadedFileType
    {
        UserProfile,

    }
}

