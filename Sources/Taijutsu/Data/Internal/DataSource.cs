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

namespace Taijutsu.Data.Internal
{
    public abstract class DataSource
    {
        private IsolationLevel defaultQueryIsolationLevel = IsolationLevel.Unspecified;
        private IsolationLevel defaultWorkIsolationLevel = IsolationLevel.Unspecified;
        private string sourceName;


        protected DataSource(string sourceName = "")
        {
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                sourceName = Infrastructure.DefaultDataSourceName;
            }

            this.sourceName = sourceName;
        }


        public virtual IsolationLevel DefaultWorkIsolationLevel
        {
            get { return defaultWorkIsolationLevel; }
            set { defaultWorkIsolationLevel = value; }
        }

        public virtual IsolationLevel DefaultQueryIsolationLevel
        {
            get { return defaultQueryIsolationLevel; }
            set { defaultQueryIsolationLevel = value; }
        }


        public virtual string Name
        {
            get { return sourceName; }
        }

        protected internal abstract DataProvider BuildDataProvider(IsolationLevel isolationLevel);

        protected internal abstract ReadOnlyDataProvider BuildReadOnlyDataProvider(IsolationLevel isolationLevel);
    }
}