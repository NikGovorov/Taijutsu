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

using System.Diagnostics;

using Taijutsu.Data;
using Taijutsu.Data.Internal;
using Taijutsu.Event;
using Taijutsu.Event.Internal;
using Taijutsu.Test.Data;

namespace Taijutsu.Sample
{
    public class BatchFixture
    {
        public virtual void Run()
        {
            InternalEnvironment.RegisterDataSource(new DataSource(il => new NullOrmSession()));

            //Events.Subscribe(new BatchedHandlingSettings(typeof(SystemStarted), 1));

            //Events.Subscribe(new PostponedHandlerSettings(new TypedHandlerSettings<SystemStarted>(()=>new SpecEventHandler<SystemStarted>((ev) => Trace.WriteLine("Handled")))));

            Events<IEventBatch<SystemStarted>>.Subscribe(
                batch =>
                {
                    int count = batch.Events.Length;
                    var stop = true;
                    Trace.WriteLine("Handled");
                });

            using (var uow = new UnitOfWork())
            {

                Events.Publish(new SystemStarted());
                Trace.WriteLine("Raised");
                Events.Publish(new SystemStarted());
                Trace.WriteLine("Raised");
                Events.Publish(new SystemStarted());
                Trace.WriteLine("Raised");
                uow.Complete();
            }
        }
    }
}