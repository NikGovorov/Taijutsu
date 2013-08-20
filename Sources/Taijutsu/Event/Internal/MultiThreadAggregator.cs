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
using System.Collections.Generic;

namespace Taijutsu.Event.Internal
{
    public class MultiThreadAggregator : SingleThreadAggregator
    {
        private readonly object sync = new object();

        public override IDisposable Subscribe(IEventHandlingSettings handlingSettings)
        {
            if (handlingSettings == null)
            {
                throw new ArgumentNullException("handlingSettings");
            }

            if (!typeof(IEvent).IsAssignableFrom(handlingSettings.Type))
            {
                throw new Exception(string.Format("'{0}' does not implement '{1}'.", handlingSettings.Type, typeof(IEvent)));
            }

            lock (sync)
            {
                IList<IEventHandlingSettings> internalEventHandlers;
                if (!Handlers.TryGetValue(handlingSettings.Type, out internalEventHandlers))
                {
                    var newHandlers = new Dictionary<Type, IList<IEventHandlingSettings>>(Handlers) { { handlingSettings.Type, new List<IEventHandlingSettings> { handlingSettings } } };

                    Handlers = newHandlers;
                }
                else
                {
                    var newInternalEventHandlers = new List<IEventHandlingSettings>(internalEventHandlers) { handlingSettings };
                    Handlers[handlingSettings.Type] = newInternalEventHandlers;
                }
            }

            return UnsubscriptionAction(handlingSettings);
        }

        protected override IDisposable UnsubscriptionAction(IEventHandlingSettings handlingSettings)
        {
            Action action = delegate
            {
                lock (sync)
                {
                    IList<IEventHandlingSettings> internalEventHandlers;
                    if (!Handlers.TryGetValue(handlingSettings.Type, out internalEventHandlers))
                    {
                        return;
                    }

                    var newInternalEventHandlers = new List<IEventHandlingSettings>(internalEventHandlers);
                    if (newInternalEventHandlers.Remove(handlingSettings))
                    {
                        Handlers[handlingSettings.Type] = newInternalEventHandlers;
                    }
                }
            };

            return action.AsDisposable();
        }

        protected override void Reset()
        {
            lock (sync)
            {
                Handlers = new Dictionary<Type, IList<IEventHandlingSettings>>();
            }
        }
    }
}