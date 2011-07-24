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

using System.ComponentModel;
using Taijutsu.Domain;

namespace Taijutsu.Infrastructure.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SupervisorContext
    {
        private const string DefaultDataContextSupervisorKey = "DataContextSupervisor";
        private const string DefaultReadOnlyDataContextSupervisorKey = "ReadOnlyDataContextSupervisor";

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static IDataContextSupervisor DataContextSupervisor
        {
            get
            {
                var supervisor = LogicalContext.FindData<DataContextSupervisor>(DefaultDataContextSupervisorKey);
                if (supervisor == null)
                {
                    supervisor = new DataContextSupervisor();
                    LogicalContext.SetData(DefaultDataContextSupervisorKey, supervisor);
                }
                return supervisor;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static IReadOnlyDataContextSupervisor ReadOnlyDataContextSupervisor
        {
            get
            {
                var supervisor = LogicalContext.FindData<ReadOnlyDataContextSupervisor>(DefaultReadOnlyDataContextSupervisorKey);
                if (supervisor == null)
                {
                    supervisor = new ReadOnlyDataContextSupervisor();
                    LogicalContext.SetData(DefaultReadOnlyDataContextSupervisorKey, supervisor);
                }
                return supervisor;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterUnitScopeWith(IDataProviderPlanningPolicy dataContextSharing)
        {
            LogicalContext.SetData(DefaultDataContextSupervisorKey,
                                           new DataContextSupervisor(dataContextSharing));
            LogicalContext.SetData(DefaultReadOnlyDataContextSupervisorKey,
                                           new ReadOnlyDataContextSupervisor(dataContextSharing));
        }
    }
}