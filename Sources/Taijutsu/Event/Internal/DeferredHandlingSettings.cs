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
using System.ComponentModel;
using System.Diagnostics;

using Taijutsu.Annotation;
using Taijutsu.Data.Internal;

namespace Taijutsu.Event.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DeferredHandlingSettings : AbstractHandlingSettings
    {
        private readonly Action<object> deferredAction;

        public DeferredHandlingSettings([NotNull] IEventHandlingSettings origin, DelayUntil delayUntil)
            : base(origin.Type, origin.Priority)
        {
            deferredAction = ev =>
            {
                if (ev == null)
                {
                    throw new ArgumentNullException("ev");
                }

                if (Type.IsInstanceOfType(ev))
                {
                    var context = DataEnvironment.DataContextSupervisor.CurrentContext;

                    if (context != null)
                    {
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

                                if (e.Completed)
                                {
                                    // ReSharper restore AccessToModifiedClosure
                                    origin.Action(ev);
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
                    else
                    {
                        Trace.TraceWarning("Taijutsu: Source of event is outside of unit of work, event is skipped.");
                    }
                }
            };
        }

        public override Action<object> Action
        {
            get { return deferredAction; }
        }
    }
}