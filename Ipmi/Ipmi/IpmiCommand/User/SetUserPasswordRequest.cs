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
    /// Represents the IPMI 'Set User Password' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.SetUserPassword, 22)]
    internal class SetUserPasswordRequest : IpmiRequest
    {
        /// <summary>
        /// User Id.
        /// [7] Password lenght
        /// [5:0] UserId
        /// </summary>
        private byte userId;

        /// <summary>
        /// Password, 20 byte max for IPMI V2.0 RMCP, 16 byte for IPMI v1.5
        /// </summary>
        private byte[] password;

        /// <summary>
        /// Disable user.
        /// </summary>
        public const byte OperationDisableUser = 0;

        /// <summary>
        /// Enable user.
        /// </summary>
        public const byte OperationEnableUser = 1;

        /// <summary>
        /// Set Password.
        /// </summary>
        public const byte OperationSetPassword = 2;

        /// <summary>
        /// Test Password.
        /// </summary>
        public const byte OperationTestPassword = 3;

        /// <summary>
        /// Operation to perform.
        /// </summary>
        public readonly byte operation;

        /// <summary>
        /// Initializes a new instance of the SetUserPassword class.
        /// </summary>
        public SetUserPasswordRequest(byte userId, byte operation, byte[] password)
        {
            this.userId = userId;
            this.operation = operation;
            this.password = password;
        }

        /// <summary>
        /// Set the password for a specific user id.
        /// </summary>       
        [IpmiMessageData(0)]
        public byte UserId
        {
            get { return this.userId; }
        }


        /// <summary>
        /// Set the password operation.
        /// </summary>       
        [IpmiMessageData(1)]
        public byte Operation
        {
            get { return this.operation; }
        }


        /// <summary>
        /// Set the password.
        /// 20 byte password.
        /// </summary>       
        [IpmiMessageData(2, 20)]
        public byte[] Password
        {
            get { return this.password; }
        }
    }
}
