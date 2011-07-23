using System;
using Taijutsu.Infrastructure.Config;
using Taijutsu.Infrastructure.Internal;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Infrastructure
{
    public class UnitOfQuery : IUnitOfQuery, INativeUnitOf, IDisposable
    {
        private readonly IReadOnlyDataContext dataContext;

        public UnitOfQuery() : this(new UnitOfQueryConfig())
        {
        }

        public UnitOfQuery(UnitOfQueryConfig unitOfQueryConfig)
        {
            dataContext = SupervisorContext.ReadOnlyDataContextSupervisor.RegisterUnitOfQueryBasedOn(unitOfQueryConfig);
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            dataContext.Close();
        }

        #endregion

        #region INativeUnitOf Members

        object INativeUnitOf.Native
        {
            get { return dataContext.ReadOnlyProvider.NativeProvider; }
        }

        #endregion

        #region IUnitOfQuery Members

        public virtual IQueryOfEntities<TEntity> AllOf<TEntity>() where TEntity : class, IQueryableEntity
        {
            return dataContext.ReadOnlyProvider.AllOf<TEntity>();
        }

        public virtual IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key)
            where TEntity : class, IQueryableEntity
        {
            return dataContext.ReadOnlyProvider.UniqueOf<TEntity>(key);
        }

        public virtual IQueryOverBuilder<TEntity> Over<TEntity>() where TEntity : class, IQueryableEntity
        {
            return dataContext.ReadOnlyProvider.QueryOver<TEntity>();
        }

        #endregion
    }
}