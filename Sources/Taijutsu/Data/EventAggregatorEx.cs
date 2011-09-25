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
using System.ComponentModel;
using System.Threading.Tasks;
using System.Transactions;
using Taijutsu.Data.Internal;
using Taijutsu.Domain;
using Taijutsu.Domain.Event;
using Taijutsu.Domain.Event.Syntax.Subscribing;
using DueToSyntax = Taijutsu.Domain.Event.Syntax.Publishing.DueToSyntax;

namespace Taijutsu.Data
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EventAggregatorEx
    {
        public static AddressedToExSyntax<TFact> Outside<TFact>(this DueToSyntax.Init<TFact> self,
                                                                string dataSource = "") where TFact : IFact
        {
            return new AddressedToExSyntaxImpl<TFact>(dataSource, self);
        }

        public static AfterCompleteSyntax<TSource> AfterComplete<TSource>(this SubscriptionSyntax.All<TSource> self)
        {
            return new AfterCompleteSyntaxAllImpl<TSource>(self);
        }

        public static AfterCompleteSyntax<TProjection> AfterComplete<TSource, TProjection>(
            this SubscriptionSyntax.Projection<TSource, TProjection> self)
        {
            return new AfterCompleteSyntaxProjectionImpl<TSource, TProjection>(self);
        }

        // ReSharper disable InconsistentNaming

        #region Nested type: AddressedToExSyntax

        public interface AddressedToExSyntax<TFact> where TFact : IFact
            // ReSharper restore InconsistentNaming
        {
            ExternalDateSyntax<TTarget, TFact> AddressedTo<TTarget>(object key) where TTarget : class, IQueryableEntity;
        }

        #endregion

        #region Nested type: AddressedToExSyntaxImpl

        private class AddressedToExSyntaxImpl<TFact> : AddressedToExSyntax<TFact> where TFact : IFact
        {
            private readonly string dataSource;
            private readonly DueToSyntax.Init<TFact> prev;

            public AddressedToExSyntaxImpl(string dataSource, DueToSyntax.Init<TFact> prev)
            {
                this.dataSource = dataSource;
                this.prev = prev;
            }

            #region AddressedToExSyntax<TFact> Members

            ExternalDateSyntax<TTarget, TFact> AddressedToExSyntax<TFact>.AddressedTo<TTarget>(object key)
            {
                return new ExternalDateSyntaxImpl<TTarget, TFact>(key, dataSource, null, null, prev);
            }

            #endregion
        }

        #endregion

        // ReSharper disable InconsistentNaming

        // ReSharper disable InconsistentNaming

        #region Nested type: AfterCompleteSyntax

        public interface AfterCompleteSyntax<out TSource>
            // ReSharper restore InconsistentNaming
        {
            Action Subscribe(Action<TSource> subscriber, int priority = 0);
        }

        #endregion

        #region Nested type: AfterCompleteSyntaxAllImpl

        private class AfterCompleteSyntaxAllImpl<TSource> : AfterCompleteSyntaxImpl<TSource>
        {
            private SubscriptionSyntax.All<TSource> target;

            public AfterCompleteSyntaxAllImpl(SubscriptionSyntax.All<TSource> target)
            {
                this.target = target;
            }

            public override Action Subscribe(Action<TSource> subscriber, int priority = 0)
            {
                return target.Subscribe(WrapSubscriber(subscriber), priority);
            }
        }

        #endregion

        #region Nested type: AfterCompleteSyntaxImpl

        private abstract class AfterCompleteSyntaxImpl<TSource> : AfterCompleteSyntax<TSource>
        {
            #region AfterCompleteSyntax<TSource> Members

            public abstract Action Subscribe(Action<TSource> subscriber, int priority = 0);

            #endregion

            protected virtual Action<TSource> WrapSubscriber(Action<TSource> subscriber)
            {
                return source =>
                           {
                               var currentTran = Transaction.Current;

                               if (currentTran != null)
                               {
                                   currentTran.TransactionCompleted += (s, e) =>
                                                                           {
                                                                               if (
                                                                                   e.Transaction.TransactionInformation.
                                                                                       Status ==
                                                                                   TransactionStatus.Committed)
                                                                               {
                                                                                   subscriber(source);
                                                                               }
                                                                           };
                               }
                               else
                               {
                                   var maybeUnitOfWork = Infrastructure.CurrentUnitOfWork;
                                   if (maybeUnitOfWork)
                                   {
                                       maybeUnitOfWork.Value.Closed +=
                                           (success) =>
                                               {
                                                   if (success)
                                                   {
                                                       subscriber(source);
                                                   }
                                               };
                                   }
                                   //todo warn here...    
                               }
                           };
            }
        }

        #endregion

        #region Nested type: AfterCompleteSyntaxProjectionImpl

        private class AfterCompleteSyntaxProjectionImpl<TSource, TProjection> : AfterCompleteSyntaxImpl<TProjection>
        {
            private SubscriptionSyntax.Projection<TSource, TProjection> target;

            public AfterCompleteSyntaxProjectionImpl(SubscriptionSyntax.Projection<TSource, TProjection> target)
            {
                this.target = target;
            }

            public override Action Subscribe(Action<TProjection> subscriber, int priority = 0)
            {
                return target.Subscribe(WrapSubscriber(subscriber), priority);
            }
        }

        #endregion

        // ReSharper disable InconsistentNaming

        #region Nested type: ExternalDateSyntax

        public interface ExternalDateSyntax<TTarget, TFact> : PublishingExSyntax where TFact : IFact
            // ReSharper restore InconsistentNaming
        {
            NoticeExSyntax NoticedAt(DateTime noticeDate);
            OccurExSyntax OccuredAt(DateTime occurrenceDate);
        }

        #endregion

        #region Nested type: ExternalDateSyntaxImpl

        private class ExternalDateSyntaxImpl<TTarget, TFact> : ExternalDateSyntax<TTarget, TFact>,
                                                               OccurExSyntax,
                                                               NoticeExSyntax where TFact : IFact
                                                                              where TTarget : class, IQueryableEntity
        {
            private readonly string dataSource;
            private readonly object key;
            private readonly DueToSyntax.Init<TFact> prev;
            private DateTime? noticeDate;
            private DateTime? occurrenceDate;

            public ExternalDateSyntaxImpl(object key, string dataSource, DateTime? noticeDate, DateTime? occurrenceDate,
                                          DueToSyntax.Init<TFact> prev)
            {
                this.key = key;
                this.dataSource = dataSource;
                this.noticeDate = noticeDate;
                this.occurrenceDate = occurrenceDate;
                this.prev = prev;
            }

            #region ExternalDateSyntax<TTarget,TFact> Members

            void PublishingExSyntax.Publish(bool async)
            {
                Action publish = () =>
                                     {
                                         using (var uow = new UnitOfWork(dataSource))
                                         {
                                             var target = uow.UniqueOf<TTarget>(key).Query();
                                             if (noticeDate == null && occurrenceDate == null)
                                                 prev.AddressedTo(target).Publish();
                                             else if (noticeDate != null && occurrenceDate == null)
                                                 prev.NoticedAt(noticeDate.Value).AddressedTo(target).Publish();
                                             else if (noticeDate == null && occurrenceDate != null)
                                                 prev.OccuredAt(occurrenceDate.Value).AddressedTo(target).Publish();
                                             else if (noticeDate != null && occurrenceDate != null)
                                                 prev.OccuredAt(occurrenceDate.Value).NoticedAt(noticeDate.Value).
                                                     AddressedTo(target).Publish();
                                             uow.Complete();
                                         }
                                     };

                if (async)
                {
                    Task.Factory.StartNew(publish); //todo log here TaskContinuationOptions.OnlyOnFaulted
                }
                else
                {
                    publish();
                }
            }

            NoticeExSyntax ExternalDateSyntax<TTarget, TFact>.NoticedAt(DateTime date)
            {
                return new ExternalDateSyntaxImpl<TTarget, TFact>(key, dataSource, date, occurrenceDate, prev);
            }

            OccurExSyntax ExternalDateSyntax<TTarget, TFact>.OccuredAt(DateTime date)
            {
                return new ExternalDateSyntaxImpl<TTarget, TFact>(key, dataSource, noticeDate, date, prev);
            }

            #endregion

            #region NoticeExSyntax Members

            PublishingExSyntax NoticeExSyntax.OccuredAt(DateTime date)
            {
                return new ExternalDateSyntaxImpl<TTarget, TFact>(key, dataSource, noticeDate, date, prev);
            }

            #endregion

            #region OccurExSyntax Members

            PublishingExSyntax OccurExSyntax.NoticedAt(DateTime date)
            {
                return new ExternalDateSyntaxImpl<TTarget, TFact>(key, dataSource, date, occurrenceDate, prev);
            }

            #endregion
        }

        #endregion

        // ReSharper disable InconsistentNaming

        #region Nested type: NoticeExSyntax

        public interface NoticeExSyntax : PublishingExSyntax
            // ReSharper restore InconsistentNaming
        {
            PublishingExSyntax OccuredAt(DateTime occurrenceDate);
        }

        #endregion

        // ReSharper disable InconsistentNaming

        #region Nested type: OccurExSyntax

        public interface OccurExSyntax : PublishingExSyntax
            // ReSharper restore InconsistentNaming
        {
            PublishingExSyntax NoticedAt(DateTime noticeDate);
        }

        #endregion

        // ReSharper disable InconsistentNaming

        #region Nested type: PublishingExSyntax

        public interface PublishingExSyntax
            // ReSharper restore InconsistentNaming
        {
            void Publish(bool async = false);
        }

        #endregion
    }
}