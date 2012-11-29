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
using Taijutsu.Domain;

namespace Taijutsu.Test.Domain.Model
{
    public class Customer : Entity<Guid>, IDeletableEntity
    {
        protected DateTime creationDate;
        protected FullName name;

        static Customer()
        {
            Subscribe<OrderCreatedEvent>(ev => ev.Customer.Handle(ev));
        }

        protected internal Customer()
        {
        }

        public Customer(Guid id, FullName name)
        {
            creationDate = SystemTime.Now;
            this.id = id;
            this.name = name;
        }

        public Customer(FullName name): this(SeqGuid.NewGuid(), name)
        {
        }

        public virtual DateTime CreationDate
        {
            get { return creationDate; }
        }

        public virtual FullName Name
        {
            get { return name; }
        }

        public bool NotifiedAboutOrder { get; set; }

        protected virtual void Handle(OrderCreatedEvent orderCreatedEvent)
        {
            NotifiedAboutOrder = true;
        }
    }
}