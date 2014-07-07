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

using Taijutsu.Annotation;

namespace Taijutsu.Domain
{
    public interface IEntityTracker
    {
        object Save<TEntity>([NotNull] TEntity entity, object options = null) where TEntity : IAggregateRoot;

        object Save<TEntity>([NotNull] TEntity entity, EntitySaveMode mode, object options = null) where TEntity : IAggregateRoot;

        object Save<TEntity>([NotNull] Func<TEntity> entityFactory, object options = null) where TEntity : IAggregateRoot;

        void Delete<TEntity>([NotNull] TEntity entity, object options = null) where TEntity : IDeletableEntity;
    }

    public interface IEntityTracker<in T> : IEntityPersister<T>, IEntityRemover<T> where T : IDeletableEntity
    {
    }
}