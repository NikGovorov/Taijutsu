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

namespace Taijutsu.Domain.Event
{
    [Serializable]
    public abstract class DomainEvent : Entity<Guid?>, IDomainEvent
    {
        protected DateTime occurrenceDate;

        protected DomainEvent()
        {
            occurrenceDate = SystemTime.Now;
        }

        protected DomainEvent(Guid? id = null) : this()
        {
            if (id.HasValue)
            {
                base.id = id.Value;
            }
        }

        public virtual DateTime OccurrenceDate
        {
            get { return occurrenceDate; }
            protected set { occurrenceDate = value; }
        }
    }

    [Serializable]
    public abstract class DomainEvent<TInitiator> : DomainEvent, IDomainEvent<TInitiator>
        where TInitiator : IDomainObject
    {
        protected TInitiator initiator;

        protected DomainEvent()
        {
        }

        protected DomainEvent(TInitiator initiator, Guid? id = null)
            : base(id)
        {
            if (id.HasValue)
            {
                base.id = id.Value;
            }

            if (Equals(initiator, default(TInitiator)))
            {
                throw new ArgumentNullException("initiator");
            }

            this.initiator = initiator;
        }

        public virtual TInitiator Initiator
        {
            get { return initiator; }
            protected set { initiator = value; }
        }
    }

    [Serializable]
    public class DomainEvent<TInitiator, TFact> : DomainEvent<TInitiator>, IDomainEvent<TInitiator, TFact>
        where TInitiator : IDomainObject
        where TFact : IFact
    {
        protected TFact fact;

        protected DomainEvent()
        {
        }

        public DomainEvent(TInitiator initiator, TFact fact, Guid? id = null) : base(initiator, id)
        {
            if (Equals(fact, default(TFact)))
            {
                throw new ArgumentNullException("fact");
            }

            this.fact = fact;
        }

        public virtual TFact Fact
        {
            get { return fact; }
            protected set { fact = value; }
        }
    }
}