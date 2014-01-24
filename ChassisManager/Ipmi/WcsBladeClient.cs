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
    using System.Reflection;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using Microsoft.GFS.WCS.ChassisManager.Ipmi;

    internal sealed class WcsBladeClient : IpmiClientExtended
    {
        # region Private Variables

        // Serial Ipmi Packet Start Byte
        private const byte _startByte = 0xA0;
        // Serial Ipmi Packet Stop byte
        private const byte _stopByte = 0xA5;
        // Serial Ipmi Packet data escape
        private const byte _dataEscape = 0xAA;

        #region Lock Objects

        /// <summary>
        /// locker object for accessing global resources.
        /// </summary>
        private object _reqSeqLock = new object();

        /// <summary>
        /// Locker object for modifying the client state
        /// Client state is used for debug status of the client
        /// at any given time.
        /// </summary>
        private object _stateLock = new object();

        /// <summary>
        /// Locker object for modifying the client cache
        /// </summary>
        private object _cacheLock = new object();

        /// <summary>
        /// Counter for Serial Timeout.  Reset on success.
        /// </summary>
        private uint _errCnt = 0;

        #endregion

        // default client connection status.
        private IpmiClientState _status = IpmiClientState.Disconnected;
       
        // initialize session Id
        private uint _sessionId = 0;

        /// <summary>
        /// Double byte charactors to replace ipmi escape charactors.
        /// See IPMI 2.0: 14.4.1 - Basic Mode Packet Framing
        /// See IPMI 2.0: 14.4.2 - Data Byte Escaping 
        /// </summary>
        private readonly List<EscapeCharactor> _escChars = new List<EscapeCharactor>(5)
        {
            new EscapeCharactor(0xAA, new byte[2]{0xAA, 0xBA}),
            new EscapeCharactor(0xA0, new byte[2]{0xAA, 0xB0}),
            new EscapeCharactor(0xA5, new byte[2]{0xAA, 0xB5}),
            new EscapeCharactor(0xA6, new byte[2]{0xAA, 0xB6}),
            new EscapeCharactor(0x1B, new byte[2]{0xAA, 0x3B})
        };

        // blade device Id
        private readonly byte _deviceId;

        // blade type 
        private BladeType _bladeType;

        // blade Sensor Data Record
        private ConcurrentDictionary<byte, SensorMetadataBase> _sensorMetaData = new ConcurrentDictionary<byte, SensorMetadataBase>();

        // Sensor One metat Data Record
        private ConcurrentDictionary<byte, FullSensorRecord> _pwmSensorMetaData = new ConcurrentDictionary<byte, FullSensorRecord>();

        #endregion

        # region Internal Variables
        
        /// <summary>
        /// blade Device Id
        /// </summary>
        internal byte DeviceId
        {
            get {
                    return this._deviceId;
                }
        }

        /// <summary>
        /// Blade Device Type
        /// </summary>
        internal BladeType BladeClassification
        {
            get
            {
                lock (_cacheLock)
                {
                    return this._bladeType;  
                }
             }
        }

        /// <summary>
        /// Ipmi consecutive communication error counter.
        /// </summary>
        internal uint CommError
        {
            get
            {
                lock (_cacheLock)
                {
                    return this._errCnt;
                }
            }
            private set
            {
                lock (_cacheLock)
                {
                    // acceptable range 0 - 2,147,483,647 (integer)
                    { this._errCnt = (value > 2147483647 ? 0 : value); }
                }
            }
        }

        /// <summary>
        /// Sensor Data Record
        /// </summary>
        internal ConcurrentDictionary<byte, SensorMetadataBase> SensorDataRecords
        {
            get { return this._sensorMetaData; }
        }

        /// <summary>
        /// Sensor Data Record
        /// </summary>
        private ConcurrentDictionary<byte, FullSensorRecord> PwmSensorMetaData
        {
            get { return this._pwmSensorMetaData; }
        }

        #endregion

        #region Ipmi Escape Framing

        /// <summary>
        /// Replace serial framing charactors on outbound payload with 
        /// substatute byte sequence: 
        ///         IPMI 2.0: 14.4.1 - Basic Mode Packet Framing
        ///         IPMI 2.0: 14.4.2 - Data Byte Escaping 
        /// </summary>
        public byte[] ReplaceFrameChars(byte[] payload)
        {
            // initialize dictionary for tracking positions of frame charactors
            SortedDictionary<int, EscapeCharactor> instances = new SortedDictionary<int, EscapeCharactor>();

            // generate list for tracking positions
            List<int> positions = new List<int>();

            // array resize increase
            int len = 0;

            // array indexer
            int index = 0;

            // array offset
            int offset = 0;
            
            // array incrementer
            int increase = 0;

            // iterate the frame charactors
            foreach (EscapeCharactor esc in _escChars)
            {
                // use IndexOf to detect a single occurance of the frame charactor
                // if a single instance is detected, search for more.
                if (IpmiSharedFunc.GetInstance(payload, esc.Frame) >= 0)
                {
                    // list all positions of the frame char
                    positions = GetFramePositions(payload, esc.Frame);

                    // for each position found, added it to the dictionary
                    // for tracking the bit.
                    foreach (int occurance in positions)
                    {
                        instances.Add(occurance, esc);    
                    }
                }
            }

            // if instances of frame charactors have been found
            // enter into the replacement method.
            if (instances.Count > 0)
            {
                len = (payload.Length + instances.Count);
                byte[] newPayload = new byte[len];
                {
                    // reset indexers
                    index = 0; offset = 0; increase = 0;
                    foreach (KeyValuePair<int, EscapeCharactor> esc in instances)
                    {
                        // copy in the original byte array, up to the first frame char
                        Buffer.BlockCopy(payload, index, newPayload, offset, (esc.Key - index));

                        // set offset + byte offset 
                        // every pass adds 1 byte to increase
                        offset = esc.Key + increase;
                        
                        // copy in the replacement escape charactor array.
                        Buffer.BlockCopy(esc.Value.Replace, 0, newPayload, offset, esc.Value.Replace.Length);

                        // add 1 byte to the offset, as byte 1 
                        // in esc.Value.replace always overwrites,
                        // payload[index]
                        increase++;

                        // offset + 2 byte offset
                        offset = (esc.Key + increase +1);

                        // add 1 to index, to index past itself.
                        index = (esc.Key +1);
                    }
                    // copy remaining bytes into the new array
                    Buffer.BlockCopy(payload, index, newPayload, offset, (payload.Length - index));
                }

                // copy the remaining payload bytes.
                payload = newPayload;
            }

            return payload;
        }

        /// <summary>
        /// Replace serial escape charactors on received payload with 
        /// substatute byte sequence: 
        ///         IPMI 2.0: 14.4.1 - Basic Mode Packet Framing
        ///         IPMI 2.0: 14.4.2 - Data Byte Escaping 
        /// </summary>
        public byte[] ReplaceEscapeChars(byte[] payload)
        {
            // initialize dictionary for tracking positions of escape charactors
            SortedDictionary<int, EscapeCharactor> instances = new SortedDictionary<int, EscapeCharactor>();

            // generate list for tracking positions
            List<int> positions = new List<int>();

            // array resize increase
            int len = 0;

            // array indexer
            int index = 0;

            // array offset
            int offset = 0;

            // iterate the escape charactors
            foreach (EscapeCharactor esc in _escChars)
            {
                // use IndexOf to detect a single occurance of the escape charactor
                // if a single instance is detected, search for more.
                if (IpmiSharedFunc.GetInstance(payload, esc.Replace) >= 0)
                {
                    // list all positions of the escape char
                    positions = GetEscapePositions(payload, esc.Replace);

                    // for each position found, added it to the dictionary
                    // for tracking the bit.
                    foreach (int occurance in positions)
                    {
                        instances.Add(occurance, esc);
                    }
                }
            }

            // if instances of escape charactors have been found
            // enter into the replacement method.
            if (instances.Count > 0)
            {
                // lenght is payload minus the count of two byte escape sequences.
                len = (payload.Length - instances.Count);
                byte[] newPayload = new byte[len];
                {
                    // reset indexers
                    index = 0; offset = 0;
                    foreach (KeyValuePair<int, EscapeCharactor> esc in instances)
                    {
                        // copy in the original byte array, up to the first escape char
                        Buffer.BlockCopy(payload, index, newPayload, offset, (esc.Key - index));

                        // increment offset the size of bytes copied
                        offset += (esc.Key - index);

                        // increase the index based the 2 byte escape sequence
                        index = (esc.Key + 2);
                        
                        // replace escape charactors with frame charactor
                        newPayload[offset] = esc.Value.Frame;

                        // increase the offset for this new byte
                        offset++;
                    }

                    // copy remaining bytes into the new array
                    Buffer.BlockCopy(payload, index, newPayload, offset, (payload.Length - index));
                }

                // copy the remaining payload bytes.
                payload = newPayload;
            }

            return payload;
        }

        /// <summary>
        /// Detect escape charactors in payload
        /// </summary>
        /// <param name="payload">ipmi unframed payload</param>
        /// <param name="pattern">escape pattern</param>
        /// <returns>List of position integers</returns>
        private List<int> GetEscapePositions(byte[] payload, byte[] pattern)
        {
            List<int> indexes = new List<int>();

            // remove 1 from payload.lenght to avoid buffer overrun.
            for (int i = 0; i < (payload.Length -1); i++)
            {
                if (pattern[0] == payload[i] && pattern[1] == payload[i+1])
                {
                    indexes.Add(i);
                }
            }
            return indexes;
        }

        /// <summary>
        /// Detect escape charactors in payload
        /// </summary>
        /// <param name="payload">ipmi unframed payload</param>
        /// <param name="pattern">escape pattern</param>
        /// <returns>List of position integers</returns>
        private List<int> GetFramePositions(byte[] payload, byte pattern)
        {
            List<int> indexes = new List<int>();

            int index = 0, pos = 0;

            for (int i = 0; i < payload.Length; i++)
            {
                // returns -1 when index is not found
                pos = IpmiSharedFunc.GetInstance(payload, pattern, index);

                index = (pos + 1);
                
                if (pos >= 0)
                    indexes.Add(pos);

                if (pos == -1)
                    break;
            }
            return indexes;
        }

        /// <summary>
        /// Add Start & Stop Serial Framing Charactors.
        /// </summary>
        public void AddStartStopFrame(ref byte[] payload)
        {
            payload[0] = _startByte;
            payload[(payload.Length -1)] = _stopByte;
        }

        #endregion

        #region Close, LogOff & Dispose

        /// <summary>
        /// Closes the connection to the BMC device. This is the preferred method of closing any open 
        /// connection.
        /// </summary>
        public void Close()
        {
            this.Close(false);
        }

        /// <summary>
        /// Closes the connection to the BMC device. This is the preferred method of closing any open 
        /// connection.
        /// </summary>
        /// <param name="hardClose">
        /// true to close the socket without closing the IPMI session; otherwise false.
        /// </param>
        public void Close(bool hardClose)
        {
            if (hardClose == false)
            {
                this.LogOff();
            }

            this.SetClientState(IpmiClientState.Disconnected);
        }

        ~WcsBladeClient()
        {
            this.Close(true);
            
        }

        /// <summary>
        /// End an authenticated session with the BMC.
        /// </summary>
        public void LogOff()
        {
            uint session = this.GetSession();

            if (session != 0)
            {
                this.IpmiSendReceive(
                    new CloseSessionRequest(session),
                    typeof(CloseSessionResponse));
                SetSession(0);
            }
        }

        #endregion

        #region Connect & Logon

        /// <summary>
        /// Connects the client to the serial ipmi bmc on specified computer.
        /// </summary>
        /// <param name="hostName">Host computer to access via ipmi over serial.</param>
        public bool Initialize(bool guidChanged = false)
        {
            Tracer.WriteInfo("Initializing Blade {0}", DeviceId);

            if (guidChanged)
            {
                // clear full sensor meta data
                SensorDataRecords.Clear();

                // clear pwm sensor meta data
                PwmSensorMetaData.Clear();
            }

            // Attempt to Identify the blade. (Note: This command by default does not allow retry).
            ChannelAuthenticationCapabilities response = this.GetAuthenticationCapabilities(PrivilegeLevel.Administrator, PriorityLevel.System, false);

            if (response.CompletionCode == (byte)CompletionCode.Success)
            {
                // Auxilary data is used to identify the blade
                BladeType bladeType;

                if(Enum.IsDefined(typeof(BladeType), response.AuxiliaryData))
                {
                    bladeType = (BladeType)response.AuxiliaryData;
                }
                else
                {
                    bladeType = BladeType.Unknown;
                }
               
                // indicates whether device has guid
                bool hasGuid = false;
                
                // Set the blade Type.                
                lock (_cacheLock)
                {
                    _bladeType = bladeType;
                }

                // IEB blade = 0x06, Storage blade = 0x05, Compute blade = 0x04.
                switch (bladeType)
                {
                    case BladeType.Server:
                        {
                            this.SetClientState(IpmiClientState.AuthenticatingChallenge);

                            // Step 2: Get GUID.  This command can be executed out of session
                            //         by default this command does not allow timeout retries.
                            DeviceGuid deviceGuid = this.SysPriGetSystemGuid();
                            if (deviceGuid.CompletionCode == (byte)CompletionCode.Success)
                            {
                                hasGuid = true;

                                // TODO: Consider Removing Logon Code
                                // Step 1: Log On
                                LogOn();
                            }

                            // Step 3: Signal Blade as initialized
                            if (hasGuid)
                            {
                                // client state set in logon method
                                // signal initialization was completed.
                                Tracer.WriteInfo(this.DeviceId.ToString() + " Initialized = true");

                                return true;
                            }
                            else
                            {
                                Tracer.WriteInfo(this.DeviceId.ToString() + " Initialized = false");

                                return false;
                            }
                        }
                    case BladeType.Jbod: // Storage Blade.
                        // Get the blade guid
                        {
                            this.SetClientState(IpmiClientState.AuthenticatingChallenge);

                            // This command does not allow time-out logon retry.
                            DeviceGuid deviceGuid = this.SysPriGetSystemGuid();
                            if (deviceGuid.CompletionCode == (byte)CompletionCode.Success)
                            {
                                hasGuid = true;
                            }

                            if (hasGuid)
                            {
                                this.SetClientState(IpmiClientState.Authenticated);

                                Tracer.WriteInfo(this.DeviceId.ToString() + " Initialized = true");

                                return true;
                            }
                            else
                            {
                                Tracer.WriteInfo(this.DeviceId.ToString() + " Initialized = false, could not get Guid");

                                return false;
                            }
                        }
                    default:
                        Tracer.WriteError("Unknown Device Type, ChannelAuthenticationCapabilities failed for device Id: {0} Method: {1} Ipmi CompletionCode {2}", DeviceId.ToString(),
                            typeof(ChannelAuthenticationCapabilities).ToString(), IpmiSharedFunc.ByteToHexString(response.CompletionCode));
                        return false;
                }
            }
            else
            {
                // signal initializaiton failed
                this.SetClientState(IpmiClientState.Invalid);
                Tracer.WriteError("Device Initialization failed for device Id: {0} Method: {1} Ipmi CompletionCode {2}", DeviceId.ToString(), 
                    typeof(ChannelAuthenticationCapabilities).ToString(), IpmiSharedFunc.ByteToHexString(response.CompletionCode));

                return false;
            }
        }

        /// <summary>
        /// Start an authenticated session with the BMC.
        /// </summary>
        public void LogOn(PriorityLevel priority = PriorityLevel.User)
        {
            try
            {
                if (base.IpmiUserId == null || base.IpmiPassword == null)
                {
                    Tracer.WriteError(new NullReferenceException("IpmiUserId & IpmiPassword"));
                    // set to empty string
                    base.IpmiUserId = string.Empty;
                    // set to empty password
                    base.IpmiPassword = string.Empty;
                }


                // set the Ipmi Privilege level
                base.IpmiPrivilegeLevel = PrivilegeLevel.Administrator;

                // session challenge. This command does not allow retry.
                GetSessionChallengeResponse response =
                    (GetSessionChallengeResponse)this.IpmiSendReceive(
                        new GetSessionChallengeRequest(AuthenticationType.Straight, base.IpmiUserId),
                        typeof(GetSessionChallengeResponse), priority, false);

                if (response.CompletionCode == (byte)CompletionCode.Success)
                {

                    // set client state to session challenge
                    this.SetClientState(IpmiClientState.SessionChallenge);

                    // ipmi authentication code / user password logon.
                    byte[] authCode = IpmiSharedFunc.AuthCodeSingleSession(response.TemporarySessionId,
                                                                                response.ChallengeStringData,
                                                                                AuthenticationType.Straight,
                                                                                base.IpmiPassword);

                    // Session Activation.See: IPMI Table   22-21, Activate Session Command
                    // Note: This command does not allow re-try
                    ActivateSessionResponse activateResponse =
                        (ActivateSessionResponse)this.IpmiSendReceive(
                            new ActivateSessionRequest(AuthenticationType.Straight, base.IpmiPrivilegeLevel, authCode, 1),
                            typeof(ActivateSessionResponse), priority, false);

                    if (activateResponse.CompletionCode == (byte)CompletionCode.Success)
                    {
                        // set the session id for the remainder of the session
                        this.SetSession(activateResponse.SessionId);

                        // initialize the ipmi message sequence number to zero
                        ResetReqSeq();

                        // set client state to authenticated. client state
                        // is used for socket and RMCP payload type control
                        this.SetClientState(IpmiClientState.Authenticated);

                        // set session privilege level. This command does not allow retry by default
                        SetSessionPrivilegeLevelResponse privilege =
                            (SetSessionPrivilegeLevelResponse)this.IpmiSendReceive(
                            new SetSessionPrivilegeLevelRequest(PrivilegeLevel.Administrator),
                            typeof(SetSessionPrivilegeLevelResponse), priority, false);

                    }
                    else
                    {
                        // Trace the info for the state.
                        Tracer.WriteInfo(activateResponse.GetType() + " Failed State: " + base.ClientState.ToString());
                    }

                }
                else
                {
                    // Trace the info for the state.
                    Tracer.WriteInfo(response.GetType() + " Failed State: " + base.ClientState.ToString());

                    // client failed to connect, session lost.
                    this.SetClientState(IpmiClientState.Disconnected);
                }

            }
            catch (Exception ex)
            {
                // Trace the info for the state.
                Tracer.WriteError(ex);
            }
        }

        #endregion

        #region Send/Receive

        /// <summary>
        /// Generics method IpmiSendReceive for easier use
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ipmiRequest"></param>
        /// <returns></returns>
        internal override T IpmiSendReceive<T>(IpmiRequest ipmiRequest)
        {
            return (T)this.IpmiSendReceive(ipmiRequest, typeof(T));
        }

        internal override IpmiResponse IpmiSendReceive(IpmiRequest ipmiRequest, Type responseType, bool allowRetry = true)
        {
            return this.IpmiSendReceive(ipmiRequest, responseType, PriorityLevel.User, allowRetry);
        }

        /// <summary>
        /// Send Receive Ipmi messages
        /// </summary>
        private IpmiResponse IpmiSendReceive(IpmiRequest ipmiRequest, Type responseType, PriorityLevel priority, bool allowRetry = true)
        {
            // Get the request sequence.  This should be incremented
            // for every request/response pair.
            byte reqSeq = GetReqSeq();

            // Serialize the IPMI request into bytes.
            byte[] ipmiRequestMessage = this.ReplaceFrameChars(ipmiRequest.GetBytes(IpmiTransport.Serial, reqSeq));

            // inject start/stop frame bytes.
            AddStartStopFrame(ref ipmiRequestMessage);
           
            Tracer.WriteInfo(ipmiRequest.GetType().ToString());
            Tracer.WriteInfo(IpmiSharedFunc.ByteArrayToHexString(ipmiRequestMessage));

            byte[] messageResponse = { };
            byte[] ipmiResponseMessage = { };
            byte completionCode = 0xFF; // Initialize as non-zero (0xff = Unspecified Error).

            // Send the ipmi mssage over serial.
            CommunicationDevice.SendReceive(priority, (byte)DeviceType.Server, _deviceId, ipmiRequestMessage, out messageResponse);

            Tracer.WriteInfo("Response: " + IpmiSharedFunc.ByteArrayToHexString(messageResponse));

            // format the received message
            ProcessReceivedMessage(messageResponse, out ipmiResponseMessage,  out completionCode);

            Tracer.WriteInfo("Response: " + IpmiSharedFunc.ByteArrayToHexString(messageResponse));

            // messageResponse no longer needed.  data copied to ipmiResponseMessage by ProcessReceivedMessage().
            messageResponse = null;


            // Create the response based on the provided type.
            ConstructorInfo constructorInfo = responseType.GetConstructor(Type.EmptyTypes);
            IpmiResponse ipmiResponse = (IpmiResponse)constructorInfo.Invoke(new Object[0]);

            // check serial protocol completion code
            if (completionCode == (byte)CompletionCode.Success)
            {
                // if serial protocol completion code is successful (0x00).
                // set the packet response completion code to be the ipmi
                // completion code.
                ipmiResponse.CompletionCode = ipmiResponseMessage[7];
            }
            else
            {
                // if the ipmi request reported a time-out response, it is
                // possible the session was terminated unexpectedly.  try to
                // re-establish the session.
                if (completionCode == (byte)CompletionCode.IpmiTimeOutHandShake && allowRetry)
                {
                    // Issue a Retry
                    return LoginRetry(ipmiRequest, responseType, completionCode, priority);
                }
                else
                {
                    // if the Chassis Manager completion code is
                    // unsuccessful, set the ipmi completion code
                    // to the Chassis Manager completion code.
                    ipmiResponse.CompletionCode = completionCode;
                }
            }

            if (ipmiResponse.CompletionCode == (byte)CompletionCode.Success)
            {
                try
                {
                    ipmiResponse.Initialize(IpmiTransport.Serial, ipmiResponseMessage, ipmiResponseMessage.Length, reqSeq);
                    ipmiResponseMessage = null; // response message nolonger needed
                    // reset the communication error counter.
                    CommError = 0;
                }
                catch (Exception ex)
                {
                    // set an exception code for invalid data in ipmi data field, as the packet could
                    // not be converted by the InitializeSerial method.
                    ipmiResponse.CompletionCode = 0xCC;

                    Tracer.WriteError("Method: {0} Response Packet Completion Code: {1} Exception {2}", 
                                        ipmiRequest.GetType().ToString(), 
                                        IpmiSharedFunc.ByteArrayToHexString(ipmiResponseMessage), 
                                        ex.ToString());
                }
            }
            else if (ipmiResponse.CompletionCode == (byte)CompletionCode.IpmiCmdFailedInsufficientPrivLevel && allowRetry) // Catch Ipmi prevelege loss and perform login retry.
            {
                // Issue a re-logon and command retry as Ipmi completion code 
                // D4h indicates session prevelege level issue.
                return LoginRetry(ipmiRequest, responseType, ipmiResponse.CompletionCode, priority);
            }
            else
            {
                // throw ipmi/dcmi response exception with a custom string message and the ipmi completion code
                Tracer.WriteError("Request Type: {0} Response Packet: {1} Completion Code {2}", ipmiRequest.GetType().ToString(), 
                    IpmiSharedFunc.ByteArrayToHexString(ipmiResponseMessage), IpmiSharedFunc.ByteToHexString(ipmiResponse.CompletionCode));
            }

            // Response to the IPMI request message.
            return ipmiResponse;
        }

        /// <summary>
        /// Attempts to re-authenticate with the BMC if the session is dropped.
        /// </summary>
        private IpmiResponse LoginRetry(IpmiRequest ipmiRequest, Type responseType, byte completionCode, PriorityLevel priority)
        {
            CommError++;

            Tracer.WriteWarning(string.Format("Ipmi Logon retry for command {0}. Blade device retry counter: {1}",
                                ipmiRequest.GetType().ToString(),
                                CommError));

            // return resposne
            IpmiResponse response;

            // Attempt to Identify the blade.  (Note: This command does not allow re-try)
            ChannelAuthenticationCapabilities auth = this.GetAuthenticationCapabilities(PrivilegeLevel.Administrator, priority, false);

            // if get channel authentication succeeds, check if the blade is a compute blade.  If so, re-establish
            // the session and re-execute the command
            if (auth.CompletionCode == (byte)CompletionCode.Success)
            {

                // Auxilary data is used to identify the blade
                BladeType bladeType;

                if (Enum.IsDefined(typeof(BladeType), auth.AuxiliaryData))
                {
                    bladeType = (BladeType)auth.AuxiliaryData;
                }
                else
                {
                    bladeType = BladeType.Unknown;
                }

                // Set the blade Type.                
                lock (_cacheLock)
                {
                    _bladeType = bladeType;
                }

                // re-issue original command.                   
                response = IpmiSendReceive(ipmiRequest, responseType, priority, false);

                // if timing-out the issue maybe session releated caveat, 
                // GetSessionInfo cannot be checked if there is no session!
                if (response.CompletionCode == (byte)CompletionCode.IpmiTimeOutHandShake ||
                    response.CompletionCode == (byte)CompletionCode.IpmiCmdFailedInsufficientPrivLevel)
                {
                    // if compute blade, try logon and retry.
                    if (auth.AuxiliaryData == (byte)BladeType.Server)
                    {
                        this.SetClientState(IpmiClientState.Connecting);
                        
                        // login back in.
                        this.LogOn(priority);

                        // re-issue original command.                   
                        return response = IpmiSendReceive(ipmiRequest, responseType, priority, false);
                    }
                }
                else
                {
                    return response;
                }
            }
            
            // re-create the original response and return it.
            ConstructorInfo constructorInfo = responseType.GetConstructor(Type.EmptyTypes);
            response = (IpmiResponse)constructorInfo.Invoke(new Object[0]);
            // set the original response code.
            response.CompletionCode = completionCode;

            return response;
        }

        /// <summary>
        /// Process packed received from serial transport class.
        /// </summary>
        /// <param name="message">Message bytes.</param>
        private void ProcessReceivedMessage(byte[] message, out byte[] ipmiResponseMessage, out byte completionCode)
        {
            completionCode = message[0];

            // check completion code
            if (completionCode == (byte)CompletionCode.Success && message.Length > 3)
            {
                // strip the 3 byte validation message received from the 
                // transport class.
               ipmiResponseMessage = new byte[message.Length - 3];
                
                // copy response packet into respones array
                Buffer.BlockCopy(message, 3, ipmiResponseMessage, 0, (message.Length - 3));
                message = null;

                // Ipmi message heard is 7 bytes.
                if (ipmiResponseMessage.Length >= 7)
                {
                    // check resAddr
                    if (ipmiResponseMessage[1] == 0x8F || ipmiResponseMessage[1] == 0x81)
                    {
                        // replace escape charactors
                        ipmiResponseMessage = this.ReplaceEscapeChars(ipmiResponseMessage);
                        // Validate checsume before passing packet as valid.
                        if (!ValidateCRC(ipmiResponseMessage))
                        {
                            completionCode = (byte)CompletionCode.IpmiCmdFailedIllegalParameter;
                        }
                    }
                    else
                    {
                        completionCode = (byte)CompletionCode.CannotReturnRequestedDataBytes;
                        Tracer.WriteError("Response did contain ipmi packet {0}", IpmiSharedFunc.ByteArrayToHexString(ipmiResponseMessage));
                    }

                }
                else
                {
                    completionCode = (byte)CompletionCode.IpmiInvalidRequestDataLength;
                    Tracer.WriteError("Response did contain ipmi packet {0}", IpmiSharedFunc.ByteArrayToHexString(ipmiResponseMessage));
                }
            }
            else
            {
                ipmiResponseMessage = message;

                if (completionCode != 0)
                {
                    Tracer.WriteError("Malformed Packet Lenght, no Ipmi payload: {0}", IpmiSharedFunc.ByteArrayToHexString(message));
                }
                else
                {
                    Tracer.WriteError("Invalid response received: {0}", IpmiSharedFunc.ByteArrayToHexString(message));
                }
            }
        }

        /// <summary>
        /// Validate the payload checksum.  The function code checksum
        /// and rqAdd is not important to the serial client.
        /// </summary>
        private bool ValidateCRC(byte[] message)
        {
            byte checksum = IpmiSharedFunc.TwoComplementChecksum(4, (message.Length - 2), message);

            // Compare checksum
            if (message[(message.Length - 2)] == checksum)
            {
                return true;
            }
            else
            {
                Tracer.WriteWarning("CheckSum Mismatch: " + Ipmi.IpmiSharedFunc.ByteArrayToHexString(message) + " Checksum: " + checksum);
                return false;
            }
        }

        /// <summary>
        /// Client Connection Status
        /// </summary>
        protected override void SetClientState(IpmiClientState newClientState)
        {
            IpmiClientState status = GetClientState();
            if (status != newClientState)
            {
                Tracer.WriteInfo("Blade client State Changed from {0} to {1}", status.ToString(), newClientState.ToString());

                lock(_stateLock)
                {
                    this._status = newClientState;
                }
            }
        }

        #endregion

        #region ThreadSafe Methods

        /// <summary>
        /// Returns the state of the Current Client.
        /// State locks are used for debugging.
        /// 
        /// </summary>
        public IpmiClientState GetClientState()
        {
            lock (_stateLock)
                return this._status;
        }

        /// <summary>
        /// Gets a unique ReqSeq for each Ipmi message
        /// </summary>
        private byte GetReqSeq()
        {
            lock (_reqSeqLock)
            {
                return base.IpmiRqSeq++;
            }
        }

        /// <summary>
        /// Resets the ReqSeq to zero and return it.
        /// </summary>
        private void ResetReqSeq()
        {
            lock (_reqSeqLock)
            {
                base.IpmiRqSeq = 1;
            }
        }

        /// <summary>
        /// Gets the current session number
        /// </summary>
        private uint GetSession()
        {
            lock (_cacheLock)
            {
                return this._sessionId;
            }
        }

        /// <summary>
        /// Resets the session back to zero
        /// </summary>
        private uint SetSession(uint number = 0)
        {
            lock (_cacheLock)
            {
                this._sessionId = number;
                return number;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize class specifying device address
        /// </summary>
        public WcsBladeClient(byte deviceId)
        {
            this._deviceId = deviceId;
        }

        /// <summary>
        /// Initialize class specifying device address
        /// </summary>
        public WcsBladeClient(byte deviceId, string userName, string password)
        {
            this._deviceId = deviceId;
            base.IpmiUserId = userName;
            base.IpmiPassword = password;
        }

        #endregion

        #region SDR Support

        /// <summary>
        /// Create local copy of Sdr, in threadsafe concurrent dictionary
        /// </summary>
        private void GetSensorDataRecords()
        {
            if (!SensorDataRecords.IsEmpty)
                SensorDataRecords.Clear();

            try
            {
                // should only add full sensor data records. This command does not allow logon retry.
                foreach (SensorMetadataBase record in this.GetSensorMetaData(PriorityLevel.System, false))
                {
                    SensorDataRecords.AddOrUpdate(record.SensorNumber, record, (key, oldValue) => record);
                }
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);
            }
        }

        /// <summary>
        /// Create local copy of Sdr for sensor 1 in threadsafe concurrent dictionary
        /// </summary>
        private void GetFirstSensorDataRecord()
        {
            if (!PwmSensorMetaData.IsEmpty)
                PwmSensorMetaData.Clear();

            try
            {
                // should only add full sensor data records. This command does not allow logon retry.
                foreach (FullSensorRecord record in this.GetFirstSdr(PriorityLevel.System, true))
                {
                    PwmSensorMetaData.AddOrUpdate(record.SensorNumber, record, (key, oldValue) => record);
                }
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);
            }
        }

        /// <summary>
        /// Get Sensor Description 
        /// </summary>
        public string GetSensorDescription(byte SensorNumber)
        {
            if (SensorNumber == 0x01)
            {
                if (PwmSensorMetaData.Count <= 0)
                    GetFirstSensorDataRecord();

                if (PwmSensorMetaData.ContainsKey(SensorNumber))
                {
                    FullSensorRecord sdr;
                    if (this.PwmSensorMetaData.TryGetValue(SensorNumber, out sdr))
                    {
                        return sdr.Description;
                    }
                    else
                    {
                        return string.Empty;
                    }

                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                if (SensorDataRecords.Count <= 0)
                    GetSensorDataRecords();

                if (this.SensorDataRecords.ContainsKey(SensorNumber))
                {
                    SensorMetadataBase sdr;
                    if (this.SensorDataRecords.TryGetValue(SensorNumber, out sdr))
                    {
                        return sdr.Description;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Get Sensor Reading 
        /// </summary>
        public SensorReading GetSensorReading(byte SensorNumber, PriorityLevel priority = PriorityLevel.User)
        {
            if (SensorNumber == 0x01)
            {
                if (PwmSensorMetaData.Count <= 0)
                    GetFirstSensorDataRecord();

                if (PwmSensorMetaData.ContainsKey(SensorNumber))
                {
                    FullSensorRecord sdr;
                    if (this.PwmSensorMetaData.TryGetValue(SensorNumber, out sdr))
                    {
                        return this.GetSensorReading(SensorNumber, sdr.RawSensorType, priority);
                    }
                    else
                    {
                        return new SensorReading((byte)CompletionCode.IpmiInvalidDataFieldInRequest);
                    }

                }
                else
                {
                    return new SensorReading((byte)CompletionCode.IpmiInvalidDataFieldInRequest);
                }
            }
            else
            {
                if (SensorDataRecords.Count <= 0)
                    GetSensorDataRecords();

                if (this.SensorDataRecords.ContainsKey(SensorNumber))
                {
                    SensorMetadataBase sdr;
                    if (this.SensorDataRecords.TryGetValue(SensorNumber, out sdr))
                    {
                        return this.GetSensorReading(SensorNumber, sdr.RawSensorType, priority);
                    }
                    else
                    {
                        return new SensorReading((byte)CompletionCode.IpmiInvalidDataFieldInRequest);
                    }
                }
                else
                {
                    return new SensorReading((byte)CompletionCode.IpmiInvalidDataFieldInRequest);
                }
            }
        }

        /// <summary>
        /// Get Sensor Reading 
        /// </summary>
        public override SensorReading GetSensorReading(byte SensorNumber, byte SensorType)
        {
            // this method is require to override the base class.
            return this.GetSensorReading(SensorNumber, SensorType, PriorityLevel.User);
        }

        /// <summary>
        /// Get Sensor Reading 
        /// </summary>
        public SensorReading GetSensorReading(byte SensorNumber, byte SensorType, PriorityLevel priority = PriorityLevel.User)
        {
            try
            {
                SensorReading reading = this.SensorReading(SensorNumber, SensorType, priority);

                if (reading.CompletionCode == (byte)CompletionCode.Success)
                {
                    // Sensor number 1 should be PWM sensor, this should also be listed as the first
                    // sensor data record in the sdr.  It is an optimisation that this sensor is kept
                    // separately as it means the entire SDR does not need to be parsed upon initialization.
                    if (SensorNumber == 0x01)
                    {
                        if (PwmSensorMetaData.Count <= 0)
                            GetFirstSensorDataRecord();

                        if (this.PwmSensorMetaData.ContainsKey(SensorNumber))
                        {
                            FullSensorRecord sdr;
                            if (this.PwmSensorMetaData.TryGetValue(SensorNumber, out sdr))
                            {
                                reading.ConvertReading(sdr);

                                reading.Description = (sdr.Description);
                            }
                        }

                    }
                    else
                    {
                        // if no cache exists, build it.
                        if (SensorDataRecords.Count <= 0)
                        {
                            GetSensorDataRecords();
                        }

                        if (this.SensorDataRecords.ContainsKey(SensorNumber))
                        {

                            SensorMetadataBase sdr;
                            if (this.SensorDataRecords.TryGetValue(SensorNumber, out sdr))
                            {
                                if (sdr.GetType() == typeof(FullSensorRecord))
                                    reading.ConvertReading(sdr);

                                reading.EventDescription = GetSensorStateString(SensorType, sdr.EventReadingCode, reading.EventState);

                                reading.Description = sdr.Description;
                            }
                            else
                            {
                                AppendSensorTypeCode(SensorNumber, ref reading);
                            }
                        }
                        else
                        {
                            AppendSensorTypeCode(SensorNumber, ref reading);
                        }
                    }
                }
               
                return reading;
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new SensorReading((byte)CompletionCode.IpmiResponseNotProvided);
            }

        }

        /// <summary>
        /// Appends Sensor Type Code to Sensor Event Description.
        /// </summary>
        private void AppendSensorTypeCode(byte sensorNumber, ref SensorReading reading)
        {
            SensorTypeCode typeCode = GetSensorType(sensorNumber);

            if (typeCode.CompletionCode == 0x00)
            {
                reading.EventDescription = GetSensorStateString(typeCode.SensorType, typeCode.EventTypeCode, reading.EventState);
            }
        }

        /// <summary>
        /// Returns the Sensor State, Sensor Type and Event Message Type.
        /// </summary>
        /// <returns></returns>
        private string GetSensorStateString(byte sensorType, byte readingCode, byte sensorState)
        {
            string status = string.Empty;

            EventLogMsgType eventMsgType = EventLogMsgType.Unspecified;

            // Event/Reading Type Code Classification
            if (readingCode == 0x01)
            {
                // Threshold
                eventMsgType = EventLogMsgType.Threshold;
            }
            else if ((readingCode >= 0x02) && (readingCode <= 0x0C))
            {
                // Generic Discrete
                eventMsgType = EventLogMsgType.Discrete;
            }
            else if (readingCode == 0x6f)
            {
                // Specific discrete
                eventMsgType = EventLogMsgType.SensorSpecific;

                // sensor specific sensors user the sensor type
                // as the event reading code.
                readingCode = sensorType;
            }

            EventLogData evtData = ConfigLoaded.GetEventLogData(eventMsgType,
                                (int)readingCode,
                                (int)sensorState);

            return evtData.EventMessage;
        }

        #endregion

        #region Blade Hardware Status

        /// <summary>
        /// Partial Blade Info
        /// </summary>
        public BladeStatusInfo GetBladeInfo()
        {
            BladeStatusInfo response = new BladeStatusInfo((byte)CompletionCode.Success);

            response.BladeType = this.BladeClassification.ToString();

            response.DeviceId = this.DeviceId;

                try
                {
                    DeviceGuid guid = this.GetSystemGuid();

                    // if we can't get the guid we should get out.
                    if (guid.CompletionCode == (byte)CompletionCode.Success)
                    {
                        response.BladeGuid = guid.Guid;

                        SystemStatus pwrState = GetChassisState();
                        if (pwrState.CompletionCode == (byte)CompletionCode.Success)
                        {
                            response.PowerState = pwrState.PowerState.ToString();

                            if (pwrState.IdentitySupported)
                            {
                                response.LedStatus = pwrState.IdentityState.ToString();
                            }
                            else
                            {
                                response.LedStatus = IdentityState.Unknown.ToString();
                            }
                        }
                        else
                        {
                            response.CompletionCode = pwrState.CompletionCode;
                        }

                        BmcDeviceId id = this.GetDeviceId();

                        if (id.CompletionCode == (byte)CompletionCode.Success)
                        {
                              response.BmcFirmware = id.Firmware;
                        }
                        else
                        {
                            response.CompletionCode = id.CompletionCode;
                        }
                   
                        FruDevice fruData = GetFruDeviceInfo(true);

                        if (fruData.CompletionCode == (byte)CompletionCode.Success)
                        {
                            response.SerialNumber = fruData.ProductInfo.SerialNumber.ToString();
                            response.AssetTag = fruData.ProductInfo.AssetTag.ToString();
                            response.HardwareVersion = fruData.ProductInfo.ProductName.ToString();
                            //response.location = fru.ProductInfo.
                        }
                        else
                        {
                            response.CompletionCode = fruData.CompletionCode;
                        }
                    }
                    else
                    {
                        response.CompletionCode = guid.CompletionCode;
                    }
                }
                catch (Exception ex)
                {
                    if (response.CompletionCode == (byte)CompletionCode.Success)
                        response.CompletionCode = (byte)CompletionCode.UnspecifiedError;

                    Tracer.WriteError(ex);
                    return new BladeStatusInfo((byte)CompletionCode.IpmiResponseNotProvided);
                }

                return response;
        }

        /// <summary>
        /// Hardware Info All
        /// </summary>
        public HardwareStatus GetHardwareInfo()
        {
            return this.GetHardwareInfo(true, true, true, true, true,
                                        true, true, true, true);
        }

        /// <summary>
        /// Get Hardware Information
        /// </summary>
        public HardwareStatus GetHardwareInfo(bool proc, bool mem, bool disk, bool me, bool temp, bool power, bool fru, bool pcie, bool misc)
        {
            try
            {
                // get guid has to exceed for this command to proceed, 
                // otherwise blank slots will just waste more time
                // returning errors.
                DeviceGuid guid = this.GetSystemGuid();

                if (guid.CompletionCode == (byte)CompletionCode.Success)
                {                    
                    // Attempt to Identify the blade. (Note: This command by default does not allow retry).
                    ChannelAuthenticationCapabilities response = this.GetAuthenticationCapabilities(PrivilegeLevel.Administrator, PriorityLevel.User, true);
                    
                    // Auxilary data is used to identify the blade
                    BladeType bladeType;

                    if (response.CompletionCode == (byte)CompletionCode.Success)
                    {

                        if (Enum.IsDefined(typeof(BladeType), response.AuxiliaryData))
                        {
                            bladeType = (BladeType)response.AuxiliaryData;
                        }
                        else
                        {
                            bladeType = BladeType.Unknown;
                        }
                    }
                    else
                    {
                        bladeType = BladeType.Unknown;
                    }

                    if (bladeType == BladeType.Server)
                    {
                        ComputeStatus hwStatus = new ComputeStatus(response.CompletionCode, proc, mem,
                        disk, fru, misc);

                        // add guid
                        hwStatus.BladeGuid = guid.Guid;

                        // add blade type
                        hwStatus.BladeType = bladeType.ToString();

                        // add device Id
                        hwStatus.DeviceId = this.DeviceId;

                        // temp sensors
                        List<byte> tempSensors = new List<byte>();

                        // processors numbers
                        List<byte> processor = new List<byte>();

                        // intel ME
                        List<SensorMetadataBase> meModule = new List<SensorMetadataBase>();

                        // pcie cards
                        List<byte> pcieCards = new List<byte>();

                        List<byte> disks = new List<byte>();

                        // Disk Sensors
                        List<SensorMetadataBase> hwSensors = new List<SensorMetadataBase>();

                        if (SensorDataRecords.Count <= 0)
                            GetSensorDataRecords();

                        // iterate the Sdr
                        foreach (KeyValuePair<byte, SensorMetadataBase> sdr in SensorDataRecords)
                        {
                            if (sdr.Value.EntityType == IpmiEntityType.Physical
                                && sdr.Value.Entity == IpmiEntity.Processor)
                            {
                                processor.Add((byte)sdr.Value.EntityInstance);
                            }
                            else if (sdr.Value.EntityType == IpmiEntityType.Physical
                                && sdr.Value.Entity == IpmiEntity.PCIeBus)
                            {
                                pcieCards.Add((byte)sdr.Value.EntityInstance);
                            }
                            else if (sdr.Value.SensorType == SensorType.DriveSlot)
                            {
                                disks.Add(sdr.Value.SensorNumber);
                            }
                            else if (sdr.Value.Entity == IpmiEntity.MgmtCntrlFirmware
                                && sdr.Value.EntityType == IpmiEntityType.Physical)
                            {
                                meModule.Add(sdr.Value);
                            }

                            if (sdr.Value.SensorType == SensorType.Processor
                                || sdr.Value.SensorType == SensorType.Memory
                                )
                            {
                                hwSensors.Add(sdr.Value);
                            }

                            if (sdr.Value.GetType() == typeof(FullSensorRecord)
                                && sdr.Value.SensorType == SensorType.Temperature)
                            {
                                tempSensors.Add(sdr.Value.SensorNumber);
                            }
                        }

                        if (proc)
                        {
                            // Get Processor Information for each list entity.
                            foreach (byte processNo in SupportFunctions.FilterDistinct<byte>(processor))
                            {
                                ProcessorInfo procinf = GetProcessorInfo(processNo);

                                hwStatus.ProcInfo.Add(processNo, procinf);

                                if (procinf.CompletionCode != (byte)CompletionCode.Success)
                                {
                                    hwStatus.PartialError = procinf.CompletionCode;
                                }
                            }
                        }

                        if (mem)
                        {

                            MemoryIndex memIndex = GetMemoryIndex();

                            if (memIndex.CompletionCode == 0)
                            {
                                for (int i = 0; i < memIndex.SlotCount; i++)
                                {
                                    int index = (i + 1);

                                    if (memIndex.PresenceMap[index])
                                    {
                                        MemoryInfo meminf = GetMemoryInfo((byte)index);
                                        hwStatus.MemInfo.Add((byte)index, meminf);

                                        if (meminf.CompletionCode != (byte)CompletionCode.Success)
                                        {
                                            hwStatus.PartialError = meminf.CompletionCode;
                                        }
                                    }
                                    else
                                    {
                                        MemoryInfo meminf = new MemoryInfo(0x00);
                                        meminf.SetParamaters(0x00, 0x00, 0x00, (byte)MemoryType.Unknown,
                                            (byte)MemoryVoltage.Unknown, (byte)MemoryStatus.NotPresent);
                                        hwStatus.MemInfo.Add((byte)index, meminf);
                                    }
                                }
                            }
                            else
                            {
                                    hwStatus.PartialError = memIndex.CompletionCode;

                            }
                        }

                        if (disk)
                        {
                            foreach (byte diskSensor in SupportFunctions.FilterDistinct<byte>(disks))
                            {
                                SensorReading reading = GetSensorReading(diskSensor, (byte)SensorType.DriveSlot, PriorityLevel.User);

                                hwStatus.DiskSensors.Add(diskSensor, reading);

                                if (reading.CompletionCode != (byte)CompletionCode.Success)
                                {
                                    hwStatus.PartialError = reading.CompletionCode;
                                }
                            }
                        }

                        if (pcie)
                        {
                            if (pcieCards.Count > 0)
                            {
                                // Get PCIe Information for each list entity.
                                foreach (byte pci in SupportFunctions.FilterDistinct<byte>(pcieCards))
                                {
                                    PCIeInfo pciInf = GetPCIeInfo(pci);

                                    hwStatus.PcieInfo.Add(pci, pciInf);

                                    if (pciInf.CompletionCode != (byte)CompletionCode.Success)
                                    {
                                        hwStatus.PartialError = pciInf.CompletionCode;
                                    }
                                }
                            }
                            else // assume 1-3 & expect errors.
                            {
                                for (byte i = 1; i < 4; i++)
                                {
                                    PCIeInfo pciInf = GetPCIeInfo(i);

                                    if (pciInf.CompletionCode == (byte)CompletionCode.Success)
                                    {
                                        hwStatus.PcieInfo.Add(i, pciInf);
                                    }
                                }
                            }
                        }

                        if (me)
                        {
                            // add hardware sensors to the list.
                            foreach (SensorMetadataBase sensor in meModule)
                            {
                                SensorReading reading = GetSensorReading(sensor.SensorNumber, sensor.RawSensorType, PriorityLevel.User);

                                hwStatus.HardwareSdr.Add(new HardwareSensor(sensor, reading));

                                if (reading.CompletionCode != (byte)CompletionCode.Success)
                                {
                                    hwStatus.PartialError = reading.CompletionCode;
                                }
                            }
                        }
                        
                        if(misc)
                        {
                            // add hardware sensors to the list.
                            foreach (SensorMetadataBase sensor in hwSensors)
                            {
                                SensorReading reading = GetSensorReading(sensor.SensorNumber, sensor.RawSensorType, PriorityLevel.User);

                                hwStatus.HardwareSdr.Add(new HardwareSensor(sensor, reading));

                                if (reading.CompletionCode != (byte)CompletionCode.Success)
                                {
                                    hwStatus.PartialError = reading.CompletionCode;
                                }
                            }
                        }

                        if(temp)
                        {
                            // Get Temp Information for each list entity.
                            foreach (byte sensor in tempSensors)
                            {
                                SensorReading reading = GetSensorReading(sensor, (byte)SensorType.Temperature, PriorityLevel.User);

                                hwStatus.TempSensors.Add(sensor, reading);

                                if (reading.CompletionCode != (byte)CompletionCode.Success)
                                {
                                    hwStatus.PartialError = reading.CompletionCode;
                                }
                            }
                        }

                        if(power)
                        {
                            // Get Power Reading for Blade
                            List<PowerReading> readings = GetPowerReading();
                            if (readings[0].CompletionCode != (byte)CompletionCode.Success)
                            {
                                hwStatus.PartialError = readings[0].CompletionCode;
                            }

                            hwStatus.Power = readings[0];
                        }

                        if (fru)
                        {
                            AddFruData<ComputeStatus>(ref hwStatus, true);
                        }

                        return hwStatus;
                    }
                    else if (bladeType == BladeType.Jbod)
                    {
                        JbodStatus hwStatus = new JbodStatus(response.CompletionCode, proc, mem,
                            disk, fru, misc);

                        // add guid
                        hwStatus.BladeGuid = guid.Guid;

                        // add blade type
                        hwStatus.BladeType = bladeType.ToString();

                        // add device Id
                        hwStatus.DeviceId = this.DeviceId;

                        if (disk)
                        {
                            hwStatus.DiskStatus = GetDiskStatus();
                        }

                        if (temp)
                        {
                            hwStatus.DiskInfo = GetDiskInfo();
                        }

                        if (fru)
                        {
                            AddFruData<JbodStatus>(ref hwStatus, false);
                        }

                        return hwStatus;
                    }
                    else
                    {
                        return new UnknownBlade((byte)CompletionCode.UnknownBladeType);
                    }
                }
                else
                {
                    return new UnknownBlade(guid.CompletionCode);
                }
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);
                return new UnknownBlade((byte)CompletionCode.UnspecifiedError);
            }
        }

        /// <summary>
        /// appends fru data to the GetHardwareInfo command
        /// </summary>
        private void AddFruData<T>(ref T hwStatus, bool optimize) where T : HardwareStatus
        {
            // Get Fru Data
            FruDevice fruData = GetFruDeviceInfo(optimize);

            if (fruData.CompletionCode == (byte)CompletionCode.Success)
            {
                if (fruData.ProductInfo != null)
                {
                    hwStatus.SerialNumber = fruData.ProductInfo.SerialNumber.ToString();
                    hwStatus.AssetTag = fruData.ProductInfo.AssetTag.ToString();
                    hwStatus.HardwareVersion = fruData.ProductInfo.ProductName.ToString();
                }
            }
            else
            {
                hwStatus.PartialError = fruData.CompletionCode;
            }
        }

        #endregion

        #region Ipmi Commands

        /// <summary>
        /// Get Sensor Type for the IPMI Sensor.
        /// </summary>
        public override SensorTypeCode GetSensorType(byte sensorNumber)
        {
            try
            {
                return base.GetSensorType(sensorNumber);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new SensorTypeCode((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Get Fru Device info for Given Device Id
        /// </summary>
        public override FruDevice GetFruDeviceInfo(int fruId, bool maxLenght = false)
        {
            try
            {
                return base.GetFruDeviceInfo(fruId, maxLenght);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new FruDevice((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Get Fru Device info
        /// </summary>
        public override FruDevice GetFruDeviceInfo(bool maxLenght = false)
        {
            try
            {
                return base.GetFruDeviceInfo(0, maxLenght);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new FruDevice((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Queries BMC for the currently set boot device.
        /// </summary>
        /// <returns>Flags indicating the boot device.</returns>
        public override NextBoot GetNextBoot()
        {
            
            try
            {
                return base.GetNextBoot();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new NextBoot((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// The helper for several boot type setting methods, as they
        /// essentially send the same sequence of messages.
        /// </summary>
        /// <param name="bootType">The desired boot type.</param>
        public override NextBoot SetNextBoot(BootType bootType, bool uefi, bool persistent, byte bootInstance = 0x00, bool requireCommit = false)
        {
            
            try
            {
                return base.SetNextBoot(bootType, uefi, persistent, bootInstance, requireCommit);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new NextBoot((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Write Fru Data Command.  Note:
        ///     The command writes the specified byte or word to the FRU Inventory Info area. This is a ‘low level’ direct 
        ///     interface to a non-volatile storage area. The interface does not interpret or check any semantics or 
        ///     formatting for the data being written.  The offset used in this command is a ‘logical’ offset that may or may not 
        ///     correspond to the physical address. For example, FRU information could be kept in FLASH at physical address 1234h, 
        ///     however offset 0000h would still be used with this command to access the start of the FRU information.
        ///     
        ///     IPMI FRU device data (devices that are formatted per [FRU]) as well as processor and DIMM FRU data always starts 
        ///     from offset 0000h unless otherwise noted.
        /// </summary>
        public override WriteFruDevice WriteFruDevice(int deviceId, ushort offset, byte[] payload)
        {
            
            try
            {
                return base.WriteFruDevice(deviceId, offset, payload);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new WriteFruDevice((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Write Fru Data to Baseboard containing BMC FRU.
        /// </summary>
        public override WriteFruDevice WriteFruDevice(ushort address, byte[] payload)
        {
            
            try
            {
                return base.WriteFruDevice(0, address, payload);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new WriteFruDevice((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        ///  Get Sensor Data Repository. Returns SDR Info.
        /// </summary>
        public override SdrCollection GetSdr()
        {  
            try
            {
                return base.GetSdr();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new SdrCollection((byte)CompletionCode.IpmiResponseNotProvided);
            }

        }

        /// <summary>
        ///  Get Sensor Data Repository Information Incrementally. Returns SDR Info.
        /// </summary>
        public override SdrCollection GetSdrIncrement()
        {
            try
            {
                return base.GetSdrIncrement();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new SdrCollection((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Physically identify the computer by using a light or sound.
        /// </summary>
        /// <param name="interval">Identify interval in seconds or 255 for indefinite.</param>
        public override bool Identify(byte interval) 
        {
            try
            {
                return base.Identify(interval);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return false;
            }
        }

        /// <summary>
        /// Set the Power Cycle interval.
        /// </summary>
        /// <param name="interval">Identify interval in seconds or 255 for indefinite.</param>
        public bool SetPowerCycleInterval(byte interval)
        {
            try
            {
                return SetPowerOnTime(interval);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return false;
            }
        }

        /// <summary>
        /// Set the computer power state.
        /// </summary>
        /// <param name="powerState">Power state to set.</param>
        public override byte SetPowerState(IpmiPowerState powerState) 
        {
            try
            {
                return base.SetPowerState(powerState);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return (byte)CompletionCode.IpmiResponseNotProvided;
            }
        }

        /// <summary>
        /// Gets BMC firmware revision.  Returns HEX string.
        /// </summary>
        /// <returns>firmware revision</returns>
        public override BmcFirmware GetFirmware()
        {
            try
            {
                return base.GetFirmware();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new BmcFirmware((byte)CompletionCode.IpmiResponseNotProvided);
            }
            
        }

        /// <summary>
        /// Get the Power-On-Hours (POH) of the host computer.
        /// </summary>
        /// <returns>System Power On Hours.</returns>
        /// <remarks> Specification Note: Power-on hours shall accumulate whenever the system is in 
        /// the operational (S0) state. An implementation may elect to increment power-on hours in the S1 
        /// and S2 states as well.
        /// </remarks>
        public override PowerOnHours PowerOnHours()
        {
           try
           {
               return base.PowerOnHours();
           }
           catch (Exception ex)
           {
               Tracer.WriteError(ex);

               return new PowerOnHours((byte)CompletionCode.IpmiResponseNotProvided);
           }
        }

        /// <summary>
        /// Queries BMC for the GUID of the system.
        /// </summary>
        /// <returns>GUID reported by Baseboard Management Controller.</returns>
        public DeviceGuid GetSystemGuid(PriorityLevel priority)
        {
            try
            {
                if (priority == PriorityLevel.System)
                {
                    return this.SysPriGetSystemGuid();
                }
                else
                {
                    return base.GetSystemGuid();
                }
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new DeviceGuid((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Reset SEL Log
        /// </summary>
        public override bool ClearSel()
        {
            try
            {
                return base.ClearSel();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return false;
            }
        }

        /// <summary>
        /// Recursively retrieves System Event Log entries.
        /// </summary>
        public override SystemEventLog GetSel()
        {
            try
            {
                return base.GetSel();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new SystemEventLog((byte)CompletionCode.IpmiResponseNotProvided);
            }

        }

        /// <summary>
        ///  Get System Event Log Information. Returns SEL Info.
        /// </summary>
        public override SystemEventLogInfo GetSelInfo()
        {
            try
            {
                return base.GetSelInfo();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new SystemEventLogInfo((byte)CompletionCode.IpmiResponseNotProvided);
            }


        }

        /// <summary>
        /// Gets the SDR, but only temprature sensors.  This method has a performance improvement over
        /// getting the entire SDR.  Approximately 12 second to 3 second reduction.
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public SdrCollection GetSensorMetaData(PriorityLevel priority, bool retry)
        {
            try
            {
                return this.GetSensorDataRecords(priority, retry);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new SdrCollection((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Get Device Id Command
        /// </summary>
        public BmcDeviceId GetDeviceId(PriorityLevel priority)
        {
            try
            {
                if (priority == PriorityLevel.System)
                {
                    return SysPriGetDeviceId();
                }
                else
                {
                    return base.GetDeviceId();
                }
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new BmcDeviceId((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Set Chassis Power Restore Policy.
        /// </summary>
        public override PowerRestorePolicy SetPowerRestorePolicy(PowerRestoreOption policyOption)
        {
            try
            {
                return base.SetPowerRestorePolicy(policyOption);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new PowerRestorePolicy((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Switches Serial port sharing from BMC to Serial Port
        /// </summary>
        /// <param name="mux"></param>
        /// <returns></returns>
        public SerialMuxSwitch SetSerialMuxSwitch(MuxSwtich mux)
        {
            try
            {
                // Get the Channel Authentication Capabilities to identify the blade type.
                ChannelAuthenticationCapabilities auth = base.GetAuthenticationCapabilities(PrivilegeLevel.Administrator);
                if (auth.CompletionCode == (byte)CompletionCode.Success)
                {
                    // Check for device type (Compute Blade / JBOD)
                    if (auth.AuxiliaryData == 0x04)
                    {
                        // IPMI SPEC: Current Channel = 0x0E.  Serial Channel = 0x02
                        return base.SetSerialMuxSwitch(auth.ChannelNumber, mux);
                    }
                    else
                    {
                        // JBOD does not require or support SetSerialMuxSwitch, however
                        // to switch the EnableSafeMode at the Communication Layer,
                        // the mux message must be sent.  This will time-out for the JBOD,
                        // however if enablesafemode is not enabled switched the system thread
                        // will impact the JBOD session.
                        // We send a mux session to the JBOD then intercept the request
                        // in the SerialPortManager.SendReceive(ref WorkItem workItem).
                        // The MuxSwtich.BlockRequeststoSystem allows the SendReceive Method
                        // to know the system is a JBOD and not write any payload to the 
                        // device.
                        SerialMuxSwitch muxswitch = base.SetSerialMuxSwitch(auth.ChannelNumber, MuxSwtich.BlockRequeststoSystem);
                        Tracer.WriteError("MuxSwtich: {0}", muxswitch.CompletionCode);
                        // SerialPortManager.SendReceive(ref WorkItem workItem) will drop the request packet,
                        // ensuring SendReceiveServer forwards no message to the JBOD.
                        if (muxswitch.CompletionCode == (byte)CompletionCode.InvalidRequestDataLength)
                            return new SerialMuxSwitch((byte)CompletionCode.Success);
                        else
                            return muxswitch;
                    }
                }
                else
                {
                    // package and return failure message.
                    return new SerialMuxSwitch((byte)auth.CompletionCode);
                }
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new SerialMuxSwitch((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Switches Serial port sharing from System to Bmc
        /// </summary>
        public override SerialMuxSwitch ResetSerialMux()
        {
            try
            {
                return base.ResetSerialMux();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new SerialMuxSwitch((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Get the current advanced state of the host computer.
        /// </summary>
        /// <returns>ImpiPowerState enumeration.</returns>
        /// <devdoc>
        /// Originally used the 'Get ACPI Power State' message to retrieve the power state but not supported
        /// by the Arima's Scorpio IPMI card with firmware 1.10.00610100.  The 'Get Chassis Status' message
        /// returns the correct information for all IPMI cards tested.
        /// </devdoc>
        public override SystemStatus GetChassisState()
        {
            try
            {
                return base.GetChassisState();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new SystemStatus((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Get Processor Information
        /// </summary>
        public override ProcessorInfo GetProcessorInfo(byte processor)
        {
            try
            {
                return base.GetProcessorInfo(processor);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new ProcessorInfo((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Get Memory Information
        /// </summary>
        public override MemoryInfo GetMemoryInfo(byte dimm)
        {
            try
            {
                return base.GetMemoryInfo(dimm);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new MemoryInfo((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Get PCIe Information
        /// </summary>
        public override PCIeInfo GetPCIeInfo(byte device)
        {
            try
            {
                return base.GetPCIeInfo(device);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new PCIeInfo((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Get Nic Information
        /// </summary>
        public override NicInfo GetNicInfo(byte device)
        {
            try
            {
                return base.GetNicInfo(device);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new NicInfo((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Sets the BMC serial port time out in 30 second increments.
        /// Exampe:  A paramater of 2 = 1 minute.  A paramater of 4 = 2 minutes
        /// </summary>
        public bool SetSerialTimeOut(byte time)
        {
            try
            {
                return base.SetSerialConfig<SerialConfig.SessionTimeout>(new SerialConfig.SessionTimeout(time));
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return false;
            }
        }

        /// <summary>
        /// Set serial port termination paramater
        /// </summary>
        public bool SetSerialTermination(bool dcd = false, bool timeout = false)
        {
            try
            {
                return base.SetSerialConfig<SerialConfig.SessionTermination>(new SerialConfig.SessionTermination(dcd, timeout));
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return false;
            }
        }

        /// <summary>
        /// Serial Console Timeout in seconds.  Zero indicates read error
        /// </summary>
        public int GetSerialTimeOut()
        {
            try
            {
                return base.GetSerialConfig<SerialConfig.SessionTimeout>(new SerialConfig.SessionTimeout()).TimeOut;
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return 0;
            }
        }

        /// <summary>
        /// Get JBOD Disk Status
        /// </summary>
        public override DiskStatusInfo GetDiskStatus()
        {
            try
            {
                return base.GetDiskStatus();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new DiskStatusInfo((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Get JBOD Disk Info
        /// </summary>
        public override DiskInformation GetDiskInfo()
        {
            try
            {
                return base.GetDiskInfo();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new DiskInformation((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Get JBOD Disk Info
        /// </summary>
        public override DiskInformation GetDiskInfo(byte channel, byte disk)
        {
            try
            {
                return base.GetDiskInfo(channel, disk);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new DiskInformation((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }
        
        #endregion

        #region Forced overrides

        /// <summary>
        /// Queries BMC for the GUID of the system. With System Level Priority
        /// </summary>
        /// <returns>GUID reported by Baseboard Management Controller.</returns>
        private DeviceGuid SysPriGetSystemGuid()
        {
            GetSystemGuidRequest req = new GetSystemGuidRequest();

            GetSystemGuidResponse response =
                (GetSystemGuidResponse)this.IpmiSendReceive(req, typeof(GetSystemGuidResponse), PriorityLevel.System, false);

            DeviceGuid responseObj = new DeviceGuid(response.CompletionCode);

            if (response.CompletionCode == (byte)CompletionCode.Success)
            {
                responseObj.SetParamaters(response.Guid);
            }

            return responseObj;
        }

        /// <summary>
        /// Gets Device Id.  Returns HEX string, performed with System Level priority
        /// </summary>
        /// <returns>firmware revision</returns>
        private BmcDeviceId SysPriGetDeviceId()
        {
            // Get Device Id
            GetDeviceIdResponse response = (GetDeviceIdResponse)this.IpmiSendReceive(new GetDeviceIdRequest(),
              typeof(GetDeviceIdResponse), PriorityLevel.System);

            BmcDeviceId responseObj = new BmcDeviceId(response.CompletionCode);

            if (response.CompletionCode == (byte)CompletionCode.Success)
            {
                responseObj.SetParamaters(response.MajorFirmware, response.MinorFirmware,
                    response.ManufactureId, response.ProductId);
            }

            return responseObj;
        }

        /// <summary>
        ///  Get Sensor Data Repository. Returns SDR Info.
        /// </summary>
        private SdrCollection GetSensorDataRecords(PriorityLevel priority, bool retry = true)
        {
            // Default Record Off Set
            int offSet = 0;

            // Number of Bytes to Read. 0xFF for entire record.
            byte bytesToRead = 0xFF;

            // SDR RecordId (0000h for entry point)
            ushort recordId = 0;

            // Last SDR RecordId (aborts event log Loop)
            ushort lastRecordId = 65535;

            // securely step out of the while.
            int pass = 0;

            // create sdr record collection for raw SDR records.
            IpmiSdrCollection records = new IpmiSdrCollection();

            // reserve the SDR for partial reads
            ReserveSdrResponse reserve = (ReserveSdrResponse)this.IpmiSendReceive(
            new ReserveSdrRequest(), typeof(ReserveSdrResponse), priority, retry);

            if (reserve.CompletionCode == (byte)CompletionCode.Success)
            {

                // reserved LS byte
                byte reserveLs = reserve.ReservationLS;

                // reserved MS byte
                byte reserveMs = reserve.ReservationMS;

                // retrieve all records while connected by recursively calling the SDR entry command 
                while (recordId != lastRecordId || pass > 1000)
                {
                    // create SDR record
                    SdrRecord sdr = new SdrRecord();
                    {
                        // get the SEL record
                        GetSdrPartialResponse response = (GetSdrPartialResponse)this.IpmiSendReceive(
                        new GetSdrPartialRequest(reserveLs, reserveMs, recordId, offSet, bytesToRead), typeof(GetSdrPartialResponse), priority, retry);

                        if (response.CompletionCode == (byte)CompletionCode.Success)
                        {
                            sdr.completionCode = response.CompletionCode;

                            // set record id
                            sdr.RecordId = new byte[2] { response.RecordData[1], response.RecordData[0] };

                            // set the record version
                            sdr.RecordVersion = response.RecordData[2];

                            // set record type
                            sdr.RecordType = response.RecordData[3];

                            // set record lenght
                            sdr.RecordLenght = response.RecordData[4];

                            // set the record data to record data
                            sdr.RecordData = response.RecordData;

                            // update the record Id (signals loop exit)
                            recordId = BitConverter.ToUInt16(new byte[2] { response.RecordIdMsByte, response.RecordIdLsByte }, 0);
                        }
                        else
                        {
                            sdr.completionCode = response.CompletionCode;
                            break;
                        }
                    }

                    pass++;

                    // add the record to the collection
                    records.Add(sdr);
                }
            }

            // return collection
            SdrCollection sdrMessages = new SdrCollection();

            // check response collection holds values
            if (records.Count > 0)
            {
                // sdr version array
                byte[] verarr = new byte[2];

                // record id
                short id;

                foreach (SdrRecord record in records)
                {
                    if (record.completionCode == (byte)CompletionCode.Success)
                    {
                        // set the sdr collection completion code to indicate a failure occurred
                        sdrMessages.completionCode = record.completionCode;

                        // record Id
                        id = BitConverter.ToInt16(record.RecordId, 0);

                        // populdate version array
                        Buffer.BlockCopy(IpmiSharedFunc.ByteSplit(record.RecordVersion, new int[2] { 4, 0 }), 0, verarr, 0, 2);

                        string sVersion = Convert.ToUInt16(verarr[1]).ToString() + "." + Convert.ToInt16(verarr[0]).ToString();

                        // set version
                        Decimal version = 0;
                        // sdr record version number
                        if (decimal.TryParse(sVersion, out version)) { }

                        base.GetSdrMetatData(id, version, record.RecordType, record, ref sdrMessages);

                    }
                    // set the sdr completion code to indicate a failure occurred
                    sdrMessages.completionCode = record.completionCode;
                }
            }

            return sdrMessages;
        }

        /// <summary>
        /// Gets Sensor Reading
        /// </summary>
        private SensorReading SensorReading(byte SensorNumber, byte SensorType, PriorityLevel priority)
        {
            SensorReadingResponse response = (SensorReadingResponse)this.IpmiSendReceive(
            new SensorReadingRequest(SensorNumber), typeof(SensorReadingResponse), priority, true);

            SensorReading respObj = new SensorReading(response.CompletionCode);
            respObj.SensorNumber = SensorNumber;
            respObj.SensorType = SensorType;

            if (response.CompletionCode == (byte)CompletionCode.Success)
            {
                // set the raw sensor reading
                respObj.SetReading(response.SensorReading);

                byte[] statusByteArray = new byte[1];
                statusByteArray[0] = response.SensorStatus;

                BitArray sensorStatusBitArray = new BitArray(statusByteArray);
                bool eventMsgEnabled = sensorStatusBitArray[7];
                bool sensorScanEnabled = sensorStatusBitArray[6];
                bool readingUnavailable = sensorStatusBitArray[5];

                byte[] stateByteArray = new byte[1];
                stateByteArray[0] = response.StateOffset;

                BitArray stateBitArray = new BitArray(stateByteArray);

                string SensorState = string.Empty;

                #region Threshold Event
                if (SensorType == 0x01)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (stateBitArray[i])
                            respObj.SetEventState((byte)i);
                    }
                }

                #endregion

                #region Discrete Event
                else if ((SensorType >= 0x02) && (SensorType <= 0x0C))
                {
                    for (int i = 0; i < 7; i++)
                    {
                        if (stateBitArray[i])
                            respObj.SetEventState((byte)i);
                    }

                    if (response.OptionalOffset != 0x00)
                    {
                        byte[] optionalByteArray = new byte[1];
                        optionalByteArray[0] = response.OptionalOffset;

                        BitArray optionalBitArray = new BitArray(optionalByteArray);

                        for (int i = 0; i < 6; i++)
                        {
                            if (optionalBitArray[i])
                                respObj.SetEventState((byte)(i + 8));
                        }
                    }
                }
                #endregion

                #region Unspecified Event
                else
                {
                    // Unspecified
                }
                #endregion
            }

            return respObj;
        }

        /// <summary>
        ///  Get Sensor Data Repository for sensor one.. Returns SDR Info.
        /// </summary>
        private SdrCollection GetFirstSdr(PriorityLevel priority, bool retry = true)
        {
            // Default Record Off Set
            int offSet = 0;

            // Number of Bytes to Read. 0xFF for entire record.
            byte bytesToRead = 0xFF;

            // SDR RecordId (0000h for entry point)
            ushort recordId = 0;

            // return collection
            SdrCollection sdrMessages = new SdrCollection();

            // reserve the SDR for partial reads
            ReserveSdrResponse reserve = (ReserveSdrResponse)this.IpmiSendReceive(
            new ReserveSdrRequest(), typeof(ReserveSdrResponse), priority, retry);

            if (reserve.CompletionCode == (byte)CompletionCode.Success)
            {
                // create SDR record
                SdrRecord sdr = new SdrRecord();
                {
                    // get the SEL record
                    GetSdrPartialResponse response = (GetSdrPartialResponse)this.IpmiSendReceive(
                    new GetSdrPartialRequest(reserve.ReservationLS, reserve.ReservationMS, recordId, offSet, bytesToRead), typeof(GetSdrPartialResponse), priority, retry);

                    if (response.CompletionCode == (byte)CompletionCode.Success)
                    {
                        sdr.completionCode = response.CompletionCode;

                        // set record id
                        sdr.RecordId = new byte[2] { response.RecordData[1], response.RecordData[0] };

                        // set the record version
                        sdr.RecordVersion = response.RecordData[2];

                        // set record type
                        sdr.RecordType = response.RecordData[3];

                        // set record lenght
                        sdr.RecordLenght = response.RecordData[4];

                        // set the record data to record data
                        sdr.RecordData = response.RecordData;

                        // update the record Id (signals loop exit)
                        recordId = BitConverter.ToUInt16(new byte[2] { response.RecordIdMsByte, response.RecordIdLsByte }, 0);

                        // set the sdr collection completion code to indicate a failure occurred
                        sdrMessages.completionCode = sdr.completionCode;

                        // sdr version array
                        byte[] verarr = new byte[2];

                        // record Id
                        short id = BitConverter.ToInt16(sdr.RecordId, 0);

                        // populdate version array
                        Buffer.BlockCopy(IpmiSharedFunc.ByteSplit(response.RecordData[2], new int[2] { 4, 0 }), 0, verarr, 0, 2);

                        string sVersion = Convert.ToUInt16(verarr[1]).ToString() + "." + Convert.ToInt16(verarr[0]).ToString();

                        // set version
                        Decimal version = 0;
                        // sdr record version number
                        decimal.TryParse(sVersion, out version);

                        base.GetSdrMetatData(id, version, sdr.RecordType, sdr, ref sdrMessages);

                    }
                    else
                    {
                        sdr.completionCode = response.CompletionCode;
                    }
                }
            }

            return sdrMessages;
        }

        /// <summary>
        /// Negotiates the ipmi version and sets client accordingly. Also sets the authentication type for V1.5
        /// </summary>
        public ChannelAuthenticationCapabilities GetAuthenticationCapabilities(PrivilegeLevel privilegeLevel, PriorityLevel priority, bool retry = false)
        {
            // Get Channel Authentication Capabilities
            GetChannelAuthenticationCapabilitiesResponse response =
                (GetChannelAuthenticationCapabilitiesResponse)this.IpmiSendReceive(
                    new GetChannelAuthenticationCapabilitiesRequest(0x0E, privilegeLevel),
                    typeof(GetChannelAuthenticationCapabilitiesResponse), priority, retry);

            ChannelAuthenticationCapabilities authCapabilities = new ChannelAuthenticationCapabilities(response.CompletionCode);

            if (response.CompletionCode == (byte)CompletionCode.Success)
            {

                authCapabilities.SetParamaters(response.ChannelNumber,
                    response.AuthenticationTypeSupport1,
                response.AuthenticationTypeSupport2, response.ExtendedCapabilities,
                response.OemId, response.OemData);
            }

            return authCapabilities;
        }

        #endregion

        #region Dcmi Commands

        /// <summary>
        /// DCMI Get Power Limit Command
        /// </summary>
        public override PowerLimit GetPowerLimit()
        {
            try
            {
                return base.GetPowerLimit();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new PowerLimit((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// DCMI Set Power Limit Command
        /// </summary>
        public override ActivePowerLimit SetPowerLimit(short watts, int correctionTime, byte action, short samplingPeriod)
        {
            try
            {
                return base.SetPowerLimit(watts, correctionTime, action, samplingPeriod);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new ActivePowerLimit((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// DCMI Get Power Reading Command
        /// </summary>
        public override List<PowerReading> GetPowerReading()
        {
            try
            {
                return base.GetPowerReading();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);
                return new List<PowerReading>(1) { new PowerReading((byte)CompletionCode.IpmiResponseNotProvided) };
            }
        }

        /// <summary>
        /// Activate/Deactivate DCMI power limit
        /// </summary>
        /// <param name="enable">Activate/Deactivate</param>
        public override bool ActivatePowerLimit(bool enable)
        {
            try
            {
                return base.ActivatePowerLimit(enable);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return false;
            }
        }

        #endregion

        #region Bridge Commands

        /// <summary>
        /// Send sync Bridge Command
        /// </summary>
        /// <param name="channel">Channel to send command (Intel ME = 6)</param>
        /// <param name="slaveId">Channel Slave Id</param>
        /// <param name="messageData">Message payload</param>
        public override BridgeMessage SendMessage(byte channel, byte slaveId, byte[] requestMessage)
        {
            try
            {
                return base.SendMessage(channel, slaveId, requestMessage);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new BridgeMessage((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Get Message Flags
        /// </summary>
        public override MessageFlags GetMessageFlags()
        {
            try
            {
                return base.GetMessageFlags();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new MessageFlags((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Read Event Message Buffer
        /// </summary>
        public override BridgeMessage ReadEventMessageBuffer()
        {
            try
            {
                return base.ReadEventMessageBuffer();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new BridgeMessage((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Get Message Response
        /// </summary>
        public override BridgeMessage GetMessage()
        {
            try
            {
                return base.GetMessage();
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new BridgeMessage((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Get the channel state for bridging commands
        /// </summary>
        /// <param name="channel">Channel number to check</param>
        /// <param name="enabled">Channel Disabled = 0x00, Channel Enabled = 0x001</param>
        public override BridgeChannelReceive BridgeChannelEnabled(byte channel)
        {
            try
            {
                return base.BridgeChannelEnabled(channel);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new BridgeChannelReceive((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        /// <summary>
        /// Enable or Disable the Ipmi Bridge Channel
        /// </summary>
        /// <param name="channel">Channel number to enable</param>
        /// <param name="enabled">Enabled = true, Disabled = false</param>
        public override BridgeChannelReceive EnableDisableBridgeChannel(byte channel, bool enabled)
        {
            try
            {
                return base.EnableDisableBridgeChannel(channel, enabled);
            }
            catch (Exception ex)
            {
                Tracer.WriteError(ex);

                return new BridgeChannelReceive((byte)CompletionCode.IpmiResponseNotProvided);
            }
        }

        #endregion


    }
}
