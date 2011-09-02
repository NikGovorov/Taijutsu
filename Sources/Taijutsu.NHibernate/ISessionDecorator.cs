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
    public interface ISessionDecorator
    {

        object RealSession { get; }

        bool IsStataless { get; }

        object Get(string entityName, object id);

        T Get<T>(object id);

        T Get<T>(object id, LockMode lockMode);

        IQuery CreateQuery(string queryString);

        IQuery GetNamedQuery(string queryName);

        ICriteria CreateCriteria<T>() where T : class;

        ICriteria CreateCriteria<T>(string alias) where T : class;

        ICriteria CreateCriteria(Type entityType);

        ICriteria CreateCriteria(Type entityType, string alias);

        ICriteria CreateCriteria(string entityName);

        ICriteria CreateCriteria(string entityName, string alias);

        IQueryOver<T, T> QueryOver<T>() where T : class;

        IQueryOver<T, T> QueryOver<T>(Expression<Func<T>> alias) where T : class;

        ISQLQuery CreateSqlQuery(string queryString);
         
    }
}