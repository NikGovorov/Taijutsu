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
using System.Linq;

using Taijutsu.Annotation;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.Internal
{
    public abstract class DataSession<T> : IDataSession
    {
        private readonly T session;

        private readonly ICustomizationResolver customizationResolver;

        protected DataSession([NotNull] T session, ICustomizationResolver customizationResolver = null)
        {
            this.session = session;
            this.customizationResolver = customizationResolver;
        }

        object IDecorator.Origin
        {
            get { return Origin; }
        }

        public virtual T Session
        {
            get
            {
                AssertNotDisposed();
                return session;
            }
        }

        protected virtual object Origin
        {
            get { return Session; }
        }

        protected abstract bool? Completed { get; set; }

        protected abstract bool Disposed { get; set; }

        public virtual TService Resolve<TService>(object options = null) where TService : class
        {
            AssertNotDisposed();

            var service = Session as TService;

            if (service == null)
            {
                throw new Exception(string.Format("Unable to cast native session of '{0}' to '{1}'.", Session.GetType().FullName, typeof(TService).FullName));
            }

            return service;
        }

        public virtual object Save<TEntity>(TEntity entity, object options = null) where TEntity : IAggregateRoot
        {
            AssertNotCompleted();

            if (customizationResolver != null)
            {
                var resolver = customizationResolver.ResolveEntityPersister<TEntity>();

                if (resolver != null)
                {
                    return resolver().Save(entity, EntitySaveMode.Create, options);
                }
            }

            return InternalSave(entity, options);
        }

        public virtual object Save<TEntity>(TEntity entity, EntitySaveMode mode, object options = null) where TEntity : IAggregateRoot
        {
            AssertNotCompleted();

            if (customizationResolver != null)
            {
                var resolver = customizationResolver.ResolveEntityPersister<TEntity>();

                if (resolver != null)
                {
                    return resolver().Save(entity, mode, options);
                }
            }

            return InternalSave(entity, mode, options);
        }

        public virtual object Save<TEntity>(Func<TEntity> entityFactory, object options = null) where TEntity : IAggregateRoot
        {
            AssertNotCompleted();

            if (customizationResolver != null)
            {
                var resolver = customizationResolver.ResolveEntityPersister<TEntity>();

                if (resolver != null)
                {
                    return resolver().Save(entityFactory, options);
                }
            }

            return InternalSave(entityFactory, options);
        }

        public virtual void Delete<TEntity>(TEntity entity, object options = null) where TEntity : IDeletableEntity
        {
            AssertNotCompleted();

            if (customizationResolver != null)
            {
                var resolver = customizationResolver.ResolveEntityRemover<TEntity>();

                if (resolver != null)
                {
                    resolver().Delete(entity, options);
                }
            }

            InternalDelete(entity, options);
        }

        public abstract IQuerySourceProvider<TEntity> Query<TEntity>(object options = null) where TEntity : class, IQueryableEntity;

        public abstract TEntity Load<TEntity>(object id, bool required = true, bool locked = false, bool optimized = false, object options = null) where TEntity : IQueryableEntity;

        public abstract void Flush();

        public abstract void Complete();

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual TQuery LocateQuery<TEntity, TQuery>(string name = null, object options = null) where TQuery : IQuery<TEntity>
        {
            AssertNotDisposed();

            if (customizationResolver != null)
            {
                var resolver = customizationResolver.ResolveQuery<TEntity, TQuery>(name);

                if (resolver != null)
                {
                    return resolver(name, options);
                }
            }

            return InternalLocateQuery<TEntity, TQuery>(name, options);
        }

        protected virtual TQuery InternalLocateQuery<TEntity, TQuery>(string name = null, object options = null) where TQuery : IQuery<TEntity>
        {
            var type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).FirstOrDefault(t => typeof(TQuery).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            if (type == null)
            {
                throw new Exception(string.Format("Query that is assignable from '{0}' has not been found.", typeof(TQuery).FullName));
            }

            try
            {
                return (TQuery)Activator.CreateInstance(type, BuildQueryParameters(name, options));
            }
            catch (Exception exception)
            {
                throw new Exception(
                    string.Format("Exception occurred during activation instance of '{0}', see inner exception. By default ctor({1}) is expected.", type.FullName, typeof(T).FullName), 
                    exception);
            }
        }

        protected virtual object[] BuildQueryParameters(string name, object options)
        {
            return new object[] { Session };
        }

        protected TRepository LocateRepository<TEntity, TRepository>(string name = null, object options = null) where TRepository : IRepository<TEntity>
        {
            AssertNotDisposed();

            if (customizationResolver != null)
            {
                var resolver = customizationResolver.ResolveRepository<TEntity, TRepository>(name);

                if (resolver != null)
                {
                    return resolver(name, options);
                }
            }

            return InternalLocateRepository<TEntity, TRepository>(name, options);
        }

        protected virtual TRepository InternalLocateRepository<TEntity, TRepository>(string name = null, object options = null) where TRepository : IRepository<TEntity>
        {
            var type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).FirstOrDefault(t => typeof(TRepository).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            if (type == null)
            {
                throw new Exception(string.Format("Repository that is assignable from '{0}' has not been found.", typeof(TRepository).FullName));
            }

            try
            {
                return (TRepository)Activator.CreateInstance(type, BuildRepositoryParameters(name, options));
            }
            catch (Exception exception)
            {
                throw new Exception(
                    string.Format("Exception occurred during activation instance of '{0}', see inner exception. By default ctor({1}) is expected.", type.FullName, typeof(T).FullName), 
                    exception);
            }
        }

        protected virtual object[] BuildRepositoryParameters(string name, object options)
        {
            return new object[] { Session };
        }

        protected abstract object InternalSave<TEntity>(TEntity entity, object options = null) where TEntity : IAggregateRoot;

        protected abstract object InternalSave<TEntity>(TEntity entity, EntitySaveMode mode, object options = null) where TEntity : IAggregateRoot;

        protected abstract object InternalSave<TEntity>(Func<TEntity> entityFactory, object options = null) where TEntity : IAggregateRoot;

        protected abstract void InternalDelete<TEntity>(TEntity entity, object options = null) where TEntity : IDeletableEntity;

        protected abstract void Dispose(bool disposing);

        protected virtual void AssertNotCompleted()
        {
            if (Completed.HasValue)
            {
                throw new Exception(string.Format("Data Session has already been completed(with success - '{0}'), so it is not usable for write anymore.", Completed));
            }

            AssertNotDisposed();
        }

        protected virtual void AssertNotDisposed()
        {
            if (Disposed)
            {
                throw new Exception(string.Format("Data Session has already been disposed(with success - '{0}'), so it is not usable anymore.", Completed));
            }
        }
    }
}