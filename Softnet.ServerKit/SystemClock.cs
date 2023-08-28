/*
*   Copyright 2023 Robert Koifman
*   
*   Licensed under the Apache License, Version 2.0 (the "License");
*   you may not use this file except in compliance with the License.
*   You may obtain a copy of the License at
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
*   Unless required by applicable law or agreed to in writing, software
*   distributed under the License is distributed on an "AS IS" BASIS,
*   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*   See the License for the specific language governing permissions and
*   limitations under the License.
*/

using System;
using System.Diagnostics;

namespace Softnet.ServerKit
{
    public static class SystemClock
    {
        static Stopwatch _Stopwatch;
        static long TicksPerMinute;
        static long TicksPerMillisecond;
        static long TicksPerMicrosecond;

        static SystemClock()
        {
            _Stopwatch = new Stopwatch();
            _Stopwatch.Start();

            TicksPerMinute = Stopwatch.Frequency * 60;
            TicksPerMillisecond = Stopwatch.Frequency / 1000;
            TicksPerMicrosecond = Stopwatch.Frequency / 1000000;
        }

        public static int Minutes
        {
            get { return (int)(_Stopwatch.ElapsedTicks / TicksPerMinute); }
        }

        public static long Seconds
        {
            get { return _Stopwatch.ElapsedTicks / Stopwatch.Frequency; }
        }

        public static long Milliseconds
        {
            get { return _Stopwatch.ElapsedTicks / TicksPerMillisecond; }
        }

        public static long Microseconds
        {
            get { return _Stopwatch.ElapsedTicks / TicksPerMicrosecond; }
        }
    }
}
