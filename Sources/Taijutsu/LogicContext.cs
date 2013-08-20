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

using System;
using System.ComponentModel;

using Taijutsu.Annotation;

namespace Taijutsu
{
    public static class LogicContext
    {
        private static readonly object sync = new object();

        private static ILogicContext context = new DefaultLogicContext();

        private static bool initialized;

        public static void Customize([NotNull] ILogicContext initialContext)
        {
            if (initialContext == null)
            {
                throw new ArgumentNullException("initialContext");
            }

            lock (sync)
            {
                if (initialized)
                {
                    throw new Exception(
                        "Context has already been initialized. Call of any method causes initialization of the context. Context can be customized only once before initialization.");
                }

                initialized = true;

                context = initialContext;
            }
        }

        [CanBeNull]
        public static object FindData([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            CheckInitialization();

            return context.FindData(name);
        }

        public static void SetData([NotNull] string name, object value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            CheckInitialization();

            context.SetData(name, value);
        }

        public static void ReleaseData([NotNull] string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            CheckInitialization();

            context.ReleaseData(name);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static void Reset()
        {
            initialized = false;
            context = new DefaultLogicContext();
        }

        private static void CheckInitialization()
        {
            lock (sync)
            {
                if (initialized)
                {
                    return;
                }

                initialized = true;
            }
        }
    }
}