// Copyright 2009-2011 Taijutsu.
//   
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//  
//      http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System;

namespace Taijutsu.Domain.Event
{
    public static class EventAggregatorFactory
    {
        private const string LocalEventAggregatorName = "LocalEventAggregator";

        private static readonly SystemEventAggregator systemEventAggregator = new SystemEventAggregator();

        public static Func<SystemEventAggregator> SystemEventAggregatorResolvingFunction = () => systemEventAggregator;

        public static Func<LocalEventAggregator> LocalEventAggregatorResolvingFunction =
            () =>
                {
                    var localAggregator = LogicalContext.FindData<LocalEventAggregator>(LocalEventAggregatorName);

                    if (localAggregator == null)
                    {
                        var
                            realLocalAggregator = new LocalEventAggregator
                                (() => LogicalContext.ReleaseData(LocalEventAggregatorName));

                        LogicalContext.SetData(LocalEventAggregatorName, realLocalAggregator);
                        return realLocalAggregator;
                    }
                    return localAggregator;
                };

        public static Func<Action<object>> RaiserResolvingFunction = () => ev =>
                                                                               {
                                                                                   object localAggregator =
                                                                                       LogicalContext.FindData
                                                                                           <LocalEventAggregator>(
                                                                                               LocalEventAggregatorName);

                                                                                   if (localAggregator != null)
                                                                                   {
                                                                                       ((LocalEventAggregator)
                                                                                        localAggregator).Raise(ev);
                                                                                   }
                                                                                   ResolveSystem().Raise(ev);
                                                                               };

        public static SystemEventAggregator ResolveSystem()
        {
            return SystemEventAggregatorResolvingFunction();
        }

        public static LocalEventAggregator ResolveLocal()
        {
            return LocalEventAggregatorResolvingFunction();
        }

        public static Action<object> ResolveRaiser()
        {
            return RaiserResolvingFunction();
        }
    }
}