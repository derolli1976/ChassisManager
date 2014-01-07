/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Author:  srsankar@microsoft.com                     *
*            Bryankel@Microsoft.com                     *
*                                                   	*
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager
{

    /// <summary>
    /// Fan Class
    /// </summary>
    internal class Fan : ChassisSendReceive
    {
        /// <summary>
        /// device Id
        /// </summary>
        private byte deviceId;

        /// <summary>
        ///  Device Type
        /// </summary>
        private DeviceType deviceType;
        
        public Fan(byte deviceId)
        {
            // set the type as Fan
            this.deviceType = DeviceType.Fan;

            // set the device Id
            this.deviceId = deviceId;
        }

        #region Fan Commands

        /// <summary>
        /// Gets the fan speed in Rpm
        /// </summary>
        internal FanSpeedResponse GetFanSpeed()
        {
            return GetFanSpeed(this.deviceId);
        }

        /// <summary>
        /// Gets Fan speed in RPM
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        internal FanSpeedResponse GetFanSpeed(byte deviceId)
        {
            // Get Fan Requirement
            FanSpeedResponse response = (FanSpeedResponse)this.SendReceive(deviceType, deviceId, new FanSpeedRequest(),
              typeof(FanSpeedResponse), (byte) PriorityLevel.System);

            if (response.CompletionCode != (byte)CompletionCode.Success)
            {
                Tracer.WriteError("GetFanSpeed - error getting fan speed, completion code: {0:X}", response.CompletionCode);
            }
            
            return response;
        }

        /// <summary>
        /// Sets the RPM of the fan
        /// </summary>
        /// <param name="RPM"></param>
        /// <returns></returns>
        public byte SetFanSpeed(byte pwm)
        {
            return SetFanSpeed(this.deviceId, pwm);
        }

        /// <summary>
        /// Sets RPM for a particular Fan
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="RPM"></param>
        /// <returns></returns>
        public byte SetFanSpeed(byte deviceId, byte PWM)
        {
            // Set fan speed and return set value
            FanSetResponse response = (FanSetResponse)this.SendReceive(deviceType, deviceId, new FanSetRpmRequest(PWM),
              typeof(FanSetResponse), (byte)PriorityLevel.System);

            return response.CompletionCode;
        }

        /// <summary>
        /// Gets status of fan. Calls GetFanSpeed internally to check if fan has a particular RPM 
        /// </summary>
        /// <returns></returns>
        public bool GetFanStatus()
        {
            FanSpeedResponse fanspeed = GetFanSpeed(this.deviceId);
            if (fanspeed.CompletionCode == (byte)CompletionCode.Success 
                && fanspeed.Rpm != 0)
            {
                return true;
            }

            return false;
        }
        
        #endregion
        
    }
}
