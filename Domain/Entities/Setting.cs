using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities
{
    public class Setting : IEntity<long>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int IsSecurity { get; set; }
        public bool Enable{ get; set; }

        public string? Description { get; set; }
    }

}
