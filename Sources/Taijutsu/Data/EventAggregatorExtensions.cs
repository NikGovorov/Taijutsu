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
using System.ComponentModel;
using System.Diagnostics;
using System.Transactions;

using Taijutsu.Data.Internal;
using Taijutsu.Event;

namespace Taijutsu.Data
{
    public interface IHandledSafelySyntax<out TSource>
    {
        Action Subscribe(Action<TSource> subscriber, int priority = 0);
    }

    [PublicApi]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EventAggregatorExtensions
    {
        public static IHandledSafelySyntax<TSource> HandledSafely<TSource>(this SubscriptionSyntax.All<TSource> self)
        {
            if (self == null)
            {
                throw new ArgumentNullException("self");
            }

            return new HandledSafelySyntaxAllImpl<TSource>(self);
        }

        private class HandledSafelySyntaxAllImpl<TSource> : HandledSafelySyntaxImpl<TSource>
        {
            private readonly SubscriptionSyntax.All<TSource> target;

            public HandledSafelySyntaxAllImpl(SubscriptionSyntax.All<TSource> target)
            {
                if (target == null)
                {
                    throw new ArgumentNullException("target");
                }

                this.target = target;
            }

            public override Action Subscribe(Action<TSource> subscriber, int priority = 0)
            {
                if (subscriber == null)
                {
                    throw new ArgumentNullException("subscriber");
                }

                return target.Subscribe(WrapSubscriber(subscriber), priority);
            }
        }

        private abstract class HandledSafelySyntaxImpl<TSource> : IHandledSafelySyntax<TSource>
        {
            public abstract Action Subscribe(Action<TSource> subscriber, int priority = 0);

            protected virtual Action<TSource> WrapSubscriber(Action<TSource> subscriber)
            {
                if (subscriber == null)
                {
                    throw new ArgumentNullException("subscriber");
                }

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
                            EventHandler<ScopeFinishedEventArgs> action = (sender, e) =>
                            {
                                if (e.Completed)
                                {
                                    subscriber(source);
                                }
                            };

                            context.Finished += (sender, e) =>
                            {
                                context.Finished -= action;
                                action(sender, e);
                            };
                        }
                        else
                        {
                            Trace.TraceWarning("Event source is not inside of unit of work or transaction scope.");
                        }
                    }
                };
            }
        }
    }
}