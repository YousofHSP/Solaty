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
    public class Province : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public List<City> Cities { get; set; }
    }

    public class ProvinceConfiguration : IEntityTypeConfiguration<Province>
    {
        public void Configure(EntityTypeBuilder<Province> builder)
        {
            builder.Property(p => p.Title).HasMaxLength(100);
            builder
                .HasMany(p => p.Cities)
                .WithOne(c => c.Province)
                .HasForeignKey(c => c.ProvinceId);
        }
    }
}
