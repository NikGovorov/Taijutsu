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
using Taijutsu.Domain.Event.Internal;

namespace Taijutsu.Domain.Event.Syntax.Publishing
{
    public static class AddressedToSyntax
    {
        // ReSharper disable InconsistentNaming
        public interface Init<TFact> : IHideObjectMembers where TFact : IFact
            // ReSharper restore InconsistentNaming
        {
            PublishingSyntax.Prepared AddressedTo<TTarget>(TTarget target) where TTarget : IEntity;
            PublishingSyntax.Prepared AddressedTo<TTarget>(Func<TTarget> targetProvider) where TTarget : IEntity;
        }


        public class InitImpl<TFact> : Init<TFact> where TFact : IFact
        {
            protected TFact fact;
            protected Action<IEvent> publishingAction;
            protected DateTime? occurrenceDate;
            protected DateTime? noticeDate;

            public InitImpl(Action<IEvent> publishingAction, TFact fact, DateTime? noticeDate = null, DateTime? occurrenceDate = null)
            {
                this.fact = fact;
                this.noticeDate = noticeDate;
                this.occurrenceDate = occurrenceDate;
                this.publishingAction = publishingAction;
            }

            PublishingSyntax.Prepared Init<TFact>.AddressedTo<TTarget>(TTarget target)
            {
                return (this as Init<TFact>).AddressedTo(() => target);
            }

            PublishingSyntax.Prepared Init<TFact>.AddressedTo<TTarget>(Func<TTarget> targetProvider)
            {
                var target = targetProvider();
                var externalEvent = ExternalEventActivator<ExternalEvent<TTarget,TFact>>
                                        .Current
                                                .CreateInstance(target, fact, occurrenceDate, noticeDate, SeqGuid.NewGuid());
                return new PublishingSyntax.PreparedImpl(() => { publishingAction(externalEvent); });
            }
        }
    }
}
