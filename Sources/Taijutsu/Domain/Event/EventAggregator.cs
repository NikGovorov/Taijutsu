#region License

// Copyright 2009-2012 Taijutsu.
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
using System.ComponentModel;
using Taijutsu.Domain.Event.Internal;
using Taijutsu.Domain.Event.Syntax.Subscribing;

namespace Taijutsu.Domain.Event
{
    public class EventAggregator
    {
        private const string LocalEventAggregatorName = "LocalEventAggregator";

        private static readonly IEventAggregator globalEventAggregator = new MultiThreadAggregator();

        private static IEventAggregator FindLocalEventAggregator()
        {
            return LogicContext.FindData(LocalEventAggregatorName) as IEventAggregator;
        }

        public static void Publish<TEvent>(TEvent ev) where TEvent : IEvent
        {
            var localEventAggregator = FindLocalEventAggregator();

            if (localEventAggregator != null)
            {
                localEventAggregator.Publish(ev);
            }

            globalEventAggregator.Publish(ev);
        }

        public static IEventStream OnStream
        {
            get { return globalEventAggregator.OnStream; }
        }

        public static SubscriptionSyntax.All<TEvent> OnStreamOf<TEvent>() where TEvent : class, IEvent
        {
            return globalEventAggregator.OnStream.Of<TEvent>();
        }

        public static Action Subscribe<TEvent>(Action<TEvent> subscriber, int priority = 0) where TEvent : class, IEvent
        {
            return globalEventAggregator.OnStream.Of<TEvent>().Subscribe(subscriber, priority);
        }

        public static Action Subscribe<TEvent>(IHandler<TEvent> subscriber, int priority = 0)
            where TEvent : class, IEvent
        {
            return globalEventAggregator.OnStream.Of<TEvent>().Subscribe(subscriber, priority);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEventAggregator Global
        {
            get { return globalEventAggregator; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEventAggregator Local
        {
            get
            {
                var localEventAggregator = FindLocalEventAggregator();

                if (localEventAggregator == null)
                {
                    localEventAggregator = new SingleThreadAggregator();
                    LogicContext.SetData(LocalEventAggregatorName, localEventAggregator);
                }

                return localEventAggregator;
            }
        }
    }
}