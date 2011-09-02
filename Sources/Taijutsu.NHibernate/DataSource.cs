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
    public class DataSource : Internal.DataSource, IDataQueryLocator
    {
        private readonly ISessionFactory sessionFactory;
        private readonly IDataQueryLocator dataQueryLocator;

        public DataSource(ISessionFactory sessionFactory, IDataQueryLocator dataQueryLocator, string sourceName = "")
            : base(sourceName)
        {
            this.sessionFactory = sessionFactory;
            this.dataQueryLocator = dataQueryLocator;
        }

        public virtual ISessionFactory SessionFactory
        {
            get { return sessionFactory; }
        }

        protected override Internal.DataProvider BuildDataProvider(IsolationLevel isolationLevel)
        {
            return new DataProvider(this);
        }

        protected override Internal.ReadOnlyDataProvider BuildReadOnlyDataProvider(IsolationLevel isolationLevel)
        {
            return new ReadOnlyDataProvider(this);
        }

        TQuery IDataQueryLocator.LocateQuery<TQuery>(ISessionDecorator session, string name)
        {
            return dataQueryLocator.LocateQuery<TQuery>(session, name);
        }

        TRepository IDataQueryLocator.LocateRepository<TRepository>(ISessionDecorator session, string name)
        {
            return dataQueryLocator.LocateQuery<TRepository>(session, name);
        }
    }
}