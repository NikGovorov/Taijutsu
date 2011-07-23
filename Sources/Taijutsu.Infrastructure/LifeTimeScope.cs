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
using Taijutsu.Infrastructure.Internal;

namespace Taijutsu.Infrastructure
{
    public class LifeTimeScope : IDisposable
    {
        private readonly IDataProviderPlanningPolicy dataContextSharing;

        public LifeTimeScope()
            : this(new DataProviderPlanningPolicy())
        {
        }

        protected internal LifeTimeScope(IDataProviderPlanningPolicy dataContextSharing)
        {
            SupervisorContext.RegisterUnitScopeWith(dataContextSharing);
            this.dataContextSharing = dataContextSharing;
        }

        #region IDisposable Members

        public void Dispose()
        {
            dataContextSharing.Dispose();
        }

        #endregion

        #region Nested type: DataProviderPlanningPolicy

        private sealed class DataProviderPlanningPolicy : IDataProviderPlanningPolicy
        {
            private readonly IList<DataProvider> providers = new List<DataProvider>();
            private readonly IList<ReadOnlyDataProvider> readOnlyProviders = new List<ReadOnlyDataProvider>();

            #region IDataProviderPlanningPolicy Members

            public DataProvider Register(UnitOfWorkConfig config)
            {
                return Internal.Infrastructure.DataProviderFactory.Create(config);
            }

            public ReadOnlyDataProvider Register(UnitOfQueryConfig config)
            {
                return Internal.Infrastructure.ReadOnlyDataProviderFactory.Create(config);
            }

            public void Dispose()
            {
                try
                {
                    foreach (var dataProvider in providers)
                    {
                        try
                        {
                            dataProvider.Close();
                        }
                        catch (Exception exception)
                        {
                            Trace.TraceWarning(exception.ToString());
                        }
                    }
                    
                    foreach (var readOnlyDataProvider in readOnlyProviders)
                    {
                        try
                        {
                            readOnlyDataProvider.Close();
                        }
                        catch (Exception exception)
                        {
                            Trace.TraceWarning(exception.ToString());
                        }
                    }
                }
                finally
                {
                    providers.Clear();
                    readOnlyProviders.Clear();
                }
            }

            public void Terminate(DataProvider dataProvider)
            {
                providers.Add(dataProvider);
            }

            public void Terminate(ReadOnlyDataProvider readOnlyDataProvider)
            {
                readOnlyProviders.Add(readOnlyDataProvider);
            }

            #endregion
        }

        #endregion
    }
}