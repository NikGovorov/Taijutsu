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
using System.Collections.Generic;
using System.ComponentModel;

namespace Taijutsu.Data.Internal
{

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class Infrastructure
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static string DefaultDataSourceName = "Core";

        private static IDictionary<string, DataSource> dataSources = new Dictionary<string, DataSource>();
        private static object sync = new object();

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static DataSource DataSource(string name)
        {
            DataSource dataSource;

            if (!dataSources.TryGetValue(name, out dataSource))
            {
                throw new Exception(string.Format("Data source '{0}' has not yet been initialized.", name));
            }
            return dataSource;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void RegisterDataSource(DataSource dataSource)
        {
            DataSource exDataSource;

            if (!dataSources.TryGetValue(dataSource.Name, out exDataSource))
            {
                lock (sync)
                {
                    if (!dataSources.TryGetValue(dataSource.Name, out exDataSource))
                    {
                        IDictionary<string, DataSource> newDataSources = new Dictionary<string, DataSource>(dataSources);
                        newDataSources.Add(dataSource.Name, dataSource);
                        dataSources = newDataSources;
                    }
                    return;
                }
            }
            throw new Exception(string.Format("Data source '{0}' has already been initialized.", dataSource.Name));
        }
        
        
        /*  
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Maybe<IAdvancedUnitOfWork> CurrentUnitOfWork
        {
            get
            {
                if (!SupervisorContext.DataContextSupervisor.CurrentContext)
                {
                    return Maybe<IAdvancedUnitOfWork>.Empty;
                }

                return new Maybe<IAdvancedUnitOfWork>(SupervisorContext.DataContextSupervisor.CurrentContext.Value);
            }
        }*/
    }
}