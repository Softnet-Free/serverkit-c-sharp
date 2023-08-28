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
    public static class SHA1Hash
    {
        static SHA1CryptoServiceProvider s_Sha1CSP;

        static SHA1Hash()
        {
            s_Sha1CSP = new SHA1CryptoServiceProvider();
        }

        static object mutex = new object();

        public static byte[] Compute(byte[] buffer)
        {
            lock(mutex)
            {
                return s_Sha1CSP.ComputeHash(buffer);
            }
        }
    }
}
