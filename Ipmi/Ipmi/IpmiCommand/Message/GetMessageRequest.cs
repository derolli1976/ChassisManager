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
    /// Represents the IPMI 'Get Message' application request message.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Application, IpmiCommand.GetMessage)]
    internal class GetMessageRequest : IpmiRequest
    {

        /// <summary>
        /// Initializes a new instance of the GetMessageRequest class.
        /// </summary>
        internal GetMessageRequest()
        {
        }
    }
}
