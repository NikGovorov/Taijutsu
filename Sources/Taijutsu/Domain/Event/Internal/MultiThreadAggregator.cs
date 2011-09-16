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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Taijutsu.Domain.Event.Internal
{
    public class MultiThreadAggregator : SingleThreadAggregator
    {
        private readonly object sync = new object();

        protected override void CachePotentialSubscribers(Type type, IEnumerable<Type> potentialSubscribers)
        {
            Task.Factory.StartNew(() =>
                                      {
                                          var newTargets = new Dictionary<Type, IEnumerable<Type>>(targets);
                                          newTargets[type] = potentialSubscribers;
                                          targets = newTargets;
                                      });
        }

        protected override Action Subscribe(IInternalEventHandler handler)
        {
            lock (sync)
            {
                IList<IInternalEventHandler> handlers;
                if (!handlersDictionary.TryGetValue(handler.EventType, out handlers))
                {
                    var newHandlersDictionary = new Dictionary<Type, IList<IInternalEventHandler>>(handlersDictionary) { { handler.EventType, new List<IInternalEventHandler> { handler } } };
                    handlersDictionary = newHandlersDictionary;
                }
                else
                {
                    var newHandlers = new List<IInternalEventHandler>(handlers) { handler };
                    handlersDictionary[handler.EventType] = newHandlers;
                }
            }
            
            return GenerateUnsubscriptionAction(handler);
        }


        protected override  Action GenerateUnsubscriptionAction(IInternalEventHandler handler)
        {
            return delegate
            {
                lock (sync)
                {
                    IList<IInternalEventHandler> handlers;
                    if (handlersDictionary.TryGetValue(handler.EventType, out handlers))
                    {
                        var newHandlers = new List<IInternalEventHandler>(handlers);
                        if (newHandlers.Remove(handler))
                        {
                            handlersDictionary[handler.EventType] = newHandlers;    
                        }
                    }
                }
            };
        }

        protected override void Dispose()
        {
            lock (sync)
            {
                handlersDictionary = new Dictionary<Type, IList<IInternalEventHandler>>();
            }
        }
    }
}