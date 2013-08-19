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

using System.ComponentModel;

using Taijutsu.Event.Internal;

namespace Taijutsu.Event
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EventsExtensions
    {
        // ReSharper disable once UnusedParameter.Global
        public static void Batch<TEvent>(this IEvents<TEvent> self, int priority = 0, DelayUntil until = DelayUntil.Finished) where TEvent : class, IEvent
        {
            Events.Subscribe(new BatchedHandlingSettings(typeof(TEvent), until, priority));
        }

        public static void BatchUntilPreCompleted<TEvent>(this IEvents<TEvent> self, int priority = 0) where TEvent : class, IEvent
        {
            self.Batch(priority, DelayUntil.PreCompleted);
        }

        public static void BatchUntilCompleted<TEvent>(this IEvents<TEvent> self, int priority = 0) where TEvent : class, IEvent
        {
            self.Batch(priority, DelayUntil.Completed);
        }

        public static void BatchUntilFinished<TEvent>(this IEvents<TEvent> self, int priority = 0) where TEvent : class, IEvent
        {
            self.Batch(priority);
        }

        public static ISubscriptionSyntax<TEvent> Deferred<TEvent>(this IEvents<TEvent> self, DelayUntil until = DelayUntil.Finished) where TEvent : class, IEvent
        {
            return new SubscriptionSyntax<TEvent>(origin => Events.Subscribe(new DeferredHandlingSettings(origin, until)));
        }

        public static ISubscriptionSyntax<TEvent> DeferredUntilPreCompleted<TEvent>(this IEvents<TEvent> self) where TEvent : class, IEvent
        {
            return self.Deferred(DelayUntil.PreCompleted);
        }

        public static ISubscriptionSyntax<TEvent> DeferredUntilCompleted<TEvent>(this IEvents<TEvent> self) where TEvent : class, IEvent
        {
            return self.Deferred(DelayUntil.Completed);
        }

        public static ISubscriptionSyntax<TEvent> DeferredUntilFinished<TEvent>(this IEvents<TEvent> self) where TEvent : class, IEvent
        {
            return self.Deferred();
        }
    }
}