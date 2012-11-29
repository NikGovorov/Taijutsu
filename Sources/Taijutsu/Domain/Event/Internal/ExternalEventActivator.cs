#region License

//  Copyright 2009-2013 Nikita Govorov
//    
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
//  this file except in compliance with the License. You may obtain a copy of the 
//  License at 
//   
//  http://www.apache.org/licenses/LICENSE-2.0 
//   
//  Unless required by applicable law or agreed to in writing, software distributed 
//  under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
//  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
//  specific language governing permissions and limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Taijutsu.Domain.Event.Internal
{
    internal interface IExternalEventActivator
    {
        ExternalEvent CreateInstance(IEntity recipient, IFact fact, DateTime? occurrenceDate = null,
                                     DateTime? noticeDate = null, Guid? id = null);
    }

    internal class ExternalEventActivator<T> : IExternalEventActivator where T : ExternalEvent
    {
        private const BindingFlags ConstructorBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly ExternalEventActivator<T> current = new ExternalEventActivator<T>();

        private readonly ConstructorInfo constructor;
        private Func<object[], T> ctor;

        private ExternalEventActivator()
        {
            constructor = typeof (T).GetConstructors(ConstructorBindingFlags).Single(c => c.GetParameters().Any());
        }

        public static ExternalEventActivator<T> Current
        {
            get { return current; }
        }

        public ExternalEvent CreateInstance(IEntity recipient, IFact fact, DateTime? occurrenceDate = null,
                                            DateTime? noticeDate = null, Guid? id = null)
        {
            var args = new object[] { recipient, fact, occurrenceDate, noticeDate, id };

            if (ctor == null)
            {
                var argsParameter = Expression.Parameter(typeof (object[]), "args");

                Func<ParameterInfo, int, Expression> converter = (parameter, index) =>
                    {
                        var arrayExpression = Expression.ArrayIndex(argsParameter, Expression.Constant(index));

                        if (parameter.ParameterType.IsValueType)
                            return Expression.Convert(arrayExpression, parameter.ParameterType);

                        return Expression.TypeAs(arrayExpression, parameter.ParameterType);
                    };

                var parameters = constructor.GetParameters().Select(converter).ToArray();

                var newExpression = Expression.New(constructor, parameters);

                ctor = Expression.Lambda<Func<object[], T>>(newExpression, argsParameter).Compile();
            }

            return ctor(args);
        }
    }

    internal class ExternalEventActivatorsHolder
    {
        [ThreadStatic]
        private static Dictionary<object, IExternalEventActivator> activators;

        private static Dictionary<object, IExternalEventActivator> Activators
        {
            get { return activators ?? (activators = new Dictionary<object, IExternalEventActivator>()); }
        }

        internal static IExternalEventActivator ActivatorFor(Type entityType, Type factType)
        {
            var eventTypeDef = new {entityType, factType};

            IExternalEventActivator activator;

            if (!Activators.TryGetValue(eventTypeDef, out activator))
            {
                var eventType = typeof (DomainEvent<,>).MakeGenericType(new[] {entityType, factType});
                const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
                activator = (IExternalEventActivator) typeof (ExternalEventActivator<>).MakeGenericType(eventType)
                                                                               .GetProperty("Current", flags)
                                                                               .GetValue(null, flags, null, null, CultureInfo.InvariantCulture);

                Activators.Add(eventTypeDef, activator);
            }

            return activator;
        }
    }
}