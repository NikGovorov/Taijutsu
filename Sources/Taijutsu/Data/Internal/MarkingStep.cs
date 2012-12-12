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
using Taijutsu.Domain;

namespace Taijutsu.Data.Internal
{
    // ReSharper disable UnusedTypeParameter
    public class MarkingStep<TEntity> : IMarkingStep where TEntity : IDeletableEntity, IAggregateRoot
    // ReSharper restore UnusedTypeParameter
    {
        private readonly Func<object> creatingFuction;
        private readonly Action removingAction;

        public MarkingStep(Func<object> creatingFuction, Action removingAction)
        {
            if (creatingFuction == null) throw new ArgumentNullException("creatingFuction");
            if (removingAction == null) throw new ArgumentNullException("removingAction");

            this.creatingFuction = creatingFuction;
            this.removingAction = removingAction;
        }

        public object AsCreated()
        {
            return creatingFuction();
        }

        public void AsDeleted()
        {
            removingAction();
        }
    }
}