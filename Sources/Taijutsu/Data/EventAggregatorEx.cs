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
using System.Transactions;
using Taijutsu.Data.Internal;
using Taijutsu.Domain.Event.Syntax;

namespace Taijutsu.Data
{
    public static class EventAggregatorEx
    {
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
                                                                               if (e.Transaction.TransactionInformation.Status == TransactionStatus.Committed)
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
                                       maybeUnitOfWork.Value.Completed +=
                                           () => subscriber(source);
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
    }
}