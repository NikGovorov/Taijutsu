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

using Taijutsu.Annotation;

namespace Taijutsu.Domain
{
    public interface IEntityLoader
    {
        TEntity Load<TEntity>([NotNull] object id, bool required = true, bool locked = false, bool optimized = false, object options = null) where TEntity : IAggregateRoot;
    }

    [PublicApi]
    public interface IEntityLoader<out TEntity> where TEntity : IAggregateRoot
    {
        TEntity Load([NotNull] object id, bool required = true, bool locked = false, bool optimized = false, object options = null);
    }
}