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
using System.ComponentModel;

namespace Taijutsu
{
    public static class SystemTime
    {
        private static ITimeController controller = new TimeController();

        public static DateTime Now
        {
            get { return controller.Now; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ITimeController TimeController
        {
            get { return controller; }

            set { controller = value ?? controller; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Reset()
        {
            controller = new TimeController();
        }
    }
}