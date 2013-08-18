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
using System.Diagnostics;

using Taijutsu.Event;

namespace Taijutsu.Sample
{

    interface IStartedHandler : IEventHandler<SystemStarted>
    {
         
    }

    public class SystemStartedHandler : IStartedHandler
    {
        public SystemStartedHandler()
        {
            Trace.WriteLine("SystemStaredHandler.Ctor");
        }

        public void Handle(SystemStarted subject)
        {
            Trace.WriteLine("SystemStaredHandler[SystemStarted].Handle");
        }
    }

    public class SystemStartedHandlerTwo : ISpecEventHandler<SystemStarted>, ISpecEventHandler<SubsystemStarted>, IEventHandler<SystemStopped>
    {
        public SystemStartedHandlerTwo()
        {
            Trace.WriteLine("SystemStartedHandlerTwo.Ctor");
        }

        public void Handle(SystemStarted subject)
        {
            Trace.WriteLine("SystemStartedHandlerTwo[SystemStarted].Handle");
        }

        public bool IsSatisfiedBy(SystemStarted candidate)
        {
            Trace.WriteLine("SystemStartedHandlerTwo[SystemStarted].IsSatisfiedBy");
            return true;
        }

        public void Handle(SystemStopped subject)
        {
            Trace.WriteLine("SystemStartedHandlerTwo[SystemStopped].Handle");
        }


        void IHandler<SubsystemStarted>.Handle(SubsystemStarted subject)
        {
          Trace.WriteLine("SystemStartedHandlerTwo[SubsystemStarted].HandleFromInterface");
        }

        public void Handle(SubsystemStarted subject)
        {
            Trace.WriteLine("SystemStartedHandlerTwo[SubsystemStarted].Handle");
        }

        bool ISpecHandler<SubsystemStarted>.IsSatisfiedBy(SubsystemStarted candidate)
        {
            Trace.WriteLine("SystemStartedHandlerTwo[SubsystemStarted].IsSatisfiedByFromInterface");
            return true;
        }

        public bool IsSatisfiedBy(SubsystemStarted candidate)
        {
            Trace.WriteLine("SystemStartedHandlerTwo[SubsystemStarted].IsSatisfiedBy");
            return true;
        }

    }
}