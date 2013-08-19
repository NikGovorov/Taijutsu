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

using System;

using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.Internal
{
    public abstract class OrmSpecificSession<TNative> : IOrmSession
    {
        private readonly TNative nativeSession;

        protected OrmSpecificSession(TNative nativeSession)
        {
            this.nativeSession = nativeSession;
        }

        object IWrapper.Origin
        {
            get { return Origin; }
        }

        protected virtual object Origin
        {
            get { return NativeSession; }
        }

        protected virtual TNative NativeSession
        {
            get { return nativeSession; }
        }

        public virtual T As<T>(object options = null) where T : class
        {
            var native = NativeSession as T;

            if (native == null)
            {
                throw new Exception(string.Format("Unable to cast native session of '{0}' to '{1}'.", NativeSession.GetType().FullName, typeof(T).FullName));
            }

            return native;
        }

        public abstract object MarkAsCreated<TEntity>(TEntity entity, object options = null) where TEntity : IAggregateRoot;

        public abstract object MarkAsCreated<TEntity>(Func<TEntity> entityFactory, object options = null) where TEntity : IAggregateRoot;

        public abstract void MarkAsDeleted<TEntity>(TEntity entity, object options = null) where TEntity : IDeletableEntity;

        public abstract IEntitiesQuery<TEntity> All<TEntity>(object options = null) where TEntity : class, IQueryableEntity;

        public abstract IUniqueEntityQuery<TEntity> Unique<TEntity>(object key, object options = null) where TEntity : class, IQueryableEntity;

        public abstract TQuery QueryWith<TEntity, TQuery>(string name = null) where TEntity : class, IEntity where TQuery : IQuery<TEntity>;

        public abstract TRepository QueryFrom<TEntity, TRepository>(string name = null) where TEntity : class, IEntity where TRepository : IRepository<TEntity>;

        public void Dispose()
        {
            Dispose(true);
        }

        public abstract void Complete();

        protected abstract void Dispose(bool disposing);
    }
}