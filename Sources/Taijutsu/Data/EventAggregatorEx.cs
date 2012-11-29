#region License

//  Copyright 2009-2013 Nikita Govorov
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
using System.Diagnostics;
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

        public static HandledSafelySyntax<TSource> HandledSafely<TSource>(this SubscriptionSyntax.All<TSource> self)
        {
            return new HandledSafelySyntaxAllImpl<TSource>(self);
        }

        public static HandledSafelySyntax<TProjection> HandledSafely<TSource, TProjection>
            (this SubscriptionSyntax.Projection<TSource, TProjection> self)
        {
            return new HandledSafelySyntaxProjectionImpl<TSource, TProjection>(self);
        }

        private class HandledSafelySyntaxAllImpl<TSource> : HandledSafelySyntaxImpl<TSource>
        {
            private readonly SubscriptionSyntax.All<TSource> target;

            public HandledSafelySyntaxAllImpl(SubscriptionSyntax.All<TSource> target)
            {
                this.target = target;
            }

            public override Action Subscribe(Action<TSource> subscriber, int priority = 0)
            {
                return target.Subscribe(WrapSubscriber(subscriber), priority);
            }
        }

        private class HandledSafelySyntaxProjectionImpl<TSource, TProjection> : HandledSafelySyntaxImpl<TProjection>
        {
            private readonly SubscriptionSyntax.Projection<TSource, TProjection> target;

            public HandledSafelySyntaxProjectionImpl(SubscriptionSyntax.Projection<TSource, TProjection> target)
            {
                this.target = target;
            }

            public override Action Subscribe(Action<TProjection> subscriber, int priority = 0)
            {
                return target.Subscribe(WrapSubscriber(subscriber), priority);
            }
        }

        private abstract class HandledSafelySyntaxImpl<TSource> : HandledSafelySyntax<TSource>
        {
            public abstract Action Subscribe(Action<TSource> subscriber, int priority = 0);

            protected virtual Action<TSource> WrapSubscriber(Action<TSource> subscriber)
            {
                return source =>
                    {
                        var transaction = Transaction.Current;

                        if (transaction != null)
                        {
                            TransactionCompletedEventHandler action = null;

                            action = (s, e) =>
                                {
                                    transaction.TransactionCompleted -= action;

                                    if (e.Transaction.TransactionInformation.Status == TransactionStatus.Committed)
                                    {
                                        subscriber(source);
                                    }
                                };

                            transaction.TransactionCompleted += action;
                        }
                        else
                        {
                            var context = InternalEnvironment.DataContextSupervisor.CurrentContext;

                            if (context != null)
                            {
                                Action<bool> action = isSuccessfully =>
                                    {
                                        if (isSuccessfully)
                                        {
                                            subscriber(source);
                                        }
                                    };

                                context.Finished += isSuccessfully =>
                                    {
                                        context.Finished -= action;
                                        action(isSuccessfully);
                                    };
                            }
                            else
                            {
                                Trace.TraceWarning("Event source is not surrounded with unit of work or with transaction scope.");
                            }
                        }
                    };
            }
        }

        public static AddressedToExSyntax<TFact> HappenedOutside<TFact>(this DueToSyntax.Init<TFact> self,
                                                               string dataSource = "") where TFact : IFact
        {
            return new AddressedToExSyntaxImpl<TFact>(dataSource, self);
        }

        private class AddressedToExSyntaxImpl<TFact> : AddressedToExSyntax<TFact> where TFact : IFact
        {
            private readonly string dataSource;
            private readonly DueToSyntax.Init<TFact> prev;

            public AddressedToExSyntaxImpl(string dataSource, DueToSyntax.Init<TFact> prev)
            {
                this.dataSource = dataSource;
                this.prev = prev;
            }

            ExternalDateSyntax<TRecipient, TFact> AddressedToExSyntax<TFact>.AddressedTo<TRecipient>(object key)
            {
                return new ExternalDateSyntaxImpl<TRecipient, TFact>(key, dataSource, null, null, prev);
            }
        }

        private class ExternalDateSyntaxImpl<TRecipient, TFact> : ExternalDateSyntax<TRecipient, TFact>,
                                                                  OccurExSyntax,
                                                                  NoticeExSyntax
            where TFact : IFact
            where TRecipient : class, IQueryableEntity
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

            void PublishingExSyntax.Publish(bool async)
            {
                Action publish = () =>
                    {
                        using (var uow = new UnitOfWork(dataSource))
                        {
                            var target = uow.UniqueOf<TRecipient>(key).Query();
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
                    Task.Factory
                        .StartNew(publish)
                        .ContinueWith(t => Trace.TraceError(t.Exception == null ? string.Format("Error occurred during async event publishing for '{0}'.", typeof(TRecipient)) : t.Exception.ToString()),
                              TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
                }
                else
                {
                    publish();
                }
            }

            NoticeExSyntax ExternalDateSyntax<TRecipient, TFact>.NoticedAt(DateTime date)
            {
                return new ExternalDateSyntaxImpl<TRecipient, TFact>(key, dataSource, date, occurrenceDate, prev);
            }

            OccurExSyntax ExternalDateSyntax<TRecipient, TFact>.OccuredAt(DateTime date)
            {
                return new ExternalDateSyntaxImpl<TRecipient, TFact>(key, dataSource, noticeDate, date, prev);
            }

            PublishingExSyntax NoticeExSyntax.OccuredAt(DateTime date)
            {
                return new ExternalDateSyntaxImpl<TRecipient, TFact>(key, dataSource, noticeDate, date, prev);
            }

            PublishingExSyntax OccurExSyntax.NoticedAt(DateTime date)
            {
                return new ExternalDateSyntaxImpl<TRecipient, TFact>(key, dataSource, date, occurrenceDate, prev);
            }
        }

        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedTypeParameter

        public interface HandledSafelySyntax<out TSource>
        {
            Action Subscribe(Action<TSource> subscriber, int priority = 0);
        }

        public interface AddressedToExSyntax<TFact> where TFact : IFact
        {
            ExternalDateSyntax<TRecipient, TFact> AddressedTo<TRecipient>(object key)
                where TRecipient : class, IQueryableEntity;
        }

        public interface ExternalDateSyntax<TRecipient, TFact> : PublishingExSyntax where TFact : IFact
        {
            NoticeExSyntax NoticedAt(DateTime noticeDate);
            OccurExSyntax OccuredAt(DateTime occurrenceDate);
        }

        public interface NoticeExSyntax : PublishingExSyntax
        {
            PublishingExSyntax OccuredAt(DateTime occurrenceDate);
        }

        public interface OccurExSyntax : PublishingExSyntax
        {
            PublishingExSyntax NoticedAt(DateTime noticeDate);
        }

        public interface PublishingExSyntax

        {
            void Publish(bool async = false);
        }

        // ReSharper restore UnusedTypeParameter
        // ReSharper restore InconsistentNaming
    }
}