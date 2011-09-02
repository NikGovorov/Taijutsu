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
using System.Runtime.Serialization;

namespace Taijutsu.Domain
{
    public interface IEntityException
    {
        object Key { get; }
        object Type { get; }
    }

    [Serializable]
    public abstract class EntityException : DomainException, IEntityException
    {
        protected object entityKey;
        protected object entityType;

        protected EntityException()
        {
        }

        protected EntityException(string message)
            : base(message)
        {
        }

        protected EntityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected EntityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #region IEntityException Members

        public virtual object Key
        {
            get { return entityKey; }
            protected set { entityKey = value; }
        }

        public virtual object Type
        {
            get { return entityType; }
            protected set { entityType = value; }
        }

        #endregion
    }


}