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
    public interface ISubscriptionFeature : IHideObjectMembers
    {
        IConditionStep<TEvent> ReactOn<TEvent>() where TEvent : class;
    }


    public interface IConditionStep<out TEvent> : ISubscriptionStep<TEvent> where TEvent : class
    {
        ISubscriptionStep<TEvent> Conditionally(Predicate<TEvent> condition);
    }

    public interface ISubscriptionStep<out TEvent> : IHideObjectMembers where TEvent : class
    {
        Action By(Action<TEvent> handler);
        Action By(ISubscriber<TEvent> subscriber);
    }
}