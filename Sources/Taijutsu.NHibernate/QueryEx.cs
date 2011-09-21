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

using System.ComponentModel;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using Taijutsu.Data;
using Taijutsu.Data.Internal;
using Taijutsu.Domain;

namespace Taijutsu.Data.NHibernate
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class QueryEx
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IQueryable<TEntity> AllOfEx<TEntity>(this UnitOfWork unitOfWork) where TEntity : IQueryableEntity
        {
            INative native = unitOfWork;
            var session = native.Native as ISession;
            return session.Query<TEntity>();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IQueryable<TEntity> AllOfEx<TEntity>(this UnitOfQuery unitOfQuery) where TEntity : IEntity
        {
            INative native = unitOfQuery;
            var session = native.Native as ISession;
            return session.Query<TEntity>();
        }
    }
}