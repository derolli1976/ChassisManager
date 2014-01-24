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

    [IpmiMessageRequest(IpmiFunctions.Storage, IpmiCommand.GetSelInfo)]
    internal class SelInfoResponse : IpmiResponse
    {

        /// <summary>
        /// SEL Version Number).
        /// </summary>  
        private byte selversion;

        /// <summary>
        /// Reservervation Id MS (Most Significant byte).
        /// </summary>  
        private byte lsbyte;

        /// <summary>
        /// Reservervation Id MS (Most Significant byte).
        /// </summary>
        private byte msbyte;

        /// <summary>
        /// SEL Free space.
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
        /// Gets SEL Version Number.
        /// </summary>       
        [IpmiMessageData(0)]
        public byte SELVersion
        {
            get { return this.selversion; }
            set { this.selversion = value; }
        }

        /// <summary>
        /// Number of Log Entries in the SEL (Least Significant byte).
        /// </summary>       
        [IpmiMessageData(1)]
        public byte LSByte
        {
            get { return this.lsbyte; }
            set { this.lsbyte = value; }
        }

        /// <summary>
        /// Number of Log Entries in the SEL (Most Significant Byte).
        /// </summary>       
        [IpmiMessageData(2)]
        public byte MSByte
        {
            get { return this.msbyte; }
            set { this.msbyte = value; }
           
        }

        /// <summary>
        /// SEL Free Space (Least Significant Byte).
        /// </summary> 
        [IpmiMessageData(3, 2)]
        public byte[] SelFeeSpace
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

