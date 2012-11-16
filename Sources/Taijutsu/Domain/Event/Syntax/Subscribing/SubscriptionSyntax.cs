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

using System;
using System.Collections.Generic;
using System.Linq;
using Taijutsu.Domain.Event.Internal;

namespace Taijutsu.Domain.Event.Syntax.Subscribing
{
    public static class SubscriptionSyntax
    {
        // ReSharper disable InconsistentNaming

        #region Nested type: All

        public interface All<out TSource> : IHiddenObjectMembers
        {
            All<TSource> Where(Func<TSource, bool> filter);
            Projection<TSource, TProjection> Select<TProjection>(Func<TSource, TProjection> projection);
            Action Subscribe(Action<TSource> subscriber, int priority = 0);
            Action Subscribe(IHandlerOf<TSource> subscriber, int priority = 0);
        }

        #endregion

        #region Nested type: AllImpl

        internal class AllImpl<TEvent> : All<TEvent>
            where TEvent : class
        {
            private readonly Func<IInternalEventHandler, Action> addHadlerAction;
            private readonly List<Func<TEvent, bool>> eventFilters = new List<Func<TEvent, bool>>();

            internal AllImpl(Func<IInternalEventHandler, Action> addHadlerAction,
                             IEnumerable<Func<TEvent, bool>> eventFilters = null)
            {
                this.addHadlerAction = addHadlerAction;
                if (eventFilters != null)
                {
                    this.eventFilters.AddRange(eventFilters);
                }
            }

            public Func<IInternalEventHandler, Action> AddHadlerAction
            {
                get { return addHadlerAction; }
            }

            public IEnumerable<Func<TEvent, bool>> EventFilters
            {
                get { return eventFilters; }
            }

            #region All<TEvent> Members

            public All<TEvent> Where(Func<TEvent, bool> filter)
            {
                eventFilters.Add(filter);
                return new AllImpl<TEvent>(AddHadlerAction, eventFilters);
            }

            public Projection<TEvent, TProjection> Select<TProjection>(
                Func<TEvent, TProjection> projection)
            {
                return new ProjectionImpl<TEvent, TProjection>(this, projection);
            }

            public Action Subscribe(Action<TEvent> subscriber, int priority = 0)
            {
                return
                    AddHadlerAction(new InternalEventHandler<TEvent>(subscriber, e => !eventFilters.Any(f => !f(e)), priority));
            }

            public Action Subscribe(IHandlerOf<TEvent> subscriber, int priority = 0)
            {
                return Subscribe(subscriber.Handle, priority);
            }

            #endregion
        }

        #endregion


        #region Nested type: Projection

        public interface Projection<out TSource, out TProjection> : IHiddenObjectMembers
        {
            Projection<TSource, TProjection> Where(Func<TProjection, bool> filter);
            Action Subscribe(Action<TProjection> subscriber, int priority = 0);
            Action Subscribe(IHandlerOf<TProjection> subscriber, int priority = 0);
        }

        #endregion

        #region Nested type: ProjectionImpl

        internal class ProjectionImpl<TEvent, TProjection> :
            Projection<TEvent, TProjection> where TEvent : class
        {
            private readonly AllImpl<TEvent> parent;
            private readonly Func<TEvent, TProjection> projection;
            private readonly List<Func<TProjection, bool>> projectionFilters = new List<Func<TProjection, bool>>();

            internal ProjectionImpl(AllImpl<TEvent> parent, Func<TEvent, TProjection> projection)
            {
                this.parent = parent;
                this.projection = projection;
            }

            internal ProjectionImpl(AllImpl<TEvent> parent, Func<TEvent, TProjection> projection,
                                    IEnumerable<Func<TProjection, bool>> projectionFilters)
                : this(parent, projection)
            {
                this.projectionFilters.AddRange(projectionFilters);
            }

            #region Projection<TEvent,TProjection> Members

            public Projection<TEvent, TProjection> Where(Func<TProjection, bool> filter)
            {
                return new ProjectionImpl<TEvent, TProjection>(parent, projection, projectionFilters);
            }

            public Action Subscribe(Action<TProjection> subscriber, int priority = 0)
            {
                Action<TEvent> evSubscriber = e => subscriber(projection(e));
                return parent.AddHadlerAction(new InternalEventHandler<TEvent>(evSubscriber,
                                                                       e =>
                                                                       !parent.EventFilters.Any(f => !f(e)) &&
                                                                       !projectionFilters.Any(f => !f(projection(e))),
                                                                       priority));
            }

            public Action Subscribe(IHandlerOf<TProjection> subscriber, int priority = 0)
            {
                return Subscribe(subscriber.Handle, priority);
            }

            #endregion
        }

        #endregion

        // ReSharper restore InconsistentNaming
    }
}