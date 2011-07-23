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
using System.Data;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;
using Taijutsu.Infrastructure.Internal;

namespace Taijutsu.Infrastructure
{
    public class UnitOfQuery : IUnitOfQuery, INativeUnitOf, IDisposable
    {
        private readonly IReadOnlyDataContext dataContext;

        public UnitOfQuery(string source = "", IsolationLevel isolation = IsolationLevel.Unspecified,
                           Require require = Require.None) : this(new UnitOfQueryConfig(source, isolation, require))
        {
        }

        public UnitOfQuery(IsolationLevel isolation = IsolationLevel.Unspecified)
            : this(new UnitOfQueryConfig("", isolation, Require.None))
        {
        }

        public UnitOfQuery(Require require)
            : this(new UnitOfQueryConfig("", IsolationLevel.Unspecified, require))
        {
        }

        public UnitOfQuery(string source)
            : this(new UnitOfQueryConfig(source, IsolationLevel.Unspecified, Require.None))
        {
        }

        public UnitOfQuery(string source = "", bool actAsUnitOfWorkPart = false)
            : this(
                new UnitOfQueryConfig(source, IsolationLevel.Unspecified, Require.None)
                    {ActAsUnitOfWorkPart = actAsUnitOfWorkPart})
        {
        }


        public UnitOfQuery()
            : this(new UnitOfQueryConfig())
        {
        }

        public UnitOfQuery(UnitOfQueryConfig unitOfQueryConfig)
        {
            if (unitOfQueryConfig.ActAsUnitOfWorkPart 
                && !SupervisorContext.ReadOnlyDataContextSupervisor.HasTopLevel(unitOfQueryConfig)
                &&  SupervisorContext.DataContextSupervisor.HasTopLevel(unitOfQueryConfig))
            {
                dataContext = SupervisorContext.DataContextSupervisor.RegisterUnitOfWorkBasedOn(unitOfQueryConfig);    
            }
            dataContext = SupervisorContext.ReadOnlyDataContextSupervisor.RegisterUnitOfQueryBasedOn(unitOfQueryConfig);
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            var ctx = dataContext as IDataContext;
            
            if (ctx != null)
            {
                ctx.Commit();
            }

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

        #endregion

        public virtual IQueryOverBuilder<TEntity> Over<TEntity>() where TEntity : class, IQueryableEntity
        {
            return dataContext.ReadOnlyProvider.QueryOver<TEntity>();
        }
    }
}