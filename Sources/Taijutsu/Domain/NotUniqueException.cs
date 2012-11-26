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
    public class NotUniqueException<TEntity> : NotUniqueException where TEntity : IEntity
    {
        public NotUniqueException(object id, Exception innnerException = null)
            : base(id, typeof (TEntity), innnerException)
        {
        }

        public NotUniqueException(string queryDescription, Exception innnerException = null)
            : base(queryDescription, typeof (TEntity), innnerException)
        {
        }
    }

    [Serializable]
    public class NotUniqueException : EntityException
    {
        public NotUniqueException(string queryDescription, Type type, Exception innnerException = null)
            : base(
                string.Format("Entity of '{1}' type is not unique. Query description: '{0}'.", queryDescription, type),
                innnerException)
        {
            entityId = "unknown";
            entityType = type;
        }


        public NotUniqueException(object id, object type, Exception innnerException = null)
            : base(
                string.Format("Entity with '{0}' id and '{1}' type is not unique.", id, type),
                innnerException)
        {
            entityId = id;
            entityType = type;
        }
    }
}