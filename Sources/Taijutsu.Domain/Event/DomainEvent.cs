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

namespace Taijutsu.Domain.Event
{
    public abstract class DomainEvent : IDomainEvent
    {
        protected DateTime dateOfOccurrence;

        protected DomainEvent()
        {
            dateOfOccurrence = SystemTime.Now;
        }

        #region IDomainEvent Members

        public virtual DateTime DateOfOccurrence
        {
            get { return dateOfOccurrence; }
        }

        #endregion
    }


    public abstract class DomainEvent<TSubject> : DomainEvent, IDomainEvent<TSubject> where TSubject : IDomainObject
    {
        protected TSubject initiatedBy;

        protected DomainEvent()
        {
        }


        protected DomainEvent(TSubject initiatedBy)
        {
            if (Equals(initiatedBy, default(TSubject)))
            {
                throw new ArgumentNullException("initiatedBy");
            }

            this.initiatedBy = initiatedBy;
        }

        #region IDomainEvent<TSubject> Members

        public virtual TSubject InitiatedBy
        {
            get { return initiatedBy; }
        }

        #endregion
    }


    public class DomainEvent<TSubject, TFact> : DomainEvent<TSubject>, IDomainEvent<TSubject, TFact>
        where TSubject : IDomainObject
        where TFact : IFact
    {
        protected TFact fact;

        protected DomainEvent()
        {
        }

        public DomainEvent(TSubject initiatedBy, TFact fact) : base(initiatedBy)
        {
            if (Equals(fact, default(TFact)))
            {
                throw new ArgumentNullException("fact");
            }
            this.fact = fact;
        }

        #region IDomainEvent<TSubject,TFact> Members

        public virtual TFact Fact
        {
            get { return fact; }
        }

        #endregion
    }
}