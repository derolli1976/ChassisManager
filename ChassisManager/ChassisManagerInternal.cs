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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using Microsoft.GFS.WCS.ChassisManager.Ipmi;

namespace Microsoft.GFS.WCS.ChassisManager
{

    /// <summary>
    /// Class for Chassis Manager internal commands
    /// </summary>
    public class ChassisManagerInternal
    {
        /// <summary>
        /// Defining all class global variables
        /// </summary>
        private int MaxFanCount = ConfigLoaded.NumFans;     // number of fans in app.cofig
        private int MaxSledCount = ConfigLoaded.Population; // number of blades in app.cofig
        private int MaxPsuCount = ConfigLoaded.NumPsus;     // number of psu in app.config
        
        /// <summary>
        /// Define variables needed for timeperiod and internal chassis manager operations
        /// </summary>
        private uint GetTimePeriod; // in milliseconds
        private uint SetTimePeriod; // in milliseconds
        private byte MaxPWM;
        private byte InputSensor;
        private byte PrevFanPWM;

        /// <summary>
        /// Blade Requirement table contains the fan speeds that are set from querying the blade
        /// </summary>
        byte[] BladeRequirementTable;

        /// <summary>
        /// Chassis manager class constructor
        /// </summary>
        public ChassisManagerInternal()
        {
            // Initialize variables from ConfigLoader
            GetTimePeriod = (uint)ConfigLoaded.GetTimePeriod;
            SetTimePeriod = (uint)ConfigLoaded.SetTimePeriod;
            MaxPWM = (byte)ConfigLoaded.MaxPWM;
            InputSensor = (byte)ConfigLoaded.InputSensor;
            PrevFanPWM = (byte)ConfigLoaded.MinPWM;

            // Initialize Blade state tables
            BladeRequirementTable = new byte[MaxSledCount];

        }

        private void ChassisInternalInitialize()
        {
            //Initialize Psus
            PsuInitialize();

            if (WcsBladeFacade.Initialized > 0)
            {
                // Identify what kind of sleds these are
                for (byte loop = 1; loop <= MaxSledCount; loop++)
                {
                    byte devideId = WcsBladeFacade.clients[loop].DeviceId;
                    ChassisState.BladeTypeCache[devideId - 1] = (byte) WcsBladeFacade.clients[loop].BladeClassification;
                }
            }
        }

        /// <summary>
        /// Identifies the PSU vendor at each psu slot using the modelnumber API of the PsuBase class
        /// (Assumes all PSU vendors implement the MFR_MODEL Pmbus command)
        /// Based on the model number, we bind the Psu class object to the corresponding child (vendor) class object
        /// </summary>
        private void PsuInitialize()
        {
            for (uint psuIndex = 0; psuIndex < MaxPsuCount; psuIndex++)
            {
                PsuModelNumberPacket modelNumberPacket = new PsuModelNumberPacket();
                modelNumberPacket = ChassisState.Psu[psuIndex].GetPsuModel();
                string psuModelNumber = modelNumberPacket.ModelNumber;
                PsuModel model = ChassisState.ConvertPsuModelNumberToPsuModel(psuModelNumber);

                switch (model)
                {
                    case PsuModel.Delta:
                        ChassisState.Psu[psuIndex] = new DeltaPsu((byte)(psuIndex + 1));
                        Tracer.WriteInfo("Delta Psu identified at slot-{0}", psuIndex+1);
                        break;
                    case PsuModel.Emerson:
                        ChassisState.Psu[psuIndex] = new EmersonPsu((byte)(psuIndex + 1));
                        Tracer.WriteInfo("Emerson Psu identified at slot-{0}", psuIndex+1);
                        break;
                    default:
                        ChassisState.Psu[psuIndex] = new PsuBase((byte)(psuIndex + 1));
                        Tracer.WriteInfo("Unidentified PSU at slot-{0}", psuIndex+1);
                        break;
                }
            }
        }

        /// <summary>
        /// Initialize Chassis constants and configs
        /// </summary>
        internal byte Initialize()
        {
            Tracer.WriteInfo("Initializing state");

            byte status = (byte) CompletionCode.UnspecifiedError;
            ChassisState.Initialize();
                
            Tracer.WriteInfo("Initializing Communication Device");
            // Initializer lower layer communication device
            CompletionCode completionCode = CommunicationDevice.Init();

            if (CompletionCodeChecker.Failed(completionCode))
            {
                Tracer.WriteWarning("Initialization failed: {0}", completionCode);
                int loop = 0;

                // Retry 3 times before failing completely
                for (loop = 0; loop <  ConfigLoaded.MaxRetries; loop++)
                {
                    Tracer.WriteInfo("Initialization Retry: {0}", loop);

                    completionCode = CommunicationDevice.Init();
                    if (CompletionCodeChecker.Succeeded(completionCode))
                    {
                        break;
                    }
                }

                if (loop == ConfigLoaded.MaxRetries)
                {
                    Tracer.WriteError("Re-attempt at Communication Device Initialization failed with code: {0}", completionCode);
                    return status;
                }
            }
            if (CompletionCodeChecker.Succeeded(completionCode))
            {
                Tracer.WriteInfo("Communication Device Initialized");
                status = (byte)CompletionCode.Success;
            }

            // Get power status of enable pin for each blade and update blade state
            for (byte deviceId = 1; deviceId <= MaxSledCount; deviceId++)
            {
                CheckPowerEnableState(deviceId);
            }

            // Initialize Wcs Blade - TODO: This initialize should return some status
            WcsBladeFacade.Initialize();  // This method just creates IPMI Client Class for each blade.
            Tracer.WriteInfo("IPMI Facade Initialized, Number of blades initialized: {0}", WcsBladeFacade.Initialized);

            // check all client initialization status and update state
            Tracer.WriteInfo("Checking client status for {0} blades", MaxSledCount);
            for (byte deviceId = 1; deviceId <= MaxSledCount; deviceId++)
            {
                // TODO: How to check initialized status, now that this has become a function
                if (WcsBladeFacade.clients[deviceId].Initialize()) // This method logs on to an IPMI session.
                {
                    // If initialized is true, change state to probation
                    Tracer.WriteInfo("State Transition for Sled {0}: {1} -> Probation", deviceId,
                        ChassisState.GetStateName(deviceId));

                    ChassisState.SetBladeState(deviceId, (byte)BladeState.Probation);
                }
                else
                {
                    Tracer.WriteInfo("Blade not initialized: Blade ", +deviceId);
                }
            }

            Tracer.WriteInfo("Initializing Watchdog Timer");
            
            // Initialize WatchDog Timer
            ChassisState.Wdt.EnableWatchDogTimer();

            Tracer.WriteInfo("Watchdog timer initialized");

            // Initialize internal chassis manager tables
            this.ChassisInternalInitialize();

            return status;
        }

        /// <summary>
        /// Checks the power enable state of the sled and changes state accordingly
        /// </summary>
        /// <param name="deviceId"></param>
        private void CheckPowerEnableState(byte deviceId)
        {
            // Serialize power behavior
            lock (ChassisState._lock[deviceId - 1])
            {
                BladePowerStatePacket response = ChassisState.BladePower[deviceId - 1].GetBladePowerState();

                if (response.CompletionCode != CompletionCode.Success)
                {
                    Tracer.WriteInfo("Sled Power Enable state read failed (Completion Code: {0:X})", response.CompletionCode);
                }
                else
                {
                    if (response.BladePowerState == (byte)Contracts.PowerState.ON)
                    {
                        if (ChassisState.GetBladeState((byte)deviceId) == (byte)BladeState.HardPowerOff)
                        {
                            // Blade is powered on, move to initialization state
                            Tracer.WriteInfo("State Transition for Sled {0}: {1} -> Initialization", deviceId,
                                ChassisState.GetStateName(deviceId));

                            ChassisState.SetBladeState((byte)deviceId, (byte)BladeState.Initialization);
                        }
                    }
                    else if (response.BladePowerState == (byte)Contracts.PowerState.OFF)
                    {
                        if (ChassisState.GetBladeState((byte)deviceId) != (byte)BladeState.HardPowerOff)
                        {
                            // Blade is powered off, move to PowerOff state
                            Tracer.WriteInfo("State Transition for Sled {0}: {1} -> HardPowerOff", deviceId,
                                    ChassisState.GetStateName(deviceId));

                            ChassisState.SetBladeState((byte)deviceId, (byte)BladeState.HardPowerOff);
                        }
                    }
                    else
                    {
                        Tracer.WriteInfo("Getting out of else block");
                        // TODO: do we need to do anything for state that is NA
                    }
                }
            }
        }

        /// <summary>
        /// Reinitialize the sled and set chassis state
        /// </summary>
        private void ReInitialize(byte sledId)
        {
            // Serialize initialize and power behavior per sled
            lock (ChassisState._lock[sledId - 1])
            {
                ChassisState.FailCount[sledId - 1] = 0; // reset fail count since we are going to reinitialize the blade

                bool status = WcsBladeFacade.InitializeClient(sledId); // TODO: no completion code, only byte status returned

                if (status != true)
                {
                    // Initialization failed - move to fail state before retrying again
                    Tracer.WriteInfo("Reinitialization failed with code: {0} for Sled: {1}", status, sledId);
                    Tracer.WriteInfo("State Transition for Sled {0}: {1} -> Fail", sledId,
                        ChassisState.GetStateName(sledId));

                    ChassisState.SetBladeState((byte)sledId, (byte)BladeState.Fail);

                    // check power status to see if the blade was manually switched off or removed
                    BladePowerStatePacket response = ChassisState.BladePower[sledId - 1].GetBladePowerState();

                    // If the blade was turned off, set correct status / TODO: do we need this here?
                    if (response.BladePowerState == (byte)Contracts.PowerState.OFF)
                    {
                        Tracer.WriteInfo("SledId {0} is in hard power off state", sledId);
                        Tracer.WriteInfo("State Transition for Sled {0}: {1} -> HardPowerOff", sledId,
                            ChassisState.GetStateName(sledId));

                        ChassisState.SetBladeState(sledId, (byte)BladeState.HardPowerOff);

                    }
                }
                else
                {
                    // State change: I -> P 
                    Tracer.WriteInfo("Reinitialization of Sled: {0} succeeded with status {1}", sledId, status);
                    
                    Tracer.WriteInfo("State Transition for Sled {0}: {1} -> Probation", sledId,
                            ChassisState.GetStateName(sledId));
                    ChassisState.SetBladeState(sledId, (byte)BladeState.Probation);

                    // Initialize Blade Type (Type might have changed when Blades were reinserted)
                    if (WcsBladeFacade.clients.ContainsKey(sledId))
                    {
                        ChassisState.BladeTypeCache[sledId - 1] = (byte)WcsBladeFacade.clients[sledId].BladeClassification;
                    }
                    else
                    {
                        ChassisState.BladeTypeCache[sledId - 1] = (byte)BladeType.Unknown;
                    }
                }
            }
        }

        /// <summary>
        /// Function that gets all the fan speed requirements 
        /// from the Blade.  It also updates the balde state
        /// </summary>
        private void GetAllBladePwmRequirements()
        {
            // Rate is required to timestep over each individual Blade call   
            double rate = (double)GetTimePeriod / (double)MaxSledCount;
            double timeDiff = 0;

            for (byte blade = 1; blade <= MaxSledCount; blade++)
            {
                // Handle shutdown state
                if (ChassisState.ShutDown)
                {
                    return;
                }

                // default PWM setting
                byte PWM = (byte)ConfigLoaded.MinPWM;

                // Query blade type from IPMI layer
                ChassisState.BladeTypeCache[blade-1] = (byte)WcsBladeFacade.clients[blade].BladeClassification;

                // wait for rate limiter which includes the previous time difference for sensor get, and then issue get fan requirement

                double sleepTime = rate - timeDiff;

                if (sleepTime > rate)
                {
                    sleepTime = rate;
                }
                if (sleepTime > 0)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(sleepTime));
                }
                if (CommunicationDevice.IsSafeMode())
                {
                    // Do not perform any sensor reading - continue in the for loop
                    Tracer.WriteInfo("Monitoring thread: Safe Mode, Skipping sensor read");
                    continue;
                }
                Tracer.WriteInfo("GetBladeRequirement called at {0} for sledId {1} (state: {2})", DateTime.Now, blade,
                    ChassisState.GetStateName(blade));
                                
                // Check for the condition where known state is hardpoweroff, but someone plugged a new blade in
                if (ChassisState.GetBladeState(blade) == (byte)BladeState.HardPowerOff)
                {
                    ChassisState.PowerFailCount[blade - 1]++;
                    // TODO: identify if this period is sufficient to do this check
                    if (ChassisState.PowerFailCount[blade - 1] > (ConfigLoaded.MaxRetries * ConfigLoaded.Population))
                    {
                        CheckPowerEnableState(blade);
                        ChassisState.PowerFailCount[blade - 1] = 0;
                    }
                }
                // Log Start time 
                DateTime startTime = DateTime.Now;

                // If blade was in Fail state
                if (ChassisState.GetBladeState(blade) == (byte)BladeState.Fail)
                {
                    // If failed count is greater than a maximum value, we move it to Initialization state

                    if (ChassisState.FailCount[blade - 1] > ConfigLoaded.MaxFailCount)
                    {
                        // Move to Initialization state so that this sled could be reinitialized
                        Tracer.WriteInfo("State Transition for Sled {0}: {1} -> Initialization", blade,
                            ChassisState.GetStateName(blade));
                        ChassisState.SetBladeState(blade, (byte)BladeState.Initialization);
                    }
                    else
                    {
                        // Moving out of Fail state - First we use a light-weight get GUID to check whether the blade is there
                        DeviceGuid guid = WcsBladeFacade.GetSystemGuid(blade);
                        if (guid.CompletionCode == (byte)CompletionCode.Success)
                        {
                            Tracer.WriteInfo("GUID present for sled {0}, GUID: {1}", blade, guid.Guid.ToString());

                            // Change state to Probation
                            Tracer.WriteInfo("State Transition for Sled {0}: {1} -> Probation", blade,
                                ChassisState.GetStateName(blade));
                            ChassisState.SetBladeState(blade, (byte)BladeState.Probation);
                        }
                        else
                        {
                            Tracer.WriteInfo("Get System GUID returns a bad completion status: {0}", guid.CompletionCode);
                        }
                    }
                    // Increase time spent in Fail state everytime we are in this state
                    ChassisState.FailCount[blade - 1]++;
                }

                // Handles Initialization
                if (ChassisState.GetBladeState(blade) == (byte)BladeState.Initialization)
                {
                    this.ReInitialize(blade);
                }

                // Normal operation - possible states are probation or healthy
                if (ChassisState.GetBladeState(blade) == (byte)BladeState.Probation ||
                    ChassisState.GetBladeState(blade) == (byte)BladeState.Healthy)
                {
                    if (ChassisState.GetBladeType(blade) == (byte)BladeType.Jbod)
                    {
                        DeviceGuid guid = WcsBladeFacade.GetSystemGuid(blade);
                        if (guid.CompletionCode == (byte)CompletionCode.Success)
                        {
                            Tracer.WriteInfo("GUID present for jbod {0}, GUID: {1}", blade, guid.Guid.ToString());

                            // Change state to Probation
                            Tracer.WriteInfo("State Transition for jbod {0}: {1} -> Healthy", blade, ChassisState.GetStateName(blade));
                            ChassisState.SetBladeState(blade, (byte)BladeState.Healthy);
                        }
                        else
                        {
                            Tracer.WriteInfo("Get System GUID for jbod {0} failed with status {1}", blade, guid.CompletionCode);
                            // Set it to failed state, where we will retry guids and reinitialize if needed
                            Tracer.WriteInfo("State Transition for jbod {0}: {1} -> Fail", blade, ChassisState.GetStateName(blade));
                            ChassisState.SetBladeState(blade, (byte)BladeState.Fail);
                        }

                        // No need to check for sensor reading, just continue
                        continue;
                    }
                    // Call temperature reading list command
                    SensorReading Temps = WcsBladeFacade.GetSensorReading((byte)blade, (byte)ConfigLoaded.InputSensor, PriorityLevel.System);

                    if (Temps.CompletionCode != (byte)CompletionCode.Success)
                    {
                        Tracer.WriteWarning("SledId: {0} - getTempSensorReading failed with code {1:X}", blade, Temps.CompletionCode);

                        // Move to Fail state if no readings were obtained
                        Tracer.WriteInfo("State Transition for Sled {0}: {1} -> Fail", blade,
                            ChassisState.GetStateName(blade));

                        ChassisState.SetBladeState(blade, (byte)BladeState.Fail);
                    }
                    else
                    {
                        Tracer.WriteInfo("#### Sledid= " + blade + " Sensor id= " + ConfigLoaded.InputSensor + " Sensor reading= " +
                                        Temps.Reading + " Raw= " + Temps.RawReading + ", LowerNonCritical= " +
                                        ConfigLoaded.SensorLowThreshold + ", UpperNonCritical= " + ConfigLoaded.SensorHighThreshold);

                        // Handle state logic if needed
                        // Probation state should be shifted to Healthy since there was no timeout, & sensorread succeeded
                        if (ChassisState.GetBladeState(blade) == (byte)BladeState.Probation)
                        {
                            // Change state to healthy
                            Tracer.WriteInfo("State Transition for Sled {0}: {1} -> Healthy", blade,
                                ChassisState.GetStateName(blade));

                            ChassisState.SetBladeState(blade, (byte)BladeState.Healthy);
                            ChassisState.FailCount[blade - 1] = 0; // reset the fail count
                        }

                        PWM = GetPwmFromTemperature(Temps.Reading,
                                 ConfigLoaded.SensorLowThreshold,
                                 ConfigLoaded.SensorHighThreshold);

                        Tracer.WriteInfo("PWM value for Sensor {0} = {1}", InputSensor, PWM);
                    }
                }

                // write value into requirements table
                BladeRequirementTable[blade - 1] = PWM;

                // Log end time and capture time of execution for sensor get command
                DateTime endTime = DateTime.Now;
                timeDiff = endTime.Subtract(startTime).TotalMilliseconds; // convert time difference into milliseconds
            }
        }

        /// <summary>
        /// Thread function for running get blade requirement continuously
        /// </summary>
        internal void RunGetAllBladeRequirements()
        {
            while (true)
            {
                try
                {
                    while (true)
                    {
                        if (ChassisState.ShutDown)
                        {
                            return;
                        }
                        
                        // Step 1.  Get Blade Pwm requirements.
                        GetAllBladePwmRequirements();
                    }
                }
                catch (Exception ex)
                {
                    Tracer.WriteWarning("Chassis manager Get thread encountered an exception " + ex);
                }
            }
        }

        /// <summary>
        /// Converts inlet temperature to PWM value
        /// </summary>
        /// <param name="temperature"></param>
        /// <returns></returns>
        private byte GetPwmFromTemperature(double temperature, double lowThreshold, double highThreshold)
        {
            byte PWM = (byte) ConfigLoaded.MinPWM; // set to min as default

            if (lowThreshold >= highThreshold)
            {
                Tracer.WriteWarning("Low Threshold Temperature is greater or equal compared to high threshold");
                return PWM;
            }
            // PWM should never be higher or lower than the threshold.
            if (temperature < lowThreshold || temperature > highThreshold)
            {
                Tracer.WriteWarning("Temperature value {0} is out of range (lowThreshold {1} - highThreshold {2})", 
                    temperature, lowThreshold, highThreshold);
                return PWM;
            }

            // Find PWM corresponding to temperature value from low threshold and range value
            // Linear extrapolation requires current value, range for consideration and the low-threshold so that 
            // we can compute the PWM (as a value between 20-100)
            if (temperature <= highThreshold)
            {
                // These thresholds are read from threshold values in SDR record
                double range = highThreshold - lowThreshold; 
                double value = ConfigLoaded.MinPWM + ((temperature - lowThreshold) / range) * (MaxPWM - ConfigLoaded.MinPWM); 
                PWM = (byte)value;
                
                // Reset to MinPWM if calculated PWM is lower than MinPWM
                if (PWM < ConfigLoaded.MinPWM)
                {
                    PWM = (byte) ConfigLoaded.MinPWM;
                }

                // Set PWM to MaxPWM if calculated PWM is more than MaxPWM
                if (PWM > MaxPWM)
                {
                    PWM = MaxPWM;
                }
            }
            
            return PWM;
        }

        /// <summary>
        /// Gets maximum value from the Blade requirement table
        /// </summary>
        /// <returns></returns>
        internal byte GetMaxRequirement()
        {
            HelperFunction.MaxPwmRequirement = BladeRequirementTable.Max();
            return HelperFunction.MaxPwmRequirement;
        }

        /// <summary>
        /// Gets the current status of all fans and returns number of fans that is working as expected
        /// </summary>
        private int GetAllFanStatus()
        {
            int countStatus = 0;
            for (int numFans = 0; numFans < ChassisState.Fans.Length; numFans++)
            {
                if (ChassisState.Fans[numFans].GetFanStatus())
                {
                    countStatus++;
                }
            }
            return countStatus;
        }

        /// <summary>
        /// Sets the chassis fan speed 
        /// </summary>
        private void SetAllFanSpeeds()
        {
            // rate limiter for setting thread
            Thread.Sleep(TimeSpan.FromMilliseconds(SetTimePeriod));

            // Get max requirement from the bladerequirement table
            byte maxFanRequest = this.GetMaxRequirement();

            if(CommunicationDevice.IsSafeMode())
            {
                // Set fan speed to maximum
                Tracer.WriteInfo("Safe mode: Setting Fan speed to max");
                maxFanRequest = MaxPWM;
            }

            Tracer.WriteInfo("Max value got from Blade table = {0} (at {1})", maxFanRequest, DateTime.Now);

            // Check Fan Status and get number of working fans
            int numFansWorking = GetAllFanStatus();

            // Handle one fan failure
            if (numFansWorking == MaxFanCount - 1)
            {
                // Alert that one fan has failed!
                Tracer.WriteError("Fan failure, applying conversion");

                // Denote fan failure in chassis
                ChassisState.FanFailure = true;

                double conversion = (double) MaxFanCount / (double) (MaxFanCount - 1);
                maxFanRequest = (byte)(conversion * maxFanRequest);
            }
            else if (numFansWorking < MaxFanCount - 1)
            {
                // Set fan speed to max for fan failures more than N-1
                maxFanRequest = MaxPWM; // this is to set at max speed
                Tracer.WriteError("More than 1 Fans failed");

                // Denote that this is a fan failure in chassis
                ChassisState.FanFailure = true;
            }
            else
            {
                // All fans are working fine - check rear attention LED and if on, turn it off (by setting fanFailure to false)
                ChassisState.FanFailure = false;
            }

            // Do altitude correction
            maxFanRequest = (byte)((1 + ConfigLoaded.AltitudeCorrectionFactor * (int)(ConfigLoaded.Altitude / 1000)) * maxFanRequest);

            // Bound fan request to the maximum possible
            if (maxFanRequest > MaxPWM)
            {
                maxFanRequest = MaxPWM;
            }

            // Enable Ramp Down in smaller steps
            if (PrevFanPWM >= maxFanRequest + 2 * ConfigLoaded.StepPWM)
            {
                maxFanRequest = (byte)(PrevFanPWM - ConfigLoaded.StepPWM);
            }

            // Set fan speed for all fans - setting one fan device is enough, since setfanspeed is for all fan devices
            byte status = ChassisState.Fans[0].SetFanSpeed(maxFanRequest);

            // Trace the speed of fan
            Tracer.WriteInfo("Fan speed = " + ChassisState.Fans[0].GetFanSpeed());
            
            if (status != (byte)CompletionCode.Success)
            {
                Tracer.WriteWarning("SetFanSpeed failed with Completion Code: {0:X}", status);
            }
            else
            {
                Tracer.WriteInfo("Fan Speed set to {0}", maxFanRequest);
            }

            // Store current fan PWM in PrevFanPWM for next iteration
            PrevFanPWM = maxFanRequest;
        }

        /// <summary>
        /// Thread functions to run the set commands
        /// </summary>
        internal void RunSetDeviceCommands()
        {
           while (true)
            {
                try
                {
                    while (true)
                    {
                        if (ChassisState.ShutDown)
                        {
                            return;
                        }

                        if (ConfigLoaded.EnableFan)
                        {
                            // Step 1. Set All Fan Speeds if EnableFan is set.
                            SetAllFanSpeeds();
                        }

                        // Step 2. Check Serial Console Inactivity.
                        CheckBladeConsoleInactivity();
                        
                        // Step 3. Reset Watch Dog Timer
                        ResetWatchDog();

                        // Step 4. Get PSU Status
                        GetAllPsuStatus();

                        // Step 5. Set Attention Leds
                        SetAttentionLeds();
                    }
                }
                catch (Exception ex)
                {
                    Tracer.WriteWarning("Chassis Manager Set thread encountered an exception " + ex);
                }
            }
        }

        /// <summary>
        /// Resets Chassis Manager Fan -> High Watch dog timer
        /// </summary>
        private void ResetWatchDog()
        {
            // Reset WatchDogTimer every TimePeriod after setting fan speeds
            ChassisState.Wdt.ResetWatchDogTimer();

            Tracer.WriteInfo("WatchDogTimer was reset");
        }

        /// <summary>
        /// Performs Blade Serial Console Inativity check
        /// </summary>
        private void CheckBladeConsoleInactivity()
        {
            BladeSerialSessionMetadata.BladeSerialSessionInactivityCheck();
            for (int numPorts = 0; numPorts < ConfigLoaded.MaxSerialConsolePorts; numPorts++)
            {
                ChassisState.SerialConsolePortsMetadata[numPorts].SerialPortConsoleInactivityCheck(ChassisManagerUtil.GetSerialConsolePortIdFromIndex(numPorts));
            }
        }

        /// <summary>
        /// Set the Chassis Manager attention LED based on Fan and 
        /// PSU status in the Chassis Manager State Class
        /// </summary>
        private void SetAttentionLeds()
        {
            // Set Rear Attention LED
            if ((ConfigLoaded.EnableFan && ChassisState.FanFailure) || ChassisState.PsuFailure)
            {
                // Set rear attention LED On
                if (this.GetRearAttentionLedStatus() != (byte)LedStatus.On)
                {
                    this.SetRearAttentionLedStatus((byte)LedStatus.On);
                }
            }
            else
            {
                // Set rear attention LED Off
                if (this.GetRearAttentionLedStatus() != (byte)LedStatus.Off)
                {
                    this.SetRearAttentionLedStatus((byte)LedStatus.Off);
                }
            }
        }

        /// <summary>
        /// Gets the PSU status for all PSUs
        /// </summary>
        private void GetAllPsuStatus()
        {
            int countStatus = 0;
            for (int numPsus = 0; numPsus < MaxPsuCount; numPsus++)
            {
                try
                {
                    PsuStatusPacket psuStatusPacket = new PsuStatusPacket();
                    psuStatusPacket = ChassisState.Psu[numPsus].GetPsuStatus();
                    if (psuStatusPacket.CompletionCode != CompletionCode.Success)
                    {
                        Tracer.WriteWarning("PSU ({0}) get status request failed with return code {1}", numPsus + 1, psuStatusPacket.CompletionCode);
                    }
                    else
                    {
                        if (psuStatusPacket.PsuStatus == (byte)Contracts.PowerState.ON)
                        {
                            countStatus++;
                        }
                        else
                        {
                            Tracer.WriteWarning("PSU ({0}) PowerGood signal is negated - check trace log for fault information", numPsus + 1);
                            CompletionCode clearFaultCompletionCode = ChassisState.Psu[numPsus].SetPsuClearFaults();
                            if (clearFaultCompletionCode != CompletionCode.Success)
                            {
                                Tracer.WriteError("PsuStatus clear fault (invoked upon negated power good signal) command failed. Completion code({0})", clearFaultCompletionCode);
                            }
                            else
                            {
                                Tracer.WriteWarning("PsuStatus clear fault (invoked upon negated power good signal) command succeeded.");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Tracer.WriteError("PSU {0} status check failed with Exception: {1}", numPsus+1, e);
                }
            }

            // Identify whether there is a PSU failure or not
            if (countStatus != MaxPsuCount)
            {
                ChassisState.PsuFailure = true;
            }
            else
            {
                ChassisState.PsuFailure = false;
            }
        }

        /// <summary>
        /// Sets the rear attention LED to on or off based on ledState param (0 - off, 1 - on)
        /// </summary>
        /// <param name="ledState"></param>
        private byte SetRearAttentionLedStatus(byte ledState)
        {
            byte completionCode = (byte) CompletionCode.UnspecifiedError;

            if (ledState == (byte) LedStatus.On)
            {
                completionCode = ChassisState.AttentionLed.TurnLedOn();
                Tracer.WriteInfo("Internal setRearAttentionLEDStatus - LEDOn Return: {0:X}", completionCode);
            }
            if (ledState == (byte) LedStatus.Off)
            {
                completionCode = ChassisState.AttentionLed.TurnLedOff();
                Tracer.WriteInfo("Internal setRearAttentionLEDStatus - LEDOff Return: {0:X}", completionCode);
            }
            if (completionCode != (byte)CompletionCode.Success)
            {
                Tracer.WriteWarning("Internal setRearAttentionLEDStatus error - completion code: {0:X}", completionCode);
            }
            return completionCode;
        }

        /// <summary>
        /// Gets the current status of rear attention LED for the chassis
        /// </summary>
        /// <returns></returns>
        private byte GetRearAttentionLedStatus()
        {
            // Gets the LED status response
            Contracts.LedStatusResponse ledStatus = new Contracts.LedStatusResponse();
            ledStatus = ChassisState.AttentionLed.GetLedStatus();

            if (ledStatus.completionCode != Contracts.CompletionCode.Success)
            {
                Tracer.WriteWarning("Internal getRearAttentionLedStatus - getting status failed with Completion Code {0:X}", 
                    ledStatus.completionCode);
                return (byte)LedStatus.NA;
            }
            else
            {
                return (byte) ledStatus.ledState;
            }
        }
        
        // TODO (M2): get GUID every time period - log it for asset management
        // TODO (M2 code optimization): cache power status on/off state, system guid state (M2)
    }
}



        
