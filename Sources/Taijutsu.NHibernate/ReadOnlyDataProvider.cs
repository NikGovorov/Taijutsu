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
using System.Transactions;
using NHibernate;
using Taijutsu.Data.Internal;
using Taijutsu.Domain.Query;
using Taijutsu.Data.NHibernate.Query.Criteria;
using IsolationLevel = System.Data.IsolationLevel;

namespace Taijutsu.Data.NHibernate
{
    public class ReadOnlyDataProvider : Internal.ReadOnlyDataProvider
    {
        //private readonly IStatelessSession session;

        private readonly ISession session;
        private readonly DataSource dataSource;

        public ReadOnlyDataProvider(DataSource dataSource)
        {
            this.dataSource = dataSource;
            //session = dataSource.SessionFactory.OpenStatelessSession();
            session = dataSource.SessionFactory.OpenSession();
            session.FlushMode = FlushMode.Never;
            session.DefaultReadOnly = true;
        }


        protected virtual ISession Session
        {
            get { return session; }
        }

//        protected virtual IStatelessSession Session
//        {
//            get { return session; }
//        }

        protected virtual ITransaction CurrentTransaction { get; set; }

        public override IQueryOfEntities<TEntity> AllOf<TEntity>()
        {
           //return new QueryOfEntities<TEntity>(new StatelessSession(Session));
            return new QueryOfEntities<TEntity>(new Session(Session));
        }

        public override IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key)
        {
            //return new QueryOfEntityByKey<TEntity>(key, new StatelessSession(Session));
            return new QueryOfEntityByKey<TEntity>(key, new Session(Session));
        }

        public override IQueryOverBuilder<TEntity> QueryOver<TEntity>()
        {
            //return new QueryOverBuilder<TEntity>(dataSource, new StatelessSession(Session));
            return new QueryOverBuilder<TEntity>(dataSource, new Session(Session));
        }

        public override void Close()
        {
            try
            {
                CurrentTransaction = null;
                Session.Dispose();
            }
            catch (Exception)
            {
                throw; //TODO Enable logging here
            }
        }

        public override void BeginTransaction(IsolationLevel level)
        {
            CurrentTransaction = session.BeginTransaction(level);
        }

        public override void CommitTransaction()
        {
            if (Transaction.Current!=null)
            {
                CurrentTransaction.Commit();    
            }
            else
            {
                CurrentTransaction.Rollback();    
            }
            
            CurrentTransaction.Dispose();
        }

        public override void RollbackTransaction()
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