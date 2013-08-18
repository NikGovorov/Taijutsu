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

        public override IDisposable Subscribe(IEventHandlerSettings handlerSettings)
        {
            if (!typeof(IEvent).IsAssignableFrom(handlerSettings.Type))
            {
                throw new Exception(string.Format("'{0}' does not implement '{1}'.", handlerSettings.Type, typeof(IEvent)));
            }

            lock (sync)
            {
                IList<IEventHandlerSettings> internalEventHandlers;
                if (!Handlers.TryGetValue(handlerSettings.Type, out internalEventHandlers))
                {
                    var newHandlers = new Dictionary<Type, IList<IEventHandlerSettings>>(Handlers) { { handlerSettings.Type, new List<IEventHandlerSettings> { handlerSettings } } };

                    Handlers = newHandlers;
                }
                else
                {
                    var newInternalEventHandlers = new List<IEventHandlerSettings>(internalEventHandlers) { handlerSettings };
                    Handlers[handlerSettings.Type] = newInternalEventHandlers;
                }
            }

            return UnsubscriptionAction(handlerSettings);
        }

        protected override IDisposable UnsubscriptionAction(IEventHandlerSettings handlerSettings)
        {
            Action action = delegate
            {
                lock (sync)
                {
                    IList<IEventHandlerSettings> internalEventHandlers;
                    if (!Handlers.TryGetValue(handlerSettings.Type, out internalEventHandlers))
                    {
                        return;
                    }

                    var newInternalEventHandlers = new List<IEventHandlerSettings>(internalEventHandlers);
                    if (newInternalEventHandlers.Remove(handlerSettings))
                    {
                        Handlers[handlerSettings.Type] = newInternalEventHandlers;
                    }
                }
            };

            return action.AsDisposable();
        }

        protected override void Reset()
        {
            lock (sync)
            {
                Handlers = new Dictionary<Type, IList<IEventHandlerSettings>>();
            }
        }
    }
}