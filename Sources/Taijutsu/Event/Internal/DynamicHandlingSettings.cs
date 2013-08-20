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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Taijutsu.Event.Internal
{
    internal delegate void Handle(object handler, object ev);

    internal delegate bool IsSatisfiedBy(object handler, object ev);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DynamicHandlingSettings : AbstractHandlingSettings
    {
        [ThreadStatic]
        private static Dictionary<Type, Dictionary<Type, Tuple<Handle, IsSatisfiedBy>>> delegatesCache;

        private readonly Func<IEnumerable<Func<object>>> resolver;

        public DynamicHandlingSettings(Type type, Func<IEnumerable<Func<object>>> resolver, int priority) : base(type, priority)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (resolver == null)
            {
                throw new ArgumentNullException("resolver");
            }

            this.resolver = resolver;
        }

        public override Action<object> Action
        {
            get
            {
                return e =>
                {
                    if (e == null)
                    {
                        throw new ArgumentNullException("e");
                    }

                    var handlerResolvers = resolver().ToArray();

                    if (handlerResolvers.Length > 0)
                    {
                        var ev = Type.IsInstanceOfType(e) ? e : null;
                        if (ev != null)
                        {
                            foreach (var handlerResolver in handlerResolvers)
                            {
                                if (handlerResolver != null)
                                {
                                    var handler = handlerResolver();

                                    if (handler != null)
                                    {
                                        var delegates = ResolveMethods(handler.GetType());

                                        if (delegates.Item2 == null || delegates.Item2(handler, ev))
                                        {
                                            delegates.Item1(handler, ev);
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            }
        }

        private static Dictionary<Type, Dictionary<Type, Tuple<Handle, IsSatisfiedBy>>> DelegatesCache
        {
            get { return delegatesCache ?? (delegatesCache = new Dictionary<Type, Dictionary<Type, Tuple<Handle, IsSatisfiedBy>>>()); }
        }

        private Tuple<Handle, IsSatisfiedBy> ResolveMethods(Type handlerImplType)
        {
            Dictionary<Type, Tuple<Handle, IsSatisfiedBy>> cache;
            Tuple<Handle, IsSatisfiedBy> methods;

            if (DelegatesCache.TryGetValue(handlerImplType, out cache))
            {
                if (cache.TryGetValue(Type, out methods))
                {
                    return methods;
                }

                methods = CompileMethods(handlerImplType);
                cache[Type] = methods;
                return methods;
            }

            methods = CompileMethods(handlerImplType);
            DelegatesCache[handlerImplType] = new Dictionary<Type, Tuple<Handle, IsSatisfiedBy>> { { Type, methods } };
            return methods;
        }

        private Tuple<Handle, IsSatisfiedBy> CompileMethods(Type handlerImplType)
        {
            var handlerType = typeof(IHandler<>).MakeGenericType(Type);
            var specHandlerType = typeof(ISpecHandler<>).MakeGenericType(Type);

            var handle = handlerType.GetMethods().Single();
            var isSatisfiedBy = handlerImplType.GetInterfaces().Contains(specHandlerType) ? specHandlerType.GetMethods().Single() : null;

            var instanceParameter = Expression.Parameter(typeof(object), "handler");
            var argumentParameter = Expression.Parameter(typeof(object), "ev");

            // ReSharper disable AssignNullToNotNullAttribute
            // ReSharper disable PossiblyMistakenUseOfParamsMethod
            var call = Expression.Call(
                Expression.Convert(instanceParameter, handle.DeclaringType), 
                handle, 
                Expression.Convert(argumentParameter, handle.GetParameters().Single().ParameterType));

            var handleLambda = Expression.Lambda<Handle>(call, instanceParameter, argumentParameter);

            var handleCaller = handleLambda.Compile();

            IsSatisfiedBy isSatisfiedByCaller = null;

            if (isSatisfiedBy != null)
            {
                call = Expression.Call(
                    Expression.Convert(instanceParameter, isSatisfiedBy.DeclaringType), 
                    isSatisfiedBy, 
                    Expression.Convert(argumentParameter, isSatisfiedBy.GetParameters().Single().ParameterType));

                var isSatisfiedByLambda = Expression.Lambda<IsSatisfiedBy>(Expression.Convert(call, typeof(bool)), instanceParameter, argumentParameter);

                isSatisfiedByCaller = isSatisfiedByLambda.Compile();
            }

            // ReSharper restore PossiblyMistakenUseOfParamsMethod
            // ReSharper restore AssignNullToNotNullAttribute
            return new Tuple<Handle, IsSatisfiedBy>(handleCaller, isSatisfiedByCaller);
        }
    }
}