// Copyright © Microsoft Open Technologies, Inc.
// All Rights Reserved
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0 

// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 
// See the Apache 2 License for the specific language governing permissions and limitations under the License. 

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    /// <summary>
    /// Double byte charactors to replace ipmi escape charactors.
    /// See IPMI 2.0: 14.4.1 - Basic Mode Packet Framing
    /// See IPMI 2.0: 14.4.2 - Data Byte Escaping 
    /// </summary>
    internal class EscapeCharactor
    {
        internal byte Frame;
        internal byte[] Replace;

        internal EscapeCharactor(byte frame, byte[] replace)
        {
            this.Frame = frame;
            this.Replace = replace;
        }
    }  

}
