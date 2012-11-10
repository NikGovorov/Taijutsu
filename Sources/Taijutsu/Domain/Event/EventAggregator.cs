// Copyright 2009-2012 Taijutsu.
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

using System.ComponentModel;
using Taijutsu.Domain.Event.Internal;
using Taijutsu.Domain.Event.Syntax.Subscribing;

namespace Taijutsu.Domain.Event
{
    public class EventAggregator
    {
        protected static readonly IEventAggregator InternalEventAggregator = new MultiThreadAggregator();

        public static void Publish<TEvent>(TEvent ev) where TEvent : IEvent
        {
            InternalEventAggregator.Publish(ev);
        }

        public static Syntax.Publishing.DueToSyntax.Init<TFact> DueTo<TFact>(TFact fact) where TFact : IFact
        {
            return InternalEventAggregator.DueTo(fact);
        }

        public static IObservableSyntax OnStream
        {
            get { return InternalEventAggregator.OnStream; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEventAggregator Instance
        {
            get { return InternalEventAggregator; }
        }
    }
}