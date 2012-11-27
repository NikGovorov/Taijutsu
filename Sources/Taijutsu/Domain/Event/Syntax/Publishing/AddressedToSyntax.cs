#region License

// Copyright 2009-2013 Nikita Govorov
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
using Taijutsu.Domain.Event.Internal;

namespace Taijutsu.Domain.Event.Syntax.Publishing
{
    public static class AddressedToSyntax
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedTypeParameter
        public interface Init<TFact> : IHiddenObjectMembers where TFact : IFact
        // ReSharper restore UnusedTypeParameter
        // ReSharper restore InconsistentNaming
        {
            PublishingSyntax.Prepared AddressedTo<TRecipient>(TRecipient recipient) where TRecipient : IEntity;
            PublishingSyntax.Prepared AddressedTo<TRecipient>(Func<TRecipient> recipientProvider) where TRecipient : IEntity;
        }


        public class InitImpl<TFact> : Init<TFact> where TFact : IFact
        {
            protected readonly TFact fact;
            protected readonly Action<IEvent> publishAction;
            protected readonly DateTime? occurrenceDate;
            protected readonly DateTime? noticeDate;

            public InitImpl(Action<IEvent> publishAction, TFact fact, DateTime? noticeDate = null, DateTime? occurrenceDate = null)
            {
                this.fact = fact;
                this.noticeDate = noticeDate;
                this.occurrenceDate = occurrenceDate;
                this.publishAction = publishAction;
            }

            PublishingSyntax.Prepared Init<TFact>.AddressedTo<TRecipient>(TRecipient recipient)
            {
                return (this as Init<TFact>).AddressedTo(() => recipient);
            }

            PublishingSyntax.Prepared Init<TFact>.AddressedTo<TRecipient>(Func<TRecipient> recipientProvider)
            {
                var recipient = recipientProvider();
                var externalEvent = ExternalEventActivator<ExternalEvent<TRecipient, TFact>>.Current.CreateInstance(recipient, fact, occurrenceDate, noticeDate, SeqGuid.NewGuid());
                return new PublishingSyntax.PreparedImpl(() => publishAction(externalEvent));
            }
        }
    }
}