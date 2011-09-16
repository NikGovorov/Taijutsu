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
using System.ComponentModel;
using System.Linq;

namespace Taijutsu.Data.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class Infrastructure
    {
        private const string DataContextSupervisorKey = "DataContextSupervisor";
        private const string ReadOnlyDataContextSupervisorKey = "ReadOnlyDataContextSupervisor";
        private const string OperationScopeKey = "OperationScope";
        private static IDictionary<string, DataSource> dataSources = new Dictionary<string, DataSource>();
        private static readonly object sync = new object();

        internal static string DefaultDataSourceName = "Core";

        internal static DataContextSupervisor DataContextSupervisor
        {
            get
            {
                var supervisor = (DataContextSupervisor) LogicalContext.FindData(DataContextSupervisorKey);
                if (supervisor == null)
                {
                    supervisor = new DataContextSupervisor();
                    LogicalContext.SetData(DataContextSupervisorKey, supervisor);
                }
                return supervisor;
            }
        }

        internal static ReadOnlyDataContextSupervisor ReadOnlyDataContextSupervisor
        {
            get
            {
                var supervisor =
                    (ReadOnlyDataContextSupervisor) LogicalContext.FindData(ReadOnlyDataContextSupervisorKey);
                if (supervisor == null)
                {
                    supervisor = new ReadOnlyDataContextSupervisor();
                    LogicalContext.SetData(ReadOnlyDataContextSupervisorKey, supervisor);
                }
                return supervisor;
            }
        }

        internal static bool CheckDataContextSupervisorForRelease()
        {
            if (LogicalContext.FindData(OperationScopeKey) == null)
            {
                var supervisor = LogicalContext.FindData(DataContextSupervisorKey);

                if (supervisor != null && !((DataContextSupervisor) supervisor).Roots.Any())
                {
                    LogicalContext.ReleaseData(DataContextSupervisorKey);
                    return true;
                }
            }
            return false;
        }

        internal static bool CheckReadOnlyDataContextSupervisorForRelease()
        {
            if (LogicalContext.FindData(OperationScopeKey) == null)
            {
                var supervisor = LogicalContext.FindData(ReadOnlyDataContextSupervisorKey);

                if (supervisor != null && !((ReadOnlyDataContextSupervisor) supervisor).Roots.Any())
                {
                    LogicalContext.ReleaseData(ReadOnlyDataContextSupervisorKey);
                    return true;
                }
            }
            return false;
        }


        internal static void RegisterOperationScopeWith(IProviderLifecyclePolicy contextSharing)
        {
            if (LogicalContext.FindData(OperationScopeKey) != null)
            {
                throw new Exception("Only one operation scope is allowed simultaneously.");
            }

            if (LogicalContext.FindData(DataContextSupervisorKey) != null)
            {
                throw new Exception("Operation scope can not be included in the scope of unit of work.");
            }

            if (LogicalContext.FindData(ReadOnlyDataContextSupervisorKey) != null)
            {
                throw new Exception("Operation scope can not be included in the scope of unit of query.");
            }

            LogicalContext.SetData(OperationScopeKey, new object());

            LogicalContext.SetData(DataContextSupervisorKey,
                                   new DataContextSupervisor(contextSharing));
            LogicalContext.SetData(ReadOnlyDataContextSupervisorKey,
                                   new ReadOnlyDataContextSupervisor(contextSharing));
        }

        internal static void UnRegisterOperationScope()
        {
            LogicalContext.ReleaseData(DataContextSupervisorKey);
            LogicalContext.ReleaseData(ReadOnlyDataContextSupervisorKey);
            LogicalContext.ReleaseData(OperationScopeKey);
        }


        internal static DataSource DataSource(string name)
        {
            DataSource dataSource;

            if (!dataSources.TryGetValue(name, out dataSource))
            {
                throw new Exception(string.Format("Data source '{0}' has not yet been initialized.", name));
            }
            return dataSource;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterDataSource(DataSource dataSource)
        {
            DataSource exDataSource;

            if (!dataSources.TryGetValue(dataSource.Name, out exDataSource))
            {
                lock (sync)
                {
                    if (!dataSources.TryGetValue(dataSource.Name, out exDataSource))
                    {
                        IDictionary<string, DataSource> newDataSources = new Dictionary<string, DataSource>(dataSources);
                        newDataSources.Add(dataSource.Name, dataSource);
                        dataSources = newDataSources;
                    }
                    return;
                }
            }
            throw new Exception(string.Format("Data source '{0}' has already been initialized.", dataSource.Name));
        }


         
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static Maybe<IAdvancedUnitOfWork> CurrentUnitOfWork
        {
            get
            {
                var maybeCtx = DataContextSupervisor.Current;
                return !maybeCtx ? Maybe<IAdvancedUnitOfWork>.Empty : new Maybe<IAdvancedUnitOfWork>(maybeCtx.Value);
            }
        }
    }
}