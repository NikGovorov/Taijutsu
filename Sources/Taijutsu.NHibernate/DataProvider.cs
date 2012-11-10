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
using Taijutsu.Data.Internal;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;
using Taijutsu.Data.NHibernate.Query.Criteria;

namespace Taijutsu.Data.NHibernate
{
    public class DataProvider : Internal.DataProvider
    {
        private readonly ISession session;
        private readonly DataSource dataSource;

        public DataProvider(DataSource dataSource)
        {
            this.dataSource = dataSource;
            session = dataSource.SessionFactory.OpenSession();
            session.FlushMode = FlushMode.Auto;
        }

        protected virtual ISession Session
        {
            get { return session; }
        }


        public override IQueryOverBuilder<TEntity> QueryOver<TEntity>()
        {
            return new QueryOverBuilder<TEntity>(dataSource, new Session(Session));
        }

        protected virtual ITransaction CurrentTransaction { get; set; }

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
            CurrentTransaction.Commit();
            CurrentTransaction.Dispose();
            Session.FlushMode = FlushMode.Never;
        }

        public override void RollbackTransaction()
        {
            CurrentTransaction.Rollback();
            CurrentTransaction.Dispose();
            Session.FlushMode = FlushMode.Never;
        }

        public override object MarkAsCreated<TEntity>(TEntity entity)
        {
            return Session.Save(entity);
        }

        public override void MarkAsRemoved<TEntity>(TEntity entity)
        {
            Session.Delete(entity);
        }

        public override IQueryOfEntities<TEntity> AllOf<TEntity>()
        {
            return new QueryOfEntities<TEntity>(new Session(Session));
        }

        public override IQueryOfEntityByKey<TEntity> UniqueOf<TEntity>(object key)
        {
            return new QueryOfEntityByKey<TEntity>(key, new Session(Session));
        }

        public override IMarkingStep<TEntity> Mark<TEntity>(TEntity entity)
        {
            return new MarkingStep<TEntity>(() => MarkAsCreated(entity), () => MarkAsRemoved(entity));
        }

        public override object NativeProvider
        {
            get { return Session; }
        }

    }
}