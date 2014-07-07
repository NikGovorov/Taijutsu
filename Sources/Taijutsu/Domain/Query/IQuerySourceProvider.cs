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

using Taijutsu.Annotation;

namespace Taijutsu.Domain.Query
{
    [PublicApi]
    public interface IQuerySourceProvider<TEntity> : IEntitiesQuery<TEntity> where TEntity : IQueryableEntity
    {
        TQuery With<TQuery>(string name = null, object options = null) where TQuery : IQuery<TEntity>;

        TRepository From<TRepository>(string name = null, object options = null) where TRepository : IRepository<TEntity>;
    }
}