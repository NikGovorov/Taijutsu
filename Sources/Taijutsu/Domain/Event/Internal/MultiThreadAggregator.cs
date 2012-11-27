#region License

//  Copyright 2009-2013 Nikita Govorov
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

using System;
using System.Collections.Generic;

namespace Taijutsu.Domain.Event.Internal
{
    public class MultiThreadAggregator : SingleThreadAggregator
    {
        private readonly object sync = new object();

        protected override Action Subscribe(IInternalEventHandler handler)
        {
            lock (sync)
            {
                IList<IInternalEventHandler> internalEventHandlers;
                if (!Handlers.TryGetValue(handler.EventType, out internalEventHandlers))
                {
                    var newHandlers = new Dictionary<Type, IList<IInternalEventHandler>>(Handlers)
                        {
                            {handler.EventType, new List<IInternalEventHandler> {handler}}
                        };

                    Handlers = newHandlers;
                }
                else
                {
                    var newInternalEventHandlers = new List<IInternalEventHandler>(internalEventHandlers) {handler};
                    Handlers[handler.EventType] = newInternalEventHandlers;
                }
            }

            return UnsubscriptionAction(handler);
        }

        protected override Action UnsubscriptionAction(IInternalEventHandler handler)
        {
            return delegate
                {
                    lock (sync)
                    {
                        IList<IInternalEventHandler> internalEventHandlers;
                        if (Handlers.TryGetValue(handler.EventType, out internalEventHandlers))
                        {
                            var newInternalEventHandlers = new List<IInternalEventHandler>(internalEventHandlers);
                            if (newInternalEventHandlers.Remove(handler))
                            {
                                Handlers[handler.EventType] = newInternalEventHandlers;
                            }
                        }
                    }
                };
        }

        protected override void Dispose()
        {
            lock (sync)
            {
                Handlers = new Dictionary<Type, IList<IInternalEventHandler>>();
            }
        }
    }
}