// Copyright 2009-2014 Nikita Govorov
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

using Taijutsu.Domain;
using Taijutsu.Event;

namespace Taijutsu.Test.Domain.Model
{
    public class Order : Entity<Guid>, IOrder
    {
        public Order(Customer customer)
        {
            Events.Publish(new OrderCreated(this, customer));
        }

        public Order(Guid id)
        {
            internalId = id;
        }

        protected internal Order()
        {
            internalId = Guid.NewGuid();
        }

        public int Total { get; set; }
    }
}