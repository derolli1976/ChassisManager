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
    /// Represents the IPMI 'Chassis Control' application response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Chassis, IpmiCommand.ChassisControl)]
    internal class ChassisControlResponse : IpmiResponse
    {


        //private byte completionCode;
        // <summary>
        // Gets and sets the Current power state.
        // </summary>
        // <value>Current power state.</value>
        //[IpmiMessageData(0)]
        //public byte CompletionCodeTest
        //{
        //    get { return this.completionCode; }
        //    set { this.completionCode = value; }
        //}
    }
}
