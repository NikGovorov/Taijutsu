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
using NHibernate.Linq;
using NHibernate.Transform;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.NHibernate.Query.Criteria
{
    public class QueryOfEntities<TEntity> : AbstractQueryOfEntities<TEntity> where TEntity : class, IEntity
    {
        public QueryOfEntities(ISessionDecorator session)
            : base(session)
        {
        }

        public override IEnumerable<TEntity> Query()
        {
            var builder = new Builder<TEntity>(QueryOver.Of<TEntity>());
            BuildQueryOptionsByDefault(builder);
            BuildQueryOptions(builder);
            return AnalyzeQueryOutput(AnalyzeQueryOutputByDefault(builder.Query.GetExecutableQueryOver(Session).List()));
        }

        public override IQueryOf<long> Count
        {
            get { return new QueryOfCount(this); }
        }

        public override IQueryOf<bool> Any
        {
            get { return new QueryOfAny(() => (Count.Query() > 0)); }
        }

        public override IQueryOfEntity<TEntity> Uniquely
        {
            get { return new QueryOfUniqueEntity(this, BuildQueryOptionsByDefault, BuildQueryOptions); }
        }

        public override IQueryOfEntities<TEntity> Take(int count)
        {
             TakeCount = count;
            return this;
        }

        public override IQueryOfEntities<TEntity> Skip(int count)
        {
            SkipCount = count;
            return this;
        }

        public override IQueryOfEntities<TEntity> ExcludeTypes(params Type[] derivedTypes)
        {
            derivedTypes.ForEach(type =>
            {
                if (type.IsSubclassOf(typeof(TEntity)))
                {
                    TypesToExclude.Add(type);
                }
            });
            return this;
        }

        public override IQueryOfEntities<TEntity> KeyInRangeOf(params object[] keys)
        {
            keys.ForEach(key => KeysToInclude.Add(key));
            return this;
        }

        protected override IEnumerable<TEntity> AnalyzeQueryOutput(IEnumerable<TEntity> entities)
        {
            return entities;
        }
        protected override IEnumerable<TEntity> AnalyzeQueryOutputByDefault(IEnumerable<TEntity> entities)
        {
            return entities;
        }

        protected virtual void BuildLockQueryOptions(Builder<TEntity> builder)
        {       
            #pragma warning disable 168
            if (LockReadWriteOperations)
            {
                var query = builder.Query.Lock().Write;
            } 
            else if (LockWriteOperations)
            {
                var query = builder.Query.Lock().Upgrade;
            }
            #pragma warning restore 168
        }

        protected virtual void BuildTakeOptions(Builder<TEntity> builder)
        {
            if (TakeCount.HasValue)
            {
                builder.Query.Take(TakeCount.Value);
            }
        }


        protected virtual void BuildSkipOptions(Builder<TEntity> builder)
        {
            if (SkipCount.HasValue)
            {
                builder.Query.Skip(SkipCount.Value);
            }
        }

        protected virtual void BuildTypeExcludingOptions(Builder<TEntity> builder)
        {
            TypesToExclude.ForEach(type => builder.Query.Where(e => e.GetType() != type));
        }

        protected virtual void BuildKeyIncludingOptions(Builder<TEntity> builder)
        {
            if (KeysToInclude.Count != 0)
            {
                builder.Query.Where(new InExpression(Projections.Id(), KeysToInclude.ToArray()));
            }
        }

        protected virtual void BuildQueryOptionsByDefault(Builder<TEntity> builder)
        {
            BuildTypeExcludingOptions(builder);
            BuildKeyIncludingOptions(builder);
            BuildTakeOptions(builder);
            BuildSkipOptions(builder);
            BuildLockQueryOptions(builder);
        }

        protected virtual void BuildQueryOptions(Builder<TEntity> builder)
        {
        }

        protected class QueryOfUniqueEntity : QueryOfEntity<TEntity>
        {
            private readonly QueryOfEntities<TEntity> parent;
            private readonly Action<Builder<TEntity>> queryOptionsByDefaultBuildingAction;
            private readonly Action<Builder<TEntity>> queryOptionsBuildingAction;

            public QueryOfUniqueEntity(QueryOfEntities<TEntity> parent, Action<Builder<TEntity>> queryOptionsByDefaultBuildingAction, Action<Builder<TEntity>> queryOptionsBuildingAction)
                : base(parent.Session)
            {
                this.parent = parent;
                this.queryOptionsByDefaultBuildingAction = queryOptionsByDefaultBuildingAction;
                this.queryOptionsBuildingAction = queryOptionsBuildingAction;
            }

            public virtual QueryOfEntities<TEntity> Parent
            {
                get { return parent; }
            }

            protected override void BuildQueryOptionsByDefault(Builder<TEntity> builder)
            {
                queryOptionsByDefaultBuildingAction(builder);
            }

            protected override void BuildQueryOptions(Builder<TEntity> builder)
            {   
                queryOptionsBuildingAction(builder);
            }

            public override IQueryOf<bool> Any
            {
                get
                {
                    return new QueryOfAny(() => (Parent.Count.Query() > 0));
                }
            }
        }

        protected internal class QueryOfCount : IQueryOf<long>
        {
            private readonly QueryOfEntities<TEntity> parent;

            public QueryOfCount(QueryOfEntities<TEntity> parent)
            {
                this.parent = parent;
            }

            protected virtual QueryOfEntities<TEntity> Parent
            {
                get { return parent; }
            }

            public virtual long Query()
            {
                var builder = new Builder<TEntity>(QueryOver.Of<TEntity>());
                Parent.BuildQueryOptionsByDefault(builder);
                Parent.BuildQueryOptions(builder);

                if (Parent.TakeCount.HasValue || Parent.SkipCount.HasValue)
                {
                    return builder.Query.Select(Projections.Id()).GetExecutableQueryOver(Parent.Session).TransformUsing(Transformers.PassThrough).List<object>().Count;                    
                }

                return builder.Query.GetExecutableQueryOver(Parent.Session).RowCountInt64();
            }

        }
    }
}