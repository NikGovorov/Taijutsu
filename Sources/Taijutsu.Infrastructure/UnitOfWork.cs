using System;
using System.Collections.Generic;
using Taijutsu.Infrastructure.Config;
using Taijutsu.Infrastructure.Internal;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IConceitedUnitOfWork, IAdvancedUnitOfWork, INativeUnitOf, IDisposable
    {
        private readonly IDataContext dataContext;

        private bool completed;

        public UnitOfWork() : this(new UnitOfWorkConfig())
        {
        }

        public UnitOfWork(UnitOfWorkConfig unitOfWorkConfig)
        {
            dataContext = SupervisorContext.DataContextSupervisor.RegisterUnitOfWorkBasedOn(unitOfWorkConfig);
        }

        #region IAdvancedUnitOfWork Members

        public virtual IUnitOfWorkLifeCycle Advanced
        {
            get { return dataContext.Advanced; }
        }

        IDictionary<string, IDisposable> IAdvancedUnitOfWork.Extension
        {
            get { return dataContext.Extension; }
        }

        #endregion

        #region IConceitedUnitOfWork Members

        IUnitOfWorkCompletion IConceitedUnitOfWork.AsCompleted()
        {
            completed = true;
            return new UnitOfWorkCompletion();
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            try
            {
                if (completed)
                {
                    dataContext.Commit();
                }
                else
                {
                    dataContext.Rollback();
                }
            }
            finally
            {
                dataContext.Close();
            }
        }

        #endregion

        #region INativeUnitOf Members

        object INativeUnitOf.Native
        {
            get { return dataContext.Provider.NativeProvider; }
        }

        #endregion

        #region IUnitOfWork Members

        public object MarkAsCreated<TEntity>(TEntity entity) where TEntity : IAggregateRoot
        {
            return dataContext.Provider.MarkAsCreated(entity);
        }

        public void MarkAsRemoved<TEntity>(TEntity entity) where TEntity : IRemovableEntity
        {
            dataContext.Provider.MarkAsRemoved(entity);
        }

        public IMarkingStep<TEntity> Mark<TEntity>(TEntity entity) where TEntity : IRemovableEntity, IAggregateRoot
        {
            return new MarkingStep<TEntity>(() => MarkAsCreated(entity), () => MarkAsRemoved(entity));
        }

        public virtual IQueryOfEntities<TEntity> AllOf<TEntity>() where TEntity : class, IQueryableEntity
        {
            return dataContext.Provider.AllOf<TEntity>();
        }

        public virtual IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key)
            where TEntity : class, IQueryableEntity
        {
            return dataContext.Provider.UniqueOf<TEntity>(key);
        }

        public virtual IQueryOverBuilder<TEntity> Over<TEntity>() where TEntity : class, IQueryableEntity
        {
            return dataContext.Provider.QueryOver<TEntity>();
        }

        #endregion

        public virtual IUnitOfWorkCompletion AsCompleted()
        {
            if (!dataContext.Completed)
            {
                throw new Exception(
                    "Current unit of work can not be successfully complete, because not all subordinate units of work successfully are completed.");
            }
            completed = true;
            return new UnitOfWorkCompletion();
        }

        #region Nested type: UnitOfWorkCompletion

        private class UnitOfWorkCompletion : IUnitOfWorkCompletion
        {
        }

        #endregion
    }
}