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
using System.Transactions;
using Taijutsu.Data.Internal;
using Taijutsu.Domain.Event.Internal;

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

        // ReSharper disable InconsistentNaming
        
        public interface HandledSafelySyntax<out TSource>
        {
            Action Subscribe(Action<TSource> subscriber, int priority = 0);
        }

        // ReSharper restore InconsistentNaming
    }
}