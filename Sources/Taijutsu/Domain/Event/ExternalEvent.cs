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
    public abstract class ExternalEvent : Entity<Guid?>, IEvent
    {
        protected DateTime occurrenceDate;
        protected DateTime noticeDate;

        protected ExternalEvent()
        {
            noticeDate = SystemTime.Now;
            occurrenceDate = noticeDate;
        }

        protected ExternalEvent(DateTime? occurrenceDate, DateTime? noticeDate, Guid? id = null)
        {
            if (id != null)
            {
                base.id = id;
            }

            this.noticeDate = noticeDate ?? SystemTime.Now;
            this.occurrenceDate = occurrenceDate ?? this.noticeDate;
        }

        public virtual DateTime OccurrenceDate
        {
            get { return occurrenceDate; }
            protected set { occurrenceDate = value; }
        }

        public virtual DateTime NoticeDate
        {
            get { return noticeDate; }
            protected set { noticeDate = value; }
        }
    }

    [Serializable]
    public class ExternalEvent<TRecipient> : ExternalEvent, IExternalEvent<TRecipient>
        where TRecipient : IEntity
    {
        protected TRecipient recipient;


        protected ExternalEvent()
        {
        }

        public ExternalEvent(TRecipient recipient, DateTime? occurrenceDate = null, DateTime? noticeDate = null,
                             Guid? id = null)
            : base(occurrenceDate, noticeDate, id)
        {
            if (Equals(recipient, default(TRecipient)))
            {
                throw new ArgumentNullException("recipient");
            }

            this.recipient = recipient;
        }

        public virtual TRecipient Recipient
        {
            get { return recipient; }
            protected set { recipient = value; }
        }
    }
}