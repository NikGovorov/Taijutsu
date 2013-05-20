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
namespace Taijutsu.Domain.Event
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [Serializable]
    public abstract class DomainEvent : Entity<Guid?>, IDomainEvent
    {
        protected DateTime occurrenceDate;

        protected DomainEvent()
        {
            this.occurrenceDate = SystemTime.Now;
        }

        protected DomainEvent(Guid? id = null)
        {
            if (id.HasValue)
            {
                this.id = id.Value;
            }
        }

        public virtual DateTime OccurrenceDate
        {
            get
            {
                return this.occurrenceDate;
            }

            protected set
            {
                this.occurrenceDate = value;
            }
        }
    }

    [Serializable]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppress the warning for generics.")]
    public abstract class DomainEvent<TInitiator> : DomainEvent, IDomainEvent<TInitiator>
        where TInitiator : IDomainObject
    {
        protected TInitiator initiator;

        protected DomainEvent()
        {
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Object.Equals is optimized with resharper code clenup.")]
        protected DomainEvent(TInitiator initiator, Guid? id = null)
            : base(id)
        {
            if (Equals(initiator, default(TInitiator)))
            {
                throw new ArgumentNullException("initiator");
            }

            if (id.HasValue)
            {
                this.id = id.Value;
            }

            this.initiator = initiator;
        }

        public virtual TInitiator Initiator
        {
            get
            {
                return this.initiator;
            }

            protected set
            {
                this.initiator = value;
            }
        }
    }
}