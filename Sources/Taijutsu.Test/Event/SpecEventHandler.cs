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

using System;

using Taijutsu.Event;

namespace Taijutsu.Test.Event
{
    public class SpecEventHandler<T> : ISpecEventHandler<T> where T : IEvent
    {
        private readonly Action<T> handler;

        private readonly Func<T, bool> filter;

        public SpecEventHandler(Action<T> handler, Func<T, bool> filter = null)
        {
            this.handler = handler;
            this.filter = filter;
        }

        public void Handle(T ev)
        {
            handler(ev);
        }

        public bool IsSatisfiedBy(T ev)
        {
            return filter == null || filter(ev);
        }
    }
}