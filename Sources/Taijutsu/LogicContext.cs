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

using System.Runtime.Remoting.Messaging;

namespace Taijutsu
{
    public static class LogicContext
    {
        private static ILogicContext defaultContext = new DefaultLogicContext();

        public static void Initialize(ILogicContext context)
        {
            defaultContext = context; // todo allow initialization only once
        }

        public static object FindData(string name)
        {
            return defaultContext.FindData(name);
        }

        public static void SetData(string name, object value)
        {
            defaultContext.SetData(name, value);
        }

        public static void ReleaseData(string name)
        {
            defaultContext.ReleaseData(name);
        }


        private class DefaultLogicContext : ILogicContext
        {
            object ILogicContext.FindData(string name)
            {
                return CallContext.GetData(name);
            }

            void ILogicContext.SetData(string name, object value)
            {
                CallContext.SetData(name, value);
            }

            void ILogicContext.ReleaseData(string name)
            {
                CallContext.FreeNamedDataSlot(name);
            }
        }
    }
}