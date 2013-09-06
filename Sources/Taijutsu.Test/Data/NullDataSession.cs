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
using System.Diagnostics.CodeAnalysis;

using Taijutsu.Data.Internal;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Test.Data
{
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Reviewed. Acceptable for tests.")]
    public class NullDataSession : IDataSession
    {
        object IWrapper.WrappedObject
        {
            get { return WrappedObject; }
        }

        protected object WrappedObject
        {
            get { return this; }
        }

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Reviewed. Acceptable for tests.")]
        public virtual void Dispose()
        {
        }

        public virtual void Complete()
        {
        }

        public virtual T Resolve<T>(object options = null) where T : class
        {
            throw new NotSupportedException("NullDataSession does not support internal services.");
        }

        public object MarkAsCreated<TEntity>(TEntity entity, object options = null) where TEntity : IAggregateRoot
        {
            return new object();
        }

        public object MarkAsCreated<TEntity>(Func<TEntity> entityFactory, object options = null) where TEntity : IAggregateRoot
        {
            return new object();
        }

        public void MarkAsDeleted<TEntity>(TEntity entity, object options = null) where TEntity : IDeletableEntity
        {
        }

        public IEntitiesQuery<TEntity> All<TEntity>(object options = null) where TEntity : class, IQueryableEntity
        {
            throw new NotSupportedException("NullDataSession does not support queries.");
        }

        public IUniqueEntityQuery<TEntity> Unique<TEntity>(object key, object options = null) where TEntity : class, IQueryableEntity
        {
            throw new NotSupportedException("NullDataSession does not support queries.");
        }

        public TQuery QueryWith<TEntity, TQuery>(string name = null) where TEntity : class, IEntity where TQuery : IQuery<TEntity>
        {
            throw new NotSupportedException();
        }

        public TRepository QueryFrom<TEntity, TRepository>(string name = null) where TEntity : class, IEntity where TRepository : IReadOnlyRepository<TEntity>
        {
            throw new NotSupportedException();
        }
    }
}