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
using System.Collections.Generic;

using Taijutsu.Annotation;

namespace Taijutsu.Domain
{
    [PublicApi]
    public static class ObjectCastingRegistry
    {
        private static IObjectCasting native = new ObjectCasting();

        private static IList<IObjectCasting> plugins = new List<IObjectCasting>();

        public static IObjectCasting Default
        {
            get { return native; }
            set { native = value ?? new ObjectCasting(); }
        }

        public static IEnumerable<IObjectCasting> Plugins
        {
            get { return plugins; }
        }

        public static void CustomizeWith([NotNull] IEnumerable<IObjectCasting> castings)
        {
            if (castings == null)
            {
                throw new ArgumentNullException("castings");
            }

            plugins = new List<IObjectCasting>(castings);
        }

        private class ObjectCasting : IObjectCasting
        {
            public bool IsApplicableFor(object entity)
            {
                return true;
            }

            public T As<T>(object source) where T : class
            {
                return source as T;
            }

            public bool Is<T>(object source) where T : class
            {
                return source is T;
            }

            public T Cast<T>(object source)
            {
                return (T)source;
            }
        }
    }
}