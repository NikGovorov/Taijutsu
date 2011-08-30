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
using System.Linq;
using NHibernate.Criterion;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.NHibernate.Query.Criteria
{
    public class QueryOfEntity<TEntity> : AbstractQueryOfEntity<TEntity>, IQueryOfNotUniqueEntity<TEntity>
        where TEntity : class, IEntity
    {
        public QueryOfEntity(ISessionDecorator session)
            : base(session)
        {
        }
        
        public override IQueryOf<bool> Any
        {
            get
            {
                var builder = new Builder<TEntity>(QueryOver.Of<TEntity>());
                BuildQueryOptionsByDefault(builder);
                BuildQueryOptions(builder);
                return new QueryOfAny(() => builder.Query.GetExecutableQueryOver(Session).RowCountInt64() > 0);
            }
        }

        public override IQueryOfNotUniqueEntity<TEntity> NotUniquely
        {
            get
            {
                Unique = false;
                return this;
            }
        }

        #region IQueryOfNotUniqueEntity<TEntity> Members

        public override TEntity Query()
        {
            var builder = new Builder<TEntity>(QueryOver.Of<TEntity>());
            BuildQueryOptionsByDefault(builder);
            BuildQueryOptions(builder);
            return AnalyzeQueryOutput(AnalyzeQueryOutputByDefault(builder.Query.GetExecutableQueryOver(Session).List())).FirstOrDefault();
        }

        public virtual IQueryOf<IEnumerable<TEntity>> All
        {
            get
            {
                var builder = new Builder<TEntity>(QueryOver.Of<TEntity>());
                BuildQueryOptionsByDefault(builder);
                BuildQueryOptions(builder);
                var lazy = new Lazy<IEnumerable<TEntity>>(() => builder.Query.GetExecutableQueryOver(Session).List());
                return new QueryOfNotUniqueEntities(lazy);
            }
        }

        #endregion

        protected virtual IEnumerable<TEntity> AnalyzeQueryOutputUniqueness(IEnumerable<TEntity> output)
        {
            // ReSharper disable PossibleMultipleEnumeration
            if (Unique && output.Count() > 1)
            {
                throw new EntityNotUniqueException<TEntity>(GetType().Name);
            }
            return output;
            // ReSharper restore PossibleMultipleEnumeration
        }

        protected virtual IEnumerable<TEntity> AnalyzeQueryOutputExistence(IEnumerable<TEntity> output)
        {
            // ReSharper disable PossibleMultipleEnumeration
            if (Strict && output.Count() == 0)
            {
                throw new EntityNotFoundException<TEntity>(GetType().Name);
            }
            return output;
            // ReSharper restore PossibleMultipleEnumeration
        }


        protected override IEnumerable<TEntity> AnalyzeQueryOutputByDefault(IEnumerable<TEntity> output)
        {
            return AnalyzeQueryOutputUniqueness(AnalyzeQueryOutputExistence(output));
        }

        protected virtual void BuildLockQueryOptions(Builder<TEntity> builder)
        {
            if (LockReadWriteOperations)
            {
                #pragma warning disable 168
                var query = builder.Query.Lock().Upgrade;
                #pragma warning restore 168
            }
        }

        protected virtual void BuildQueryOptionsByDefault(Builder<TEntity> builder)
        {
            BuildLockQueryOptions(builder);
        }

        protected virtual void BuildQueryOptions(Builder<TEntity> builder)
        {
        }

        #region Nested type: QueryOfNotUniqueEntities

        protected class QueryOfNotUniqueEntities : IQueryOf<IEnumerable<TEntity>>
        {
            private readonly Lazy<IEnumerable<TEntity>> entities;

            public QueryOfNotUniqueEntities(Lazy<IEnumerable<TEntity>> entities)
            {
                this.entities = entities;
            }

            protected virtual IEnumerable<TEntity> Entities
            {
                get { return entities.Value; }
            }

            #region IQueryOf<IQueryable<TEntity>> Members

            public virtual IEnumerable<TEntity> Query()
            {
                return Entities;
            }

            #endregion
        }

        #endregion
    }
}