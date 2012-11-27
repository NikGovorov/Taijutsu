#region License

// Copyright 2009-2013 Nikita Govorov
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

using System.Data;

namespace Taijutsu.Data.Internal
{
    public static class IsolationLevelExtensions
    {
        public static bool IsCompatible(this IsolationLevel isolationLevel, IsolationLevel comparing)
        {
            if (isolationLevel == IsolationLevel.Snapshot)
            {
                return comparing == IsolationLevel.Unspecified || comparing == IsolationLevel.Chaos ||
                       comparing == IsolationLevel.ReadUncommitted || comparing == IsolationLevel.ReadCommitted
                       || comparing == IsolationLevel.Snapshot;
            }

            if (comparing == IsolationLevel.Snapshot)
            {
                return isolationLevel == IsolationLevel.Snapshot || isolationLevel == IsolationLevel.RepeatableRead ||
                       comparing == IsolationLevel.Serializable;
            }

            return isolationLevel >= comparing;
        }
    }
}