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

namespace Taijutsu.Data.Internal
{
    public class DataContextSupervisor
    {
        private readonly IProviderLifecyclePolicy providerLifecyclePolicy;
        private readonly IList<DataContext> unitsOfWork = new List<DataContext>();

        public DataContextSupervisor()
            : this(new ProviderLifecyclePolicy())
        {
        }

        public DataContextSupervisor(IProviderLifecyclePolicy providerLifecyclePolicy)
        {
            this.providerLifecyclePolicy = providerLifecyclePolicy;
        }

        protected virtual IProviderLifecyclePolicy ProviderLifecyclePolicy
        {
            get { return providerLifecyclePolicy; }
        }

        public virtual IEnumerable<UnitOfWorkConfig> Roots
        {
            get { return unitsOfWork.Select(u => u.UnitOfWorkConfig); }
        }

        public virtual IDataContext Register(UnitOfWorkConfig unitOfWorkConfig)
        {
            if (unitOfWorkConfig.Require == Require.New)
            {
                var newContext = new DataContext(unitOfWorkConfig, ProviderLifecyclePolicy.Register(unitOfWorkConfig),
                                                 RegisterForTerminate);
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
                return new ChildDataContext(context);
            }

            if (unitOfWorkConfig.Require == Require.Existing)
                throw new Exception(
                    "Unit of work requires existing of unit of work at top level, but nothing has not been found.");

            context = new DataContext(unitOfWorkConfig, ProviderLifecyclePolicy.Register(unitOfWorkConfig),
                                      RegisterForTerminate);
            unitsOfWork.Add(context);
            return context;
        }


        protected virtual void RegisterForTerminate(DataContext dataContext)
        {
            try
            {
                ProviderLifecyclePolicy.Terminate(dataContext.DataProvider);
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
        }
    }
}