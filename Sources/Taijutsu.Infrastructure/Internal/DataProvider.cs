using System.Data;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Infrastructure.Internal
{
    public abstract class DataProvider : IDataProvider
    {
        #region IDataProvider Members

        public abstract object MarkAsCreated<TEntity>(TEntity entity) where TEntity : IAggregateRoot;
        public abstract void MarkAsRemoved<TEntity>(TEntity entity) where TEntity : IRemovableEntity;
        public abstract IQueryOfEntities<TEntity> AllOf<TEntity>() where TEntity : class, IQueryableEntity;

        public abstract IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key)
            where TEntity : class, IQueryableEntity;

        public abstract IQueryOverBuilder<TEntity> QueryOver<TEntity>() where TEntity : class, IQueryableEntity;
        public abstract object NativeProvider { get; }

        #endregion

        public abstract void Close();
        public abstract void BeginTransaction(IsolationLevel level);
        public abstract void Commit();
        public abstract void Rollback();

        public abstract IMarkingStep<TEntity> Mark<TEntity>(TEntity entity)
            where TEntity : IRemovableEntity, IAggregateRoot;
    }
}