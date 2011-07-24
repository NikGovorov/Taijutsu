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
using System.Diagnostics;
using System.Linq;
using Taijutsu.Domain;

namespace Taijutsu.Infrastructure.Internal
{
    public abstract class AbstractDataContextSupervisor
    {
        private readonly IDataProviderPlanningPolicy dataContextSharingPolicy;

        protected AbstractDataContextSupervisor()
            : this(new DataProviderPlanningPolicy())
        {
        }

        protected AbstractDataContextSupervisor(IDataProviderPlanningPolicy dataContextSharingPolicy)
        {
            this.dataContextSharingPolicy = dataContextSharingPolicy;
        }

        protected virtual IDataProviderPlanningPolicy DataContextSharingPolicy
        {
            get { return dataContextSharingPolicy; }
        }
    }

    public interface IDataContextSupervisor
    {
        Maybe<IDataContext> CurrentContext { get; }
        bool HasTopLevel(UnitOfWorkConfig unitOfQueryConfig);
        IEnumerable<UnitOfWorkConfig> Roots { get; }
        IDataContext RegisterUnitOfWorkBasedOn(UnitOfWorkConfig unitOfWorkConfig);
    }

    public class DataContextSupervisor : AbstractDataContextSupervisor, IDataContextSupervisor
    {
        private readonly IList<DataContext> unitsOfWork = new List<DataContext>();

        public DataContextSupervisor()
        {
        }

        public DataContextSupervisor(IDataProviderPlanningPolicy dataContextSharingPolicy)
            : base(dataContextSharingPolicy)
        {
        }

        #region IDataContextSupervisor Members

        public virtual Maybe<IDataContext> CurrentContext
        {
            get
            {
                try
                {
                    return new Maybe<IDataContext>(unitsOfWork.LastOrDefault());
                }
                catch (InvalidOperationException)
                {
                    return Maybe<IDataContext>.Empty;
                }
            }
        }

        public virtual bool HasTopLevel(UnitOfWorkConfig unitOfQueryConfig)
        {
            var context = (from query in unitsOfWork
                           where query.UnitOfWorkConfig.SourceName == unitOfQueryConfig.SourceName
                           select query).LastOrDefault();
            return context != null && context.UnitOfWorkConfig.IsolationLevel.IsCompatible(unitOfQueryConfig.IsolationLevel);
        }

        public virtual IEnumerable<UnitOfWorkConfig> Roots
        {
            get { return unitsOfWork.Select(u => u.UnitOfWorkConfig); }
        }

        public virtual IDataContext RegisterUnitOfWorkBasedOn(UnitOfWorkConfig unitOfWorkConfig)
        {
            if (unitOfWorkConfig.Require == Require.New)
            {
                var newContext = new DataContext(unitOfWorkConfig, this);
                unitsOfWork.Add(newContext);
                return newContext;
            }

            var context = (from unit in unitsOfWork
                           where unit.UnitOfWorkConfig.SourceName == unitOfWorkConfig.SourceName
                           select unit).LastOrDefault();


            if (context != null)
            {
                if (!context.UnitOfWorkConfig.IsolationLevel.IsCompatible(unitOfWorkConfig.IsolationLevel))
                {
                    throw new Exception(string.Format("Isolation level '{0}' is not compatible with '{1}'.",
                                                      context.UnitOfWorkConfig.IsolationLevel,
                                                      unitOfWorkConfig.IsolationLevel));
                }
                return new DataContextDecorator(context);
            }

            if (unitOfWorkConfig.Require == Require.Existing)
                throw new Exception(
                    "Unit of work requires existing of unit of work at top level, but nothing has not been found.");

            context = new DataContext(unitOfWorkConfig, this);
            unitsOfWork.Add(context);
            return context;
        }

        

        #endregion

        internal virtual DataProvider CreateDataProvider(UnitOfWorkConfig unitOfWorkConfig)
        {
            return DataContextSharingPolicy.Register(unitOfWorkConfig);
        }

        internal virtual DataProvider RegisterForTerminate(DataContext dataContext)
        {
            try
            {
                DataContextSharingPolicy.Terminate(dataContext.DataProvider);
            }
            catch (Exception exception)
            {
                Trace.TraceError(exception.ToString());
            }
            finally
            {
                try
                {
                    unitsOfWork.Remove(dataContext);
                }
                catch (Exception exception)
                {
                    Trace.TraceError(exception.ToString());
                }
            }
            return new OfflineDataProvider();
        }
    }
}