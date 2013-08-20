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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

using Taijutsu.Data.Internal;

namespace Taijutsu.Event.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BatchedHandlingSettings : AbstractHandlingSettings
    {
        [ThreadStatic]
        private static IDictionary<Type, Func<IEnumerable, object>> constructors;

        private readonly Action<object> batchedAction;

        public BatchedHandlingSettings(Type type, DelayUntil delayUntil, int priority) : base(type, priority)
        {
            batchedAction = ev =>
            {
                if (ev == null)
                {
                    throw new ArgumentNullException("ev");
                }

                if (Type.IsInstanceOfType(ev))
                {
                    var context = InternalEnvironment.DataContextSupervisor.CurrentContext;

                    if (context != null)
                    {
                        var extension = InitializeExtension(context);

                        if (!extension.ContainsKey(this))
                        {
                            extension[this] = new List<object>();
                            EventHandler<FinishedEventArgs> action = null;

                            action = (sender, e) =>
                            {
                                try
                                {
                                    // ReSharper disable AccessToModifiedClosure
                                    switch (delayUntil)
                                    {
                                        case DelayUntil.PreCompleted:
                                            context.BeforeCompleted -= action;
                                            break;
                                        case DelayUntil.Completed:
                                            context.AfterCompleted -= action;
                                            break;
                                        case DelayUntil.Finished:
                                            context.Finished -= action;
                                            break;
                                    }

                                    var events = (List<object>)extension[this];

                                    extension.Remove(this);

                                    // ReSharper restore AccessToModifiedClosure
                                    if (events.Count > 0 && e.Completed)
                                    {
                                        Events.Publish(ResolveConstructor()(events));
                                    }
                                }
                                finally
                                {
                                    context = null;
                                    action = null;
                                }
                            };

                            switch (delayUntil)
                            {
                                case DelayUntil.PreCompleted:
                                    context.BeforeCompleted += action;
                                    break;
                                case DelayUntil.Completed:
                                    context.AfterCompleted += action;
                                    break;
                                case DelayUntil.Finished:
                                    context.Finished += action;
                                    break;
                            }
                        }

                        ((List<object>)extension[this]).Add(ev);
                    }
                    else
                    {
                        Trace.TraceWarning("Taijutsu: Source of event is outside of unit of work, event is skipped.");
                    }
                }
            };
        }

        public override Action<object> Action
        {
            get { return batchedAction; }
        }

        private static IDictionary<Type, Func<IEnumerable, object>> ConstructorsCache
        {
            get { return constructors ?? (constructors = new Dictionary<Type, Func<IEnumerable, object>>()); }
        }

        private static IDictionary<object, object> InitializeExtension(IDataContext context)
        {
            var extensions = (Dictionary<string, object>)context.Extra.Extensions;

            const string name = "Taijutsu.BatchedHandlerSettings";

            object extension;
            if (!extensions.TryGetValue(name, out extension))
            {
                extension = new Dictionary<object, object>();
                extensions[name] = extension;
            }

            return (Dictionary<object, object>)extension;
        }

        private Func<IEnumerable, object> ResolveConstructor()
        {
            Func<IEnumerable, object> ctor;

            if (!ConstructorsCache.TryGetValue(Type, out ctor))
            {
                var param = Expression.Parameter(typeof(IEnumerable), "events");

                // ReSharper disable once AssignNullToNotNullAttribute
                var constructorCall = Expression.New(typeof(EventBatch<>).MakeGenericType(Type).GetConstructor(new[] { typeof(IEnumerable) }), param);
                var constructorLambda = Expression.Lambda(constructorCall, param);
                ctor = (Func<IEnumerable, object>)constructorLambda.Compile();
                ConstructorsCache[Type] = ctor;
            }

            return ctor;
        }
    }
}