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

namespace Taijutsu.Domain
{
    [Serializable]
    public class EntityNotUniqueException<TEntity> : EntityNotUniqueException where TEntity : IEntity
    {
        public EntityNotUniqueException(object key, Exception innnerException = null)
            : base(key, typeof(TEntity), innnerException)
        {
        }

        public EntityNotUniqueException(string queryDescription, Exception innnerException = null)
            : base(queryDescription, typeof(TEntity), innnerException)
        {
        }
    }

    [Serializable]
    public class EntityNotUniqueException : EntityException
    {
        public EntityNotUniqueException(string queryDescription, Type type, Exception innnerException = null)
            : base(
                string.Format("Entity type of '{1}' is not unique. Query description: '{0}'.", queryDescription, type),
                innnerException)
        {
            entityKey = "unknown";
            entityType = type;
        }


        public EntityNotUniqueException(object key, object type, Exception innnerException = null)
            : base(
                string.Format("Entity with '{0}' key and type of '{1}' is not unique.", key, type),
                innnerException)
        {
            entityKey = key;
            entityType = type;
        }
    }
}