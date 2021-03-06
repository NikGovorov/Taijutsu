﻿// Copyright 2009-2013 Nikita Govorov
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

using Taijutsu.Annotation;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.Internal
{
    public abstract class DataSession<T> : IDataSession
    {
        private readonly T session;

        protected DataSession([NotNull] T session)
        {
            this.session = session;
        }

        object IWrapper.WrappedObject
        {
            get { return WrappedObject; }
        }

        public virtual T Session
        {
            get { return session; }
        }

        protected virtual object WrappedObject
        {
            get { return Session; }
        }

        public virtual TService Resolve<TService>(object options = null) where TService : class
        {
            var service = Session as TService;

            if (service == null)
            {
                throw new Exception(string.Format("Unable to cast native session of '{0}' to '{1}'.", Session.GetType().FullName, typeof(TService).FullName));
            }

            return service;
        }

        public abstract object MarkAsCreated<TEntity>(TEntity entity, object options = null) where TEntity : IAggregateRoot;

        public abstract object MarkAsCreated<TEntity>(Func<TEntity> entityFactory, object options = null) where TEntity : IAggregateRoot;

        public abstract void MarkAsDeleted<TEntity>(TEntity entity, object options = null) where TEntity : IDeletableEntity;

        public abstract IEntitiesQuery<TEntity> All<TEntity>(object options = null) where TEntity : class, IQueryableEntity;

        public abstract IUniqueEntityQuery<TEntity> Unique<TEntity>(object key, object options = null) where TEntity : class, IQueryableEntity;

        public void Dispose()
        {
            Dispose(true);
        }

        public abstract void Complete();

        protected abstract void Dispose(bool disposing);
    }
}