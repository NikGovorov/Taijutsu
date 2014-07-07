﻿// Copyright 2009-2014 Nikita Govorov
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

namespace Taijutsu.Domain.Query
{
    public interface IEntitiesQuery<TEntity> : ISingleOrDefaultQuery<TEntity>, 
        IFirstOrDefaultQuery<TEntity>, 
        IListQuery<TEntity>, 
        IArrayQuery<TEntity>, 
        IDictionaryQuery<TEntity>, 
        IAsQueryableQuery<TEntity>, 
        IAnyQuery<TEntity>, 
        ICountQuery<TEntity>, 
        IIdFilter<IEntitiesQuery<TEntity>>, 
        ILimitFilter<IEntitiesQuery<TEntity>>, 
        ITypeFilter<IEntitiesQuery<TEntity>>, 
        ITypeFilter<TEntity, IEntitiesQuery<TEntity>>, 
        INotTypeFilter<IEntitiesQuery<TEntity>>, 
        INotTypeFilter<TEntity, IEntitiesQuery<TEntity>>, 
        ILockOption<IEntitiesQuery<TEntity>>
        where TEntity : IEntity
    {
    }
}