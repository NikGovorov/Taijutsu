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

namespace Taijutsu.Infrastructure.Config
{
    public class UnitOfQueryConfig
    {
        protected string dataSourceName = Internal.Infrastructure.DefaultUnitSourceName;

        protected internal UnitOfQueryConfig()
        {
        }

        protected internal UnitOfQueryConfig(string sourceName)
        {
            if (string.IsNullOrEmpty(sourceName))
            {
                sourceName = Internal.Infrastructure.DefaultUnitSourceName;
            }
            dataSourceName = sourceName;
        }

        public virtual string SourceName
        {
            get { return dataSourceName; }
            protected set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = Internal.Infrastructure.DefaultUnitSourceName;
                }
                dataSourceName = value;
            }
        }

        public static implicit operator UnitOfQueryConfig(string dataSourceName)
        {
            return new UnitOfQueryConfig {SourceName = dataSourceName};
        }
    }
}