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

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{

    using System;
    using System.Collections.Generic;

    internal abstract class IpmiClientExtended : IpmiClientAdvance
    {

        #region Send/Receive

        /// <summary>
        /// Send Receive Ipmi messages
        /// </summary>
        internal override abstract IpmiResponse IpmiSendReceive(IpmiRequest ipmiRequest, Type responseType, bool allowRetry = true);

        #endregion

        #region DCMI Commands

        /// <summary>
        /// DCMI Get Power Limit Command
        /// </summary>
        public virtual PowerLimit GetPowerLimit()
        {
            // Get DCMI Power Limit
            GetDcmiPowerLimitResponse response = (GetDcmiPowerLimitResponse)this.IpmiSendReceive(
            new GetDcmiPowerLimitRequest(), typeof(GetDcmiPowerLimitResponse));

            // Return item
            PowerLimit pwr = new PowerLimit(response.CompletionCode);


            if (response.CompletionCode == 0 || // Active Limit set
                response.CompletionCode == 0x80) // No Active Limit set
            {
                pwr.SetParamaters(
                  response.PowerLimit,
                  response.SamplingPeriod,
                  response.ExceptionActions,
                  response.CorrectionTime);
            }
            
            return pwr;
        }

        /// <summary>
        /// DCMI Set Power Limit Command
        /// </summary>
        public virtual ActivePowerLimit SetPowerLimit(short watts, int correctionTime, byte action, short samplingPeriod)
        {
            // Set DCMI Power Limit
            SetDcmiPowerLimitResponse response = (SetDcmiPowerLimitResponse)this.IpmiSendReceive(
            new SetDcmiPowerLimitRequest(watts, correctionTime, action, samplingPeriod), typeof(SetDcmiPowerLimitResponse));

            ActivePowerLimit act = new ActivePowerLimit(response.CompletionCode);

            byte[] activeLimit = new byte[1];
            activeLimit[0] = response.CompletionCode;

            // verify valid response
            act.SetParamaters(activeLimit);

            return act;
        }

        /// <summary>
        /// DCMI Get Power Reading Command
        /// </summary>
        public virtual List<PowerReading> GetAdvancedPowerReading()
        {
            // Return item
            List<PowerReading> returnlist = new List<PowerReading>();

            // Index Offset of 0 into ResponseData
            int index = 0;

            // Get DCMI Capabilities to check if power management is supported, if so 
            // check if advanced power stats are supported.
            GetDcmiCapabilitiesResponse response = (GetDcmiCapabilitiesResponse)this.IpmiSendReceive(
            new GetDcmiCapabilitiesRequest(0x01), typeof(GetDcmiCapabilitiesResponse));

            byte[] powerSupport = new byte[2] { 0x00, 0x00 };

            if (response.CompletionCode == 0)
            {
                // power management support byte array.  response.ResponseData[1] = platform capabilities.
                // [7-1]  Reserved, Power Management.
                byte[] tempArray = IpmiSharedFunc.ByteSplit(response.ResponseData[1], new int[2] { 1, 0 });
                Buffer.BlockCopy(tempArray, 0, powerSupport, 0, 2);
            }

            if (powerSupport[1] == 0x01)
            {
                // Check DCMI paramater revision 0x02 = DCMI errata for advanced
                // power management.  If the paramater version is 2, it should
                // support advanced power management.
                if (response.ParameterRevision == 0x02)
                {
                    // Get DCMI Capabilities for advanced power averages
                    response = (GetDcmiCapabilitiesResponse)this.IpmiSendReceive(
                    new GetDcmiCapabilitiesRequest(0x05), typeof(GetDcmiCapabilitiesResponse));

                    if (response.CompletionCode == 0)
                    {
                        // GetDcmiCapabilitiesResponse Response Data
                        byte[] capabilities = response.ResponseData;

                        // The number of supported rolling average time periods
                        int averages = (int)capabilities[0];

                        if (averages > 0)
                        {
                            // Loop through the available averages
                            for (int i = 0; i < averages; i++)
                            {
                                // Increment the Index Offset
                                index++;

                                //[7:6]: Time duration units 
                                //[5-0]: Time duration (Maximum of 63 units) 
                                byte[] timeperiod = IpmiSharedFunc.ByteSplit(capabilities[index], new int[2] { 6, 0 });

                                // Get Power Readings Array (DCMI spec)
                                // Reading mode: 0x01 = standard, 0x02 = advanced
                                PowerReadingSupport powerreadings = PowerReadingSupport(0x02, capabilities[index]);

                                // create a new instance of the power reading class
                                PowerReading pwr = new PowerReading(powerreadings.CompletionCode);

                                if (pwr.CompletionCode == 0)
                                {
                                    pwr.SetParamaters(powerreadings.Readings);

                                    // Units of time (number of units)
                                    pwr.TimeNumber = Convert.ToInt32(timeperiod[1]);

                                    // time sample (hours, minutes etc)
                                    pwr.TimeUnit = Convert.ToInt16(timeperiod[0]);
                                }
                                // add pwr to the return list
                                returnlist.Add(pwr);
                            }
                        }
                        else // get standard power statistics
                        {
                            return GetPowerReading();
                        }
                    }
                    else
                    {
                        // create a new instance of the power reading class
                        PowerReading pwr = new PowerReading(response.CompletionCode);
                        // add ERROR pwr to the return list
                        returnlist.Add(pwr);
                    }
                }
                else // standard power statistics 
                {
                    return GetPowerReading();
                }
            }
            else
            {
                // power management is unsupported
                // create a new instance of the power reading class
                PowerReading pwr = new PowerReading(response.CompletionCode);

                // system does not support power readings
                pwr.PowerSupport = false;

                // add pwr to the return list
                returnlist.Add(pwr);
            }

            return returnlist;
        }

        public virtual List<PowerReading> GetPowerReading()
        {
            // Return item
            List<PowerReading> returnlist = new List<PowerReading>();

            // Get Power Readings Array (DCMI spec)
            // Reading mode: 0x01 = standard, 0x02 = advanced
            PowerReadingSupport powerreadings = this.PowerReadingSupport(0x01, 0x00);

            // create a new instance of the power reading class
            PowerReading pwr = new PowerReading(powerreadings.CompletionCode);

            if (powerreadings.CompletionCode == 0)
            {
                // system does support power readings
                pwr.SetParamaters(powerreadings.Readings);
            }

            // add pwr to the return list
            returnlist.Add(pwr);

            // return the list
            return returnlist;
        }

        /// <summary>
        /// Activate/Deactivate DCMI power limit
        /// </summary>
        /// <param name="enable">Activate/Deactivate</param>
        public virtual bool ActivatePowerLimit(bool enable)
        {
            // Dcmi Activate/Deactivate power limit
            DcmiActivatePowerLimitResponse response = (DcmiActivatePowerLimitResponse)this.IpmiSendReceive(
            new DcmiActivatePowerLimitRequest(enable), typeof(DcmiActivatePowerLimitResponse));

            if (response.CompletionCode == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Command Support: DCMI

        /// <summary>
        /// Get DCMI Power Reading Values
        /// </summary>
        /// <param name="AverageByte">Rolling Average Byte (DCMI Capabilities)</param>
        private PowerReadingSupport PowerReadingSupport(byte readingMode, byte averageByte)
        {
            // Function Return Value
            byte[] readings = new byte[12];

            // Get Power reading passing in the Rolling Average Byte
            GetDcmiPowerReadingResponse response = (GetDcmiPowerReadingResponse)this.IpmiSendReceive(
            new GetDcmiPowerReadingRequest(readingMode, averageByte), typeof(GetDcmiPowerReadingResponse));

            PowerReadingSupport returnObj = new PowerReadingSupport(response.CompletionCode);

            if (response.CompletionCode == 0)
            {
                // Add Current Power Reading to the return array
                readings[0] = response.CurrentPower[0];
                readings[1] = response.CurrentPower[1];

                // Add Minimum Power Reading to the return array
                readings[2] = response.MinimumPower[0];
                readings[3] = response.MinimumPower[1];

                // Add Maximum Power Reading to the return array
                readings[4] = response.MaximumPower[0];
                readings[5] = response.MaximumPower[1];

                // Add Average Power Reading to the return array
                readings[6] = response.AveragePower[0];
                readings[7] = response.AveragePower[1];

                // Add Statistics reporting time period 
                Buffer.BlockCopy(response.Statistics, 0, readings, 8, 4);

                returnObj.SetParamaters(readings);
            }

            return returnObj;
        }

        #endregion

        #region Serial Modem

        /// <summary>
        /// Set Serial Mux Switch to System for Console Redirection.
        /// </summary>
        public virtual SerialMuxSwitch SetSerialMuxSwitch()
        {
            ChannelAuthenticationCapabilities auth = 
                this.GetAuthenticationCapabilities(PrivilegeLevel.Administrator, false);

            if (auth.CompletionCode == 0x00)
            {
                byte channel = auth.ChannelNumber;

                return this.SetSerialMuxSwitch(channel, MuxSwtich.ForceSystem);
            }
            else
            {
                return new SerialMuxSwitch(auth.CompletionCode);
            }
            
        }

        /// <summary>
        /// Switches Serial control from BMC to serial port for console redirection
        /// </summary>
        public virtual SerialMuxSwitch SetSerialMuxSwitch(byte channel, MuxSwtich mux)
        {
            SetSerialMuxResponse setMux = (SetSerialMuxResponse)this.IpmiSendReceive(
                new SetSerialMuxRequest(channel, mux), typeof(SetSerialMuxResponse), false);

            SerialMuxSwitch response = new SerialMuxSwitch(setMux.CompletionCode);

            if (setMux.CompletionCode == 0)
            {
                setMux.GetMux();

                response.SetParamaters(
                                        setMux.AlertInProgress,
                                        setMux.MessagingActive,
                                        setMux.MuxSetToSystem,
                                        setMux.MuxSwitchAllowed,
                                        setMux.RequestAccepted,
                                        setMux.RequestToBmcAllowed
                                      );
            }


            return response;

        }

        /// <summary>
        /// Switches Serial control from System serial port to Bmc to close console redirection
        /// </summary>
        public virtual SerialMuxSwitch ResetSerialMux()
        {
            //Sent an Ipmi Command
            ChannelAuthenticationCapabilities auth = GetAuthenticationCapabilities(PrivilegeLevel.Administrator, false);

            // try 1 more time, as serial console snooping my not have detected the 1st request
            if (auth.CompletionCode != 0)
            {
                auth = GetAuthenticationCapabilities(PrivilegeLevel.Administrator, false);
            }

            // create response package
            SerialMuxSwitch response = new SerialMuxSwitch(auth.CompletionCode);

            if (auth.CompletionCode == 0)
            {
                response.SetParamaters(true, true, true,
                    true, true, true);
            }
            else
            {
                response.SetParamaters(true, false, true,
                    true, false, true);
            }

            return response;
        }

        /// <summary>
        /// Ipmi Set Serial/Modem Configuration
        /// </summary>
        public virtual bool SetSerialConfig<T>(T paramater) where T : SerialConfig.SerialConfigBase
        {
            bool success = false;

            // serial channel is 2
            byte channel = 0x02;

            SetSerialModemConfigResponse setInProgress =
                    (SetSerialModemConfigResponse)this.IpmiSendReceive(
                        new SetSerialModemConfigRequest<SerialConfig.SetInProcess>(channel, new SerialConfig.SetInProcess(0x01)),
                        typeof(SetSerialModemConfigResponse), false);
            //10 0E 00 01 5C A5
            if (setInProgress.CompletionCode == 0)
            {

                SetSerialModemConfigResponse response =
                    (SetSerialModemConfigResponse)this.IpmiSendReceive(
                        new SetSerialModemConfigRequest<T>(channel, paramater),
                        typeof(SetSerialModemConfigResponse), false);

                if (response.CompletionCode == 0)
                {
                    success = true;
                }
                else
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }

            this.IpmiSendReceive(
                new SetSerialModemConfigRequest<SerialConfig.SetInProcess>(channel, new SerialConfig.SetInProcess(0x02)),
                typeof(SetSerialModemConfigResponse), false);

            SetSerialModemConfigResponse setComplete =
                    (SetSerialModemConfigResponse)this.IpmiSendReceive(
                    new SetSerialModemConfigRequest<SerialConfig.SetInProcess>(channel, new SerialConfig.SetInProcess(0x00)),
                    typeof(SetSerialModemConfigResponse), false);

            if (setComplete.CompletionCode != 0)
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Ipmi Get Channel Info command
        /// </summary>
        public virtual ChannelInfo GetChannelInfo(byte channel = 0x0E)
        {
            GetChannelInfoResponse response =
                    (GetChannelInfoResponse)this.IpmiSendReceive(
                        new GetChannelInfoRequest(channel),
                        typeof(GetChannelInfoResponse));

            ChannelInfo responseObj = new ChannelInfo(response.CompletionCode);

            if (response.CompletionCode == 0)
            {
                responseObj.SetParamaters(
                                            response.ChannelNumber,
                                            response.ChannelMedium,
                                            response.ChannelProtocol,
                                            response.ChannelSessionSupport,
                                            response.NumberOfSessions);
            }

            return responseObj;

        }

        /// <summary>
        /// Get Serial/Modem Configuration
        /// </summary>
        public virtual T GetSerialConfig<T>(T paramater) where T : SerialConfig.SerialConfigBase
        {
            // channel request is being processed
            byte channel = 0x02;

            GetSerialModemConfigResponse response =
                (GetSerialModemConfigResponse)this.IpmiSendReceive(
                    new GetSerialModemConfigRequest<T>(channel, paramater),
                    typeof(GetSerialModemConfigResponse));

            if (response.CompletionCode == 0)
                paramater.Initialize(response.Payload);

            return paramater;
        }

        /// <summary>
        /// Get Serial/Modem Configuration
        /// </summary>
        public virtual T GetSerialConfig<T>(byte channel, T paramater) where T : SerialConfig.SerialConfigBase
        {
            GetSerialModemConfigResponse response =
                (GetSerialModemConfigResponse)this.IpmiSendReceive(
                    new GetSerialModemConfigRequest<T>(channel, paramater),
                    typeof(GetSerialModemConfigResponse));

            if (response.CompletionCode == 0)
                paramater.Initialize(response.Payload);

            return paramater;
        }

        #endregion

        #region JBOD

        /// <summary>
        /// Gets the Disk Status of JBODs
        /// </summary>
        public virtual DiskStatusInfo GetDiskStatus()
        {
            GetDiskStatusResponse jbod = (GetDiskStatusResponse)this.IpmiSendReceive(
                new GetDiskStatusRequest(), typeof(GetDiskStatusResponse));

            DiskStatusInfo response = new DiskStatusInfo(jbod.CompletionCode);

            if (jbod.CompletionCode == 0)
            {
                response.SetParamaters(jbod.Channel, jbod.DiskCount,
                    jbod.StatusData);
            }

            return response;

        }

        /// <summary>
        /// Gets the Disk Status of JBODs
        /// </summary>
        public virtual DiskInformation GetDiskInfo()
        {
            // Call Get Disk Info with Default values
            return this.GetDiskInfo(0x00, 0x00);
        }

        /// <summary>
        /// Gets the Disk Status of JBODs
        /// </summary>
        public virtual DiskInformation GetDiskInfo(byte channel, byte disk)
        {
            GetDiskInfoResponse jbod = (GetDiskInfoResponse)this.IpmiSendReceive(
                new GetDiskInfoRequest(channel, disk), typeof(GetDiskInfoResponse));


            DiskInformation response = new DiskInformation(jbod.CompletionCode);

            if (jbod.CompletionCode == 0)
            {
                response.SetParamaters(jbod.Unit, jbod.Multiplier, jbod.Reading);
            }

            return response;

        }

        #endregion

        #region OEM

        /// <summary>
        /// Gets Processor Information
        /// </summary>
        public virtual ProcessorInfo GetProcessorInfo(byte processor)
        {
            GetProcessorInfoResponse procInfo = (GetProcessorInfoResponse)this.IpmiSendReceive(
                new GetProcessorInfoRequest(processor), typeof(GetProcessorInfoResponse));


            ProcessorInfo response = new ProcessorInfo(procInfo.CompletionCode);

            if (procInfo.CompletionCode == 0)
            {
                response.SetParamaters(procInfo.Frequency, procInfo.ProcessorType,
                    procInfo.ProcessorState);
            }

            return response;

        }

        /// <summary>
        /// Gets Memory Information
        /// </summary>
        public virtual MemoryInfo GetMemoryInfo(byte dimm)
        {
            GetMemoryInfoResponse memInfo = (GetMemoryInfoResponse)this.IpmiSendReceive(
                new GetMemoryInfoRequest(dimm), typeof(GetMemoryInfoResponse));

            MemoryInfo response = new MemoryInfo(memInfo.CompletionCode);

            if (memInfo.CompletionCode == 0)
            {
                response.SetParamaters(memInfo.MemorySpeed, memInfo.MemorySize, memInfo.RunningSpeed,
                    memInfo.MemoryType, memInfo.Voltage, memInfo.Status);

            }

            return response;

        }

        /// <summary>
        /// Gets Memory Information
        /// </summary>
        public virtual MemoryIndex GetMemoryIndex()
        {
            GetMemoryIndexResponse memIndex = (GetMemoryIndexResponse)this.IpmiSendReceive(
                new GetMemoryIndexRequest(), typeof(GetMemoryIndexResponse));

            MemoryIndex response = new MemoryIndex(memIndex.CompletionCode);

            if (memIndex.CompletionCode == 0)
            {
                response.SetParamaters(memIndex.SlotCount, memIndex.Presence);
            }

            return response;

        }

        /// <summary>
        /// Gets PCie Information
        /// </summary>
        public virtual PCIeInfo GetPCIeInfo(byte device)
        {
            GetPCIeInfoResponse pcieInfo = (GetPCIeInfoResponse)this.IpmiSendReceive(
                new GetPCIeInfoRequest(device), typeof(GetPCIeInfoResponse));

            PCIeInfo response = new PCIeInfo(pcieInfo.CompletionCode);
            response.SlotIndex = device;

            if (pcieInfo.CompletionCode == 0)
            {
                if (pcieInfo.VendorId == 65535 && pcieInfo.SystemId == 65535)
                {
                    response.SetParamaters(PCIeState.NotPresent, 0, 0,
                        0, 0);
                }
                else
                {
                    response.SetParamaters(PCIeState.Present, pcieInfo.VendorId, pcieInfo.DeviceId,
                    pcieInfo.SystemId, pcieInfo.SubSystemId);
                }
            }

            return response;

        }

        /// <summary>
        /// Gets Nic Information
        /// </summary>
        public virtual NicInfo GetNicInfo(byte device)
        {
            // BIOS only supports nic from 0-3 (logical 1-4).
            if ((device >= 0) && (device <= 4))
            {
                // Ipmi OEM Nic Info uses zero based indexing. Nic interfaces 
                // logically use 1 based indexing.
                if (device != 0)
                    device = (byte)(device - 1);

                GetNicInfoResponse nicInfo = (GetNicInfoResponse)this.IpmiSendReceive(
                    new GetNicInfoRequest(device), typeof(GetNicInfoResponse));

                NicInfo response = new NicInfo(nicInfo.CompletionCode);
                response.DeviceId = (int)(device+1); // add 1 for Nic Number.

                // if success attempt to parse the mac address
                if (nicInfo.CompletionCode == 0)
                {
                    response.SetParamaters(nicInfo.HardwareAddress);
                }

                return response;
            }
            else
            {
                // index out of range.
                return new NicInfo(0xC9);
            }
        }

        /// <summary>
        /// Enables the output of KCS and Serial command trace debug messages in the BMC diagnostic debug console
        /// </summary>
        public virtual bool BmcDebugEnable(BmcDebugProcess process, bool enable)
        {
            BmcDebugResponse response = 
                (BmcDebugResponse)this.IpmiSendReceive(
                    new BmcDebugRequest(process, enable), typeof(BmcDebugResponse));

            if (response.CompletionCode == 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        #endregion
    }
}
