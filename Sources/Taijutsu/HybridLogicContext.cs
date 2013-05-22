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
namespace Taijutsu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class HybridLogicContext : ILogicContext
    {
        private readonly IEnumerable<ILogicContext> contexts;

        public HybridLogicContext(IEnumerable<ILogicContext> contexts)
        {
            if (contexts == null)
            {
                throw new ArgumentNullException("contexts");
            }

            this.contexts = contexts.ToList();
        }

        public bool Applicable
        {
            get
            {
                return true;
            }
        }

        object ILogicContext.FindData(string name)
        {
            return this.DetermineContext().FindData(name);
        }

        void ILogicContext.SetData(string name, object value)
        {
            this.DetermineContext().SetData(name, value);
        }

        void ILogicContext.ReleaseData(string name)
        {
            this.DetermineContext().ReleaseData(name);
        }

        private ILogicContext DetermineContext()
        {
            var context = this.contexts.FirstOrDefault(c => c.Applicable);

            if (context == null)
            {
                throw new Exception("Unable to determine context implementation in the current runtime environment.");
            }

            return context;
        }
    }
}