#region License

// Copyright 2009-2012 Taijutsu.
//    
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
//  this file except in compliance with the License. You may obtain a copy of the 
//  License at 
//   
//  http://www.apache.org/licenses/LICENSE-2.0 
//   
//  Unless required by applicable law or agreed to in writing, software distributed 
//  under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
//  CONDITIONS OF ANY KIND, either express or implied. See the License for the 
//  specific language governing permissions and limitations under the License.

#endregion

using System;

namespace Taijutsu
{
    public static class SeqGuid
    {
        private static readonly DateTime baseDate = new DateTime(1900, 1, 1);
        private static readonly long baseTicks = baseDate.Ticks;

        public static Guid NewGuid()
        {
            var guidArray = Guid.NewGuid().ToByteArray();
            var now = SystemTime.Now;
            var days = new TimeSpan(now.Ticks - baseTicks);
            var msecs = now.TimeOfDay;

            var daysArray = BitConverter.GetBytes(days.Days);
            var msecsArray = BitConverter.GetBytes((long) (msecs.TotalMilliseconds/3.333333));

            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }

        public static DateTime ToDateTime(Guid combGuid)
        {
            var daysArray = new byte[4];
            var msecsArray = new byte[4];
            var guidArray = combGuid.ToByteArray();

            Array.Copy(guidArray, guidArray.Length - 6, daysArray, 2, 2);
            Array.Copy(guidArray, guidArray.Length - 4, msecsArray, 0, 4);

            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            var days = BitConverter.ToInt32(daysArray, 0);
            var msecs = BitConverter.ToInt32(msecsArray, 0);

            var date = baseDate.AddDays(days);
            date = date.AddMilliseconds(msecs*3.333333);

            return date;
        }
    }
}