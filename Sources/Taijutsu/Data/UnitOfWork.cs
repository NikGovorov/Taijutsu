// Copyright 2009-2014 Nikita Govorov
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

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;

using Taijutsu.Annotation;
using Taijutsu.Data.Internal;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data
{
    [PublicApi]
    public class UnitOfWork : IUnitOfWork, IWrapper
    {
        private readonly IDataContext dataContext;

        private bool disposed;

        private bool? completed;

        public UnitOfWork([NotNull] string source = "", IsolationLevel? isolation = null, Require require = Require.None)
            : this(new UnitOfWorkOptions(source, isolation ?? IsolationLevel.Unspecified, require))
        {
        }

        public UnitOfWork(IsolationLevel? isolation = null)
            : this(new UnitOfWorkOptions(string.Empty, isolation ?? IsolationLevel.Unspecified, Require.None))
        {
        }

        public UnitOfWork(Require require)
            : this(new UnitOfWorkOptions(string.Empty, IsolationLevel.Unspecified, require))
        {
        }

        public UnitOfWork([NotNull] string source)
            : this(new UnitOfWorkOptions(source, IsolationLevel.Unspecified, Require.None))
        {
        }

        public UnitOfWork([NotNull] string source = "", Require require = Require.None)
            : this(new UnitOfWorkOptions(source, IsolationLevel.Unspecified, require))
        {
        }

        public UnitOfWork([NotNull] string source = "", IsolationLevel? isolation = null)
            : this(new UnitOfWorkOptions(source, isolation ?? IsolationLevel.Unspecified, Require.None))
        {
        }

        public UnitOfWork()
            : this(new UnitOfWorkOptions(string.Empty, IsolationLevel.Unspecified, Require.None))
        {
        }

        public UnitOfWork([NotNull] UnitOfWorkOptions unitOfWorkOptions)
        {
            if (unitOfWorkOptions == null)
            {
                throw new ArgumentNullException("unitOfWorkOptions");
            }

            dataContext = DataEnvironment.DataContextSupervisor.Register(unitOfWorkOptions);
        }

        object IWrapper.WrappedObject
        {
            get { return WrappedObject; }
        }

        protected virtual object WrappedObject
        {
            get
            {
                AssertNotDisposed();

                return dataContext.Session.WrappedObject;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Reviewed. The method is supposed to be used only by using block.")]
        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        public virtual void Complete()
        {
            AssertNotDisposed();

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

        public virtual T Complete<T>([NotNull] Func<IUnitOfWork, T> toReturn)
        {
            AssertNotDisposed();
            var result = toReturn(this);
            Complete();
            return result;
        }

        public virtual T Complete<T>([NotNull] Func<T> toReturn)
        {
            AssertNotDisposed();
            var result = toReturn();
            Complete();
            return result;
        }

        public virtual T Complete<T>(T toReturn)
        {
            AssertNotDisposed();
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposed || !disposing)
            {
                return;
            }

            try
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
                    DataEnvironment.CheckDataContextSupervisorForRelease();
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
    }
}