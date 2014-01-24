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

namespace Microsoft.GFS.WCS.ChassisManager
{
    using System;

    static class BladeSerialSessionMetadata
    {
        private static Object lockObject = new Object();
        private static int bladeId;
        private static string sessionToken;
        private static DateTime lastActivity;

        static BladeSerialSessionMetadata()
        {
            lock (lockObject)
            {
                bladeId = ConfigLoaded.InactiveBladePortId;
                sessionToken = ConfigLoaded.InactiveBladeSerialSessionToken;
                lastActivity = DateTime.MinValue;
            }
        }

        internal static bool ResetMetadata()
        {
            lock (lockObject)
            {
                bladeId = ConfigLoaded.InactiveBladePortId;
                sessionToken = ConfigLoaded.InactiveBladeSerialSessionToken;
                lastActivity = DateTime.MinValue;
                return true;
            }
        }
        internal static bool SetSecretMetadataIfInactive(DateTime bound)
        {
            lock (lockObject)
            {
                if (bladeId == ConfigLoaded.InactiveBladePortId && sessionToken == ConfigLoaded.InactiveBladeSerialSessionToken)
                    return true;

                if (DateTime.Compare(lastActivity, bound) < 0)
                {
                    bladeId = ConfigLoaded.SecretBladePortId; // Secret code for handling serialization between timer reset and other serial session APIs
                    sessionToken = ConfigLoaded.SecretBladeSerialSessionToken;
                    lastActivity = DateTime.MinValue;
                    return true;
                }
                else
                    return false;
            }
        }

        internal static CompletionCode CompareAndSwapMetadata(int currId, string currToken, int newId, string newToken, DateTime newActivityTime)
        {
            lock (lockObject)
            {
                // TODO: Check if null can be compared with null - use compare method of string
                if (bladeId == currId && sessionToken == currToken)
                {
                    lastActivity = newActivityTime;
                    bladeId = newId;
                    sessionToken = newToken;
                    return CompletionCode.Success;
                }
                else
                    return CompletionCode.UnspecifiedError;
            }
        }

        internal static CompletionCode CompareAndSwapMetadata(int currId, string currToken, int newId, string newToken)
        {
            lock (lockObject)
            {
                if (bladeId == currId && sessionToken == currToken)
                {
                    bladeId = newId;
                    sessionToken = newToken;
                    return CompletionCode.Success;
                }
                else
                    return CompletionCode.UnspecifiedError;
            }
        }

        // Function to be called by the getbladerequirement monitoring thread if chassisManagerSafeState is true
        internal static void BladeSerialSessionInactivityCheck()
        {
            // Timespan time parameters are in the order -- hours, minutes, and seconds.
            TimeSpan span = new TimeSpan(0, 0, ConfigLoaded.TimeoutBladeSerialSessionInSecs);
            DateTime currTime = DateTime.Now;
            DateTime lastActivityBound = currTime.Subtract(span);

            int currBladeId = bladeId;

            if ( CompareAndSwapMetadata(ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken,
                    ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken) ==
                CompletionCode.Success)
            {
                return;
            }

            // If there is an active session, if it has been inactive in the last 'x' minutes (2), then kill the session
            if (SetSecretMetadataIfInactive(lastActivityBound))
            {
                Contracts.ChassisResponse sessionResponse = new Contracts.ChassisResponse();
                sessionResponse = StopBladeSerialSession(ConfigLoaded.SecretBladePortId,
                    ConfigLoaded.SecretBladeSerialSessionToken, true);
                if (sessionResponse.completionCode != Contracts.CompletionCode.Success)
                {
                    Tracer.WriteError(
                        "BladeSerialSessionMetadata.BladeSerialSessionInactiveCheck: Error StopBladeSerialSession failure");
                }
            }
        }

        /// <summary>
        /// Checks for the existence of a blade serial console session.
        /// </summary>
        /// <returns><c>true</c> if a blade serial console session is in progress, <c>false</c> otherwise.</returns>
        internal static bool IsSerialConsoleSessionActive()
        {
            if (CompareAndSwapMetadata(ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken,
                ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken) != CompletionCode.Success) 
            {
                return true;
            } 
            else 
            {
                return false;
            }
        }

        // This function should be called before executing any service APIs
        // TODO: Add other checks that needs to be done before executing APIs - like check for chassis terminating etc
        internal static CompletionCode ApiGreenSignalling()
        {
            CompletionCode response = new CompletionCode();
            response = CompletionCode.UnspecifiedError;

            int currBladeId = bladeId;

            // Return Success if there is no active serial console session in progress
            if (!BladeSerialSessionMetadata.IsSerialConsoleSessionActive())
            {
                response = CompletionCode.Success;
                return response;
            }

            // If App.config parameter, KillSerialConsoleSession, is enabled,
            // terminate existing serial console session and proceed executing incoming commmands.
            if (ConfigLoaded.KillSerialConsoleSession)
            {
                response = CompletionCode.UnspecifiedError;

                Tracer.WriteInfo("Kill serial console session is enabled to allow incoming commands to succeed.");
                Contracts.ChassisResponse sessionResponse = new Contracts.ChassisResponse();
                sessionResponse = StopBladeSerialSession(ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken, true);
                if (sessionResponse.completionCode != Contracts.CompletionCode.Success)
                {
                    Tracer.WriteError("BladeSerialSessionMetadata.ApiGreenSignalling:Error stopserialsession failed");
                    return response;
                }

                response = CompletionCode.Success;
                return response;
            }
            // If App.config parameter, KillSerialConsoleSession, is not  enabled, maintain existing serial console session,
            // and ignore incoming commands, with explicit failure message output as warning in trace, since it will not
            // affect service correctness.
            else
            {
                    response = CompletionCode.SerialPortOtherErrors;
                    Tracer.WriteWarning("Serial console session in progress. Ignore incoming command with message:{0}", response);
                    
                    // We are returning non-Success completion code (represented as SerialPortOtherErrors)
                    // to indicate/trigger the failure of this incoming command.
                    return response;
            }
        }

        internal static Contracts.StartSerialResponse StartBladeSerialSession(int bladeId, int timeoutInSecs)
        {
            Contracts.StartSerialResponse response = new Contracts.StartSerialResponse();
            response.completionCode = Contracts.CompletionCode.Failure;
            response.serialSessionToken = null;
            Tracer.WriteInfo("BladeSerialSessionMetadata StartBladeSerialSession({0})", bladeId);

            // If there is an already existing Blade serial session (indicated by a valid bladeId and a valid sessionToken), return failure with appropriate completion code
            if (CompareAndSwapMetadata(ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken, ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken) != CompletionCode.Success)
            {
                Tracer.WriteError("StartBladeSerialSession({0}): Start failed because of already active session.", bladeId);
                response.completionCode = Contracts.CompletionCode.SerialSessionActive;
                return response;
            }

            // Ipmi command to indicate start of serial session
            // This has to be executed before Enabling comm. dev. safe mode otherwise this will fail
            Ipmi.SerialMuxSwitch sms = WcsBladeFacade.SetSerialMuxSwitch((byte)bladeId, Ipmi.MuxSwtich.SwitchSystem);

            // If set serial mux fails - reverse all operations
            if (sms.CompletionCode != (byte)CompletionCode.Success)
            {
                Tracer.WriteError("BladeSerialSessionMetadata.StartBladeSerialSession({0}): Ipmi SetSerialMuxSwitch Failed", bladeId);
                if (!CommunicationDevice.DisableSafeMode())
                {
                    Tracer.WriteError("BladeSerialSessionMetadata.StartBladeSerialSession({0}): Unable to disable comm.dev. safe mode", bladeId);
                }
                // Whenever we disable safe mode, make sure that no more serial session activity may be performed - by reseting metadata
                if (!BladeSerialSessionMetadata.ResetMetadata())
                {
                    Tracer.WriteError("BladeSerialSessionMetadata.StopBladeSerialSession({0}): Unable to reset metadata", bladeId);
                }
                return response;
            }

            byte[] randomNumber = new byte[8];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetNonZeroBytes(randomNumber);

            // Initialize Blade Serial Session MetaData - Init function does this automically
            // This function acts as a serialization point - only one active thread can proceed beyond this
            if (CompareAndSwapMetadata(ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken, bladeId, BitConverter.ToString(randomNumber), DateTime.Now) != CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.SerialSessionActive;
                return response;
            }

            response.completionCode = Contracts.CompletionCode.Success;
            response.serialSessionToken = BitConverter.ToString(randomNumber);
            // Initializing TimeoutBladeSerialSessionInSecs with user defined session timeout
            ConfigLoaded.TimeoutBladeSerialSessionInSecs = timeoutInSecs;
            return response;
        }

        internal static Contracts.ChassisResponse SendBladeSerialData(int bladeId, string sessionToken, byte[] data)
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Failure;
            Tracer.WriteInfo("BladeSerialSessionMetadata.SendBladeSerialData({0})", bladeId);

            // If there is NOT an already existing Blade serial session (indicated by a invalid bladeId and a invalid sessionToken), return failure with appropriate completion code
            if (CompareAndSwapMetadata(ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken, ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken) == CompletionCode.Success)
            {
                Tracer.WriteError("SendBladeSerialData({0}): Send failed because of no active session.", bladeId);
                response.completionCode = Contracts.CompletionCode.NoActiveSerialSession;
                return response;
            }

            // If this bladeid currently holds the serial session, update the timestamp else return failure
            if (BladeSerialSessionMetadata.CompareAndSwapMetadata(bladeId, sessionToken, bladeId, sessionToken, DateTime.Now) != CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.SerialSessionActive;
                return response;
            }

            BladeSerialSession currSession = new BladeSerialSession((byte)bladeId);
            SerialStatusPacket serialStatus = new SerialStatusPacket();
            serialStatus = currSession.sendSerialData(data);
            if (serialStatus.completionCode != CompletionCode.Success)
            {
                Tracer.WriteError("BladeSerialSessionMetadata.SendBladeSerialData({0}): Error in BladeSerialSession.sendSerialData()", bladeId);
                return response;
            }
            response.completionCode = Contracts.CompletionCode.Success;
            return response;
        }

        internal static Contracts.SerialDataResponse ReceiveBladeSerialData(int bladeId, string sessionToken)
        {
            Contracts.SerialDataResponse response = new Contracts.SerialDataResponse();
            response.completionCode = Contracts.CompletionCode.Failure;
            Tracer.WriteInfo("BladeSerialSessionMetadata.ReceiveBladeSerialData({0})", bladeId);

            // If there is NOT an already existing Blade serial session (indicated by a invalid bladeId and a invalid sessionToken), return failure with appropriate completion code
            if (CompareAndSwapMetadata(ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken, ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken) == CompletionCode.Success)
            {
                Tracer.WriteError("ReceiveBladeSerialData({0}): Receive failed because of no active session.", bladeId);
                response.completionCode = Contracts.CompletionCode.NoActiveSerialSession;
                return response;
            }

            // If this bladeid do not currently hold the serial session, return failure
            if (BladeSerialSessionMetadata.CompareAndSwapMetadata(bladeId, sessionToken, bladeId, sessionToken) != CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.SerialSessionActive;
                return response;
            }

            BladeSerialSession currSession = new BladeSerialSession((byte)bladeId);
            SerialDataPacket serialData = new SerialDataPacket();
            serialData = currSession.receiveSerialData();
            if (serialData.completionCode != CompletionCode.Success)
            {
                // Common-case: lots of timeouts if device do not have any data to send over serial
                if (serialData.completionCode == CompletionCode.Timeout)
                {
                    Tracer.WriteInfo("BladeSerialSessionMetadata.ReceiveBladeSerialData({0}) Timeout in BladeSerialSession.receiveSerialData()", bladeId);
                    response.completionCode = Contracts.CompletionCode.Timeout;
                    return response;
                }
                Tracer.WriteError("BladeSerialSessionMetadata.ReceiveBladeSerialData({0}) Error in BladeSerialSession.receiveSerialData()", bladeId);
                return response;
            }

            response.data = serialData.data;
            response.completionCode = Contracts.CompletionCode.Success;
            return response;
        }

        internal static Contracts.ChassisResponse StopBladeSerialSession(int bladeId, string sessionToken, bool forceKill=false)
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Failure;
            Tracer.WriteInfo("BladeSerialSessionMetadata.Received Stopbladeserialsession({0})", bladeId);

            // If there is NOT an already existing Blade serial session (indicated by a invalid bladeId and a invalid sessionToken), return failure with appropriate completion code
            if (CompareAndSwapMetadata(ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken,
                    ConfigLoaded.InactiveBladePortId, ConfigLoaded.InactiveBladeSerialSessionToken) == CompletionCode.Success)
            {
                Tracer.WriteError("StopBladeSerialSession({0}): Stop failed because of no active session.", bladeId);
                response.completionCode = Contracts.CompletionCode.NoActiveSerialSession;
                return response;
            }

            // Normal scenario when forcekill option is not true.. check for bladeid correctness and if it currently holds the serial session
            if (!forceKill)
            {
                if (ChassisManagerUtil.CheckBladeId((byte) bladeId) != (byte) CompletionCode.Success)
                {
                    response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                    return response;
                }

                // If this bladeid do not currently hold the serial session, return failure
                if (CompareAndSwapMetadata(bladeId, sessionToken, bladeId, sessionToken) != CompletionCode.Success)
                {
                    response.completionCode = Contracts.CompletionCode.SerialSessionActive;
                    return response;
                }
            }

            // Communication device has to come out of safe mode - should allow IPMI commands to go to the BMC
            if (!CommunicationDevice.DisableSafeMode())
            {
                Tracer.WriteError(
                    "BladeSerialSessionMetadata.StopBladeSerialSession({0}): CommunicationDevice.DisableSafeMode Failed",
                    bladeId);
            }

            Ipmi.SerialMuxSwitch rms; 
            // If forcekill parameter is false, then use the bladeid that is passed by the user
            if (!forceKill)
            {
                rms = WcsBladeFacade.ResetSerialMux((byte) bladeId);
            }
            // If forcekill parameter is true, then use the bladeid that currently holds the serial session
            else
            {
                rms = WcsBladeFacade.ResetSerialMux((byte)BladeSerialSessionMetadata.bladeId);
            }

            if (rms.CompletionCode != (byte)CompletionCode.Success)
            {
                Tracer.WriteError("BladeSerialSessionMetadata.StopBladeSerialSession({0}): Ipmi ReSetSerialMuxSwitch Failed", bladeId);
            }

            if (!BladeSerialSessionMetadata.ResetMetadata())
            {
                Tracer.WriteError("BladeSerialSessionMetadata.StopBladeSerialSession: Unable to reset metadata");
            }
            response.completionCode = Contracts.CompletionCode.Success;
            // Resetting TimeoutBladeSerialSessionInSecs to 0 to account for default or user provided session timeout value
            ConfigLoaded.TimeoutBladeSerialSessionInSecs = 0;
            return response;
        }
    }
}
