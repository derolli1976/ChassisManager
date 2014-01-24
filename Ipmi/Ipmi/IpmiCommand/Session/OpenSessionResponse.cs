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
    /// Represents the RMCP+ 'Open Session' response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.SessionSetup, IpmiCommand.OpenSessionResponse)]
    internal class OpenSessionResponse : IpmiResponse
    {
        /// <summary>
        /// Orignal message tag provided in the OpenSession request message.
        /// </summary>
        private byte messageTag;

        /// <summary>
        /// Status code of the request.
        /// </summary>
        private byte statusCode;

        /// <summary>
        /// Maximum privilege level for this session.
        /// </summary>
        private PrivilegeLevel maximumPrivilegeLevel;

        /// <summary>
        /// Remote console session id.
        /// </summary>
        private uint remoteSessionId;

        /// <summary>
        /// Managed system session id.
        /// </summary>
        private uint managedSessionId;

        /// <summary>
        /// Authentication payload type, always 0x00.
        /// </summary>
        private byte authenticationType;

        /// <summary>
        /// Authentication payload length in bytes, always 0x08.
        /// </summary>
        private byte authenticationLength = 0x08;

        /// <summary>
        /// Authentication alogrithm.
        /// </summary>
        private byte authenticationAlgorithm;

        /// <summary>
        /// Integrity payload type, always 0x01.
        /// </summary>
        private byte integrityType = 0x01;

        /// <summary>
        /// Integrity payload length in bytes, always 0x08.
        /// </summary>
        private byte integrityLength = 0x08;

        /// <summary>
        /// Integrity alogrithm.
        /// </summary>
        private byte integrityAlgorithm;

        /// <summary>
        /// Confidentiality payload type, always 0x02.
        /// </summary>
        private byte confidentialityType = 0x02;

        /// <summary>
        /// Confidentiality payload length in bytes, always 0x08.
        /// </summary>
        private byte confidentialityLength = 0x08;

        /// <summary>
        /// Confidentiality alogrithm.
        /// </summary>
        private byte confidentialityAlgorithm;

        /// <summary>
        /// Gets and sets the remote console message tag.
        /// </summary>
        /// <value>byte representing the remote console message tag.</value>
        [IpmiMessageData(0)]
        public byte MessageTag
        {
            get { return this.messageTag; }
            set { this.messageTag = value; }
        }

        /// <summary>
        /// Gets and sets the status code of the request.
        /// </summary>
        /// <value>Status code.</value>
        [IpmiMessageData(1)]
        public byte StatusCode
        {
            get { return this.statusCode; }
            set { this.statusCode = value; }
        }

        /// <summary>
        /// Gets and sets the maximum privilege level for this session.
        /// </summary>
        /// <value>PrivilegeLevel.</value>
        [IpmiMessageData(2)]
        public byte MaximumPrivilegeLevel
        {
            get { return (byte)this.maximumPrivilegeLevel; }
            set { this.maximumPrivilegeLevel = (PrivilegeLevel)value; }
        }

        /// <summary>
        /// Gets and sets the remote console session id.
        /// </summary>
        [IpmiMessageData(4)]
        public uint RemoteSessionId
        {
            get { return this.remoteSessionId; }
            set { this.remoteSessionId = value; }
        }

        /// <summary>
        /// Gets and sets the managed session id.
        /// </summary>
        [IpmiMessageData(8)]
        public uint ManagedSessionId
        {
            get { return this.managedSessionId; }
            set { this.managedSessionId = value; }
        }

        /// <summary>
        /// Gets and sets the authentication payload type, always 0x00.
        /// </summary>
        [IpmiMessageData(12)]
        public byte AuthenticationType
        {
            get { return this.authenticationType; }
            set { this.authenticationType = value; }
        }

        /// <summary>
        /// Gets and sets the authentication payload length in bytes.
        /// </summary>
        /// <value>Always 0x08.</value>
        [IpmiMessageData(15)]
        public byte AuthenticationLength
        {
            get { return this.authenticationLength; }
            set { this.authenticationLength = value; }
        }

        /// <summary>
        /// Gets and sets the authentication alogrithm.
        /// </summary>
        [IpmiMessageData(16)]
        public byte AuthenticationAlgorithm
        {
            get { return this.authenticationAlgorithm; }
            set { this.authenticationAlgorithm = value; }
        }

        /// <summary>
        /// Gets and sets the integrity payload type, always 0x00.
        /// </summary>
        [IpmiMessageData(20)]
        public byte IntegrityType
        {
            get { return this.integrityType; }
            set { this.integrityType = value; }
        }

        /// <summary>
        /// Gets and sets the integrity payload length in bytes.
        /// </summary>
        [IpmiMessageData(23)]
        public byte IntegrityLength
        {
            get { return this.integrityLength; }
            set { this.integrityLength = value; }
        }

        /// <summary>
        /// Gets and sets the integrity alogrithm.
        /// </summary>
        [IpmiMessageData(24)]
        public byte IntegrityAlgorithm
        {
            get { return this.integrityAlgorithm; }
            set { this.integrityAlgorithm = value; }
        }

        /// <summary>
        /// Gets and sets the confidentiality payload type.
        /// </summary>
        [IpmiMessageData(28)]
        public byte ConfidentialityType
        {
            get { return this.confidentialityType; }
            set { this.confidentialityType = value; }
        }

        /// <summary>
        /// Gets and sets the confidentiality payload length in bytes.
        /// </summary>
        /// <value>Always 0x08.</value>
        [IpmiMessageData(31)]
        public byte ConfidentialityLength
        {
            get { return this.confidentialityLength; }
            set { this.confidentialityLength = value; }
        }

        /// <summary>
        /// Gets and sets the confidentiality alogrithm.
        /// </summary>
        [IpmiMessageData(32)]
        public byte ConfidentialityAlgorithm
        {
            get { return this.confidentialityAlgorithm; }
            set { this.confidentialityAlgorithm = value; }
        }
    }
}
