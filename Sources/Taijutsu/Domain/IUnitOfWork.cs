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
namespace Taijutsu.Domain
{
    using System;

    using Taijutsu.Domain.Query;

    public interface IUnitOfWork
    {
        object MarkAsCreated<TEntity>(TEntity entity, object options = null) where TEntity : IAggregateRoot;

        object MarkAsCreated<TEntity>(Func<TEntity> entityFactory, object options = null) where TEntity : IAggregateRoot;

        void MarkAsDeleted<TEntity>(TEntity entity, object options = null) where TEntity : IDeletableEntity;

        IEntitiesQuery<TEntity> All<TEntity>(object options = null) where TEntity : class, IQueryableEntity;

        IUniqueEntityQuery<TEntity> Unique<TEntity>(object id, object options = null)
            where TEntity : class, IQueryableEntity;
    }

    [PublicApi]
    public interface IMarkingStep
    {
        object AsCreated();

        void AsDeleted();
    }
}