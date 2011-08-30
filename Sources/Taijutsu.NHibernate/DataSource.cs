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

using System.Data;
using NHibernate;

namespace Taijutsu.Data.NHibernate
{
    public class DataSource : Data.Internal.DataSource
    {
        private readonly ISessionFactory sessionFactory;

        public DataSource(string sourceName, ISessionFactory sessionFactory) : base(sourceName)
        {
            this.sessionFactory = sessionFactory;
        }

        public override Data.Internal.DataProvider BuildDataProvider(IsolationLevel isolationLevel)
        {
            return new DataProvider(sessionFactory);
        }

        public override Data.Internal.ReadOnlyDataProvider BuildReadOnlyDataProvider(IsolationLevel isolationLevel)
        {
            return new ReadOnlyDataProvider(sessionFactory);
        }
    }
}