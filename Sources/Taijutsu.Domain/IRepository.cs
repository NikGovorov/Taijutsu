// Copyright 2009-2011 Taijutsu.
//   
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//  
//      http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System.Collections.Generic;

namespace Taijutsu.Domain
{
    public interface IRepository<TEntity> : IHideObjectMembers where TEntity : IQueryableEntity
    {
        TEntity this[object key] { get; }
        TDerivedEntity Retrieve<TDerivedEntity>(object key) where TDerivedEntity : TEntity;
        TEntity Retrieve(object key);
        Maybe<TDerivedEntity> FindBy<TDerivedEntity>(object key) where TDerivedEntity : TEntity;
        Maybe<TEntity> FindBy(object key);
        bool TryFindBy<TDerivedEntity>(object key, out TDerivedEntity value) where TDerivedEntity : TEntity;
        bool TryFindBy(object key, out TEntity value);
        IEnumerable<TDerivedEntity> FindAll<TDerivedEntity>() where TDerivedEntity : TEntity;
        IEnumerable<TEntity> FindAll();
        bool Exists(object key);
        bool Exists();
        long Count();
    }
}