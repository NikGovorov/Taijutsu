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

namespace Taijutsu.Event
{
    public interface IEvents
    {
        IEventStream OnStream { get; }

        SubscriptionSyntax.All<TEvent> OnStreamOf<TEvent>() where TEvent : class, IEvent;

        Action Subscribe<TEvent>(Action<TEvent> subscriber, int priority = 0) where TEvent : class, IEvent;

        Action Subscribe<TEvent>(IEventHandler<TEvent> subscriber, int priority = 0) where TEvent : class, IEvent;

        void Publish<TEvent>(TEvent ev) where TEvent : IEvent;
    }
}