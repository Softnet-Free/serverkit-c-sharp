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
using System.Security.Cryptography;

namespace Softnet.ServerKit
{
    public static class PasswordHash
    {
        static SHA1CryptoServiceProvider s_Sha1CSP;
        static object mutex = new object();

        static PasswordHash()
        {
            s_Sha1CSP = new SHA1CryptoServiceProvider();
        }

        public static byte[] Compute(byte[] key1, byte[] key2, byte[] password)
        {
            byte[] buffer = new byte[key1.Length + key2.Length + password.Length];
            Buffer.BlockCopy(key1, 0, buffer, 0, key1.Length);
            Buffer.BlockCopy(key2, 0, buffer, key1.Length, key2.Length);
            Buffer.BlockCopy(password, 0, buffer, key1.Length + key2.Length, password.Length);

            lock(mutex)
            {
                return s_Sha1CSP.ComputeHash(buffer);
            }
        }
    }
}
