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
using System.Diagnostics.CodeAnalysis;

using Taijutsu.Data.Internal;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Test.Data
{
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Reviewed. Acceptable for tests.")]
    public class NullDataSession : IDataSession
    {
        object IDecorator.Origin
        {
            get { return Origin; }
        }

        protected object Origin
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

        public void Flush()
        {
        }

        public object Save<TEntity>(TEntity entity, object options = null) where TEntity : IAggregateRoot
        {
            return new object();
        }

        public object Save<TEntity>(TEntity entity, EntitySaveMode mode = EntitySaveMode.Auto, object options = null) where TEntity : IAggregateRoot
        {
            return new object();
        }

        public object Save<TEntity>(Func<TEntity> entityFactory, object options = null) where TEntity : IAggregateRoot
        {
            return new object();
        }

        public void Delete<TEntity>(TEntity entity, object options = null) where TEntity : IDeletableEntity
        {
        }

        public TEntity Load<TEntity>(object id, bool required = true, bool locked = false, bool optimized = false, object options = null) where TEntity : IQueryableEntity
        {
            throw new NotSupportedException("NullDataSession does not support Load method.");
        }

        public IQuerySourceProvider<TEntity> Query<TEntity>(object options = null) where TEntity : class, IQueryableEntity
        {
            throw new NotSupportedException("NullDataSession does not support Query method.");
        }
    }
}