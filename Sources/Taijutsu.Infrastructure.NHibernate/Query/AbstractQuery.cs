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


using System.Collections.Generic;
using Taijutsu.Domain;
using Taijutsu.Domain.Query;

namespace Taijutsu.Infrastructure.NHibernate.Query
{
    public abstract class AbstractQuery : IQuery//, IReadWritePessimisticQuery, IWritePessimisticQuery
    {
        private readonly ISessionDecorator session;

        protected AbstractQuery(ISessionDecorator session)
        {
            this.session = session;
        }

        protected virtual ISessionDecorator Session
        {
            get { return session; }
        }

        //bool IReadWritePessimisticQuery.LockReadWriteOperations { set { LockReadWriteOperations = value; } }

        //bool IWritePessimisticQuery.LockWriteOperations { set { LockWriteOperations = value; } }

        protected virtual bool LockReadWriteOperations { get; set; }

        protected virtual bool LockWriteOperations { get; set; }

    }

    public abstract class AbstractQuery<TEntity> : AbstractQuery where TEntity : IEntity
    {
        protected AbstractQuery(ISessionDecorator session)
            : base(session)
        {
        }

        protected virtual IEnumerable<TEntity> AnalyzeQueryOutput(IEnumerable<TEntity> output)
        {
            return output;
        }

        protected virtual IEnumerable<TEntity> AnalyzeQueryOutputByDefault(IEnumerable<TEntity> output)
        {
            return output;
        }

    }
}