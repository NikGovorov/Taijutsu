﻿// Copyright 2009-2013 Nikita Govorov
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

using System;
using System.Data;

using Taijutsu.Annotation;

namespace Taijutsu.Data.Internal
{
    public class DataSource
    {
        private readonly string name = string.Empty;

        private readonly IsolationLevel defaultIsolationLevel;

        private readonly Func<IsolationLevel, IDataSession> sessionBuilder;

        public DataSource([NotNull] Func<IsolationLevel, IDataSession> sessionBuilder)
            : this(string.Empty, IsolationLevel.RepeatableRead, sessionBuilder)
        {
        }

        public DataSource([NotNull] string name, [NotNull] Func<IsolationLevel, IDataSession> sessionBuilder)
            : this(name, IsolationLevel.RepeatableRead, sessionBuilder)
        {
        }

        public DataSource([NotNull] string name, IsolationLevel defaultIsolationLevel, [NotNull] Func<IsolationLevel, IDataSession> sessionBuilder)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (sessionBuilder == null)
            {
                throw new ArgumentNullException("sessionBuilder");
            }

            this.name = name;
            this.defaultIsolationLevel = defaultIsolationLevel;
            this.sessionBuilder = sessionBuilder;
        }

        public virtual string Name
        {
            get { return name; }
        }

        public virtual IsolationLevel DefaultIsolationLevel
        {
            get { return defaultIsolationLevel; }
        }

        public virtual IDataSession BuildSession(IsolationLevel isolationLevel)
        {
            return sessionBuilder(isolationLevel);
        }
    }
}