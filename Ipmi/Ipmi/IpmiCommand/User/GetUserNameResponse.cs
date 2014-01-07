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
        /// Represents the IPMI 'Get User Name' response message.
        /// </summary>
        [IpmiMessageResponse(IpmiFunctions.Application, IpmiCommand.GetUserName)]
        internal class GetUserNameResponse : IpmiResponse
        {

            /// <summary>
            /// User Name String Data.
            /// </summary>
            private byte[] userName = {};

            

            /// <summary>
            /// Gets UserName.
            /// </summary>          
            [IpmiMessageData(0)]
            public byte[] UserName
            {
                get { return this.userName; }
                set { this.userName = value; }

            }


        }
   
}
