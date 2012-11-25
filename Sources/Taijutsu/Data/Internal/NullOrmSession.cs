#region License

// Copyright 2009-2012 Taijutsu.
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
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.Internal
{
    public class NullOrmSession : IOrmSession
    {
        public void Dispose()
        {
        }

        public void Complete()
        {
        }

        public virtual T As<T>(dynamic options = null) where T : class
        {
            throw new NotSupportedException("NullOrmSession does not support internal services.");
        }

        public object MarkAsCreated<TEntity>(TEntity entity, dynamic options = null) where TEntity : IAggregateRoot
        {
            return new object();
        }

        public object MarkAsCreated<TEntity>(Func<TEntity> entityFactory, dynamic options = null) where TEntity : IAggregateRoot
        {
            return new object();
        }

        public void MarkAsDeleted<TEntity>(TEntity entity, dynamic options = null) where TEntity : IDeletableEntity
        {
        }

        public IQueryOfEntities<TEntity> AllOf<TEntity>(dynamic options = null) where TEntity : class, IEntity
        {
            throw new NotSupportedException("NullOrmSession does not support queries.");
        }

        public IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key, dynamic options = null) where TEntity : class, IEntity
        {
            throw new NotSupportedException("NullOrmSession does not support queries.");
        }

        public IQueryOverContinuation<TEntity> QueryOver<TEntity>() where TEntity : class, IEntity
        {
            throw new NotSupportedException("NullOrmSession does not support queries.");
        }

        object IHasNativeObject.NativeObject { get { return this; } }

    }
}