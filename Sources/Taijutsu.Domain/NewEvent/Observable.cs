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
using System.Collections.Generic;
using Taijutsu.Domain.NewEvent.Syntax;

namespace Taijutsu.Domain.NewEvent
{
    public class Observable : IObservable, IObservableSyntax, IEventStreamSyntax
    {
        private readonly object sync = new object();
        private IDictionary<Type, IList<IEventHandler>> handlers = new Dictionary<Type, IList<IEventHandler>>();

        #region IEventStreamSyntax Members

        DueToSyntax.Init<TFact> IEventStreamSyntax.DueTo<TFact>()
        {
            return new DueToSyntax.InitImpl<TFact>(Subscribe);
        }

        InitiatedBySyntax.Init<TEntity> IEventStreamSyntax.InitiatedBy<TEntity>()
        {
            throw new NotImplementedException();
        }

        AddressedToSyntax.All<TEntity> IEventStreamSyntax.AddressedTo<TEntity>()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IObservable Members

        public virtual IObservableSyntax OnStream
        {
            get { return this; }
        }

        #endregion

        #region IObservableSyntax Members

        IEventStreamSyntax IObservableSyntax.OfEvents
        {
            get { return this; }
        }

        SubscriptionSyntax.All<TEvent> IObservableSyntax.Of<TEvent>()
        {
            return new SubscriptionSyntax.AllImpl<TEvent>(Subscribe);
        }

        #endregion

        internal virtual Action Subscribe(IEventHandler handler)
        {
            return () => { };
        }
    }
}