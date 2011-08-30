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
using Taijutsu.Data.Internal;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Data.NHibernate
{
    public class QueryOverBuilder<TEntity> : IQueryOverBuilder<TEntity> where TEntity : class, IEntity
    {
        private readonly ISessionDecorator session;

        public QueryOverBuilder(ISessionDecorator session)
        {
            this.session = session;
        }

        protected virtual ISessionDecorator Session
        {
            get { return session; }
        }

        public virtual TQuery Using<TQuery>() where TQuery : IQueryOver<TEntity>
        {
            throw new NotImplementedException();
        }

        public virtual TQuery Using<TQuery>(string queryName) where TQuery : IQueryOver<TEntity>
        {
            throw new NotImplementedException();
        }

        public virtual TRepository In<TRepository>() where TRepository : IRepository<TEntity>
        {
            throw new NotImplementedException();
        }

        public virtual TRepository In<TRepository>(string repositoryName) where TRepository : IRepository<TEntity>
        {
            throw new NotImplementedException();
        }
    }
}