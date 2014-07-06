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
using System.Diagnostics.CodeAnalysis;

namespace Taijutsu.Domain
{
    [Serializable]
    public class NotUniqueException : EntityException
    {
        public NotUniqueException(object id, object type, Exception innnerException = null)
            : base(string.Format("Entity with '{0}' id and '{1}' type is not unique.", id ?? "unknown", type ?? typeof(IDomainObject)), innnerException)
        {
            Id = id;
            Type = type;
        }

        public NotUniqueException(object id, Type type, Exception innnerException = null) : this(id, (object)type, innnerException)
        {
        }

        public NotUniqueException(string query, object type, Exception innnerException = null)
            : base(
                string.Format("Entity of '{1}' type is not unique. Query requires unique results. Query description: '{0}'.", query ?? "unknown", type ?? typeof(IDomainObject)), 
                innnerException)
        {
            Id = "unknown";
            Type = type;
        }

        public NotUniqueException(string query, Type type, Exception innnerException = null)
            : this(query, (object)type, innnerException)
        {
        }
    }

    [Serializable]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppress the warning for generics.")]
    public class NotUniqueException<TEntity> : NotUniqueException
        where TEntity : IEntity
    {
        public NotUniqueException(object id, Exception innnerException = null)
            : base(id, typeof(TEntity), innnerException)
        {
        }

        public NotUniqueException(string query, Exception innnerException = null)
            : base(query, typeof(TEntity), innnerException)
        {
        }
    }
}