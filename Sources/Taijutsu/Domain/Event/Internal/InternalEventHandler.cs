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
namespace Taijutsu.Domain.Event.Internal
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IInternalEventHandler
    {
        Action<object> HandlerAction { get; }

        Type EventType { get; }

        int Priority { get; }
    }

    internal class InternalEventHandler<TEvent> : IInternalEventHandler
        where TEvent : class
    {
        private readonly Type eventType;

        private readonly Action<TEvent> handlerAction;

        private readonly Predicate<TEvent> predicate;

        private readonly int priority;

        public InternalEventHandler(Action<TEvent> handlerAction, Predicate<TEvent> predicate, int priority)
        {
            this.eventType = typeof(TEvent);
            this.handlerAction = handlerAction;
            this.predicate = predicate;
            this.priority = priority;
        }

        int IInternalEventHandler.Priority
        {
            get
            {
                return this.priority;
            }
        }

        Action<object> IInternalEventHandler.HandlerAction
        {
            get
            {
                return e =>
                    {
                        var ev = e as TEvent;
                        if (ev != null && this.predicate(ev))
                        {
                            this.handlerAction(ev);
                        }
                    };
            }
        }

        Type IInternalEventHandler.EventType
        {
            get
            {
                return this.eventType;
            }
        }
    }
}