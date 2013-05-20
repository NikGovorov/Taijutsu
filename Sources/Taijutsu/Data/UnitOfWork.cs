#region License

//  Copyright 2009-2013 Nikita Govorov
//    
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
//  this file except in compliance with the License. You may obtain a copy of the 
//  License at 
//   
//  http://www.apache.org/licenses/LICENSE-2.0 
//   
//  Unless required by applicable law or agreed to in writing, software distributed 
//  under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
//  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
//  specific language governing permissions and limitations under the License.

#endregion

using System;
using System.Data;
using Taijutsu.Data.Internal;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable, IWrapper
    {
        private bool disposed;
        private readonly IDataContext dataContext;
        private bool? completed;

        public UnitOfWork(string source = "", IsolationLevel? isolation = null,
                          Require require = Require.None)
            : this(new UnitOfWorkConfig(source, isolation ?? IsolationLevel.Unspecified, require))
        {
        }

        public UnitOfWork(IsolationLevel? isolation = null)
            : this(new UnitOfWorkConfig("", isolation ?? IsolationLevel.Unspecified, Require.None))
        {
        }

        public UnitOfWork(Require require)
            : this(new UnitOfWorkConfig("", IsolationLevel.Unspecified, require))
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
            : this(new UnitOfWorkConfig("", IsolationLevel.Unspecified, Require.None))
        {
        }

        public UnitOfWork(UnitOfWorkConfig unitOfWorkConfig)
        {
            if (unitOfWorkConfig == null) throw new ArgumentNullException("unitOfWorkConfig");

            dataContext = InternalEnvironment.DataContextSupervisor.Register(unitOfWorkConfig);
        }

        void IDisposable.Dispose()
        {
            Dispose();

        }

        protected virtual void Dispose()
        {
            try
            {
                if (!disposed)
                {
                    try
                    {
                        if (!completed.HasValue)
                        {
                            completed = false;
                        }

                        dataContext.Dispose();
                    }
                    finally
                    {
                        InternalEnvironment.CheckDataContextSupervisorForRelease();
                    }
                }
            }
            finally
            {
                disposed = true;
            }
        }

        protected virtual void AssertNotCompleted()
        {
            if (completed.HasValue)
            {
                throw new Exception(string.Format("Unit of work has already been completed(with success - '{0}'), so it is not usable for write anymore.", completed));    
            }
        }

        protected virtual void AssertNotDisposed()
        {
            if (disposed)
            {
                throw new Exception(string.Format("Unit of work has already been disposed(with success - '{0}'), so it is not usable anymore.", completed));
            }
        }

        public virtual void Complete()
        {
            if (completed.HasValue)
            {
                if (!completed.Value)
                {
                    throw new Exception(string.Format("Unit of work has already been completed without success."));
                }
                return;
            }
            try
            {
                dataContext.Complete();
                completed = true;
            }
            catch
            {
                completed = false;
                throw;
            }
        }

        public virtual T Complete<T>(Func<IUnitOfWork, T> toReturn)
        {
            var result = toReturn(this);
            Complete();
            return result;
        }

        public virtual T Complete<T>(Func<T> toReturn)
        {
            var result = toReturn();
            Complete();
            return result;
        }

        public virtual T Complete<T>(T toReturn)
        {
            Complete();
            return toReturn;
        }

        public virtual object MarkAsCreated<TEntity>(TEntity entity, object options = null) where TEntity : IAggregateRoot
        {
            AssertNotCompleted();
            return dataContext.Session.MarkAsCreated(entity, options);
        }

        public virtual object MarkAsCreated<TEntity>(Func<TEntity> entityFactory, object options = null) where TEntity : IAggregateRoot
        {
            AssertNotCompleted();
            return dataContext.Session.MarkAsCreated(entityFactory, options);
        }

        public virtual void MarkAsDeleted<TEntity>(TEntity entity, object options = null) where TEntity : IDeletableEntity
        {
            AssertNotCompleted();
            dataContext.Session.MarkAsDeleted(entity, options);
        }

        public virtual IMarkingStep Mark<TEntity>(TEntity entity, object options = null) where TEntity : IDeletableEntity, IAggregateRoot
        {
            return new MarkingStep<TEntity>(() => MarkAsCreated(entity, options), () => MarkAsDeleted(entity, options));
        }

        public virtual IEntitiesQuery<TEntity> All<TEntity>(object options = null) where TEntity : class, IQueryableEntity
        {
            AssertNotDisposed();
            return dataContext.Session.All<TEntity>(options);
        }

        public virtual IUniqueEntityQuery<TEntity> Unique<TEntity>(object id, object options = null) where TEntity : class, IQueryableEntity
        {
            AssertNotDisposed();
            return dataContext.Session.Unique<TEntity>(id, options);
        }

        public virtual IQueryOverContinuation<TEntity> Query<TEntity>() where TEntity : class, IQueryableEntity
        {
            AssertNotDisposed();
            return new QueryOverContinuation<TEntity>(dataContext.Session);
        }

        object IWrapper.Original
        {
            get
            {
                AssertNotDisposed();
                return dataContext.Session.Original;
            }
        }

        class QueryOverContinuation<TEntity> : IQueryOverContinuation<TEntity> where TEntity : class, IQueryableEntity
        {
            private readonly IOrmSession session;

            public QueryOverContinuation(IOrmSession session)
            {
                this.session = session;
            }

            public TQuery With<TQuery>(string name = null) where TQuery : class, IQuery<TEntity>
            {
                return session.QueryWith<TEntity, TQuery>(name);
            }

            public TRepository From<TRepository>(string name = null) where TRepository : class, IRepository<TEntity>
            {
                return session.QueryFrom<TEntity, TRepository>(name);
            }
        }
    }
}