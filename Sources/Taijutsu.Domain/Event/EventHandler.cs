﻿// Copyright 2009-2011 Taijutsu.
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

namespace Taijutsu.Domain.Event
{
    public interface IEventHandler
    {
        Action<object> HandlerAction { get; }
        Type EventType { get; }
        bool Suitable(object ev);
    }

    public class EventHandler<TEvent> : IEventHandler where TEvent : class
    {
        private readonly Type eventType;
        private readonly Action<TEvent> handlerAction;
        private readonly Predicate<TEvent> predicate;

        public EventHandler(Action<TEvent> handlerAction, Predicate<TEvent> predicate)
        {
            eventType = typeof (TEvent);
            this.handlerAction = handlerAction;
            this.predicate = predicate;
        }

        #region IEventHandler Members

        bool IEventHandler.Suitable(object ev)
        {
            var typedEv = ev as TEvent;
            return typedEv != null && predicate(typedEv);
        }

        Action<object> IEventHandler.HandlerAction
        {
            get
            {
                return e =>
                           {
                               var ev = e as TEvent;
                               if (ev != null)
                               {
                                   handlerAction(ev);
                               }
                           };
            }
        }

        public Type EventType
        {
            get { return eventType; }
        }

        #endregion
    }
}