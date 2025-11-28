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
    public class UploadedFile : BaseEntity
    {
        public string SavedName { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public UploadedFileType Type { get; set; }
        public bool Enable{ get; set; }
        public string ModelType { get; set; } = string.Empty;
        public long ModelId { get; set; }

    }

    public enum UploadedFileType
    {
        UserProfile,

    }
}

