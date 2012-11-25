#region License

// Copyright 2009-2012 Taijutsu.
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

namespace Taijutsu.Domain
{
    [Serializable]
    public class EntityNotFoundException : EntityException
    {
        public EntityNotFoundException(object id, Type type, Exception innnerException = null)
            : this(id, (object) type, innnerException)
        {
        }

        public EntityNotFoundException(object id, object type, Exception innnerException = null)
            : base(string.Format("Entity with '{0}' id and '{1}' type has not been found.", id, type), innnerException)
        {
            entityId = id;
            entityType = type;
        }

        public EntityNotFoundException(string queryDescription, Type type, Exception innnerException = null)
            : base(
                string.Format("Entity of '{1}' type has not been found. Query description: '{0}'.", queryDescription, type),
                innnerException)
        {
            entityId = "unknown";
            entityType = type;
        }
    }

    [Serializable]
    public class EntityNotFoundException<TEntity> : EntityNotFoundException where TEntity : IEntity
    {
        public EntityNotFoundException(object id, Exception innnerException = null)
            : base(id, typeof (TEntity), innnerException)
        {
        }

        public EntityNotFoundException(string queryDescription, Exception innnerException = null)
            : base(queryDescription, typeof (TEntity), innnerException)
        {
        }
    }
}