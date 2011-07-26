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

namespace Taijutsu.Infrastructure.Internal
{
    public class UnitOfWorkConfig
    {
        protected string dataSourceName = Infrastructure.DefaultDataSourceName;
        protected IsolationLevel isolation = IsolationLevel.Unspecified;
        protected Require req = Require.None;

        protected internal UnitOfWorkConfig()
        {
        }

        protected internal UnitOfWorkConfig(string sourceName, IsolationLevel? isolationLevel, Require require)
        {
            dataSourceName = string.IsNullOrEmpty(sourceName) ? Infrastructure.DefaultDataSourceName : sourceName;
            isolation = isolationLevel ?? Infrastructure.DefaultWorkIsolationLevel;
            req = require;
        }

        public Require Require
        {
            get { return req; }
        }

        public IsolationLevel IsolationLevel
        {
            get { return isolation; }
        }

        public virtual string SourceName
        {
            get { return dataSourceName; }
        }
    }
}