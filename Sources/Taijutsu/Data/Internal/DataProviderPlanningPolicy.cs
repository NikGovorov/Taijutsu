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


namespace Taijutsu.Data.Internal
{
    public class DataProviderPlanningPolicy : IDataProviderPlanningPolicy
    {
        #region IDataProviderPlanningPolicy Members

        public DataProvider Register(UnitOfWorkConfig config)
        {
            return Infrastructure.DataSource(config.SourceName).BuildDataProvider(config.IsolationLevel);
        }

        public ReadOnlyDataProvider Register(UnitOfQueryConfig config)
        {
            return Infrastructure.DataSource(config.SourceName).BuildReadOnlyDataProvider(config.IsolationLevel);
        }
        public virtual void Terminate(ReadOnlyDataProvider readOnlyDataProvider)
        {
            readOnlyDataProvider.Close();
        }

        public virtual void Terminate(DataProvider dataProvider)
        {
            dataProvider.Close();
        }


        public virtual void Dispose()
        {
        }

        #endregion
    }
}