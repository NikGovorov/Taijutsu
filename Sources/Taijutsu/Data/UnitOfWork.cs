// Copyright 2009-2013 Nikita Govorov
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Taijutsu.Data
{
    using System;
    using System.Data;

    using Taijutsu.Data.Internal;
    using Taijutsu.Domain;
    using Taijutsu.Domain.Query;

    [PublicApi]
    public class UnitOfWork : IUnitOfWork, IDisposable, IWrapper
    {
        private readonly IDataContext dataContext;

        private bool disposed;

        private bool? completed;

        public UnitOfWork(string source = "", IsolationLevel? isolation = null, Require require = Require.None)
            : this(new UnitOfWorkConfig(source, isolation ?? IsolationLevel.Unspecified, require))
        {
        }

        public UnitOfWork(IsolationLevel? isolation = null)
            : this(new UnitOfWorkConfig(string.Empty, isolation ?? IsolationLevel.Unspecified, Require.None))
        {
        }

        public UnitOfWork(Require require)
            : this(new UnitOfWorkConfig(string.Empty, IsolationLevel.Unspecified, require))
        {
        }

        public UnitOfWork(string source)
            : this(new UnitOfWorkConfig(source, IsolationLevel.Unspecified, Require.None))
        {
        }

        public UnitOfWork(string source = "", Require require = Require.None)
            : this(new UnitOfWorkConfig(source, IsolationLevel.Unspecified, require))
        {
        }

        public UnitOfWork(string source = "", IsolationLevel? isolation = null)
            : this(new UnitOfWorkConfig(source, isolation ?? IsolationLevel.Unspecified, Require.None))
        {
        }

        public UnitOfWork()
            : this(new UnitOfWorkConfig(string.Empty, IsolationLevel.Unspecified, Require.None))
        {
        }

        public UnitOfWork(UnitOfWorkConfig unitOfWorkConfig)
        {
            if (unitOfWorkConfig == null)
            {
                throw new ArgumentNullException("unitOfWorkConfig");
            }

            this.dataContext = InternalEnvironment.DataContextSupervisor.Register(unitOfWorkConfig);
        }

        object IWrapper.Original
        {
            get
            {
                this.AssertNotDisposed();
                return this.dataContext.Session.Original;
            }
        }

        void IDisposable.Dispose()
        {
            this.Dispose();
        }

        public virtual void Complete()
        {
            if (this.completed.HasValue)
            {
                if (!this.completed.Value)
                {
                    throw new Exception(string.Format("Unit of work has already been completed without success."));
                }

                return;
            }

            try
            {
                this.dataContext.Complete();
                this.completed = true;
            }
            catch
            {
                this.completed = false;
                throw;
            }
        }

        public virtual T Complete<T>(Func<IUnitOfWork, T> toReturn)
        {
            var result = toReturn(this);
            this.Complete();
            return result;
        }

        public virtual T Complete<T>(Func<T> toReturn)
        {
            var result = toReturn();
            this.Complete();
            return result;
        }

        public virtual T Complete<T>(T toReturn)
        {
            this.Complete();
            return toReturn;
        }

        public virtual object MarkAsCreated<TEntity>(TEntity entity, object options = null) where TEntity : IAggregateRoot
        {
            this.AssertNotCompleted();
            return this.dataContext.Session.MarkAsCreated(entity, options);
        }

        public virtual object MarkAsCreated<TEntity>(Func<TEntity> entityFactory, object options = null) where TEntity : IAggregateRoot
        {
            this.AssertNotCompleted();
            return this.dataContext.Session.MarkAsCreated(entityFactory, options);
        }

        public virtual void MarkAsDeleted<TEntity>(TEntity entity, object options = null) where TEntity : IDeletableEntity
        {
            this.AssertNotCompleted();
            this.dataContext.Session.MarkAsDeleted(entity, options);
        }

        public virtual IMarkingStep Mark<TEntity>(TEntity entity, object options = null) where TEntity : IDeletableEntity, IAggregateRoot
        {
            return new MarkingStep<TEntity>(() => MarkAsCreated(entity, options), () => this.MarkAsDeleted(entity, options));
        }

        public virtual IEntitiesQuery<TEntity> All<TEntity>(object options = null) where TEntity : class, IQueryableEntity
        {
            this.AssertNotDisposed();
            return this.dataContext.Session.All<TEntity>(options);
        }

        public virtual IUniqueEntityQuery<TEntity> Unique<TEntity>(object id, object options = null) where TEntity : class, IQueryableEntity
        {
            this.AssertNotDisposed();
            return this.dataContext.Session.Unique<TEntity>(id, options);
        }

        public virtual IQueryOverContinuation<TEntity> Query<TEntity>() where TEntity : class, IQueryableEntity
        {
            this.AssertNotDisposed();
            return new QueryOverContinuation<TEntity>(this.dataContext.Session);
        }

        protected virtual void Dispose()
        {
            try
            {
                if (this.disposed)
                {
                    return;
                }

                try
                {
                    if (!this.completed.HasValue)
                    {
                        this.completed = false;
                    }

                    this.dataContext.Dispose();
                }
                finally
                {
                    InternalEnvironment.CheckDataContextSupervisorForRelease();
                }
            }
            finally
            {
                this.disposed = true;
            }
        }

        protected virtual void AssertNotCompleted()
        {
            if (this.completed.HasValue)
            {
                throw new Exception(string.Format("Unit of work has already been completed(with success - '{0}'), so it is not usable for write anymore.", this.completed));
            }
        }

        protected virtual void AssertNotDisposed()
        {
            if (this.disposed)
            {
                throw new Exception(string.Format("Unit of work has already been disposed(with success - '{0}'), so it is not usable anymore.", this.completed));
            }
        }

        private class QueryOverContinuation<TEntity> : IQueryOverContinuation<TEntity>
            where TEntity : class, IQueryableEntity
        {
            private readonly IOrmSession session;

            public QueryOverContinuation(IOrmSession session)
            {
                this.session = session;
            }

            public TQuery With<TQuery>(string name = null) where TQuery : class, IQuery<TEntity>
            {
                return this.session.QueryWith<TEntity, TQuery>(name);
            }

            public TRepository From<TRepository>(string name = null) where TRepository : class, IRepository<TEntity>
            {
                return this.session.QueryFrom<TEntity, TRepository>(name);
            }
        }
    }
}