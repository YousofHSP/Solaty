using System.Linq.Expressions;
using Common.Utilities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Domain.Entities;
using Domain.Entities.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {

        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("");
        //    base.OnConfiguring(optionsBuilder);
        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("rad");

            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

            var entitiesAssembly = typeof(IEntity).Assembly;
            modelBuilder.RegisterAllEntities<IEntity>(entitiesAssembly);
            modelBuilder.RegisterEntityTypeConfiguration(entitiesAssembly);
            modelBuilder.AddRestrictDeleteBehaviorConvention();
            modelBuilder.AddSequentialGuidForIdConverntion();
            modelBuilder.AddPluralizingTableNameConvention();
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    entityType.SetQueryFilter(CreateSoftDeleteFilter(entityType.ClrType));
                }
            }
        }

        private static LambdaExpression CreateSoftDeleteFilter(Type entityType)
        {
            var parameter = Expression.Parameter(entityType, "e");
            var property = Expression.Property(parameter, nameof(ISoftDelete.DeleteDate));
            var nullValue = Expression.Constant(null, typeof(DateTime?));
            var condition = Expression.Equal(property, nullValue);

            return Expression.Lambda(condition, parameter);
        }

        public override int SaveChanges()
        {
            _beforeSave();
            var result = base.SaveChanges();
            return result;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            _beforeSave();
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            return result;
        }

        // public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        //     CancellationToken cancellationToken = default)
        // {
        //     _beforeSave();
        //     var result = base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        //     _afterSave();
        //     _auditApplied = false;
        //     return result;
        // }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _beforeSave();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void _beforeSave()
        {
            _cleanString();
        }


        private void _softDelete()
        {
            foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
            {
                if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete softDeleteEntity) // از حذف فیزیکی جلوگیری می‌کنیم
                {
                    if (entry.Entity is Audit)
                        continue;

                    entry.State = EntityState.Modified;
                    softDeleteEntity.DeleteDate = DateTimeOffset.Now;
                }
            }
        }

        private void _cleanString()
        {
            var changedEntities = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);
            foreach (var item in changedEntities)
            {
                if (item.Entity == null)
                    continue;

                var properties = item.Entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(string));
                foreach (var property in properties)
                {
                    var propName = property.Name;
                    var val = (string)property.GetValue(item.Entity, null);

                    if (val.HasValue())
                    {
                        var newVal = val.Fa2En().FixPersianChars();
                        if (newVal == val)
                            continue;
                        property.SetValue(item.Entity, newVal, null);
                    }
                }
            }
        }
    }
}