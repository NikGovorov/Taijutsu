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
using System.Collections.Generic;
using System.Linq;

namespace Taijutsu.Infrastructure.Internal
{
    public interface IReadOnlyDataContextSupervisor
    {
        IReadOnlyDataContext RegisterUnitOfQueryBasedOn(UnitOfQueryConfig unitOfQueryConfig);
        bool HasTopLevel(UnitOfQueryConfig unitOfQueryConfig);
    }

    public class ReadOnlyDataContextSupervisor : AbstractDataContextSupervisor, IReadOnlyDataContextSupervisor
    {
        private readonly IList<ReadOnlyDataContext> unitsOfQuery = new List<ReadOnlyDataContext>();

        public ReadOnlyDataContextSupervisor()
        {
        }

        public ReadOnlyDataContextSupervisor(IDataProviderPlanningPolicy dataContextSharingPolicy)
            : base(dataContextSharingPolicy)
        {
        }

        #region IReadOnlyDataContextSupervisor Members

        public virtual IReadOnlyDataContext RegisterUnitOfQueryBasedOn(UnitOfQueryConfig unitOfQueryConfig)
        {
            if (unitOfQueryConfig.Require == Require.New)
            {
                var newContext = new ReadOnlyDataContext(unitOfQueryConfig, this);
                unitsOfQuery.Add(newContext);
                return newContext;
            }

            var context = (from query in unitsOfQuery
                           where query.UnitOfQueryConfig.SourceName == unitOfQueryConfig.SourceName
                           select query).LastOrDefault();

            if (context != null)
            {
                if (!context.UnitOfQueryConfig.IsolationLevel.IsCompatible(unitOfQueryConfig.IsolationLevel))
                {
                    throw new Exception(string.Format("Isolation level '{0}' is not compatible with '{1}'.", context.UnitOfQueryConfig.IsolationLevel, unitOfQueryConfig.IsolationLevel));
                }
                return new ReadOnlyDataContextDecorator(context);
            }

            if(unitOfQueryConfig.Require == Require.Existing)
                throw new Exception("Unit of query requires existing of unit of query at top level, but nothing has not been found.");

            context = new ReadOnlyDataContext(unitOfQueryConfig, this);
            unitsOfQuery.Add(context);
            return context;
        }

        public virtual bool HasTopLevel(UnitOfQueryConfig unitOfQueryConfig)
        {
            var context = (from query in unitsOfQuery
             where query.UnitOfQueryConfig.SourceName == unitOfQueryConfig.SourceName
             select query).LastOrDefault();
            return context != null && context.UnitOfQueryConfig.IsolationLevel.IsCompatible(unitOfQueryConfig.IsolationLevel);
        }

        #endregion

        protected internal ReadOnlyDataProvider RegisterForTerminate(ReadOnlyDataContext context)
        {
            try
            {
                DataContextSharingPolicy.Terminate(context.DataProvider);
            }
            finally
            {
                unitsOfQuery.Remove(context);
            }

            return new OfflineReadOnlyDataProvider();
        }

        protected internal ReadOnlyDataProvider CreateDataProvider(UnitOfQueryConfig unitOfQueryConfig)
        {
            return DataContextSharingPolicy.Register(unitOfQueryConfig);
        }
    }
}