// Copyright 2009-2014 Nikita Govorov
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
using System.Diagnostics.CodeAnalysis;

using Taijutsu.Annotation;

namespace Taijutsu.Data.Internal
{
    public class DataSessionBuilder
    {
        private readonly Func<object, IDataSession> factory;

        private readonly string name;

        public DataSessionBuilder([NotNull] Func<IDataSession> factory, [NotNull] string name)
            : this(options => factory(), name)
        {
        }

        public DataSessionBuilder([NotNull] Func<object, IDataSession> factory, [NotNull] string name)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.factory = factory;
            this.name = name;
        }

        public virtual string Name
        {
            get { return name; }
        }

        public virtual IDataSession BuildSession(object options = null)
        {
            using (new ConstructionScope())
            {
                return factory(options);
            }
        }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppress the warning for generics.")]
    public class DataSessionBuilder<T> : DataSessionBuilder
    {
        public DataSessionBuilder([NotNull] Func<IDataSession> factory, string name = null)
            : this(options => factory(), name)
        {
        }

        public DataSessionBuilder([NotNull] Func<object, IDataSession> factory, string name = null)
            : base(factory, name ?? typeof(T).FullName)
        {
        }
    }
}