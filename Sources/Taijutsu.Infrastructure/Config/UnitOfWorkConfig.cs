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

namespace Taijutsu.Infrastructure.Config
{
    public class UnitOfWorkConfig : UnitOfQueryConfig
    {
        private IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;


        protected internal UnitOfWorkConfig()
        {
        }


        protected internal UnitOfWorkConfig(string sourceName, IsolationLevel isolationLevel)
            : base(sourceName)
        {
            this.isolationLevel = isolationLevel;
        }

        protected internal UnitOfWorkConfig(string sourceName)
            : base(sourceName)
        {
        }

        protected internal UnitOfWorkConfig(IsolationLevel isolationLevel)
        {
            this.isolationLevel = isolationLevel;
        }

        public virtual IsolationLevel IsolationLevel
        {
            get { return isolationLevel; }
            protected set { isolationLevel = value; }
        }

        public static implicit operator UnitOfWorkConfig(string dataSourceName)
        {
            return new UnitOfWorkConfig {SourceName = dataSourceName};
        }

        public static implicit operator UnitOfWorkConfig(IsolationLevel isolationLevel)
        {
            return new UnitOfWorkConfig {IsolationLevel = isolationLevel};
        }
    }

    public static class IsolationLevelEx
    {
        public static UnitOfWorkConfig InContextOf(this IsolationLevel isolationLevel, string source)
        {
            return new UnitOfWorkConfig(source, isolationLevel);
        }
    }
}