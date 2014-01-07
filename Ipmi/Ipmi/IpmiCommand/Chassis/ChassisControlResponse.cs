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
    /// Represents the IPMI 'Chassis Control' application response message.
    /// </summary>
    [IpmiMessageResponse(IpmiFunctions.Chassis, IpmiCommand.ChassisControl)]
    internal class ChassisControlResponse : IpmiResponse
    {


        //private byte completionCode;
        // <summary>
        // Gets and sets the Current power state.
        // </summary>
        // <value>Current power state.</value>
        //[IpmiMessageData(0)]
        //public byte CompletionCodeTest
        //{
        //    get { return this.completionCode; }
        //    set { this.completionCode = value; }
        //}
    }
}
