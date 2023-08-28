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
using System.Collections.Generic;

namespace Softnet.ServerKit
{
    public static class TaskScheduler
    {
        static Queue<ScheduledTask>[] TimeGrid;
        static Thread ExecThread;

        public const int MAX_WAIT_SECONDS = 600;
        public const int SECONDS_600 = 600;
        const int TIME_ARRAY_SIZE = MAX_WAIT_SECONDS + 2;

        static int current_index;

        static TaskScheduler()
        {
            current_index = 0;
            TimeGrid = new Queue<ScheduledTask>[TIME_ARRAY_SIZE];
        }

        public static void Start()
        {
            for (int i = 0; i < TIME_ARRAY_SIZE; i++)
                TimeGrid[i] = new Queue<ScheduledTask>();

            ExecThread = new Thread(new ThreadStart(Execute));
            ExecThread.Start();
        }

        static bool running = true;

        public static void Close()
        {
            running = false;
        }

        static void Execute()
        {
            while (running)
            {
                Queue<ScheduledTask> Tasks = TimeGrid[current_index];
                ThreadPool.QueueUserWorkItem(delegate { ProcessTasks(Tasks); });

                Thread.Sleep(1000);

                current_index++;

                if (current_index == TIME_ARRAY_SIZE)
                    current_index = 0;
            }
        }

        static void ProcessTasks(Queue<ScheduledTask> Tasks)
        {
            lock (Tasks)
            {
                while (Tasks.Count > 0)
                {
                    ScheduledTask Task = Tasks.Dequeue();

                    if (Task.Complete())
                    {
                        ThreadPool.QueueUserWorkItem(Task.Callback, Task.State);
                    }
                }
            }        
        }

        public static void Add(ScheduledTask task, int waitSeconds)
        {
            int Index = current_index + waitSeconds + 1;
            
            Index = Index % TIME_ARRAY_SIZE;

            lock (TimeGrid[Index])
            {
                TimeGrid[Index].Enqueue(task);
            }
        }
    }
}
