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

namespace Taijutsu.Domain
{
    [Serializable]
    public class NotFoundException : EntityException
    {
        public NotFoundException(object id, object type, Exception innnerException = null)
            : base(string.Format("Entity with '{0}' id and '{1}' type has not been found.", id ?? "unknown", type ?? typeof(IDomainObject)), innnerException)
        {
            entityId = id;
            entityType = type;
        }

        public NotFoundException(object id, Type type, Exception innnerException = null)
            : this(id, (object) type, innnerException)
        {
        }

        public NotFoundException(string query, object type, Exception innnerException = null)
            : base(
                string.Format("Entity of '{1}' type has not been found. Query requires not empty results. Query description: '{0}'.", query ?? "unknown", type ?? typeof(IDomainObject)),
                innnerException)
        {
            entityId = "unknown";
            entityType = type;
        }

        public NotFoundException(string query, Type type, Exception innnerException = null)
            : this(query, (object) type, innnerException)
        {
        }
    }

    [Serializable]
    public class NotFoundException<TEntity> : NotFoundException where TEntity : IEntity
    {
        public NotFoundException(object id, Exception innnerException = null)
            : base(id, typeof (TEntity), innnerException)
        {
        }

        public NotFoundException(string query, Exception innnerException = null)
            : base(query, typeof (TEntity), innnerException)
        {
        }
    }
}