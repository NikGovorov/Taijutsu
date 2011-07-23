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
using Taijutsu.Infrastructure.Config;


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
        IDataContext RegisterUnitOfWorkBasedOn(UnitOfWorkConfig unitOfWorkConfig);
        IDataContext RegisterUnitOfWorkBasedOn(SubUnitOfWorkConfig unitOfWorkConfig);
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

        public virtual IDataContext RegisterUnitOfWorkBasedOn(UnitOfWorkConfig unitOfWorkConfig)
        {
            var dataContext = new DataContext(unitOfWorkConfig, this);
            unitsOfWork.Add(dataContext);
            return dataContext;
        }

        public virtual IDataContext RegisterUnitOfWorkBasedOn(SubUnitOfWorkConfig unitOfWorkConfig)
        {
            DataContext dataContext = (from unit in unitsOfWork
                                       where
                                           unit.UnitOfWorkConfig.SourceName ==
                                           unitOfWorkConfig.UnitOfWorkConfig.SourceName
                                       select unit).LastOrDefault();

            if (dataContext == null)
            {
                if (unitOfWorkConfig.RunAsRoot)
                {
                    return SupervisorContext.DataContextSupervisor.RegisterUnitOfWorkBasedOn(unitOfWorkConfig.UnitOfWorkConfig);
                }
                throw new Exception("Creation of subunit of work is not allowed without having root unit of work.");
            }
            var subContext = new DataContextDecorator(dataContext);
            return subContext;
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