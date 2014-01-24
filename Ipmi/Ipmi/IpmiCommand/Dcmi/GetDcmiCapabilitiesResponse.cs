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
    /// Represents the DCMI 'Get DCMI Capabilities' response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Dcgrp, IpmiCommand.DcmiCapability)]
    internal class GetDcmiCapabilitiesResponse : IpmiResponse
    {
        /// <summary>
        /// Group Extension.
        /// </summary>
        private byte groupExtension;

        /// <summary>
        /// Specification Conformance (first byte).
        /// </summary>
        private byte specificationMajorVersion;

        /// <summary>
        /// Specification Conformance (second byte).
        /// </summary>
        private byte specificationMinorVersion;

        /// <summary>
        /// Parameter Revision.
        /// </summary>
        private byte parameterRevision;

        /// <summary>
        /// Response Data. Depends on Request Selector
        /// </summary>
        private byte[] responseData;

        /// <summary>
        /// Gets and sets the Group Extension.
        /// </summary>
        /// <value>Group Extension.</value>
        [IpmiMessageData(0)]
        public byte GroupExtension
        {
            get { return this.groupExtension; }
            set { this.groupExtension = value; }
        }

        /// <summary>
        /// Gets and sets the Specification Major Version (first byte).
        /// </summary>
        /// <value>Specification Conformance (first byte).</value>
        [IpmiMessageData(1)]
        public byte SpecificationMajorVersion
        {
            get { return this.specificationMajorVersion; }
            set { this.specificationMajorVersion = value; }
        }

        /// <summary>
        /// Gets and sets the Specification Major Version (second byte).
        /// </summary>
        /// <value>Specification Conformance (second byte).</value>
        [IpmiMessageData(2)]
        public byte SpecificationMinorVersion
        {
            get { return this.specificationMinorVersion; }
            set { this.specificationMinorVersion = value; }
        }

        /// <summary>
        /// Gets and sets the ParameterRevision (reserved).
        /// </summary>
        /// <value>ParameterRevision(reserved).</value>
        [IpmiMessageData(3)]
        public byte ParameterRevision
        {
            get { return this.parameterRevision; }
            set { this.parameterRevision = value; }
        }

        /// <summary>
        /// Gets and sets the Response Data.
        /// </summary>
        /// <value>Capabilities Response Data.</value>
        [IpmiMessageData(4)]
        public byte[] ResponseData
        {
            get { return this.responseData; }
            set { this.responseData = value; }
        }
    }
}
