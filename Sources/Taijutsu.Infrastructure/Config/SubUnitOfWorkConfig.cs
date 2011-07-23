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
    public class SubUnitOfWorkConfig
    {
        private bool runAsRoot;
        private UnitOfWorkConfig unitOfWorkConfig;

        protected internal SubUnitOfWorkConfig(bool runAsRoot, UnitOfWorkConfig unitOfWorkConfig)
        {
            this.unitOfWorkConfig = unitOfWorkConfig;
            this.runAsRoot = runAsRoot;
        }

        public virtual bool RunAsRoot
        {
            get { return runAsRoot; }
            protected set { runAsRoot = value; }
        }

        public virtual UnitOfWorkConfig UnitOfWorkConfig
        {
            get { return unitOfWorkConfig; }
            protected set { unitOfWorkConfig = value; }
        }

        public static implicit operator SubUnitOfWorkConfig(string dataSourceName)
        {
            return new SubUnitOfWorkConfig(false, new UnitOfWorkConfig(dataSourceName));
        }
    }

    public static class AsRoot
    {
        public static SubUnitOfWorkConfig IfThereIsNoRoot(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, string source = "")
        {
            return new SubUnitOfWorkConfig(true, new UnitOfWorkConfig(source, isolationLevel));
        }
    }
}