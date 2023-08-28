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
    public class Triple<T1, T2, T3>
    {
        public Triple() { }

        public Triple(T1 first, T2 second, T3 third)
        {
            this.First = first;
            this.Second = second;
            this.Third = third;
        }

        public T1 First { get; set; }
        public T2 Second { get; set; }
        public T3 Third { get; set; }
    }
}
