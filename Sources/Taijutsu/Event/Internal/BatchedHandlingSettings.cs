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

using Stage = Taijutsu.Event.Internal.DeferredHandlingSettings.Stage;

namespace Taijutsu.Event.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BatchedHandlingSettings : AbstractHandlingSettings
    {
        [ThreadStatic]
        private static IDictionary<Type, Func<IEnumerable, object>> constructors;

        private Action<object> batchedAction;

        private List<object> events;

        public BatchedHandlingSettings(Type type, Stage stage, int priority)
            : base(type, priority)
        {
            events = new List<object>();

            batchedAction = ev =>
            {
                var context = InternalEnvironment.DataContextSupervisor.CurrentContext;

                if (context != null)
                {
                    var extension = InitializeExtension(context);

                    if (!extension.ContainsKey(this))
                    {
                        extension[this] = this;
                        EventHandler<FinishedEventArgs> action = null;

                        action = (sender, e) =>
                        {
                            try
                            {
                                // ReSharper disable AccessToModifiedClosure
                                switch (stage)
                                {
                                    case Stage.BeforeCompleted:
                                        context.BeforeCompleted -= action;
                                        break;
                                    case Stage.AfterCompleted:
                                        context.AfterCompleted -= action;
                                        break;
                                    case Stage.Finished:
                                        context.Finished -= action;
                                        break;
                                }

                                // ReSharper restore AccessToModifiedClosure

                                if (events.Count > 0)
                                {
                                    Events.Publish(ResolveConstructor()(events));
                                }
                            }
                            finally
                            {
                                context = null;
                                events = null;
                                batchedAction = null;
                                action = null;
                            }
                        };

                        switch (stage)
                        {
                            case Stage.BeforeCompleted:
                                context.BeforeCompleted += action;
                                break;
                            case Stage.AfterCompleted:
                                context.AfterCompleted += action;
                                break;
                            case Stage.Finished:
                                context.Finished += action;
                                break;
                        }
                    }

                    events.Add(ev);
                }
                else
                {
                    Trace.TraceWarning("Taijutsu: Source of event is not inside of unit of work, event is skipped.");
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

        private static IDictionary<object, BatchedHandlingSettings> InitializeExtension(IDataContext context)
        {
            var extensions = (Dictionary<string, object>)context.Extra.Extensions;

            object extension;
            if (!extensions.TryGetValue("Taijutsu.BatchedHandlerSettings", out extension))
            {
                extension = new Dictionary<object, BatchedHandlingSettings>();
                extensions["Taijutsu.BatchedHandlerSettings"] = extension;
            }

            return (Dictionary<object, BatchedHandlingSettings>)extension;
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