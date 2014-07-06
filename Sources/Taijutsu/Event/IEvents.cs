// Copyright 2009-2014 Nikita Govorov
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
using System.ComponentModel;

using Taijutsu.Annotation;
using Taijutsu.Event.Internal;

namespace Taijutsu.Event
{
    [PublicApi]
    public interface IEvents
    {
        IEvents<TEvent> OfType<TEvent>() where TEvent : class, IEvent;

        ISubscriptionSyntax<TEvent> Where<TEvent>([NotNull] Func<TEvent, bool> filter) where TEvent : class, IEvent;

        [EditorBrowsable(EditorBrowsableState.Never)]
        IDisposable Subscribe([NotNull] IEventHandlingSettings handlingSettings);

        IDisposable Subscribe<TEvent>([NotNull] Action<TEvent> handler, int priority = 0) where TEvent : class, IEvent;

        [EditorBrowsable(EditorBrowsableState.Never)]
        void Publish([NotNull] object ev);

        void Publish<TEvent>([NotNull] TEvent ev) where TEvent : class, IEvent;
    }

    // ReSharper disable once UnusedTypeParameter
    public interface IEvents<TEvent> : ISubscriptionSyntax<TEvent> where TEvent : class, IEvent
    {
        void Publish([NotNull] TEvent ev);

        [EditorBrowsable(EditorBrowsableState.Never)]
        IDisposable Subscribe([NotNull] IEventHandlingSettings handlingSettings);
    }
}