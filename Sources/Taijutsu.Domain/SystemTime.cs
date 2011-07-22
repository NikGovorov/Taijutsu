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
using System.ComponentModel;

namespace Taijutsu.Domain
{
    public static class SystemTime
    {
        private static readonly ITimeController controller = new InternalTimeController();

        public static DateTime Now
        {
            get { return controller.CurrentDateTime; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ITimeController TimeController
        {
            get { return controller; }
        }

        #region Nested type: InternalTimeController

        private class InternalTimeController : ITimeController
        {
            private Func<DateTime> dateFunc = () => DateTime.Now;

            #region ITimeController Members

            public DateTime CurrentDateTime
            {
                get { return dateFunc(); }
            }

            public void Customize(Func<DateTime> func)
            {
                dateFunc = func;
            }

            public void SetDate(DateTime date)
            {
                var whnStd = DateTime.Now;
                Func<DateTime> func = () => date + (DateTime.Now - whnStd);
                dateFunc = func;
            }

            public void SetFrozenDate(DateTime date)
            {
                dateFunc = () => date;
            }

            public void Reset()
            {
                dateFunc = () => DateTime.Now;
            }

            #endregion
        }

        #endregion
    }

    public interface ITimeController
    {
        DateTime CurrentDateTime { get; }
        void Customize(Func<DateTime> dateFunc);
        void SetDate(DateTime date);
        void SetFrozenDate(DateTime date);
        void Reset();
    }
}