/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  srsankar@microsoft.com                     *
*                                                   	*
********************************************************/


namespace Microsoft.GFS.WCS.ChassisManager
{
    /// <summary>
    /// Set Fan Speed Request
    /// </summary>
    [ChassisMessageRequest(FunctionCode.SetFanSpeed)]
    internal class FanSetRpmRequest : ChassisRequest 
    {
       
        /// <summary>
        /// Set the Fan Speed
        /// </summary>
        public FanSetRpmRequest(byte pwm)
        {
            this.Pwm = pwm;
        }

        /// <summary>
        /// Sets RPM for the Fan
        /// </summary>
        [ChassisMessageData(0)]
        public byte Pwm
        {
            get; set;            
        }
    }

    [ChassisMessageResponse(FunctionCode.SetFanSpeed)]
    internal class FanSetResponse : ChassisResponse
    {
    }
}
