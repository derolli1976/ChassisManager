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

    [IpmiMessageResponse(IpmiFunctions.Chassis, IpmiCommand.GetSystemBootOptions)]
    internal class GetSystemBootOptionsResponse : IpmiResponse
    {
        private byte parameterVersion;
        private byte parameterValid;
        private byte[] parameterData;

        [IpmiMessageData(0)]
        public byte ParameterVersion
        {
            get { return this.parameterVersion; }
            set { this.parameterVersion = value; }
        }

        [IpmiMessageData(1)]
        public byte ParameterValid
        {
            get { return this.parameterValid; }
            set { this.parameterValid = value; }
        }

        [IpmiMessageData(2)]
        public byte[] ParameterData
        {
            get { return this.parameterData; }
            set { this.parameterData = value; }
        }
    }
}
