#region License

// Copyright 2009-2012 Taijutsu.
//    
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
//  this file except in compliance with the License. You may obtain a copy of the 
//  License at 
//   
//  http://www.apache.org/licenses/LICENSE-2.0 
//   
//  Unless required by applicable law or agreed to in writing, software distributed 
//  under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
//  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
//  specific language governing permissions and limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Taijutsu.Data.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class InternalEnvironment
    {
        private const string DataContextSupervisorKey = "Taijutsu.DataContextSupervisor";
        private const string OperationScopeKey = "Taijutsu.OperationScope";
        private const string SuppressionScopeKey = "Taijutsu.SuppressionScope";
        private const string ConstructionScopeKey = "Taijutsu.ConstructionScope";
        private static readonly object sync = new object();

        private static IDictionary<string, DataSource> dataSources = new Dictionary<string, DataSource>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static DataContextSupervisor DataContextSupervisor
        {
            get
            {
                var supervisor = (DataContextSupervisor) LogicContext.FindData(DataContextSupervisorKey);
                if (supervisor == null)
                {
                    supervisor = new DataContextSupervisor(() => new ReadOnlyDictionary<string, DataSource>(dataSources));
                    LogicContext.SetData(DataContextSupervisorKey, supervisor);
                }
                return supervisor;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static bool IsInsideSuppressionScope
        {
            get
            {
                var isInSuppressionScope = LogicContext.FindData(SuppressionScopeKey);
                return isInSuppressionScope != null && (bool) isInSuppressionScope;
            }
            set { LogicContext.SetData(SuppressionScopeKey, value); }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static bool IsInsideConstructionScope
        {
            get
            {
                var isInConstructionScope = LogicContext.FindData(ConstructionScopeKey);
                return isInConstructionScope != null && (bool) isInConstructionScope;
            }
            set { LogicContext.SetData(ConstructionScopeKey, value); }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static void CheckDataContextSupervisorForRelease()
        {
            var supervisor = LogicContext.FindData(DataContextSupervisorKey);

            if (supervisor != null && !((DataContextSupervisor)supervisor).IsActive && LogicContext.FindData(OperationScopeKey) == null)
            {
                LogicContext.ReleaseData(DataContextSupervisorKey);
            }
        }


        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static void RegisterOperationScope(IOrmSessionTerminationPolicy policy)
        {
            if (LogicContext.FindData(OperationScopeKey) != null)
            {
                throw new Exception("Only one operation scope is allowed simultaneously.");
            }

            if (LogicContext.FindData(DataContextSupervisorKey) != null)
            {
                throw new Exception("Operation scope can not be included in the scope of unit of work.");
            }

            LogicContext.SetData(OperationScopeKey, new object());

            var supervisor = new DataContextSupervisor(() => new ReadOnlyDictionary<string, DataSource>(dataSources), policy);

            LogicContext.SetData(DataContextSupervisorKey, supervisor);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static void UnregisterOperationScope()
        {
            LogicContext.ReleaseData(DataContextSupervisorKey);
            LogicContext.ReleaseData(OperationScopeKey);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterDataSource(DataSource dataSource, bool throwIfExists = false)
        {
            lock (sync)
            {
                var newDataSources = new Dictionary<string, DataSource>(dataSources);

                if (throwIfExists && newDataSources.ContainsKey(dataSource.Name))
                {
                    throw new Exception(string.Format("Data source with name: '{0}' has already been registered.",
                                                      dataSource.Name));
                }

                newDataSources[dataSource.Name] = dataSource;
                dataSources = newDataSources;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void UnregisterDataSource(string name = "")
        {
            lock (sync)
            {
                var newDataSources = new Dictionary<string, DataSource>(dataSources);
                newDataSources.Remove(name);
                dataSources = newDataSources;
            }
        }
    }
}