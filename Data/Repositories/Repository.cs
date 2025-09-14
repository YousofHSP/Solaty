using Common.Utilities;
using Data.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Domain.Entities.Common;
using Microsoft.AspNetCore.Http;

namespace Data.Reprositories
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity<int>
    {
        protected readonly ApplicationDbContext dbContext;
        public DbSet<TEntity> Entities { get; }
        public virtual IQueryable<TEntity> Table => Entities.AsTracking();
        public virtual IQueryable<TEntity> TableNoTracking => Entities.AsNoTracking();

        public Repository(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            Entities = dbContext.Set<TEntity>(); // City => Cities
        }

        #region Async Method

        public virtual async Task<TEntity?> GetByIdAsync(CancellationToken cancellationToken, params object[] ids)
        {
            return await Entities.FindAsync(ids, cancellationToken);
        }

        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            await Entities.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            if (saveNow)
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken,
            bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            await Entities.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            if (saveNow)
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Update(entity);
            if (saveNow)
                await dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken,
            bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.UpdateRange(entities);
            if (saveNow)
                await dbContext.SaveChangesAsync(cancellationToken);
        }


        public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Remove(entity);
            if (saveNow)
                await dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken,
            bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.RemoveRange(entities);
            if (saveNow)
                await dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Sync Methods

        public virtual TEntity GetById(params object[] ids)
        {
            return Entities.Find(ids);
        }

        public virtual void Add(TEntity entity, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Add(entity);
            if (saveNow)
                dbContext.SaveChanges();
        }

        public virtual void AddRange(IEnumerable<TEntity> entities, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.AddRange(entities);
            if (saveNow)
                dbContext.SaveChanges();
        }

        public virtual void Update(TEntity entity, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Update(entity);
            dbContext.SaveChanges();
        }

        public virtual void UpdateRange(IEnumerable<TEntity> entities, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.UpdateRange(entities);
            if (saveNow)
                dbContext.SaveChanges();
        }

        public virtual void Delete(TEntity entity, bool saveNow = true)
        {
            Assert.NotNull(entity, nameof(entity));
            Entities.Remove(entity);
            if (saveNow)
                dbContext.SaveChanges();
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entities, bool saveNow = true)
        {
            Assert.NotNull(entities, nameof(entities));
            Entities.RemoveRange(entities);
            if (saveNow)
                dbContext.SaveChanges();
        }

        #endregion

        #region Attach & Detach

        public virtual void Detach(TEntity entity)
        {
            Assert.NotNull(entity, nameof(entity));
            var entry = dbContext.Entry(entity);
            if (entry != null)
                entry.State = EntityState.Detached;
        }

        public virtual void Attach(TEntity entity)
        {
            Assert.NotNull(entity, nameof(entity));
            if (dbContext.Entry(entity).State == EntityState.Detached)
                Entities.Attach(entity);
        }

        #endregion

        #region Explicit Loading

        public virtual async Task LoadCollectionAsync<TProperty>(TEntity entity,
            Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty, CancellationToken cancellationToken)
            where TProperty : class
        {
            Attach(entity);

            var collection = dbContext.Entry(entity).Collection(collectionProperty);
            if (!collection.IsLoaded)
                await collection.LoadAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual void LoadCollection<TProperty>(TEntity entity,
            Expression<Func<TEntity, IEnumerable<TProperty>>> collectionProperty)
            where TProperty : class
        {
            Attach(entity);
            var collection = dbContext.Entry(entity).Collection(collectionProperty);
            if (!collection.IsLoaded)
                collection.Load();
        }

        public virtual async Task LoadReferenceAsync<TProperty>(TEntity entity,
            Expression<Func<TEntity, TProperty>> referenceProperty, CancellationToken cancellationToken)
            where TProperty : class
        {
            Attach(entity);
            var reference = dbContext.Entry(entity).Reference(referenceProperty);
            if (!reference.IsLoaded)
                await reference.LoadAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual void LoadReference<TProperty>(TEntity entity,
            Expression<Func<TEntity, TProperty>> referenceProperty)
            where TProperty : class
        {
            Attach(entity);
            var reference = dbContext.Entry(entity).Reference(referenceProperty);
            if (!reference.IsLoaded)
                reference.Load();
        }

        #endregion

    }
}