using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Infrastructure.Internal
{
    public abstract class ReadOnlyDataProvider : IReadOnlyDataProvider
    {
        #region IReadOnlyDataProvider Members

        public abstract IQueryOfEntities<TEntity> AllOf<TEntity>() where TEntity : class, IQueryableEntity;

        public abstract IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key)
            where TEntity : class, IQueryableEntity;


        public abstract IQueryOverBuilder<TEntity> QueryOver<TEntity>() where TEntity : class, IQueryableEntity;
        public abstract object NativeProvider { get; }

        #endregion

        public abstract void Close();
    }
}