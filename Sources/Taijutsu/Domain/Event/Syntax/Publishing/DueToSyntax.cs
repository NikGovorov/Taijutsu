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

namespace Taijutsu.Domain.Event.Syntax.Publishing
{
    public static class DueToSyntax
    {
        // ReSharper disable InconsistentNaming
        public interface Init<TFact> : AddressedToSyntax.Init<TFact> where TFact : IFact
        // ReSharper restore InconsistentNaming
        {
            Occur<TFact> OccuredAt(DateTime occurrenceDate);
            Notice<TFact> NoticedAt(DateTime noticeDate);
        }

        // ReSharper disable InconsistentNaming
        public interface Occur<TFact> : AddressedToSyntax.Init<TFact> where TFact : IFact
        // ReSharper restore InconsistentNaming
        {
            AddressedToSyntax.Init<TFact> NoticedAt(DateTime noticeDate);
        }

        // ReSharper disable InconsistentNaming
        public interface Notice<TFact> : AddressedToSyntax.Init<TFact> where TFact : IFact
        // ReSharper restore InconsistentNaming
        {
            AddressedToSyntax.Init<TFact> OccuredAt(DateTime occurrenceDate);
        }


        internal class InitImpl<TFact> : AddressedToSyntax.InitImpl<TFact>, Init<TFact> where TFact : IFact
        {
            public InitImpl(Action<IEvent> publishingAction, TFact fact, DateTime? noticeDate, DateTime? occurrenceDate) : base(publishingAction, fact, noticeDate, occurrenceDate)
            {
            }

            Occur<TFact> Init<TFact>.OccuredAt(DateTime date)
            {
                return new OccurImpl<TFact>(publishingAction, fact, noticeDate, date);
            }

            Notice<TFact> Init<TFact>.NoticedAt(DateTime date)
            {
                return new NoticeImpl<TFact>(publishingAction, fact, date, occurrenceDate);
            }
        }

        internal class OccurImpl<TFact> : AddressedToSyntax.InitImpl<TFact>, Occur<TFact> where TFact : IFact
        {
            public OccurImpl(Action<IEvent> publishingAction, TFact fact, DateTime? noticeDate, DateTime? occurrenceDate) : base(publishingAction, fact, noticeDate, occurrenceDate)
            {
            }

            AddressedToSyntax.Init<TFact> Occur<TFact>.NoticedAt(DateTime date)
            {
                return new InitImpl<TFact>(publishingAction, fact, date, occurrenceDate);
            }
        }


        internal class NoticeImpl<TFact> : AddressedToSyntax.InitImpl<TFact>, Notice<TFact> where TFact : IFact
        {
            public NoticeImpl(Action<IEvent> publishingAction, TFact fact, DateTime? noticeDate, DateTime? occurrenceDate)
                : base(publishingAction, fact, noticeDate, occurrenceDate)
            {
            }

            AddressedToSyntax.Init<TFact> Notice<TFact>.OccuredAt(DateTime date)
            {
                return new InitImpl<TFact>(publishingAction, fact, noticeDate, date);
            }
        }
    }
}