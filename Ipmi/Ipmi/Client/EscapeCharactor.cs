/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*                                       	            *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    /// <summary>
    /// Double byte charactors to replace ipmi escape charactors.
    /// See IPMI 2.0: 14.4.1 - Basic Mode Packet Framing
    /// See IPMI 2.0: 14.4.2 - Data Byte Escaping 
    /// </summary>
    internal class EscapeCharactor
    {
        internal byte Frame;
        internal byte[] Replace;

        internal EscapeCharactor(byte frame, byte[] replace)
        {
            this.Frame = frame;
            this.Replace = replace;
        }
    }  

}
