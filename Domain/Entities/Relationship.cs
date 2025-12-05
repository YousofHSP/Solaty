using System.ComponentModel.DataAnnotations;
using Domain.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Entities;

public class Relationship : IBaseEntity<long>
{
    public DateTimeOffset CreateDate { get; set; }
    public long Id { get; set; }
    public long MaleUserInfoId { get; set; }
    public long FemaleUserInfoId { get; set; }
    public bool Enable { get; set; }

    public DateTimeOffset UpdateDate { get; set; }
    public RelationshipStatus Status { get; set; }
    
    #region Rels

    public UserInfo MaleUserInfo { get; set; } = null!;
    public UserInfo FemaleUserInfo { get; set; } = null!;

    #endregion

}

public class RelationshipConfiguration : IEntityTypeConfiguration<Relationship>
{
    public void Configure(EntityTypeBuilder<Relationship> builder)
    {
        builder.HasOne(r => r.MaleUserInfo)
            .WithMany(u => u.MaleRelationships)
            .HasForeignKey(r => r.MaleUserInfoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.FemaleUserInfo)
            .WithMany(u => u.FemaleRelationships)
            .HasForeignKey(r => r.FemaleUserInfoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public enum RelationshipStatus
{
    [Display(Name = "در انتظار تایید")]
    Pending,
    [Display(Name = "وصال")]
    Connected,
    [Display(Name = "قهر")]
    Quarrel, 
    [Display(Name = "آشتی")]
    Reconciled, 
    [Display(Name = "جدا")]
    Blocked
}