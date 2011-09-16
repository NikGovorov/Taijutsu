using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Taijutsu.Domain.Event.Internal
{

    internal interface IDomainEventActivator
    {
        DomainEvent CreateInstance(IEntity subject, IFact fact, Guid? eventKey = null);
    }

    internal class DomainEventActivator<T> : IDomainEventActivator where T : DomainEvent
    {
        private const BindingFlags ConstructorBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // ReSharper disable StaticFieldInGenericType
        private static DomainEventActivator<T> current = new DomainEventActivator<T>();
        // ReSharper restore StaticFieldInGenericType

        private readonly ConstructorInfo constructor;
        private Func<object[], T> ctor;

        private DomainEventActivator()
        {
            constructor = typeof(T).GetConstructors(ConstructorBindingFlags).Where(c => c.GetParameters().Any()).Single();
        }

        public static DomainEventActivator<T> Current
        {
            get { return current; }
        }

        #region IDomainEventActivator Members

        public DomainEvent CreateInstance(IEntity subject, IFact fact, Guid? eventKey = null)
        {
            var args = new object[] { subject, fact, eventKey };

            if (ctor == null)
            {
                var argsParameter = Expression.Parameter(typeof(object[]), "args");

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

        #endregion
    }

    internal class DomainEventActivatorsHolder
    {
        private class TypedActivator
        {
            private Type type;
            private IDomainEventActivator domainEventActivator;

            public TypedActivator(Type type, IDomainEventActivator domainEventActivator)
            {
                this.type = type;
                this.domainEventActivator = domainEventActivator;
            }

            public Type Type
            {
                get { return type; }
            }

            public IDomainEventActivator DomainEventActivator
            {
                get { return domainEventActivator; }
            }
        }

        [ThreadStatic]
        private static Dictionary<object, TypedActivator> activators;

        private static Dictionary<object, TypedActivator> Activators
        {
            get { return activators ?? (activators = new Dictionary<object, TypedActivator>()); }
        }

        internal static IDomainEventActivator ActivatorFor(Type entityType, Type factType)
        {
            var eventTypeDef = new { entityType, factType };

            TypedActivator activator;
            if (!Activators.TryGetValue(eventTypeDef, out activator))
            {
                var eventType = typeof(DomainEvent<,>).MakeGenericType(new[] { entityType, factType });
                const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
                var domainEventActivator = (IDomainEventActivator)typeof(DomainEventActivator<>).MakeGenericType(eventType)
                                                  .GetProperty("Current", flags)
                                                  .GetValue(null, flags, null, null, CultureInfo.InvariantCulture);

                activator = new TypedActivator(eventType, domainEventActivator);
                Activators.Add(eventTypeDef, activator);
            }
            return activator.DomainEventActivator;
        }
    }
}