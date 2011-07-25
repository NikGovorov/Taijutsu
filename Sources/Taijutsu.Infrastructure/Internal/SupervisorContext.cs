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
using System.ComponentModel;
using Taijutsu.Domain;
using System.Linq;

namespace Taijutsu.Infrastructure.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SupervisorContext
    {
        private const string DataContextSupervisorKey = "DataContextSupervisor";
        private const string ReadOnlyDataContextSupervisorKey = "ReadOnlyDataContextSupervisor";
        private const string OperationScopeKey = "OperationScope";

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static IDataContextSupervisor DataContextSupervisor
        {
            get
            {
                var supervisor = (DataContextSupervisor)LogicalContext.FindData(DataContextSupervisorKey);
                if (supervisor == null)
                {
                    supervisor = new DataContextSupervisor();
                    LogicalContext.SetData(DataContextSupervisorKey, supervisor);
                }
                return supervisor;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static IReadOnlyDataContextSupervisor ReadOnlyDataContextSupervisor
        {
            get
            {
                var supervisor = (ReadOnlyDataContextSupervisor)LogicalContext.FindData(ReadOnlyDataContextSupervisorKey);
                if (supervisor == null)
                {
                    supervisor = new ReadOnlyDataContextSupervisor();
                    LogicalContext.SetData(ReadOnlyDataContextSupervisorKey, supervisor);
                }
                return supervisor;
            }
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static bool CheckDataContextSupervisorForRelease()
        {
            if (LogicalContext.FindData(OperationScopeKey) == null)
            {
                var supervisor = LogicalContext.FindData(DataContextSupervisorKey);

                if (supervisor != null && !((DataContextSupervisor)supervisor).Roots.Any())
                {
                    LogicalContext.ReleaseData(DataContextSupervisorKey);
                    return true;
                }
            }
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static bool CheckReadOnlyDataContextSupervisorForRelease()
        {
            if (LogicalContext.FindData(OperationScopeKey) == null)
            {
                var supervisor = LogicalContext.FindData(ReadOnlyDataContextSupervisorKey);

                if (supervisor != null && !((ReadOnlyDataContextSupervisor)supervisor).Roots.Any())
                {
                    LogicalContext.ReleaseData(ReadOnlyDataContextSupervisorKey);
                    return true;
                }
            }
            return false;
        }


        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static void RegisterOperationScopeWith(IDataProviderPlanningPolicy dataContextSharing)
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
                                           new DataContextSupervisor(dataContextSharing));
            LogicalContext.SetData(ReadOnlyDataContextSupervisorKey,
                                           new ReadOnlyDataContextSupervisor(dataContextSharing));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static void UnRegisterOperationScope()
        {
            LogicalContext.ReleaseData(DataContextSupervisorKey);
            LogicalContext.ReleaseData(ReadOnlyDataContextSupervisorKey);
            LogicalContext.ReleaseData(OperationScopeKey);
        }
    }
}