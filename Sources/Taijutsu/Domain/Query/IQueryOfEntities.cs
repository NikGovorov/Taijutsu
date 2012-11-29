#region License

//  Copyright 2009-2013 Nikita Govorov
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

using System.Collections.Generic;
using Taijutsu.Domain.Query.Narrowing;
using Taijutsu.Domain.Query.Option;

namespace Taijutsu.Domain.Query
{
    public interface IQueryOfEntities<out TEntity> : IQueryOver<TEntity>,
                                                     IQueryOf<IEnumerable<TEntity>>,
                                                     IKeyIncludingQuery<IQueryOfEntities<TEntity>>,
                                                     ITypeExcludingQuery<IQueryOfEntities<TEntity>>,
                                                     ILimitQuery<IQueryOfEntities<TEntity>>,
                                                     ICountNarrowing,
                                                     IAnyNarrowing
        where TEntity : IEntity
    {
    }
}