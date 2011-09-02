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


using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.NHibernate.Query
{
    public abstract class AbstractQueryOfEntity<TEntity> : AbstractQuery<TEntity>,
                                                           IQueryOfEntity<TEntity> where TEntity : IEntity
    {
        private bool unique;
        private bool strict;

        protected AbstractQueryOfEntity(ISessionDecorator session, bool unique = true, bool strict = false)
            : base(session)
        {
        }
        
        protected virtual bool Strict
        {
            get { return strict; }
            set { strict = value; }
        }

        protected virtual bool Unique
        {
            get { return unique; }
            set { unique = value; }
        }

        #region IQueryOfEntity<TEntity> Members

        public virtual IQueryOf<TEntity> Strictly
        {
            get
            {
                Strict = true;
                return this;
            }
        }

        public abstract TEntity Query();

        public abstract IQueryOf<bool> Any { get; }

        public abstract IQueryOfNotUniqueEntity<TEntity> NotUniquely { get; }

        #endregion
    }
}