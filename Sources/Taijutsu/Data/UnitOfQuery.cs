// Copyright 2009-2012 Taijutsu.
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
using Taijutsu.Data.Internal;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data
{
    public enum Require
    {
        New,
        Existing,
        None
    }

    public class UnitOfQuery : IUnitOfQuery, INative, IDisposable
    {
        private readonly IReadOnlyDataContext dataContext;

        public UnitOfQuery(string source = "", IsolationLevel? isolation = null,
                           Require require = Require.None) : this(new UnitOfQueryConfig(source, isolation, require))
        {
        }

        public UnitOfQuery(IsolationLevel? isolation = null)
            : this(new UnitOfQueryConfig("", isolation, Require.None))
        {
        }

        public UnitOfQuery(Require require)
            : this(new UnitOfQueryConfig("", null, require))
        {
        }

        public UnitOfQuery(string source)
            : this(new UnitOfQueryConfig(source, null, Require.None))
        {
        }

        public UnitOfQuery(string source = "", Require require = Require.None)
            : this(new UnitOfQueryConfig(source, null, require))
        {
        }

        public UnitOfQuery()
            : this(new UnitOfQueryConfig("", null, Require.None))
        {
        }

        public UnitOfQuery(UnitOfQueryConfig unitOfQueryConfig)
        {
            dataContext = Infrastructure.ReadOnlyDataContextSupervisor.Register(unitOfQueryConfig);
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            try
            {
                dataContext.Commit();
            }
            finally
            {
                try
                {
                    dataContext.Close();
                }
                finally
                {
                    if (dataContext.IsRoot)
                    {
                        Infrastructure.CheckReadOnlyDataContextSupervisorForRelease();
                    }
                }
            }
        }

        #endregion

        #region INative Members

        object INative.Native
        {
            get { return dataContext.ReadOnlyProvider.NativeProvider; }
        }

        #endregion

        #region IUnitOfQuery Members

        public virtual IQueryOfEntities<TEntity> AllOf<TEntity>() where TEntity : class, IEntity
        {
            return dataContext.ReadOnlyProvider.AllOf<TEntity>();
        }

        public virtual IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key)
            where TEntity : class, IEntity
        {
            return dataContext.ReadOnlyProvider.UniqueOf<TEntity>(key);
        }

        #endregion

        public virtual IQueryOverBuilder<TEntity> Over<TEntity>() where TEntity : class, IEntity
        {
            return dataContext.ReadOnlyProvider.QueryOver<TEntity>();
        }
    }
}