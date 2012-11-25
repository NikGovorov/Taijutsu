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
using System.Collections.Generic;
using System.Diagnostics;
using Taijutsu.PrevData.Internal;

namespace Taijutsu.PrevData
{
    public class OperationScope : IDisposable
    {
        private readonly IProviderLifecyclePolicy contextSharing;

        public OperationScope()
            : this(new ScopedProviderLifecyclePolicy())
        {
        }

        protected internal OperationScope(IProviderLifecyclePolicy contextSharing)
        {
            Infrastructure.RegisterOperationScopeWith(contextSharing);
            this.contextSharing = contextSharing;
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            contextSharing.Dispose();
            Infrastructure.UnRegisterOperationScope();
        }

        #endregion

        #region Nested type: ScopedProviderLifecyclePolicy

       
        #endregion
    }


    internal sealed class ScopedProviderLifecyclePolicy : IProviderLifecyclePolicy
    {
        private readonly IList<DataProvider> providers = new List<DataProvider>();
        private readonly IList<ReadOnlyDataProvider> readOnlyProviders = new List<ReadOnlyDataProvider>();

        #region IProviderLifecyclePolicy Members

        public DataProvider Register(UnitOfWorkConfig config)
        {
            return Infrastructure.DataSource(config.SourceName).BuildDataProvider(config.IsolationLevel);
        }

        public ReadOnlyDataProvider Register(UnitOfQueryConfig config)
        {
            return Infrastructure.DataSource(config.SourceName).BuildReadOnlyDataProvider(config.IsolationLevel);
        }

        public void Dispose()
        {
            try
            {
                Exception lastEx = null;
                foreach (var dataProvider in providers)
                {
                    try
                    {
                        dataProvider.Close();
                    }
                    catch (Exception exception)
                    {
                        Trace.TraceError(exception.ToString());
                        lastEx = exception;
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
                        Trace.TraceError(exception.ToString());
                        lastEx = exception;
                    }
                }

                if (lastEx != null)
                {
                    throw lastEx;
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

}