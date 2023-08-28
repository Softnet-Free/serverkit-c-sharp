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
using System.Threading;

namespace Softnet.ServerKit
{
    public static class Monitor
    {
        class ControlItem
        {
            public Monitorable Entity = null;
            public ControlItem NextItem = null;
            public ControlItem PrevItem = null;
        }

        static int PeriodSeconds;
        static object mutex = new object();
        static ControlItem FirstItem;
        static ControlItem LastItem;

        public static void Start(int periodSeconds)
        {
            PeriodSeconds = periodSeconds;

            FirstItem = new ControlItem(); 
            LastItem = new ControlItem();

            FirstItem.NextItem = LastItem;
            LastItem.PrevItem = FirstItem;

            ScheduledTask task = new ScheduledTask(ValidateElements, null);
            TaskScheduler.Add(task, PeriodSeconds);
        }

        public static void Add(Monitorable entity)
        {
            ControlItem newItem = new ControlItem();
            lock (mutex)
            {
                newItem.PrevItem = LastItem;
                LastItem.NextItem = newItem;
                LastItem.Entity = entity;
                LastItem = newItem;
            }
        }

        static void ValidateElements(object noData)
        {
            long currentSeconds = SystemClock.Seconds;
            ControlItem currentItem = FirstItem.NextItem;

            while (currentItem.Entity != null)
            {
                if (currentItem.Entity.IsAlive(currentSeconds))
                {
                    currentItem = currentItem.NextItem;
                }
                else
                {
                    currentItem.PrevItem.NextItem = currentItem.NextItem;
                    currentItem.NextItem.PrevItem = currentItem.PrevItem;

                    currentItem = currentItem.NextItem;
                }
            }

            ScheduledTask task = new ScheduledTask(ValidateElements, null);
            TaskScheduler.Add(task, PeriodSeconds);
        }
    }
}
