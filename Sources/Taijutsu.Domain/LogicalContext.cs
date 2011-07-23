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

using System.Runtime.Remoting.Messaging;

namespace Taijutsu.Domain
{
    public static class LogicalContext
    {
        private static ILogicalContext defaultContext = new DefaultLogicalContext();


        public static void Initialize(ILogicalContext context)
        {
            defaultContext = context;
        }

        public static T FindData<T>(string name)
        {
            return defaultContext.FindData<T>(name);
        }

        public static void SetData<T>(string name, T value)
        {
            defaultContext.SetData(name, value);
        }

        public static void ReleaseData(string name)
        {
            defaultContext.ReleaseData(name);
        }

        #region Nested type: DefaultLogicalContext

        private class DefaultLogicalContext : ILogicalContext
        {
            #region ILogicalContext Members

            T ILogicalContext.FindData<T>(string name)
            {
                return (T) CallContext.GetData(name);
            }

            void ILogicalContext.SetData<T>(string name, T value)
            {
                CallContext.SetData(name, value);
            }

            void ILogicalContext.ReleaseData(string name)
            {
                CallContext.FreeNamedDataSlot(name);
            }

            #endregion
        }

        #endregion
    }
}