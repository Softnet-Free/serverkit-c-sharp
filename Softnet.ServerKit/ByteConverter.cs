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

namespace Softnet.ServerKit
{
    public static class ByteConverter
    {
        public static Guid ToGuid(byte[] buffer, int offset)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] guid_host_bytes = new byte[16];

                guid_host_bytes[3] = buffer[offset];
                guid_host_bytes[2] = buffer[offset + 1];
                guid_host_bytes[1] = buffer[offset + 2];
                guid_host_bytes[0] = buffer[offset + 3];

                guid_host_bytes[5] = buffer[offset + 4];
                guid_host_bytes[4] = buffer[offset + 5];

                guid_host_bytes[7] = buffer[offset + 6];
                guid_host_bytes[6] = buffer[offset + 7];

                guid_host_bytes[8] = buffer[offset + 8];
                guid_host_bytes[9] = buffer[offset + 9];
                guid_host_bytes[10] = buffer[offset + 10];
                guid_host_bytes[11] = buffer[offset + 11];
                guid_host_bytes[12] = buffer[offset + 12];
                guid_host_bytes[13] = buffer[offset + 13];
                guid_host_bytes[14] = buffer[offset + 14];
                guid_host_bytes[15] = buffer[offset + 15];

                return new Guid(guid_host_bytes);
            }
            else
            {
                byte[] guid_bytes = new byte[16];
                Buffer.BlockCopy(buffer, offset, guid_bytes, 0, 16);
                return new Guid(guid_bytes);
            }
        }

        public static Guid ToGuid(byte[] buffer)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] guid_host_bytes = new byte[16];

                guid_host_bytes[3] = buffer[0];
                guid_host_bytes[2] = buffer[1];
                guid_host_bytes[1] = buffer[2];
                guid_host_bytes[0] = buffer[3];

                guid_host_bytes[5] = buffer[4];
                guid_host_bytes[4] = buffer[5];

                guid_host_bytes[7] = buffer[6];
                guid_host_bytes[6] = buffer[7];

                guid_host_bytes[8] = buffer[8];
                guid_host_bytes[9] = buffer[9];
                guid_host_bytes[10] = buffer[10];
                guid_host_bytes[11] = buffer[11];
                guid_host_bytes[12] = buffer[12];
                guid_host_bytes[13] = buffer[13];
                guid_host_bytes[14] = buffer[14];
                guid_host_bytes[15] = buffer[15];

                return new Guid(guid_host_bytes);
            }
            else
            {
                 return new Guid(buffer);
            }
        }

        public static byte[] GetBytes(Guid value)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] guid_bytes = value.ToByteArray();
                byte[] guid_network_bytes = new byte[16];

                guid_network_bytes[3] = guid_bytes[0];
                guid_network_bytes[2] = guid_bytes[1];
                guid_network_bytes[1] = guid_bytes[2];
                guid_network_bytes[0] = guid_bytes[3];

                guid_network_bytes[5] = guid_bytes[4];
                guid_network_bytes[4] = guid_bytes[5];

                guid_network_bytes[7] = guid_bytes[6];
                guid_network_bytes[6] = guid_bytes[7];

                guid_network_bytes[8] = guid_bytes[8];
                guid_network_bytes[9] = guid_bytes[9];
                guid_network_bytes[10] = guid_bytes[10];
                guid_network_bytes[11] = guid_bytes[11];
                guid_network_bytes[12] = guid_bytes[12];
                guid_network_bytes[13] = guid_bytes[13];
                guid_network_bytes[14] = guid_bytes[14];
                guid_network_bytes[15] = guid_bytes[15];

                return guid_network_bytes;
            }
            else
            {
                return value.ToByteArray();
            }
        }

        public static UInt16 ToUInt16(byte[] buffer, int offset)
        {
            int a = buffer[offset];
            int b = buffer[offset + 1];
            return (UInt16)((a << 8) | b);
        }

        public static short ToInt16(byte[] buffer, int offset)
        {
            int a = buffer[offset];
            int b = buffer[offset + 1];
            if (a <= 127)
                return (short)((a << 8) | b);
            else
                return (short)(-65536 | (a << 8) | b);                
        }

        public static int ToInt32(byte[] buffer, int offset)
        {
            int a = buffer[offset];
            int b = buffer[offset + 1];
            int c = buffer[offset + 2];
            int d = buffer[offset + 3];
            return (a << 24) | (b << 16) | (c << 8) | d;
        }

        public static UInt32 ToUInt32(byte[] buffer, int offset)
        {
            uint a = buffer[offset];
            uint b = buffer[offset + 1];
            uint c = buffer[offset + 2];
            uint d = buffer[offset + 3];
            return (a << 24) | (b << 16) | (c << 8) | d;
        }

        public static long ToInt64(byte[] buffer, int offset)
        {
            long a = buffer[offset];
            long b = buffer[offset + 1];
            long c = buffer[offset + 2];
            long d = buffer[offset + 3];
            long e = buffer[offset + 4];
            long f = buffer[offset + 5];
            long g = buffer[offset + 6];
            long h = buffer[offset + 7];
            return (a << 56) | (b << 48) | (c << 40) | (d << 32) | (e << 24) | (f << 16) | (g << 8) | h;
        }

        public static byte[] GetBytes(UInt16 value)
        {
            byte[] buffer = new byte[2];
            buffer[0] = (byte)((value >> 8) & 0xFF);
            buffer[1] = (byte)(value & 0xFF); 
            return buffer;
        }

        public static byte[] GetBytes(short value)
        {
            byte[] buffer = new byte[2];
            buffer[0] = (byte)((value >> 8) & 0xFF);
            buffer[1] = (byte)(value & 0xFF);
            return buffer;
        }

        public static byte[] GetBytes(int value)
        {
            byte[] buffer = new byte[4];
            buffer[0] = (byte)((value >> 24) & 0xFF);
            buffer[1] = (byte)((value >> 16) & 0xFF);
            buffer[2] = (byte)((value >> 8) & 0xFF);
            buffer[3] = (byte)(value & 0xFF);
            return buffer;
        }

        public static byte[] GetBytes(UInt32 value)
        {
            byte[] buffer = new byte[4];
            buffer[0] = (byte)((value >> 24) & 0xFF);
            buffer[1] = (byte)((value >> 16) & 0xFF);
            buffer[2] = (byte)((value >> 8) & 0xFF);
            buffer[3] = (byte)(value & 0xFF);
            return buffer;
        }

        public static byte[] GetBytes(long value)
        {
            byte[] buffer = new byte[8];
            buffer[0] = (byte)((value >> 56) & 0xFF);
            buffer[1] = (byte)((value >> 48) & 0xFF);
            buffer[2] = (byte)((value >> 40) & 0xFF);
            buffer[3] = (byte)((value >> 32) & 0xFF);
            buffer[4] = (byte)((value >> 24) & 0xFF);
            buffer[5] = (byte)((value >> 16) & 0xFF);
            buffer[6] = (byte)((value >> 8) & 0xFF);
            buffer[7] = (byte)(value & 0xFF);
            return buffer;
        }

        public static int ToInt32FromInt16(byte[] buffer, int offset)
        {
            int b1 = buffer[offset];
            int b0 = buffer[offset + 1];
            if (b1 <= 127)
                return (b1 << 8) | b0;
            else
                return -65536 | (b1 << 8) | b0;
        }

        public static int ToInt32FromUInt16(byte[] buffer, int offset)
        {
            int b1 = buffer[offset];
            int b0 = buffer[offset + 1];            
            return (b1 << 8) | b0;
        }

        public static void WriteAsInt16(int value, byte[] buffer, int offset)
        {
            if (-32768 <= value && value <= 32767)
            {
                buffer[offset] = (byte)((value >> 8) & 0xFF);
                buffer[offset + 1] = (byte)(value & 0xFF);
                return;
            }

            throw new ArgumentException("The value is out of the range [-32768, 32767].");
        }

        public static void WriteAsUInt16(int value, byte[] buffer, int offset)
        {
            if (0 <= value && value <= 65535)
            {
                buffer[offset] = (byte)((value >> 8) & 0xFF);
                buffer[offset + 1] = (byte)(value & 0xFF);
                return;
            }

            throw new ArgumentException("The value is out of the range [0, 65535].");
        }
    }
}
