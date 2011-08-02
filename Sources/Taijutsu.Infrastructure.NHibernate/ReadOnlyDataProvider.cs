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
using System.Data;
using NHibernate;
using Taijutsu.Domain.Query;
using Taijutsu.Infrastructure.NHibernate.Query.Criteria;

namespace Taijutsu.Infrastructure.NHibernate
{
    public class ReadOnlyDataProvider : Internal.ReadOnlyDataProvider
    {
        private readonly IStatelessSession session;

        public ReadOnlyDataProvider(ISessionFactory factory)
        {
            session = factory.OpenStatelessSession();
        }

        protected virtual IStatelessSession Session
        {
            get { return session; }
        }

        protected virtual ITransaction CurrentTransaction { get; set; }

        public override IQueryOfEntities<TEntity> AllOf<TEntity>()
        {
            return new QueryOfEntities<TEntity>(new SessionDecorator(Session));
        }

        public override IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key)
        {
            return new QueryOfEntityByKey<TEntity>(key, new SessionDecorator(Session));
        }

        public override IQueryOverBuilder<TEntity> QueryOver<TEntity>()
        {
            return new QueryOverBuilder<TEntity>(new SessionDecorator(Session));
        }

        public override void Close()
        {
            try
            {
                CurrentTransaction = null;
                Session.Close();
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
        }

        public override void BeginTransaction(IsolationLevel level)
        {
            CurrentTransaction = session.BeginTransaction(level);
        }

        public override void Commit()
        {
        }

        public override void Rollback()
        {
            CurrentTransaction.Rollback();
            CurrentTransaction.Dispose();
        }

        public override object NativeProvider
        {
            get { return session; }
        }

    }
}