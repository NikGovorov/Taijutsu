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
    public abstract class ExternalEvent : Entity<Guid?>, IEvent
    {
        protected DateTime occurrenceDate;
        protected DateTime noticeDate;

        protected ExternalEvent()
        {
            noticeDate = SystemTime.Now;
            occurrenceDate = noticeDate;
        }

        protected ExternalEvent(DateTime? occurrenceDate, DateTime? noticeDate, Guid? key = null)
        {
            if (key != null)
            {
                entityKey = key;
            }

            this.noticeDate = noticeDate ?? SystemTime.Now;
            this.occurrenceDate = occurrenceDate ?? this.noticeDate;
            
        }

        public virtual DateTime DateOfOccurrence
        {
            get { return occurrenceDate; }
        }

        public virtual DateTime DateOfNotice
        {
            get { return noticeDate; }
        }
    }

    [Serializable]
    public class ExternalEvent<TTarget, TFact>  : ExternalEvent, IExternalEvent<TTarget, TFact> where TFact : IFact where TTarget : IEntity
    {
        protected TFact dueToFact;
        protected TTarget target;


        protected ExternalEvent()
        {
            
        }

        public ExternalEvent(TTarget target, TFact dueToFact, DateTime? occurrenceDate = null, DateTime? noticeDate = null, Guid? key = null) : base(occurrenceDate, noticeDate, key)
        {
            
            this.target = target;
            this.dueToFact = dueToFact;
        }

        public virtual TTarget AddressedTo
        {
            get { return target; }
        }

        public virtual TFact Fact
        {
            get { return dueToFact; }
        }
    }
}