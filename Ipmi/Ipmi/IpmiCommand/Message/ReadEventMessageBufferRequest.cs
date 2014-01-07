/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*   							                        *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    /// <summary>
    /// Represents the IPMI 'Read Event Message Buffer' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.ReadEventMessageBuffer)]
    internal class ReadEventMessageBufferRequest : IpmiRequest
    {

        /// <summary>
        /// Initializes a new instance of the ReadEventMessageBufferRequest class.
        /// </summary>
        internal ReadEventMessageBufferRequest()
        {
        }
    }
}
