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

namespace Microsoft.GFS.WCS.ChassisManager
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Represents a client session to a computer via sled over the network.
    /// </summary>
    public abstract class ChassisSendReceive
    {   
        #region Send/Receive Methods

        /// <summary>
        /// Generics method SendReceive for easier use
        /// </summary>
        public T SendReceive<T>(DeviceType deviceType, byte deviceId, ChassisRequest chassisRequest) where T : ChassisResponse
        {
            // call SendReceive
            return (T)SendReceive(deviceType, deviceId, chassisRequest, typeof(T));
        }


        /// <summary>
        /// Send Receive chassis messages
        /// </summary>
        public ChassisResponse SendReceive(DeviceType deviceType, byte deviceId, ChassisRequest chassisRequest, Type responseType, byte priority = (byte)PriorityLevel.User)
        {
            // Serialize the OMC request into bytes.
            byte[] chassisRequestMessage = chassisRequest.GetBytes();
            byte[] chassisResponseMessage;

            CommunicationDevice.SendReceive((PriorityLevel)priority, (byte)deviceType, deviceId, chassisRequestMessage, out chassisResponseMessage);

            // Create the response based on the provided type and message bytes.
            ConstructorInfo constructorInfo = responseType.GetConstructor(Type.EmptyTypes);
            ChassisResponse chassisResponse = (ChassisResponse)constructorInfo.Invoke(new Object[0]);

            // Expected Packet Format:
            //        4            5-6       N         
            // |Completion Code|Byte Count|Payload|
            //       0 byte       2 byte    3+ byte
            if (chassisResponseMessage.Length >= 3)
            {
                chassisResponse.Initialize(chassisResponseMessage, chassisResponseMessage.Length);
            }
            else
            {
                chassisResponse.CompletionCode = (byte)CompletionCode.ResponseNotProvided;
            }
            // Response to the OMC request message.
            return chassisResponse;
        }

        #endregion
    }
}
