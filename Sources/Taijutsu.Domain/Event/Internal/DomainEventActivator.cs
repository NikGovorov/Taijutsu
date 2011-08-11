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
        DomainEvent CreateInstance(IEntity subject, IFact fact);
    }

    internal class DomainEventActivator<T> : IDomainEventActivator where T : DomainEvent
    {
        private const BindingFlags ConstructorBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // ReSharper disable StaticFieldInGenericType
        private static DomainEventActivator<T> current = new DomainEventActivator<T>();
        // ReSharper restore StaticFieldInGenericType

        private readonly ConstructorInfo ñonstructor;
        private Func<object[], T> ctor;

        private DomainEventActivator()
        {
            ñonstructor = typeof(T).GetConstructors(ConstructorBindingFlags).Where(c => c.GetParameters().Any()).Single();
        }

        public static DomainEventActivator<T> Current
        {
            get { return current; }
        }

        #region IEventActivator Members

        public DomainEvent CreateInstance(IEntity subject, IFact fact)
        {
            var args = new object[]{subject, fact};

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

                var parameters = ñonstructor.GetParameters().Select(converter).ToArray();

                var newExpression = Expression.New(ñonstructor, parameters);

                ctor = Expression.Lambda<Func<object[], T>>(newExpression, argsParameter).Compile();
            }

            return ctor(args);
        }

        #endregion
    }

    internal class DomainEventActivators
    {
        private class Activator
        {
            private Type type;
            private IDomainEventActivator activator;

            public Activator(Type type, IDomainEventActivator activator)
            {
                this.type = type;
                this.activator = activator;
            }

            public Type Type
            {
                get { return type; }
            }

            public IDomainEventActivator EventActivator
            {
                get { return activator; }
            }
        }

        [ThreadStatic]
        private static Dictionary<object, Activator> activators;

        private static Dictionary<object, Activator> Activators
        {
            get { return activators ?? (activators = new Dictionary<object, Activator>()); }
        }

        internal static IDomainEventActivator ActivatorFor(Type entityType, Type factType)
        {
            var eventTypeDef = new { entityType, factType };

            Activator activator;
            if (!Activators.TryGetValue(eventTypeDef, out activator))
            {
                var eventType = typeof(DomainEvent<,>).MakeGenericType(new[] { entityType, factType });
                const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
                var domainEventActivator = (IDomainEventActivator)typeof(DomainEventActivator<>).MakeGenericType(eventType)
                                                  .GetProperty("Current", flags)
                                                  .GetValue(null, flags, null, null, CultureInfo.InvariantCulture);

                activator = new Activator(eventType, domainEventActivator);
                Activators.Add(eventTypeDef, activator);
            }
            return activator.EventActivator;
        }
    }
}