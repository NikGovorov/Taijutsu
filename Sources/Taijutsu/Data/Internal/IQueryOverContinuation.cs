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
namespace Taijutsu.Data.Internal
{
    using Taijutsu.Domain;
    using Taijutsu.Domain.Query;

    [PublicApi]
    public interface IQueryOverContinuation<TEntity> : IHiddenObjectMembers
        where TEntity : class, IEntity
    {
        TQuery With<TQuery>(string name = null) where TQuery : class, IQuery<TEntity>;

        TRepository From<TRepository>(string name = null) where TRepository : class, IRepository<TEntity>;
    }
}