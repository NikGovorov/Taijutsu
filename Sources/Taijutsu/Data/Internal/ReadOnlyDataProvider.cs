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


using System.Data;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.Internal
{
    public abstract class ReadOnlyDataProvider : IReadOnlyDataProvider
    {
        #region IReadOnlyDataProvider Members

        public abstract IQueryOfEntities<TEntity> AllOf<TEntity>() where TEntity : class, IEntity;

        public abstract IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key)
            where TEntity : class, IEntity;


        public abstract IQueryOverBuilder<TEntity> QueryOver<TEntity>() where TEntity : class, IEntity;
        public abstract object NativeProvider { get; }

        #endregion

        public abstract void Close();

        public abstract void BeginTransaction(IsolationLevel level);

        public abstract void Commit();

        public abstract void Rollback();
    }
}