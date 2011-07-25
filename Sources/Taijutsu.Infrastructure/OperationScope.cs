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
using System.Transactions;
using Taijutsu.Infrastructure.Internal;

namespace Taijutsu.Infrastructure
{
    public class OperationScope : IDisposable
    {
        private readonly IDataProviderPlanningPolicy dataContextSharing;

        public OperationScope()
            : this(new DataProviderPlanningPolicy())
        {
        }

        protected internal OperationScope(IDataProviderPlanningPolicy dataContextSharing)
        {
            SupervisorContext.RegisterOperationScopeWith(dataContextSharing);
            this.dataContextSharing = dataContextSharing;
        }

        #region IDisposable Members

        public virtual void Dispose()
        {
            dataContextSharing.Dispose();
            SupervisorContext.UnRegisterOperationScope();
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
                return Internal.Infrastructure.DataProviderFactory(config);
            }

            public ReadOnlyDataProvider Register(UnitOfQueryConfig config)
            {
                return Internal.Infrastructure.ReadOnlyDataProviderFactory(config);
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
                            Trace.TraceError(exception.ToString());
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

    public class AtomicOperationScope : OperationScope
    {
        private readonly TransactionScope transactionScope;

        public AtomicOperationScope(TransactionScopeOption transactionScopeOption, TimeSpan scopeTimeout)
            : this(new TransactionScope(transactionScopeOption, scopeTimeout), new DataProviderPlanningPolicy())
        {
        }

        public AtomicOperationScope(TransactionScopeOption transactionScopeOption, TransactionOptions transactionOptions,
                                    EnterpriseServicesInteropOption enterpriseServicesInteropOption)
            : this(
                new TransactionScope(transactionScopeOption, transactionOptions, enterpriseServicesInteropOption),
                new DataProviderPlanningPolicy())
        {
        }

        public AtomicOperationScope(TransactionScopeOption transactionScopeOption, TransactionOptions transactionOptions)
            : this(new TransactionScope(transactionScopeOption, transactionOptions), new DataProviderPlanningPolicy())
        {
        }

        public AtomicOperationScope(TransactionScopeOption transactionScopeOption)
            : this(new TransactionScope(transactionScopeOption), new DataProviderPlanningPolicy())
        {
        }

        public AtomicOperationScope()
            : this(new TransactionScope(), new DataProviderPlanningPolicy())
        {
        }

        protected internal AtomicOperationScope(TransactionScope transactionScope,
                                                IDataProviderPlanningPolicy dataContextSharing)
            : base(dataContextSharing)
        {
            this.transactionScope = transactionScope;
        }

        public virtual void Complete()
        {
            transactionScope.Complete();
        }

        public override void Dispose()
        {
            base.Dispose();
            transactionScope.Dispose();
        }
    }
}