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
using System.Linq.Expressions;
using NHibernate;

namespace Taijutsu.Data.NHibernate
{
    public class Session : ISessionDecorator
    {
        private readonly ISession realSession;

        public Session(ISession realSession)
        {
            this.realSession = realSession;
        }

        public virtual object RealSession
        {
            get { return realSession; }
        }

        public virtual bool IsStataless
        {
            get { return false; }
        }

        public virtual object Get(string entityName, object id)
        {
            return realSession.Get(entityName, id);
        }

        public virtual T Get<T>(object id)
        {
            return realSession.Get<T>(id);
        }

        public virtual T Get<T>(object id, LockMode lockMode)
        {
            return realSession.Get<T>(id, lockMode);
        }

        public virtual IQuery CreateQuery(string queryString)
        {
            return realSession.CreateQuery(queryString);
        }

        public virtual IQuery GetNamedQuery(string queryName)
        {
            return realSession.GetNamedQuery(queryName);
        }

        public virtual ICriteria CreateCriteria<T>() where T : class
        {
            return realSession.CreateCriteria<T>();
        }

        public virtual ICriteria CreateCriteria<T>(string alias) where T : class
        {
            return realSession.CreateCriteria<T>(alias);
        }

        public virtual ICriteria CreateCriteria(Type entityType)
        {
            return realSession.CreateCriteria(entityType);
        }

        public virtual ICriteria CreateCriteria(Type entityType, string alias)
        {
            return realSession.CreateCriteria(entityType, alias);
        }

        public virtual ICriteria CreateCriteria(string entityName)
        {
            return realSession.CreateCriteria(entityName);
        }

        public virtual ICriteria CreateCriteria(string entityName, string alias)
        {
            return realSession.CreateCriteria(entityName, alias);
        }

        public virtual IQueryOver<T, T> QueryOver<T>() where T : class
        {
            return realSession.QueryOver<T>();
        }

        public virtual IQueryOver<T, T> QueryOver<T>(Expression<Func<T>> alias) where T : class
        {
            return realSession.QueryOver(alias);
        }

        public virtual ISQLQuery CreateSqlQuery(string queryString)
        {
            return realSession.CreateSQLQuery(queryString);
        }

        public void Dispose()
        {
            realSession.Dispose();
        }
    }
}