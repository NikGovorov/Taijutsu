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



using NHibernate;
using NHibernate.Criterion;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.NHibernate.Query.Criteria
{
    public class QueryOfEntityByKey<TEntity> : QueryOfEntity<TEntity>, IQueryOfEntityByKey<TEntity>
        where TEntity : class, IEntity
    {
        private const string NotUniqueMessage = "More than one row with the given identifier was found";

        
        public QueryOfEntityByKey(object key, ISessionDecorator session) : base(session)
        {
            Key = key;
            strict = true;
        }

        protected object Key { get; set; }

        public override IQueryOf<bool> Any
        {
            get
            {
                var pessimistic = LockReadWriteOperations;
                return new QueryOfAny(() =>
                                          {
                                              try
                                              {
                                                  return
                                                      (pessimistic
                                                            ? Session.Get<TEntity>(Key, LockMode.Upgrade)
                                                            : Session.Get<TEntity>(Key)) != null;
                                              }
                                              catch (HibernateException ex)
                                              {
                                                  if (IsEntityNotUniqueException(ex))
                                                  {
                                                      return true;
                                                  }
                                                  throw;
                                              }
                                          });
            }
        }

        #region IQueryOfEntityByKey<TEntity> Members

         TEntity IQueryOf<TEntity>.Query()
         {
             return Unique ? Query() : base.Query();
         }

        public override TEntity Query()
        {
            try
            {
                var entity = LockReadWriteOperations ? Session.Get<TEntity>(Key, LockMode.Upgrade) : Session.Get<TEntity>(Key);
                return AnalyzeQueryOutput(AnalyzeQueryOutputByDefault(entity));
            }
            catch (HibernateException  ex)
            {
                if (IsEntityNotUniqueException(ex))
                {
                    throw new EntityNotUniqueException<TEntity>(Key, ex);
                }
                throw;
            }
        }

        public IQueryOfEntity<TEntity> NotStrictly
        {
            get
            {
                Strict = false;
                return this;
            }
        }

        #endregion

        private static bool IsEntityNotUniqueException(HibernateException ex)
        {
            return ex.Message.Contains(NotUniqueMessage);
        }

        private TEntity AnalyzeQueryOutputExistence(TEntity output)
        {
            if (Strict && output == null)
            {
                throw new EntityNotFoundException<TEntity>(Key);
            }
            return output;
        }

        private TEntity AnalyzeQueryOutputByDefault(TEntity output)
        {
            return AnalyzeQueryOutputExistence(output);
        }

        private static TEntity AnalyzeQueryOutput(TEntity output)
        {
            return output;
        }

        protected override void BuildQueryOptions(Builder<TEntity> builder)
        {
            builder.Query.Where(Restrictions.IdEq(Key));
            base.BuildQueryOptions(builder);
        }
    }
}