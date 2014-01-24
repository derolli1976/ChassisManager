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
using System.Text;

namespace Microsoft.GFS.WCS.ChassisManager
{
    class SerialConsoleMetadata
    {
        private int portId;
        private Object lockObject = new Object();
        private string sessionToken;
        private DateTime lastActivity;
        private int clientInactivityTimeoutInSecs;

        internal SerialConsoleMetadata(int id, string token, DateTime time)
        {
            lock (lockObject)
            {
                portId = id;
                sessionToken = token;
                lastActivity = time;
                clientInactivityTimeoutInSecs = ConfigLoaded.SerialPortConsoleClientSessionInactivityTimeoutInSecs;
            }
        }

        internal bool ResetMetadata()
        {
            lock (lockObject)
            {
                portId = ConfigLoaded.InactiveSerialPortId;
                sessionToken = ConfigLoaded.InactiveSerialPortSessionToken;
                lastActivity = DateTime.MinValue;
                clientInactivityTimeoutInSecs = ConfigLoaded.SerialPortConsoleClientSessionInactivityTimeoutInSecs;
                return true;
            }
        }

        internal bool SetSecretMetadataIfInactive(DateTime bound)
        {
            lock (lockObject)
            {
                if (sessionToken == ConfigLoaded.InactiveSerialPortSessionToken)
                    return false;

                if (DateTime.Compare(lastActivity, bound) < 0)
                {
                    sessionToken = ConfigLoaded.SecretSerialPortSessionToken; // Secret code for handling serialization between timer reset and other serial session APIs
                    lastActivity = DateTime.MinValue;
                    return true;
                }
                else
                    return false;
            }
        }

        internal CompletionCode CompareAndSwapMetadata(string currToken, string newToken, DateTime newActivityTime, int inactivityTimeoutInSecs)
        {
            lock (lockObject)
            {
                if (sessionToken == currToken)
                {
                    lastActivity = newActivityTime;
                    sessionToken = newToken;
                    clientInactivityTimeoutInSecs = inactivityTimeoutInSecs;
                    return CompletionCode.Success;
                }
                else
                    return CompletionCode.UnspecifiedError;
            }
        }

        internal CompletionCode CompareAndSwapMetadata(string currToken, string newToken, DateTime newActivityTime)
        {
            lock (lockObject)
            {
                if (sessionToken == currToken)
                {
                    lastActivity = newActivityTime;
                    sessionToken = newToken;
                    return CompletionCode.Success;
                }
                else
                    return CompletionCode.UnspecifiedError;
            }
        }

        internal CompletionCode CompareAndSwapMetadata(string currToken, string newToken)
        {
            lock (lockObject)
            {
                if (sessionToken == currToken)
                {
                    sessionToken = newToken;
                    return CompletionCode.Success;
                }
                else
                    return CompletionCode.UnspecifiedError;
            }
        }

        // Function to be called by the getbladerequirement monitoring thread if chassisManagerSafeState is true
        internal void SerialPortConsoleInactivityCheck(int portID)
        {
            // Negative values in clientInactivityTimeoutIn100ms indicate infinite timeout - return immediately.
            if (clientInactivityTimeoutInSecs < 0)
            {
                Tracer.WriteError("Infinite client session inactivity timeout set for COM{0}", portId);
                return;
            }

            // ConfigLoaded.SerialPortConsoleClientSessionInactivityTimeout has 100ms scaling - therefore divide by 10 to get seconds
            TimeSpan span = new TimeSpan(0, 0, 0, ConfigLoaded.SerialPortConsoleClientSessionInactivityTimeoutInSecs);

            if (clientInactivityTimeoutInSecs > 0)
            {
                span = new TimeSpan(0, 0, 0, clientInactivityTimeoutInSecs);
            }
            DateTime currTime = DateTime.Now;
            DateTime lastActivityBound = currTime.Subtract(span);

            if (CompareAndSwapMetadata(ConfigLoaded.InactiveSerialPortSessionToken, ConfigLoaded.InactiveSerialPortSessionToken) == CompletionCode.Success)
            {
                return;
            }

            // Check if inactive since 2 minutes back - if so write secret metadata
            if (!SetSecretMetadataIfInactive(lastActivityBound))
            {
                Tracer.WriteInfo("SerialConsolePortInactiveCheck({0}): Activity detected", this.portId);
                return; // Activity detected and hence not stopping the serial session
            }

            Contracts.ChassisResponse sessionResponse = new Contracts.ChassisResponse();
            sessionResponse = StopSerialPortConsole(portID, ConfigLoaded.SecretSerialPortSessionToken);
            if (sessionResponse.completionCode != Contracts.CompletionCode.Success)
                Tracer.WriteError("SerialConsolePortInactiveCheck({0}): Error StopBladeSerialSession failure", this.portId);
        }

        public Contracts.StartSerialResponse StartSerialPortConsole(int portID, int clientSessionInactivityTimeoutInSecs, int serialdeviceCommunicationTimeoutInMsecs)
        {
            Contracts.StartSerialResponse response = new Contracts.StartSerialResponse();
            response.completionCode = Contracts.CompletionCode.Failure;
            response.serialSessionToken = null;
            this.portId = portID;

            Tracer.WriteInfo("Received StartSerialPortConsole({0})", this.portId);

            int clientInactivityTimeoutInSecs = ConfigLoaded.SerialPortConsoleClientSessionInactivityTimeoutInSecs;
            if (clientSessionInactivityTimeoutInSecs < 0 || clientSessionInactivityTimeoutInSecs > 0)
            {
                clientInactivityTimeoutInSecs = clientSessionInactivityTimeoutInSecs;
            }

            byte[] randomNumber = new byte[8];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetNonZeroBytes(randomNumber);
            Tracer.WriteInfo("StartSerialPortConsole: Random string is " + BitConverter.ToString(randomNumber));

            // Initialize Serial Session MetaData including the client inactivity timeout - this function does this automically
            // This function acts as a serialization point - only one active thread can proceed beyond this
            if (this.CompareAndSwapMetadata(ConfigLoaded.InactiveSerialPortSessionToken, BitConverter.ToString(randomNumber), DateTime.Now, clientInactivityTimeoutInSecs) != CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.SerialSessionActive;
                return response;
            }

            SerialPortConsole currConsole = new SerialPortConsole((byte)portId);
            SerialStatusPacket serialStatus = new SerialStatusPacket();

            // TODO: Baudrate is hard coded here since it is not exposed to user in this V1.1 hotfix. 
            // We should later take the default value from app.config and include this as parameter in REST API
            BaudRate portBaudRate = BaudRate.Rate_9600;

            // Negative timeout values will be interpreted as infinite timeout by the device layer open serial port function
            int communicationDeviceTimeoutIn1ms = ConfigLoaded.SerialPortConsoleDeviceCommunicationTimeoutInMsecs;
            if (serialdeviceCommunicationTimeoutInMsecs < 0 || serialdeviceCommunicationTimeoutInMsecs > 0)
            {
                communicationDeviceTimeoutIn1ms = serialdeviceCommunicationTimeoutInMsecs;
                if (communicationDeviceTimeoutIn1ms > 0 && communicationDeviceTimeoutIn1ms < Constants.MinimumSerialDeviceTimeoutInMsecs)
                {
                    communicationDeviceTimeoutIn1ms = Constants.MinimumSerialDeviceTimeoutInMsecs;
                }
            }

            serialStatus = currConsole.openSerialPortConsole(communicationDeviceTimeoutIn1ms, portBaudRate);
            Tracer.WriteInfo("After calling comm dev open serial port");
            if (serialStatus.completionCode != CompletionCode.Success)
            {
                Tracer.WriteError("Error in Open Serial Port ({0})", portId);
                if (!this.ResetMetadata())
                {
                    Tracer.WriteError("StartSerialPortConsole Error: Unable to reset metadata");
                }
                return response;
            }

            response.completionCode = Contracts.CompletionCode.Success;
            response.serialSessionToken = BitConverter.ToString(randomNumber);
            return response;
        }

        public Contracts.ChassisResponse SendSerialPortData(int portID, string sessionToken, byte[] data)
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Failure;
            this.portId = portID;

            Tracer.WriteInfo("Received SendSerialPortConsoleData({0})", this.portId);

            // If there is NOT an already existing serial session (indicated by an invalid sessionToken), return failure with appropriate completion code
            if (CompareAndSwapMetadata(ConfigLoaded.InactiveSerialPortSessionToken, ConfigLoaded.InactiveSerialPortSessionToken) == CompletionCode.Success)
            {
                Tracer.WriteError("SendSerialPortData({0}): Send failed because of no active session.", portID);
                response.completionCode = Contracts.CompletionCode.NoActiveSerialSession;
                return response;
            }

            // If this bladeid currently holds the serial session, update the timestamp else return failure
            if (this.CompareAndSwapMetadata(sessionToken, sessionToken, DateTime.Now) != CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.SerialSessionActive;
                return response;
            }

            SerialPortConsole currConsole = new SerialPortConsole((byte)this.portId);
            SerialStatusPacket serialStatus = new SerialStatusPacket();
            if (data != null && data.Length != 0)
            {
                serialStatus = currConsole.sendSerialData(data);
                if (serialStatus.completionCode != CompletionCode.Success)
                {
                    Tracer.WriteError("SendBladeSerialData({0}): Error in SerialPortConsole.SendSerialData()", portId);
                    return response;
                }
            } // If data is null or if data has zero length, we are renewing activity lease and returning success to the user
            response.completionCode = Contracts.CompletionCode.Success;
            return response;
        }

        public Contracts.SerialDataResponse ReceiveSerialPortData(int portID, string sessionToken)
        {
            Contracts.SerialDataResponse response = new Contracts.SerialDataResponse();
            response.completionCode = Contracts.CompletionCode.Failure;
            this.portId = portID;

            Tracer.WriteInfo("Received ReceiveSerialPortConsoleData({0})", this.portId);

            // If there is NOT an already existing serial session (indicated by an invalid sessionToken), return failure with appropriate completion code
            if (CompareAndSwapMetadata(ConfigLoaded.InactiveSerialPortSessionToken, ConfigLoaded.InactiveSerialPortSessionToken) == CompletionCode.Success)
            {
                Tracer.WriteError("ReceiveSerialPortData({0}): Receive failed because of no active session.", portID);
                response.completionCode = Contracts.CompletionCode.NoActiveSerialSession;
                return response;
            }

            // If this bladeid currently holds the serial session, update the timestamp else return failure
            if (this.CompareAndSwapMetadata(sessionToken, sessionToken, DateTime.Now) != CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.SerialSessionActive;
                return response;
            }

            SerialPortConsole currConsole = new SerialPortConsole((byte)portId);
            SerialDataPacket serialData = new SerialDataPacket();
            serialData = currConsole.receiveSerialData();
            if (serialData.completionCode != CompletionCode.Success)
            {
                // Common-case: lots of timeouts if device do not have any data to send over serial
                if (serialData.completionCode == CompletionCode.Timeout)
                {
                    Tracer.WriteInfo("ReceiveSerialPortData({0}) Timeout in SerialConsolePort.receiveSerialData()", portId);
                    response.completionCode = Contracts.CompletionCode.Timeout;
                    return response;
                }
                Tracer.WriteError("ReceiveSerialPortData({0}) Unknown Error in SerialConsolePort.receiveSerialData()", portId);
                return response;
            }
            response.data = serialData.data;
            response.completionCode = Contracts.CompletionCode.Success;
            return response;
        }

        public Contracts.ChassisResponse StopSerialPortConsole(int portID, string sessionToken, bool forceKillExistingSession = false)
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Failure;
            this.portId = portID;

            Tracer.WriteInfo("Received StopSerialPortConsole({0}) with sessionToken({1}) and forcekill({2})", this.portId, sessionToken, forceKillExistingSession);
            Tracer.WriteUserLog("Received StopSerialPortConsole({0}) with sessionToken({1}) and forcekill({2})", this.portId, sessionToken, forceKillExistingSession);
            int currPort = this.portId;

            // If there is NOT an already existing serial session (indicated by an invalid sessionToken), return failure with appropriate completion code
            if (CompareAndSwapMetadata(ConfigLoaded.InactiveSerialPortSessionToken, ConfigLoaded.InactiveSerialPortSessionToken) == CompletionCode.Success)
            {
                Tracer.WriteError("StopSerialPortConsole({0}): Stop failed because of no active session.", portID);
                response.completionCode = Contracts.CompletionCode.NoActiveSerialSession;
                return response;
            }

            if (!forceKillExistingSession)
            {
                // If this do not currently hold the serial session, return failure
                if (CompareAndSwapMetadata(sessionToken, sessionToken) != CompletionCode.Success)
                {
                    response.completionCode = Contracts.CompletionCode.SerialSessionActive;
                    return response;
                } // else proceed further to stop the session
            }
            else
            {
                // else force kill the current session
            }

            // Ipmi command to indicate end of serial session
            if (!ResetMetadata())
            {
                Tracer.WriteError("StopSerialPortConsole({0}): Unable to reset metadata", this.portId);
            }
            SerialPortConsole currConsole = new SerialPortConsole((byte)currPort);
            SerialStatusPacket serialStatus = new SerialStatusPacket();
            serialStatus = currConsole.closeSerialPortConsole();
            if (serialStatus.completionCode != CompletionCode.Success)
            {
                Tracer.WriteError("StopSerialConsolePort({0}): Error in closeserialportconsole()", currPort);
                return response;
            }
            response.completionCode = Contracts.CompletionCode.Success;
            return response;
        }

    }
}
