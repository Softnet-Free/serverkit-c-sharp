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
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Softnet.ServerKit
{
    public class MsgSocket
    {
        Socket m_Socket;
        SaeaPool m_SaeaPool;
        SocketAsyncEventArgs m_Saea;

        public int MinLength = 2;
        public int MaxLength = 127;

        public Action<byte[]> MessageReceivedHandler;
        public Action InputCompletedHandler;
        public Action NetworkErrorHandler;
        public Action FormatErrorHandler;

        public MsgSocket(Socket socket, SocketAsyncEventArgs saea, SaeaPool saeaPool)
        {
            m_Socket = socket;
            m_Saea = saea;
            m_SaeaPool = saeaPool;
            m_OutputMessageQueue = new Queue<SoftnetMessage>();
        }

        public AddressFamily GetAddressFamily()
        {
            return m_Socket.AddressFamily;
        }

        public void Start()
        {
            try
            {
                m_Saea.SetBuffer(m_Saea.Offset, m_SaeaPool.BufferSize);
                m_Saea.Completed += OnInputCompleted;

                if (m_Socket.ReceiveAsync(m_Saea) == false)
                {
                    OnInputCompleted(null, m_Saea);
                }
            }
            catch (SocketException)
            {
                Release();
                NetworkErrorHandler();
            }
        }

        void OnInputCompleted(object noData, SocketAsyncEventArgs saea)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                if (saea.SocketError != SocketError.Success)
                {
                    if (saea.SocketError != SocketError.OperationAborted)
                    {
                        m_Socket.Close();
                        NetworkErrorHandler();
                    }
                    DisposeSaea();
                    return;
                }

                if (saea.BytesTransferred == 0)
                {
                    Release();
                    InputCompletedHandler();
                    return;
                }

                ProcessBytes(saea);
            });
        }

        enum ReceivingState
        {
            ORIGIN, LENGTH, PAYLOAD
        }
        ReceivingState m_ReceivingState = ReceivingState.ORIGIN;

        int m_FirstByte;
        byte[] m_Message;
        int m_MessageLength;
        int m_MessageBytesReceived;

        void ProcessBytes(SocketAsyncEventArgs saea)
        {
            try
            {
                byte[] buffer = saea.Buffer;
                int offset = saea.Offset;
                int bytesLeft = saea.BytesTransferred;

                while (true)
                {
                    if (m_ReceivingState == ReceivingState.ORIGIN)
                    {
                        m_FirstByte = buffer[offset];

                        if (m_FirstByte <= 127)
                        {
                            if (m_FirstByte < MinLength)
                                throw new FormatException();

                            m_Message = new byte[m_FirstByte];

                            bytesLeft -= 1;
                            offset += 1;

                            if (bytesLeft == m_FirstByte)
                            {
                                Buffer.BlockCopy(buffer, offset, m_Message, 0, m_FirstByte);

                                MessageReceivedHandler(m_Message);
                                break;
                            }
                            else if (bytesLeft > m_FirstByte)
                            {
                                Buffer.BlockCopy(buffer, offset, m_Message, 0, m_FirstByte);
                                offset += m_FirstByte;
                                bytesLeft -= m_FirstByte;

                                MessageReceivedHandler(m_Message);
                                continue;
                            }
                            else
                            {
                                m_MessageLength = m_FirstByte;
                                m_MessageBytesReceived = bytesLeft;

                                if (bytesLeft > 0)
                                {
                                    Buffer.BlockCopy(buffer, offset, m_Message, 0, bytesLeft);
                                }

                                m_ReceivingState = ReceivingState.PAYLOAD;
                                break;
                            }
                        }
                        else
                        {
                            int lengthBytes = m_FirstByte & 0x7F;

                            if (lengthBytes < 1 || lengthBytes > 4)
                                throw new FormatException();

                            bytesLeft -= 1;
                            offset += 1;

                            if (bytesLeft > lengthBytes)
                            {
                                m_MessageLength = DecodeLength(lengthBytes, buffer, offset);

                                if (m_MessageLength < MinLength || m_MessageLength > MaxLength)
                                    throw new FormatException();

                                m_Message = new byte[m_MessageLength];

                                bytesLeft -= lengthBytes;
                                offset += lengthBytes;

                                if (bytesLeft == m_MessageLength)
                                {
                                    Buffer.BlockCopy(buffer, offset, m_Message, 0, m_MessageLength);

                                    MessageReceivedHandler(m_Message);
                                    break;
                                }
                                else if (bytesLeft > m_MessageLength)
                                {
                                    Buffer.BlockCopy(buffer, offset, m_Message, 0, m_MessageLength);
                                    offset += m_MessageLength;
                                    bytesLeft -= m_MessageLength;

                                    MessageReceivedHandler(m_Message);
                                    continue;
                                }
                                else
                                {
                                    Buffer.BlockCopy(buffer, offset, m_Message, 0, bytesLeft);
                                    m_MessageBytesReceived = bytesLeft;

                                    m_ReceivingState = ReceivingState.PAYLOAD;
                                    break;
                                }
                            }
                            else if (bytesLeft == lengthBytes)
                            {
                                m_MessageLength = DecodeLength(lengthBytes, buffer, offset);

                                if (m_MessageLength < MinLength || m_MessageLength > MaxLength)
                                    throw new FormatException();

                                m_Message = new byte[m_MessageLength];
                                m_MessageBytesReceived = 0;

                                m_ReceivingState = ReceivingState.PAYLOAD;
                                break;
                            }
                            else
                            {
                                m_MessageLength = lengthBytes;
                                m_Message = new byte[lengthBytes];

                                if (bytesLeft > 0)
                                {
                                    Buffer.BlockCopy(buffer, offset, m_Message, 0, bytesLeft);
                                    m_MessageBytesReceived = bytesLeft;
                                }
                                else
                                {
                                    m_MessageBytesReceived = 0;
                                }

                                m_ReceivingState = ReceivingState.LENGTH;
                                break;
                            }
                        }
                    }
                    else if (m_ReceivingState == ReceivingState.PAYLOAD)
                    {
                        int messageBytesLeft = m_MessageLength - m_MessageBytesReceived;

                        if (bytesLeft == messageBytesLeft)
                        {
                            Buffer.BlockCopy(buffer, offset, m_Message, m_MessageBytesReceived, messageBytesLeft);

                            MessageReceivedHandler(m_Message);

                            m_ReceivingState = ReceivingState.ORIGIN;
                            break;
                        }
                        else if (bytesLeft > messageBytesLeft)
                        {
                            Buffer.BlockCopy(buffer, offset, m_Message, m_MessageBytesReceived, messageBytesLeft);
                            bytesLeft -= messageBytesLeft;
                            offset += messageBytesLeft;

                            MessageReceivedHandler(m_Message);

                            m_ReceivingState = ReceivingState.ORIGIN;
                            continue;
                        }
                        else // bytesLeft < messageBytesLeft
                        {
                            Buffer.BlockCopy(buffer, offset, m_Message, m_MessageBytesReceived, bytesLeft);
                            m_MessageBytesReceived += bytesLeft;
                            break;
                        }
                    }
                    else // m_ReceivingState = ReceivingState.LENGTH
                    {
                        int lengthBytesLeft = m_MessageLength - m_MessageBytesReceived;

                        if (bytesLeft >= lengthBytesLeft)
                        {
                            Buffer.BlockCopy(buffer, offset, m_Message, m_MessageBytesReceived, lengthBytesLeft);

                            m_MessageLength = DecodeLength(m_MessageLength, m_Message, 0);

                            if (m_MessageLength < MinLength || m_MessageLength > MaxLength)
                                throw new FormatException();

                            m_Message = new byte[m_MessageLength];
                            m_MessageBytesReceived = 0;

                            m_ReceivingState = ReceivingState.PAYLOAD;

                            if (bytesLeft > lengthBytesLeft)
                            {
                                bytesLeft -= lengthBytesLeft;
                                offset += lengthBytesLeft;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            Buffer.BlockCopy(buffer, offset, m_Message, m_MessageBytesReceived, bytesLeft);
                            m_MessageBytesReceived += bytesLeft;

                            break;
                        }
                    }
                }

                saea.SetBuffer(saea.Offset, m_SaeaPool.BufferSize);

                if (m_Socket.ReceiveAsync(saea) == false)
                {
                    OnInputCompleted(null, saea);
                }
            }
            catch (SocketException)
            {
                Release();
                NetworkErrorHandler();
            }
            catch (FormatException)
            {
                Release();
                FormatErrorHandler();
            }
            catch (ObjectDisposedException)
            {
                DisposeSaea();
            }
        }

        int DecodeLength(int lengthBytes, byte[] buffer, int offset)
        {
            if (lengthBytes == 1)
            {
                return buffer[offset];
            }
            else if (lengthBytes == 2)
            {
                return buffer[offset] * 256 + buffer[offset + 1];
            }
            else if (lengthBytes == 3)
            {
                return buffer[offset] * 65536 + buffer[offset + 1] * 256 + buffer[offset + 2];
            }
            else // lengthBytes == 4
            {
                if (buffer[offset] >= 0x80)
                    throw new FormatException();
                return buffer[offset] * 16777216 + buffer[offset + 1] * 65536 + buffer[offset + 2] * 256 + buffer[offset + 3];
            }
        }

        Queue<SoftnetMessage> m_OutputMessageQueue;
        bool m_IsSending = false;        

        public void Send(SoftnetMessage message)
        {
            try
            {
                lock (m_OutputMessageQueue)
                {
                    if (m_IsSending)
                    {
                        m_OutputMessageQueue.Enqueue(message);
                        return;                    
                    }
                    m_IsSending = true;
                }
                m_Socket.BeginSend(message.buffer, message.offset, message.length, SocketFlags.None, Socket_OnSendCompleted, message);
            }
            catch (SocketException)
            {
                m_Socket.Close();
                ThreadPool.QueueUserWorkItem(delegate { NetworkErrorHandler(); });
            }
            catch (ObjectDisposedException) { }
        }

        void Socket_OnSendCompleted(IAsyncResult ar)
        {
            try
            {
                int sentBytes = m_Socket.EndSend(ar);
                SoftnetMessage message = (SoftnetMessage)ar.AsyncState;

                if (sentBytes == message.length)
                {
                    lock (m_OutputMessageQueue)
                    {
                        if (m_OutputMessageQueue.Count == 0)
                        {
                            m_IsSending = false;
                            return;
                        }
                        message = m_OutputMessageQueue.Dequeue();
                    }
                    m_Socket.BeginSend(message.buffer, message.offset, message.length, SocketFlags.None, Socket_OnSendCompleted, message);
                }
                else
                {
                    message.offset += sentBytes;
                    m_Socket.BeginSend(message.buffer, message.offset, message.length, SocketFlags.None, Socket_OnSendCompleted, message);
                }
            }
            catch (SocketException)
            {
                m_Socket.Close();
                NetworkErrorHandler();
            }
            catch (ObjectDisposedException) { }
        }
        
        public void Close()
        {
            m_Socket.Close();
        }

        void Release()
        {
            m_Socket.Close();
            DisposeSaea();
        }

        int saea_mutex = 0;
        void DisposeSaea()
        {
            if (Interlocked.CompareExchange(ref saea_mutex, 1, 0) == 0)
            {
                m_Saea.Completed -= OnInputCompleted;
                m_SaeaPool.Add(m_Saea);
            }
        }
    }
}
