using System;
using System.Collections.Generic;
using System.Linq;

namespace Taijutsu.Domain.NewEvent.Syntax
{

    public static class SubscriptionSyntax
    {
        // ReSharper disable InconsistentNaming
        public interface All<out TSource> : IHideObjectMembers
        {
            All<TSource> Where(Func<TSource, bool> filter);
            Projection<TSource, TProjection> Select<TProjection>(Func<TSource, TProjection> projection);
            Action Subscribe(Action<TSource> subscriber, int priority = 0);
        }

        public interface Projection<out TSource, out TProjection> : IHideObjectMembers
        {
            Projection<TSource, TProjection> Where(Func<TProjection, bool> filter);
            Action Subscribe(Action<TProjection> subscriber, int priority = 0);
        }
        // ReSharper restore InconsistentNaming        


        internal class AllImpl<TEvent> : SubscriptionSyntax.All<TEvent>
           where TEvent : class
        {
            private readonly Func<IEventHandler, Action> addHadlerAction;
            private readonly List<Func<TEvent, bool>> eventFilters = new List<Func<TEvent, bool>>();

            internal AllImpl(Func<IEventHandler, Action> addHadlerAction,
                                        IEnumerable<Func<TEvent, bool>> eventFilters = null)
            {
                this.addHadlerAction = addHadlerAction;
                if (eventFilters != null)
                {
                    this.eventFilters.AddRange(eventFilters);
                }

            }

            public Func<IEventHandler, Action> AddHadlerAction
            {
                get { return addHadlerAction; }
            }

            public  IEnumerable<Func<TEvent, bool>> EventFilters
            {
                get { return eventFilters; }
            }

            #region All<TEvent> Members

            public SubscriptionSyntax.All<TEvent> Where(Func<TEvent, bool> filter)
            {
                eventFilters.Add(filter);
                return new AllImpl<TEvent>(AddHadlerAction, eventFilters);
            }

            public SubscriptionSyntax.Projection<TEvent, TProjection> Select<TProjection>(
                Func<TEvent, TProjection> projection)
            {
                return new ProjectionImpl<TEvent, TProjection>(this, projection);
            }

            public Action Subscribe(Action<TEvent> subscriber, int priority = 0)
            {
                
                return
                    AddHadlerAction(new EventHandler<TEvent>(subscriber, e => !eventFilters.Any(f => !f(e)), priority));
            }

            #endregion
        }

        internal class ProjectionImpl<TEvent, TProjection> :
              SubscriptionSyntax.Projection<TEvent, TProjection> where TEvent : class
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

            public SubscriptionSyntax.Projection<TEvent, TProjection> Where(Func<TProjection, bool> filter)
            {
                return new ProjectionImpl<TEvent, TProjection>(parent, projection, projectionFilters);
            }

            public Action Subscribe(Action<TProjection> subscriber, int priority = 0)
            {
                Action<TEvent> evSubscriber = e => subscriber(projection(e));
                return parent.AddHadlerAction(new EventHandler<TEvent>(evSubscriber,
                                                                       e =>
                                                                       !parent.EventFilters.Any(f => !f(e)) &&
                                                                       !projectionFilters.Any(f => !f(projection(e))),
                                                                       priority));
            }

            #endregion
        }

    }
}