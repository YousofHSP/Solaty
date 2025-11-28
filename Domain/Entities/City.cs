using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities
{
    public class City : IEntity<long>
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public long? ProvinceId { get; set; }


        public City? Province { get; set; } = null!;
        public List<City> Cities { get; set; } = [];
    }
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.Property(c => c.Title).HasMaxLength(100);
            builder
                .HasOne(c => c.Province)
                .WithMany(p => p.Cities)
                .HasForeignKey(c => c.ProvinceId);
        }
    }
}
