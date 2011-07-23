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


using System;
using Taijutsu.Infrastructure.Config;
using Taijutsu.Infrastructure.Internal;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Infrastructure
{
    public class UnitOfQuery : IUnitOfQuery, INativeUnitOf, IDisposable
    {
        private readonly IReadOnlyDataContext dataContext;

        public UnitOfQuery() : this(new UnitOfQueryConfig())
        {
        }

        public UnitOfQuery(UnitOfQueryConfig unitOfQueryConfig)
        {
            dataContext = SupervisorContext.ReadOnlyDataContextSupervisor.RegisterUnitOfQueryBasedOn(unitOfQueryConfig);
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            dataContext.Close();
        }

        #endregion

        #region INativeUnitOf Members

        object INativeUnitOf.Native
        {
            get { return dataContext.ReadOnlyProvider.NativeProvider; }
        }

        #endregion

        #region IUnitOfQuery Members

        public virtual IQueryOfEntities<TEntity> AllOf<TEntity>() where TEntity : class, IQueryableEntity
        {
            return dataContext.ReadOnlyProvider.AllOf<TEntity>();
        }

        public virtual IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key)
            where TEntity : class, IQueryableEntity
        {
            return dataContext.ReadOnlyProvider.UniqueOf<TEntity>(key);
        }

        public virtual IQueryOverBuilder<TEntity> Over<TEntity>() where TEntity : class, IQueryableEntity
        {
            return dataContext.ReadOnlyProvider.QueryOver<TEntity>();
        }

        #endregion
    }
}