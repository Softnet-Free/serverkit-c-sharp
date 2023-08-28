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
    public class ScheduledTask
    {
        public WaitCallback Callback;
        public object State;

        STContext m_Context;

        public ScheduledTask(WaitCallback callback, object state)
        {
            Callback = callback;
            State = state;
            m_Context = null;
        }

        public ScheduledTask(WaitCallback callback, STContext context, object state)
        {
            Callback = callback;
            State = state;
            m_Context = context;
        }

        int m_Completed = 0;

        public bool Cancel()
        {
            if (m_Context != null && m_Context.Completed)
                return false;

            if (Interlocked.CompareExchange(ref m_Completed, 1, 0) == 0)
                return true;

            return true;
        }

        public bool Complete()
        {
            if (m_Context != null && m_Context.Completed)
                return false;

            if (Interlocked.CompareExchange(ref m_Completed, 1, 0) == 0)
                return true;

            return false;
        }
    }
}
