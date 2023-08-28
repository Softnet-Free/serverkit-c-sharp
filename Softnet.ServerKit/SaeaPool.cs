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
using System.Collections.Generic;
using System.Net.Sockets;

namespace Softnet.ServerKit
{
    public class SaeaPool
    {
        int m_BufferSize;
        byte[] m_SolidBuffer;
        int m_PoolSize;
        Queue<SocketAsyncEventArgs> m_Pool;
        object mutex = new object();

        public int BufferSize
        {
            get { return m_BufferSize; }
        }

        public void Init(int poolSize, int bufferSize)
        {
            m_PoolSize = poolSize;
            m_BufferSize = bufferSize;
            m_Pool = new Queue<SocketAsyncEventArgs>();
            m_SolidBuffer = new byte[poolSize * m_BufferSize];

            for (int i = 0; i < poolSize; i++)
            {
                SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
                int offset = i * m_BufferSize;
                saea.SetBuffer(m_SolidBuffer, offset, m_BufferSize);
                m_Pool.Enqueue(saea);
            }
        }

        bool m_Closed = false;
        public void Close()
        {
            lock (mutex)
            {
                m_Closed = true;
                foreach (SocketAsyncEventArgs saea in m_Pool)
                {
                    saea.Dispose();
                }
                m_Pool.Clear();
            }
        }        

        public SocketAsyncEventArgs Get()
        {
            lock (mutex)
            {
                if (m_Pool.Count > 0)
                    return m_Pool.Dequeue();
                return null;
            }
        }

        public void Add(SocketAsyncEventArgs saea)
        {
            lock (mutex)
            {
                if (m_Closed == false)
                {
                    saea.SetBuffer(saea.Offset, this.m_BufferSize);
                    m_Pool.Enqueue(saea);
                }
                else
                    saea.Dispose();
            }
        }
    }
}
