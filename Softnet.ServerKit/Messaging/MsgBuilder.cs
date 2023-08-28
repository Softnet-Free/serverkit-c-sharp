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

using Softnet.Asn;

namespace Softnet.ServerKit
{
    public class MsgBuilder : SoftnetMessage
    {
        public byte[] buffer { get { return m_buffer; } }
        public int length { get { return buffer.Length - offset; } }
        public int offset
        { 
            get { return m_offset; }
            set { m_offset = value; }
        }
        
        byte[] m_buffer;
        int m_offset;

        private MsgBuilder(byte[] buffer, int offset)
        {
            m_buffer = buffer;
            m_offset = offset;
        }

        public static SoftnetMessage Create(byte[] message)
        {
            byte[] buffer = new byte[message.Length + 5];
            Buffer.BlockCopy(message, 0, buffer, 5, message.Length);
            int offset = EncodeLength(buffer, 5);
            return new MsgBuilder(buffer, offset);
        }

        public static SoftnetMessage Create(byte componentId, byte messageType, ASNEncoder asnEncoder)
        {
            AsnEncoding encoding = asnEncoder.GetHeadedEncoding(7);
            byte[] buffer = encoding.buffer;
            int offset = encoding.offset;
            buffer[offset - 2] = componentId;
            buffer[offset - 1] = messageType;
            offset = EncodeLength(buffer, offset - 2);
            return new MsgBuilder(buffer, offset);
        }

        public static SoftnetMessage Create(byte messageType, ASNEncoder asnEncoder)
        {
            AsnEncoding encoding = asnEncoder.GetHeadedEncoding(7);
            byte[] buffer = encoding.buffer;
            int offset = encoding.offset;
            buffer[offset - 1] = messageType;
            offset = EncodeLength(buffer, offset - 1);
            return new MsgBuilder(buffer, offset);
        }

        public static SoftnetMessage Create(byte componentId, byte messageType)
        {
            byte[] buffer = new byte[3];
            buffer[0] = 2;
            buffer[1] = componentId;
            buffer[2] = messageType;
            return new MsgBuilder(buffer, 0);
        }

        public static SoftnetMessage Create(byte messageType)
        {
            byte[] buffer = new byte[2];
            buffer[0] = 1;
            buffer[1] = messageType;
            return new MsgBuilder(buffer, 0);
        }

        public static SoftnetMessage CreateErrorMessage(byte componentId, byte messageType, int errorCode)
        {
            byte[] buffer = new byte[5];
            buffer[0] = 4;
            buffer[1] = componentId;
            buffer[2] = messageType;
            buffer[3] = (byte)(errorCode / 256);
            buffer[4] = (byte)(errorCode % 256);

            return new MsgBuilder(buffer, 0);
        }

        public static SoftnetMessage CreateErrorMessage(byte messageType, int errorCode)
        {
            byte[] buffer = new byte[4];
            buffer[0] = 3;
            buffer[1] = messageType;
            buffer[2] = (byte)(errorCode / 256);
            buffer[3] = (byte)(errorCode % 256);

            return new MsgBuilder(buffer, 0);
        }

        private static int EncodeLength(byte[] buffer, int offset)
        {
            int dataSize = buffer.Length - offset;
            if (dataSize <= 127)
            {
                offset -= 1;
                buffer[offset] = (byte)dataSize;
                return offset;
            }

            if (dataSize <= 255)
            {
                buffer[offset - 2] = (byte)0x81;
                buffer[offset - 1] = (byte)dataSize;
                offset -= 2;
                return offset;
            }

            if (dataSize <= 0x0000ffff)
            {
                buffer[offset - 3] = (byte)0x82;
                buffer[offset - 2] = (byte)((dataSize & 0x0000ff00) >> 8);
                buffer[offset - 1] = (byte)(dataSize & 0x000000ff);
                offset -= 3;
                return offset;
            }

            if (dataSize <= 0x00ffffff)
            {
                buffer[offset - 4] = (byte)0x83;
                buffer[offset - 3] = (byte)((dataSize & 0x00ff0000) >> 16);
                buffer[offset - 2] = (byte)((dataSize & 0x0000ff00) >> 8);
                buffer[offset - 1] = (byte)(dataSize & 0x000000ff);
                offset -= 4;
                return offset;
            }

            if (dataSize <= 0x7fffffff)
            {
                buffer[offset - 5] = (byte)0x84;
                buffer[offset - 4] = (byte)((dataSize & 0xff000000) >> 24);
                buffer[offset - 3] = (byte)((dataSize & 0x00ff0000) >> 16);
                buffer[offset - 2] = (byte)((dataSize & 0x0000ff00) >> 8);
                buffer[offset - 1] = (byte)(dataSize & 0x000000ff);
                offset -= 5;
                return offset;
            }

            throw new ArgumentException("The size of the softnet message exceeds 2GB.");
        }
    }
}
 