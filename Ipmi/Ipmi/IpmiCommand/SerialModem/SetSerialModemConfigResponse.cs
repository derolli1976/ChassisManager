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
    /// Represents the IPMI 'Set Serial Modem Configuration' application response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Transport, IpmiCommand.SetSerialModemConfiguration)]
    internal class SetSerialModemConfigResponse : IpmiResponse
    {
    }
}
