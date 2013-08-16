// Copyright 2009-2013 Nikita Govorov
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System.Runtime.Remoting.Messaging;

namespace Taijutsu
{
    public class DefaultLogicContext : ILogicContext
    {
        public bool Applicable
        {
            get { return true; }
        }

        public object FindData(string name)
        {
            return CallContext.GetData(name);
        }

        public void SetData(string name, object value)
        {
            CallContext.SetData(name, value);
        }

        public void ReleaseData(string name)
        {
            CallContext.FreeNamedDataSlot(name);
        }
    }
}