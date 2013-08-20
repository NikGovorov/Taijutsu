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
using System.Diagnostics.CodeAnalysis;

namespace Taijutsu.Event
{
    [Serializable]
    public abstract class Event : IEvent
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Can be used in inhertor's constructor.")]
        protected DateTime occurrenceDate;

        protected Event()
        {
            occurrenceDate = SystemTime.Now;
        }

        public virtual DateTime OccurrenceDate
        {
            get { return occurrenceDate; }
            protected set { occurrenceDate = value; }
        }
    }
}