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
    /// Represents the IPMI 'Get Sdr Repository Info' response message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.GetSdrRepositoryInfo)]
    internal class GetSdrRepositoryInfoResponse : IpmiResponse
    {
        /// <summary>
        /// Sdr Version Number.
        /// </summary>  
        private byte sdrversion;

        /// <summary>
        /// Reservervation Id Ls (Least Significant byte).
        /// </summary>  
        private byte lsbyte;

        /// <summary>
        /// Reservervation Id MS (Most Significant byte).
        /// </summary>
        private byte msbyte;

        /// <summary>
        /// Sdr Free space.
        /// </summary>
        private byte[] freespace;

        /// <summary>
        /// Most Recent Entry TimeStamp.
        /// </summary>
        private byte[] lastadded;

        /// <summary>
        /// Most Recent Record Delete/Clear TimeStamp.
        /// </summary>
        private byte[] lastremoved;

        /// <summary>
        /// Gets Sdr Version Number.
        /// </summary>       
        [IpmiMessageData(0)]
        public byte SdrVersion
        {
            get { return this.sdrversion; }
            set { this.sdrversion = value; }
        }

        /// <summary>
        /// Number of Sdr Records (Least Significant byte).
        /// </summary>       
        [IpmiMessageData(1)]
        public byte LSByte
        {
            get { return this.lsbyte; }
            set { this.lsbyte = value; }
        }

        /// <summary>
        /// Number of Sdr Records (Most Significant Byte).
        /// </summary>       
        [IpmiMessageData(2)]
        public byte MSByte
        {
            get { return this.msbyte; }
            set { this.msbyte = value; }

        }

        /// <summary>
        /// Free Space (Least Significant Byte).
        /// </summary> 
        [IpmiMessageData(3, 2)]
        public byte[] SdrFeeSpace
        {
            get { return this.freespace; }
            set { this.freespace = value; }

        }

        /// <summary>
        /// Most Recent Entry time.
        /// </summary> 
        [IpmiMessageData(5, 4)]
        public byte[] LastAdded
        {
            get { return this.lastadded; }
            set { this.lastadded = value; }

        }

        /// <summary>
        /// Last Entry Delete/Clear.
        /// </summary> 
        [IpmiMessageData(9, 4)]
        public byte[] LastRemoved
        {
            get { return this.lastremoved; }
            set { this.lastremoved = value; }

        }
    }
}
