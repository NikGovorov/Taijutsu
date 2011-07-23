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

namespace Taijutsu.Infrastructure.Internal
{
    public class MarkingStep<TEntity> : IMarkingStep<TEntity> where TEntity : IRemovableEntity, IAggregateRoot
    {
        private readonly Func<object> creationFuction;
        private readonly Action removingAction;

        public MarkingStep(Func<object> creationFuction, Action removingAction)
        {
            this.creationFuction = creationFuction;
            this.removingAction = removingAction;
        }

        #region IMarkingStep<TEntity> Members

        public object AsCreated()
        {
            return creationFuction();
        }

        public void AsRemoved()
        {
            removingAction();
        }

        #endregion
    }
}