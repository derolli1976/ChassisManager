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
    /// Represents the IPMI 'Get Message Flags' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetMessageFlags)]
    internal class GetMessageFlagsRequest : IpmiRequest
    {
        /// <summary>
        /// Initializes a new instance of the GetMessageRequest class.
        /// </summary>
        internal GetMessageFlagsRequest()
        {
        }
    }
}
