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
using System.Text;
using System.Security.Cryptography;

namespace Softnet.ServerKit
{
    public class SaltedPassword
    {
        static SHA1CryptoServiceProvider s_Sha1CSP;

        static SaltedPassword()
        {
            s_Sha1CSP = new SHA1CryptoServiceProvider();
        }

        public static byte[] Compute(byte[] salt, string password)
        {
            byte[] password_bytes = Encoding.Unicode.GetBytes(password);

            byte[] salt_and_password_bytes = new byte[password_bytes.Length + salt.Length];
            Buffer.BlockCopy(salt, 0, salt_and_password_bytes, 0, salt.Length);
            Buffer.BlockCopy(password_bytes, 0, salt_and_password_bytes, salt.Length, password_bytes.Length);

            return s_Sha1CSP.ComputeHash(salt_and_password_bytes);
        }
    }
}
