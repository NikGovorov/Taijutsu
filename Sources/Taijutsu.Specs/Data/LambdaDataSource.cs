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
using System.Data;
using Taijutsu.Data.Internal;

namespace Taijutsu.Specs.Data
{
    public class LambdaDataSource: DataSource
    {
        private Func<DataProvider> dpFactory=() => { throw new NotImplementedException();};
        
        private Func<ReadOnlyDataProvider> rdpFactory = () => { throw new NotImplementedException(); };

        public LambdaDataSource(Func<DataProvider> dpFactory, string name="") : base(name)
        {
            this.dpFactory = dpFactory;
        }

        public LambdaDataSource(Func<ReadOnlyDataProvider> rdpFactory, string name = "")
            : base(name)
        {
            this.rdpFactory = rdpFactory;
        }

        public LambdaDataSource(Func<DataProvider> dpFactory, Func<ReadOnlyDataProvider> rdpFactory, string name = "")
            : base(name)
        {
            this.dpFactory = dpFactory;
            this.rdpFactory = rdpFactory;
        }


        public override DataProvider BuildDataProvider(IsolationLevel isolationLevel)
        {
            return dpFactory();
        }

        public override ReadOnlyDataProvider BuildReadOnlyDataProvider(IsolationLevel isolationLevel)
        {
            return rdpFactory();
        }
    }
}