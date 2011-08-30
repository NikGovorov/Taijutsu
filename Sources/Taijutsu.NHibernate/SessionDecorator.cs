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
    public class SessionDecorator : ISessionDecorator
    {
        public SessionDecorator(ISession session)
        {
        }

        public SessionDecorator(IStatelessSession statelessSession)
        {
        }

        public virtual object Session
        {
            get { throw new NotImplementedException(); }
        }

        public virtual bool IsStataless
        {
            get { throw new NotImplementedException(); }
        }

        public virtual object Get(string entityName, object id)
        {
            throw new NotImplementedException();
        }

        public virtual T Get<T>(object id)
        {
            throw new NotImplementedException();
        }

        public virtual object Get(string entityName, object id, LockMode lockMode)
        {
            throw new NotImplementedException();
        }

        public virtual T Get<T>(object id, LockMode lockMode)
        {
            throw new NotImplementedException();
        }

        public virtual IQuery CreateQuery(string queryString)
        {
            throw new NotImplementedException();
        }

        public virtual IQuery GetNamedQuery(string queryName)
        {
            throw new NotImplementedException();
        }

        public virtual ICriteria CreateCriteria<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public virtual ICriteria CreateCriteria<T>(string alias) where T : class
        {
            throw new NotImplementedException();
        }

        public virtual ICriteria CreateCriteria(Type entityType)
        {
            throw new NotImplementedException();
        }

        public virtual ICriteria CreateCriteria(Type entityType, string alias)
        {
            throw new NotImplementedException();
        }

        public virtual ICriteria CreateCriteria(string entityName)
        {
            throw new NotImplementedException();
        }

        public virtual ICriteria CreateCriteria(string entityName, string alias)
        {
            throw new NotImplementedException();
        }

        public virtual IQueryOver<T, T> QueryOver<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public virtual IQueryOver<T, T> QueryOver<T>(Expression<Func<T>> alias) where T : class
        {
            throw new NotImplementedException();
        }

        public virtual ISQLQuery CreateSqlQuery(string queryString)
        {
            throw new NotImplementedException();
        }
    }
}