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
using System.Collections.Generic;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.NHibernate.Query
{
    public abstract class AbstractQueryOfEntities<TEntity> : AbstractQuery<TEntity>,
                                                             IQueryOfEntities<TEntity>
        where TEntity : IEntity
    {
        private readonly HashSet<object> keysToInclude = new HashSet<object>();
        private readonly HashSet<Type> typesToExclude = new HashSet<Type>();
        private int? skipCount;
        private int? takeCount;


        protected AbstractQueryOfEntities(ISessionDecorator session)
            : base(session)
        {
        }

        protected virtual HashSet<object> KeysToInclude
        {
            get { return keysToInclude; }
        }

        protected virtual HashSet<Type> TypesToExclude
        {
            get { return typesToExclude; }
        }

        protected virtual int? SkipCount
        {
            get { return skipCount; }
            set { skipCount = value; }
        }

        protected virtual int? TakeCount
        {
            get { return takeCount; }
            set { takeCount = value; }
        }

        #region IQueryOfSomeEntities<TEntity> Members

        public abstract IQueryOfEntity<TEntity> Uniquely { get; }

        public abstract IEnumerable<TEntity> Query();

        public abstract IQueryOf<long> Count { get; }

        public abstract IQueryOf<bool> Any { get; }

        public abstract IQueryOfEntities<TEntity> KeyInRangeOf(params object[] keys);
        public abstract IQueryOfEntities<TEntity> Skip(int count);
        public abstract IQueryOfEntities<TEntity> Take(int count);
        public abstract IQueryOfEntities<TEntity> ExcludeTypes(params Type[] derivedTypes);

        #endregion
    }
}