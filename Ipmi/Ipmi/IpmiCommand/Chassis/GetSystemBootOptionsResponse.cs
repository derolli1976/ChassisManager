/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   							                        *
*   							                        *
********************************************************/

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
