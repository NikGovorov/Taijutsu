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
using Taijutsu.Domain.Query;

namespace Taijutsu.Infrastructure.NHibernate.Query
{
    public class QueryOfAny : IQueryOf<bool>
    {
        private readonly Func<bool> predicate;

        public QueryOfAny(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public virtual Func<bool> Predicate
        {
            get { return predicate; }
        }

        #region IQueryOf<bool> Members

        public virtual bool Query()
        {
            return Predicate();
        }

        #endregion
    }
}