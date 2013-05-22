// Copyright 2009-2013 Nikita Govorov
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
namespace Taijutsu.Domain
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [PublicApi]
    public interface IEntityException
    {
        object Id { get; }

        object Type { get; }
    }

    [Serializable]
    public abstract class EntityException : DomainException, IEntityException
    {
        protected object entityId;

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

        public virtual object Id
        {
            get
            {
                return this.entityId;
            }

            protected set
            {
                this.entityId = value;
            }
        }

        public virtual object Type
        {
            get
            {
                return this.entityType;
            }

            protected set
            {
                this.entityType = value;
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("EntityId", this.entityId ?? string.Empty);
            info.AddValue("EntityType", this.entityType ?? string.Empty);
            base.GetObjectData(info, context);
        }
    }
}