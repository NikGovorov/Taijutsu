// Copyright 2009-2013 Nikita Govorov
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System.Data;

using Taijutsu.Annotation;

namespace Taijutsu.Data
{
    public enum Require
    {
        New, 

        Existing, 

        None
    }

    public class UnitOfWorkConfig
    {
        private readonly string sourceName = string.Empty;

        private readonly IsolationLevel isolationLevel = IsolationLevel.Unspecified;

        private readonly Require require = Require.None;

        public UnitOfWorkConfig([NotNull] string sourceName, IsolationLevel isolationLevel, Require require)
        {
            this.sourceName = sourceName;
            this.isolationLevel = isolationLevel;
            this.require = require;
        }

        public Require Require
        {
            get { return require; }
        }

        public IsolationLevel IsolationLevel
        {
            get { return isolationLevel; }
        }

        public string SourceName
        {
            get { return sourceName; }
        }
    }
}