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
using System.ComponentModel;
using Taijutsu.Domain;

namespace Taijutsu.Infrastructure.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class Infrastructure
    {
        private static IDataProviderFactory dataProviderFactory;
        public static string DefaultUnitSourceName = "main.";
        private static IReadOnlyDataProviderFactory readOnlyDataProviderFactory;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IReadOnlyDataProviderFactory ReadOnlyDataProviderFactory
        {
            get
            {
                if (readOnlyDataProviderFactory == null)
                {
                    throw new Exception("ReadOnlyDataProviderFactory has not been initialized.");
                }

                return readOnlyDataProviderFactory;
            }
            set
            {
                if (readOnlyDataProviderFactory != null)
                {
                    throw new Exception("ReadOnlyDataProviderFactory has been already initialized.");
                }

                readOnlyDataProviderFactory = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IDataProviderFactory DataProviderFactory
        {
            get
            {
                if (dataProviderFactory == null)
                {
                    throw new Exception("DataProviderFactory has not been initialized.");
                }

                return dataProviderFactory;
            }
            set
            {
                if (dataProviderFactory != null)
                {
                    throw new Exception("DataProviderFactory has been already initialized.");
                }

                dataProviderFactory = value;
            }
        }

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
        }
    }
}