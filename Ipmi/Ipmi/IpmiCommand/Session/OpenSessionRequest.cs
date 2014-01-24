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

    using System;
    using System.Globalization;

    /// <summary>
    /// Represents the RMCP+ 'Open Session' request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.SessionSetup, IpmiCommand.OpenSessionRequest)]
    internal class OpenSessionRequest : IpmiRequest
    {
        /// <summary>
        /// Selected by the remote console to match the open session response message.
        /// </summary>
        private readonly byte messageTag;

        /// <summary>
        /// Maximum privilege level for this session.
        /// </summary>
        private readonly PrivilegeLevel maximumPrivilegeLevel;

        /// <summary>
        /// Remote console session id.
        /// </summary>
        private readonly uint remoteSessionId;

        /// <summary>
        /// Authentication payload type, always 0x00.
        /// </summary>
        private readonly byte authenticationType = 0x00; 

        /// <summary>
        /// Authentication payload length in bytes, always 0x08.
        /// </summary>
        private readonly byte authenticationLength = 0x08;

        /// <summary>
        /// Authentication alogrithm.
        /// [7:6] Reserved
        /// 0x00 = None
        /// 0x01 = RAKP-HMAC-SHA1
        /// 0x02 = RAKP-HMAC-MD5
        /// </summary>
        private byte authenticationAlgorithm;

        /// <summary>
        /// Integrity payload type, always 0x01.
        /// </summary>
        private readonly byte integrityType = 0x01;

        /// <summary>
        /// Integrity payload length in bytes, always 0x08.
        /// </summary>
        private readonly byte integrityLength = 0x08;

        /// <summary>
        /// Integrity alogrithm.
        /// [7:6] Reserved.
        /// 0 = None
        /// 1 = HMAC-SHA1-96
        /// 2 = HMAC-MD5-128
        /// 3 = MD5-128
        /// </summary>
        private byte integrityAlgorithm; 

        /// <summary>
        /// Confidentiality payload type, always 0x02.
        /// </summary>
        private readonly byte confidentialityType = 0x02;

        /// <summary>
        /// Confidentiality payload length in bytes, always 0x08.
        /// </summary>
        private readonly byte confidentialityLength = 0x08;

        /// <summary>
        /// Confidentiality alogrithm.
        /// [7:6] Reserved
        /// 0x00 = None
        /// 0x01 = AES-CBC-128
        /// </summary>
        private byte confidentialityAlgorithm;

        /// <summary>
        /// Initializes a new instance of the OpenSessionRequest class.
        /// </summary>
        /// <param name="messageTag">Remote console message tag</param>
        /// <param name="maximumPrivilegeLevel">Maximum privilege level for this session.</param>
        /// <param name="remoteSessionId">Remote session ID.</param>
        internal OpenSessionRequest(byte messageTag, PrivilegeLevel maximumPrivilegeLevel, uint remoteSessionId, RmcpAuthentication authenticationAlgorithm, RmcpIntegrity integrityAlgorithm, RmcpConfidentiality confidentialityAlgorithm)
        {
            this.messageTag = messageTag;
            this.maximumPrivilegeLevel = maximumPrivilegeLevel;
            this.remoteSessionId = remoteSessionId;
            this.authenticationAlgorithm = Convert.ToByte(authenticationAlgorithm, CultureInfo.InvariantCulture);
            this.integrityAlgorithm = Convert.ToByte(integrityAlgorithm, CultureInfo.InvariantCulture);
            this.confidentialityAlgorithm = Convert.ToByte(confidentialityAlgorithm, CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Gets the remote console message tag.
        /// </summary>
        /// <value>Byte representing the remote console message tag.</value>
        [IpmiMessageData(0)]
        public byte MessageTag
        {
            get { return this.messageTag; }
        }

        /// <summary>
        /// Gets the maximum privilege level for this session.
        /// </summary>
        /// <value>PrivilegeLevel.</value>
        [IpmiMessageData(1)]
        public byte MaximumPrivilegeLevel
        {
            get { return (byte)this.maximumPrivilegeLevel; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(2)]
        public static ushort Reserved1
        {
            get { return 0; }
        }

        /// <summary>
        /// Remote console session id.
        /// </summary>
        [IpmiMessageData(4)]
        public uint RemoteSessionId
        {
            get { return this.remoteSessionId; }
        }

        /// <summary>
        /// Authentication payload type, always 0x00.
        /// </summary>
        [IpmiMessageData(8)]
        public byte AuthenticationType
        {
            get { return this.authenticationType; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(9)]
        public static byte AuthenticationReserved1
        {
            get { return 0; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(10)]
        public static byte AuthenticationReserved2
        {
            get { return 0; }
        }

        /// <summary>
        /// Authentication payload length in bytes.
        /// </summary>
        /// <value>Always 0x08.</value>
        [IpmiMessageData(11)]
        public byte AuthenticationLength
        {
            get { return this.authenticationLength; }
        }

        /// <summary>
        /// Authentication alogrithm.
        /// </summary>
        [IpmiMessageData(12)]
        public byte AuthenticationAlgorithm
        {
            get { return this.authenticationAlgorithm; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(13)]
        public static byte AuthenticationReserved3
        {
            get { return 0; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(14)]
        public static byte AuthenticationReserved4
        {
            get { return 0; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(15)]
        public static byte AuthenticationReserved5
        {
            get { return 0; }
        }

        /// <summary>
        /// Integrity payload type, always 0x01.
        /// </summary>
        [IpmiMessageData(16)]
        public byte IntegrityType
        {
            get { return this.integrityType; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(17)]
        public static byte IntegrityReserved1
        {
            get { return 0; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(18)]
        public static byte IntegrityReserved2
        {
            get { return 0; }
        }

        /// <summary>
        /// Integrity payload length in bytes.
        /// </summary>
        /// <value>Always 0x08.</value>
        [IpmiMessageData(19)]
        public byte IntegrityLength
        {
            get { return this.integrityLength; }
        }

        /// <summary>
        /// Integrity alogrithm.
        /// </summary>
        [IpmiMessageData(20)]
        public byte IntegrityAlgorithm
        {
            get { return this.integrityAlgorithm; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(21)]
        public static byte IntegrityReserved3
        {
            get { return 0; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(22)]
        public static byte IntegrityReserved4
        {
            get { return 0; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(23)]
        public static byte IntegrityReserved5
        {
            get { return 0; }
        }

        /// <summary>
        /// Confidentiality payload type, always 0x00.
        /// </summary>
        [IpmiMessageData(24)]
        public byte ConfidentialityType
        {
            get { return this.confidentialityType; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(25)]
        public static byte ConfidentialityReserved1
        {
            get { return 0; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(26)]
        public static byte ConfidentialityReserved2
        {
            get { return 0; }
        }

        /// <summary>
        /// Confidentiality payload length in bytes.
        /// </summary>
        /// <value>Always 0x08.</value>
        [IpmiMessageData(27)]
        public byte ConfidentialityLength
        {
            get { return this.confidentialityLength; }
        }

        /// <summary>
        /// Confidentiality alogrithm.
        /// </summary>
        [IpmiMessageData(28)]
        public byte ConfidentialityAlgorithm
        {
            get { return this.confidentialityAlgorithm; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(29)]
        public static byte ConfidentialityReserved3
        {
            get { return 0; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(30)]
        public static byte ConfidentialityReserved4
        {
            get { return 0; }
        }

        /// <summary>
        /// Reserved.
        /// </summary>
        /// <value>Always 0.</value>
        [IpmiMessageData(31)]
        public static byte ConfidentialityReserved5
        {
            get { return 0; }
        }
    }
}
