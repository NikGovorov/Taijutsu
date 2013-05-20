﻿// Copyright 2009-2013 Nikita Govorov
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

    public abstract class ExternalEvent : Entity<Guid?>, IEvent
    {
        protected DateTime occurrenceDate;

        protected DateTime noticeDate;

        protected ExternalEvent()
        {
            this.noticeDate = SystemTime.Now;
            this.occurrenceDate = this.noticeDate;
        }

        protected ExternalEvent(DateTime? occurrenceDate, DateTime? noticeDate, Guid? id = null)
        {
            if (id != null)
            {
                this.id = id;
            }

            this.noticeDate = noticeDate ?? SystemTime.Now;
            this.occurrenceDate = occurrenceDate ?? this.noticeDate;
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

        public virtual DateTime NoticeDate
        {
            get
            {
                return this.noticeDate;
            }

            protected set
            {
                this.noticeDate = value;
            }
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppress the warning for generics.")]
    [Serializable]
    public class ExternalEvent<TRecipient> : ExternalEvent, IExternalEvent<TRecipient>
        where TRecipient : IEntity
    {
        protected TRecipient recipient;

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Object.Equals is optimized with resharper code clenup.")]
        public ExternalEvent(TRecipient recipient, DateTime? occurrenceDate = null, DateTime? noticeDate = null, Guid? id = null)
            : base(occurrenceDate, noticeDate, id)
        {
            if (Equals(recipient, default(TRecipient)))
            {
                throw new ArgumentNullException("recipient");
            }

            this.recipient = recipient;
        }

        protected ExternalEvent()
        {
        }

        public virtual TRecipient Recipient
        {
            get
            {
                return this.recipient;
            }

            protected set
            {
                this.recipient = value;
            }
        }
    }
}