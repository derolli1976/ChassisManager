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
using System.Collections;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    /// <summary>
    /// Shared Functions and Methods
    /// </summary>
    internal static class SerialBaseSharedFunctions
    {
        internal enum AuthType
        {
            None = 0,
            MD2 = 1,
            MD5 = 2,
            reserved = 3,
            Password = 4,
            OEM = 5
        }

        /// <summary>
        /// Converts Authentication byte into list
        /// </summary>
        internal static List<AuthType> AuthList(byte data)
        {
            List<AuthType> authTypes = new List<AuthType>(6);

            BitArray auth = ByteToBits(data);
            for (int i = 0; i < 6; i++)
            {
                if (auth[i])
                    authTypes.Add((AuthType)i);
            }

            return authTypes;
        }

        /// <summary>
        /// Converts Authentication List into byte
        /// </summary>
        internal static byte AuthByte(List<AuthType> authTypes)
        {
            // Create bit array for byte
            BitArray auth = new BitArray(8, false);
            byte index = 0;

            // iterate list for AuthType items,
            // if present enable the item based
            // on it's index
            foreach (AuthType item in authTypes)
            {
                index = (byte)item;
                auth[index] = true;
            }
            
            return BitsToByte(auth);
        }

        /// <summary>
        /// Generates BitArray from a Byte
        /// </summary>
        internal static BitArray ByteToBits(byte data)
        {
            byte[] arr = new byte[1];
            arr[0] = data;

            return new BitArray(arr);
        }

        /// <summary>
        /// Generates BitArray from a Byte
        /// </summary>
        internal static byte BitsToByte(BitArray data)
        {
            byte[] arr = new byte[1];
            data.CopyTo(arr, 0);

            return arr[0];
        }
    }

    /// <summary>
    /// Serial Modem Config Classes
    /// </summary>
    internal class SerialConfig
    {
        internal abstract class SerialConfigBase
        {
            // Serial Modem Configuraiton Paramater Selector
            private byte _selector;

            // Serial Modem Configuration payload
            private byte[] _payload;

            /// <summary>
            /// Serial Modem Configuraiton Paramater Selector
            /// </summary>
            public byte Selector
            {
                get { return this._selector; }
                protected set { this._selector = value; }
            }

            /// <summary>
            /// Serial Modem configuration Payload
            /// </summary>
            public byte[] Payload
            {
                get { return this._payload; }
                protected set { this._payload = value; }
            }

            /// <summary>
            /// initialize class
            /// </summary>
            public SerialConfigBase(byte selector)
            {
                this._selector = selector;
            }

            /// <summary>
            /// initialize class
            /// </summary>
            internal SerialConfigBase()
            {
            }

            /// <summary>
            /// Initialize the class with payload.
            /// </summary>
            internal virtual void Initialize(byte[] payload)
            {
                this._payload = payload;
            }

        }

        /// <summary>
        /// Read-Only Set In-Progress Paramaters
        /// for Serial/Modem Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class SetInProcess : SerialConfigBase
        {
            /// <summary>
            /// [7:2] - reserved 
            /// [1:0] - 00b = set complete.
            ///       - 01b = set in progress.
            ///       - 10b = commit write (optional). 
            ///       - 11b = reserved.
            /// </summary>
            private byte _data;

            /// <summary>
            /// Process state, default = unknown.
            /// </summary>
            private ProcessState _pState = ProcessState.Unknown;

            /// <summary>
            /// Serial Modem Configuraiton 
            /// Paramater State.
            /// </summary>
            internal ProcessState SetState
            {
                get { return this._pState; }
            }

            /// <summary>
            /// Enum of allowed response states
            /// Ipmi specification [25-4]:
            /// [7:2] - reserved 
            /// [1:0] - 00b = set complete.
            ///       - 01b = set in progress.
            ///       - 10b = commit write (optional). 
            ///       - 11b = reserved.
            /// </summary>
            internal enum ProcessState : byte
            {
                SetComplete = 0x00,
                SetInProgress = 0x01,
                CommitWrite = 0x02,
                Unknown = 0xA0
            }

            internal SetInProcess()
            {
                base.Selector = 0x00;
            }

            internal SetInProcess(byte data)
                : this()
            {
                // [7:2] - reserved 
                // [1:0] - 00b = set complete.
                //       - 01b = set in progress.
                //       - 10b = commit write (optional). 
                //       - 11b = reserved.
                this._data = (byte)(data & 0x03);

                if (Enum.IsDefined(typeof(ProcessState), this._data))
                {
                    this._pState = (ProcessState)this._data;
                }

                base.Payload = new byte[1] { this._data };
            }

            internal SetInProcess(byte[] data)
                : this(data[0])
            {
                base.Payload = data;
            }

        }

        /// <summary>
        /// Read-Only Authentication Types Support Paramaters
        /// for Serial/Modem Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class AuthTypeSupport : SerialConfigBase
        {
            /// <summary>
            /// [5] -  OEM proprietary
            /// [4] -  straight password / key 
            /// [3] -  reserved 
            /// [2] -  MD5 
            /// [1] -  MD2 
            /// [0] -  none 
            /// </summary>
            private byte _data;

            /// <summary>
            /// Initializes list of authentication types.
            /// </summary>
            private List<SerialBaseSharedFunctions.AuthType> _authTypes;

            /// <summary>
            /// List of available authentiation types.
            /// </summary>
            internal List<SerialBaseSharedFunctions.AuthType> Authentication
            {
                get { return this._authTypes; }
            }

            internal AuthTypeSupport()
            {
                base.Selector = 0x01;
            }

            internal AuthTypeSupport(byte data)
                : this()
            {
                // [5] -  OEM proprietary
                // [4] -  straight password / key 
                // [3] -  reserved 
                // [2] -  MD5 
                // [1] -  MD2 
                // [0] -  none 
                this._data = (byte)(((byte)(data << 2)) >> 2);

                _authTypes = SerialBaseSharedFunctions.AuthList(this._data);
            }

            internal AuthTypeSupport(byte[] data)
                : this(data[0])
            {

            }
        }

        /// <summary>
        /// Authentication Type Enable Paramaters
        /// for Serial/Modem Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class AuthTypeEnable : SerialConfigBase
        {
            /// <summary>
            /// Initializes list of authentication types for callback.
            /// </summary>
            private List<SerialBaseSharedFunctions.AuthType> _callBack;

            /// <summary>
            /// Initializes list of authentication types for user.
            /// </summary>
            private List<SerialBaseSharedFunctions.AuthType> _user;

            /// <summary>
            /// Initializes list of authentication types for operator.
            /// </summary>
            private List<SerialBaseSharedFunctions.AuthType> _operator;

            /// <summary>
            /// Initializes list of authentication types for administrator.
            /// </summary>
            private List<SerialBaseSharedFunctions.AuthType> _administrator;

            /// <summary>
            /// Initializes list of authentication types for oem.
            /// </summary>
            private List<SerialBaseSharedFunctions.AuthType> _oem;

            /// <summary>
            /// List of available authentiation types.
            /// </summary>
            internal List<SerialBaseSharedFunctions.AuthType> Callback
            {
                get { return this._callBack; }
            }

            /// <summary>
            /// List of available authentiation types.
            /// </summary>
            internal List<SerialBaseSharedFunctions.AuthType> User
            {
                get { return this._user; }
            }

            /// <summary>
            /// List of available authentiation types.
            /// </summary>
            internal List<SerialBaseSharedFunctions.AuthType> Operator
            {
                get { return this._operator; }
            }

            /// <summary>
            /// List of available authentiation types.
            /// </summary>
            internal List<SerialBaseSharedFunctions.AuthType> Administrator
            {
                get { return this._administrator; }
            }

            /// <summary>
            /// List of available authentiation types.
            /// </summary>
            internal List<SerialBaseSharedFunctions.AuthType> OEM
            {
                get { return this._oem; }
            }

            /// <summary>
            /// Initialize Class 
            /// </summary>
            internal AuthTypeEnable()
            {
                base.Selector = 0x02;
            }

            /// <summary>
            /// Initialize Class with Payload 
            /// </summary>
            internal AuthTypeEnable(byte[] data)
                : this()
            {
                base.Payload = data;

                if (data.Length == 5)
                {
                    // [5] -  OEM proprietary
                    // [4] -  straight password / key 
                    // [3] -  reserved 
                    // [2] -  MD5 
                    // [1] -  MD2 
                    // [0] -  none 
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = (byte)(((byte)(data[i] << 2)) >> 2);
                    }

                    // set user option lists
                    _callBack = SerialBaseSharedFunctions.AuthList(data[0]);
                    _user = SerialBaseSharedFunctions.AuthList(data[1]);
                    _operator = SerialBaseSharedFunctions.AuthList(data[2]);
                    _administrator = SerialBaseSharedFunctions.AuthList(data[3]);
                    _oem = SerialBaseSharedFunctions.AuthList(data[4]);

                }
            }

            /// <summary>
            /// Initialize class with lists
            /// </summary>
            internal AuthTypeEnable(List<SerialBaseSharedFunctions.AuthType> callback, List<SerialBaseSharedFunctions.AuthType> user,
                                    List<SerialBaseSharedFunctions.AuthType> operaters, List<SerialBaseSharedFunctions.AuthType> administrators,
                                    List<SerialBaseSharedFunctions.AuthType> oem)
                : this()
            {
                // create payload byte array
                byte[] authTypes = new byte[5];
                // set bytes using SharedFunctions.AuthByte(AuthType)
                authTypes[0] = SerialBaseSharedFunctions.AuthByte(callback);
                authTypes[1] = SerialBaseSharedFunctions.AuthByte(user);
                authTypes[2] = SerialBaseSharedFunctions.AuthByte(operaters);
                authTypes[3] = SerialBaseSharedFunctions.AuthByte(administrators);
                authTypes[4] = SerialBaseSharedFunctions.AuthByte(oem);
                // set the payload byte.
                base.Payload = authTypes;
            }
        }

        /// <summary>
        /// Connection Mode Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class ConnectionMode : SerialConfigBase
        {
            // raw data byte
            private byte _data;

            // serial modem connection modes
            private List<Modes> _modes = new List<Modes>(3);

            // serial modem connection type
            private Connection _connType;

            /// <summary>
            /// Serial Modem Connection modes
            /// </summary>
            internal List<Modes> ConnectionModes
            {
                get { return this._modes; }
                set { this._modes = value; }
            }

            /// <summary>
            /// Connection type
            /// </summary>
            internal Connection ConnectionType
            {
                get { return this._connType; }
                set { this._connType = value; }
            }

            internal byte Data
            {
                get { return this._data; }
            }

            /// <summary>
            /// Serial Modem Connection Types
            /// </summary>
            internal enum Connection : byte
            {
                ModemConnect = 0x00,
                DirectConnect = 0x01,
                Unknown = 0xA0
            }

            /// <summary>
            /// Enum of allowed response states
            /// Ipmi specification [25-4]:
            /// [5:3] -  reserved 
            /// [2] -  1b =  enable Terminal mode 
            /// [1] -  1b =  enable PPP mode 
            /// [0] -  1b =  enable Basic mode 
            /// </summary>
            internal enum Modes : byte
            {
                Basic = 0x00,
                PPP = 0x01,
                Terminal = 0x02,
                Unknown = 0xA0
            }

            /// <summary>
            /// Initialize class with connection paramaters
            /// </summary>
            internal ConnectionMode(Connection connType, List<Modes> connModes)
                : this()
            {

                // clear the modes list
                _modes.Clear();

                // create bit array to set bits.
                BitArray commModes = new BitArray(8, false);

                byte index;
                foreach (Modes item in connModes)
                {
                    index = (byte)item;
                    commModes[index] = true;
                }

                if (connType == Connection.DirectConnect)
                {
                    commModes[7] = true;
                }

                // set the class payload.
                this._data = SerialBaseSharedFunctions.BitsToByte(commModes);
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal ConnectionMode()
            {
                base.Selector = 0x03;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal ConnectionMode(byte data)
                : this()
            {
                // Ipmi specification [25-4]:
                // [7] -   0b = Modem Connect mode 
                //         1b = Direct Connect mode 
                // [6] -  reserved 
                // [5:3] -  reserved 
                // [2] -  1b =  enable Terminal mode 
                //              (Note: Terminal mode auto-detect also requires that the “Enable 
                //              Baseboard-to-BMC switch on <ESC>(“ option be enabled in the Mux 
                //              Switch Configuration parameters, below.) 
                // [1] -  1b =  enable PPP mode 
                // [0] -  1b =  enable Basic mode
                BitArray connModes = SerialBaseSharedFunctions.ByteToBits(data);

                for (int i = 0; i < 3; i++)
                {
                    if (connModes[i])
                        _modes.Add((Modes)i);
                }

                if (connModes[7])
                {
                    _connType = Connection.DirectConnect;
                }
                else
                {
                    _connType = Connection.ModemConnect;
                }

                // create a payload package.
                byte[] payload = new byte[1];
                payload[0] = data;

                // set the base payload
                base.Payload = payload;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal ConnectionMode(byte[] data)
                : this(data[0])
            {

            }
        }

        /// <summary>
        /// Session Inactivity Timeout Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class SessionTimeout : SerialConfigBase
        {
            private byte _data;

            /// <summary>
            /// TimeOut in seconds.  0 = session does not timeout.
            /// </summary>
            internal int TimeOut
            {
                get { return (unchecked(Convert.ToInt32(_data) * 30)); }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal SessionTimeout()
            {
                base.Selector = 0x04;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal SessionTimeout(byte data)
                : this()
            {
                // Ipmi specification [25-4]:
                // [7:4] - Reserved
                // [3:0] - Inactivity timeout in 30 second increments. 1-based. 0h = session does not 
                //         timeout and close due to inactivity. 
                this._data = (byte)(data & 0x0F);

                // set the base payload
                base.Payload = new byte[1] { this._data };
            }

            /// <summary>
            ///  initialize override
            /// </summary>
            internal override void Initialize(byte[] payload)
            {
                // Ipmi specification [25-4]:
                // [7:4] - Reserved
                // [3:0] - Inactivity timeout in 30 second increments. 1-based. 0h = session does not 
                //         timeout and close due to inactivity. 
                this._data = (byte)(payload[0] & 0x0F);

                // set the base payload
                base.Payload = payload;
            }

        }

        /// <summary>
        /// Channel Callback Control Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class ChannelCallback : SerialConfigBase
        {
            // Callback enabled Options list
            private List<CallBackEnabled> _enabledCallBack = new List<CallBackEnabled>();

            // CBCP Negotiation list
            private List<CbcpNegotiation> _cbcpNegotiation = new List<CbcpNegotiation>();

            // data 3: Callback destination 1
            // data 4: Callback destination 2
            // data 5: Callback destination 3
            private byte[] _destinations = new byte[3];

            /// <summary>
            /// Callback Destinations
            /// </summary>
            internal byte[] Destinations
            {
                get { return this._destinations; }
            }

            /// <summary>
            /// CBCP Negotiation Options
            /// </summary>
            internal List<CbcpNegotiation> CbcpNegotiationOptions
            {
                get { return this._cbcpNegotiation; }
            }

            /// <summary>
            /// Callback protocols enabled
            /// </summary>
            internal List<CallBackEnabled> EnabledCallBack
            {
                get { return this._enabledCallBack; }
            }

            /// <summary>
            ///  Call Back Enabled Options
            /// </summary>
            internal enum CallBackEnabled : byte
            {
                Cbcp = 0x01,
                Ipmi = 0x00
            }

            /// <summary>
            /// CBCP Negotiation Options
            /// </summary>
            internal enum CbcpNegotiation : byte
            {
                /// <summary>
                /// Callback to one from list of possible numbers
                /// </summary>
                FromList = 0x03,

                /// <summary>
                /// Enable user-specifiable callback number
                /// </summary>
                UserSpecified = 0x02,

                /// <summary>
                /// Enable Pre-specified numbe
                /// </summary>
                PreSpecified = 0x01,

                /// <summary>
                /// Allow caller to request that callback not be used
                /// </summary>
                NoCallback = 0x00
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal ChannelCallback()
            {
                base.Selector = 0x05;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal ChannelCallback(byte[] data)
                : this()
            {
                // set base payload
                base.Payload = data;

                // Ipmi specification [25-3]:
                // [7:2] -   reserved 
                // [1] -   1b =  enable CBCP callback protocol 
                // [0] -   1b =  enable IPMI callback 
                if (((byte)(data[0] & 0x01)) == 0x01)
                    _enabledCallBack.Add(CallBackEnabled.Ipmi);

                if (((byte)(data[0] & 0x02)) == 0x02)
                    _enabledCallBack.Add(CallBackEnabled.Cbcp);

                // Ipmi specification [25-3]:
                // data 2 - CBCP Negotiation Options.  
                // [7:4] -  reserved. 
                // [3] -  1b = enable callback to one from list of possible numbers 
                // [2] -  1b = enable user-specifiable callback number.
                // [1] -  1b = enable Pre-specified number.
                // [0] -  1b = enable No Callback. Allow caller to request that callback not be used. 
                if (((byte)(data[1] & 0x01)) == 0x01)
                    _cbcpNegotiation.Add(CbcpNegotiation.NoCallback);
                if (((byte)(data[1] & 0x02)) == 0x02)
                    _cbcpNegotiation.Add(CbcpNegotiation.PreSpecified);
                if (((byte)(data[1] & 0x03)) == 0x03)
                    _cbcpNegotiation.Add(CbcpNegotiation.UserSpecified);
                if (((byte)(data[1] & 0x04)) == 0x04)
                    _cbcpNegotiation.Add(CbcpNegotiation.FromList);

                // copy destinations to destination array
                Buffer.BlockCopy(data, 2, _destinations, 0, 3);
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal ChannelCallback(List<CallBackEnabled> callback, List<CbcpNegotiation> cbcp, byte[] destinations)
                : this()
            {

                // copy destinations to destinations
                Buffer.BlockCopy(destinations, 0, _destinations, 0, 3);

                // get bits
                BitArray bitData = SerialBaseSharedFunctions.ByteToBits(0x00);

                foreach (CallBackEnabled item in callback)
                {
                    if (item == CallBackEnabled.Ipmi)
                        bitData.Set(0, true);

                    if (item == CallBackEnabled.Cbcp)
                        bitData.Set(1, true);
                }

                // copy modified bit array back into byte array.
                byte data1 = SerialBaseSharedFunctions.BitsToByte(bitData);

                // set all bits to false for byte 2 processing
                bitData.SetAll(false);

                // Ipmi specification [25-3]:
                // data 2 - CBCP Negotiation Options.  
                // [7:4] -  reserved. 
                // [3] -  1b = enable callback to one from list of possible numbers 
                // [2] -  1b = enable user-specifiable callback number.
                // [1] -  1b = enable Pre-specified number.
                // [0] -  1b = enable No Callback. Allow caller to request that callback not be used. 
                foreach (CbcpNegotiation item in cbcp)
                {
                    if (item == CbcpNegotiation.NoCallback)
                        bitData.Set(0, true);

                    if (item == CbcpNegotiation.PreSpecified)
                        bitData.Set(1, true);

                    if (item == CbcpNegotiation.UserSpecified)
                        bitData.Set(2, true);

                    if (item == CbcpNegotiation.FromList)
                        bitData.Set(3, true);
                }

                // copy modified bit array back into byte array.
                byte data2 = SerialBaseSharedFunctions.BitsToByte(bitData);

                byte[] payload = new byte[5];
                // copy data 1 to payload.
                payload[0] = data1;
                // copy data 2 to payload.
                payload[1] = data2;
                // copy destinations to payload.
                Buffer.BlockCopy(destinations, 0, payload, 2, 3);

                // set payload.
                base.Payload = payload;
            }

        }

        /// <summary>
        /// Session Termination Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class SessionTermination : SerialConfigBase
        {
            // payload data
            private byte _data;

            private bool _sessionTimeout;

            private bool _dcdClose;

            /// <summary>
            /// Terminate Session on Timeout
            /// Enabled:
            /// </summary>
            internal bool SessionTimeout
            {
                get { return this._sessionTimeout; }
            }

            /// <summary>
            /// Terminate Session on DCD Signal
            /// Enabled:
            /// </summary>
            internal bool DcdClose
            {
                get { return this._dcdClose; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal SessionTermination()
            {
                base.Selector = 0x06;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal SessionTermination(byte data)
                : this()
            {
                // Ipmi specification [25-3]:
                // [7:2] -  reserved 
                // [1] -  1b = enable session inactivity timeout 
                //        0b = disable session inactivity timeout 
                // [0] -  1b = close session on loss of DCD
                //        0b = ignore DCD
                this._data = (byte)(data & 0x03);

                if ((byte)(this._data & 0x01) == 0x01)
                    _dcdClose = true;

                if ((byte)(this._data & 0x02) == 0x02)
                    _sessionTimeout = true;

                // set the base payload
                base.Payload = new byte[1] { this._data };
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal SessionTermination(bool dcdClose, bool timeout)
                : this()
            {

                byte payload = 0x00;

                if (dcdClose)
                    payload = (byte)(payload | 0x01);

                if (timeout)
                    payload = (byte)(payload | 0x02);

                // set payload.
                base.Payload = new byte[1] { payload };
            }

            /// <summary>
            ///  initialize override
            /// </summary>
            internal override void Initialize(byte[] payload)
            {
                // initialize to false.
                _dcdClose = false; _sessionTimeout = false;

                // Get the payload packet.
                // [7:2] -   reserved
                // [1] - 1b = enable session inactivity timeout
                //       0b = disable session inactivity timeout
                // [0] - 1b = close session on loss of DCD (this should be used as the default setting for 
                //            both Modem Connect and Direct Connect mode) [Also see bit to enable mux 
                //            switchon DCD assertion, in Mux Switch Controlparameter, below]
                //       0b = ignore DCD (DCD is never ignored in Modem Mode)  
                byte data = (byte)(payload[0] & 0x03);
                
                if ((byte)(data & 0x01) == 0x01)
                {
                    _dcdClose = true;
                }

                if ((byte)(data & 0x02) == 0x02)
                {
                    _sessionTimeout = true;
                }
            }

        }

        /// <summary>
        /// Ipmi Message Comm Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class IpmiMessageComm : SerialConfigBase
        {
            private FlowControl _flowCtrl;

            private BitRate _bitRate;

            private bool _dtrHangup;

            internal bool DTRHangUp
            {
                get { return this._dtrHangup; }
            }

            internal BitRate Speed
            {
                get { return this._bitRate; }
            }

            internal FlowControl FlowCtrl
            {
                get { return this._flowCtrl; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal IpmiMessageComm()
            {
                base.Selector = 0x07;
            }

            internal enum FlowControl : byte
            {
                None = 0x00,
                RTS_CTS = 0x01,
                XON_XOFF = 0x02,
                Unknown = 0xA0
            }

            internal enum BitRate : byte
            {
                Unknown = 0x00,
                Baud_9600_bps = 0x06,
                Baud_19_2_kbps = 0x07,
                Baud_38_4_kbps = 0x08,
                Baud_57_6_kbps = 0x09,
                Baud_115_2_kbps = 0xA0
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal IpmiMessageComm(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - flow control, DTR hang-up, asynch format 
                // [7:6] - Flow control 
                //          00b = No flow control 
                //          01b = RTS/CTS flow control (a.k.a. hardware handshake) 
                //          10b = XON/XOFF flow control (optional)
                //          11b = Reserved. 
                // [5] -  DTR hang-up 
                //          0b = disable DTR hang-up 
                //          1b = enable DTR hang-up 
                // [4:0] -  reserved. 

                // bit shift flow control
                byte flowctrl = (byte)(data[0] >> 6);

                // data 2 - bit rate 
                // [7:4] -  reserved 
                // [3:0] -  0-5h = reserved. 
                //          6h = 9600 bps 
                //          7h = 19.2 kbps (required) 
                //          8h = 38.4 kbps 
                //          9h = 57.6 kbps 
                //          Ah = 115.2 kbps 
                byte bitRate = (byte)(((byte)(data[1] << 4)) >> 4);

                // cast flow control to enum value.
                if (Enum.IsDefined(typeof(FlowControl), flowctrl))
                {
                    // Flow Control
                    _flowCtrl = (FlowControl)flowctrl;
                }
                else
                {
                    _flowCtrl = FlowControl.Unknown;
                }

                // DTR HangUp
                if ((byte)((byte)(data[0] >> 5) & 0x01) == 0x01)
                    _dtrHangup = true;

                // Bit Rate
                if (Enum.IsDefined(typeof(BitRate), bitRate))
                {
                    _bitRate = (BitRate)bitRate;
                }
                else
                {
                    _bitRate = BitRate.Unknown;
                }

                // set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal IpmiMessageComm(FlowControl flow, bool hangUp, BitRate rate)
                : this()
            {

                byte[] payload = new byte[2] { (byte)flow, (byte)rate };

                if (hangUp)
                    payload[0] = (byte)(payload[0] | 0x20);


                // set payload.
                base.Payload = payload;
            }
        }

        /// <summary>
        /// Mux Switch Control Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class MuxSwitchControl : SerialConfigBase
        {
            private List<PortSwitching> _switching = new List<PortSwitching>(7);

            private List<PortSharing> _sharing = new List<PortSharing>(4);

            internal List<PortSwitching> SerialPortSwitching
            {
                get { return this._switching; }
            }

            internal List<PortSharing> SerialPortSharing
            {
                get { return this._sharing; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal MuxSwitchControl()
            {
                base.Selector = 0x08;
            }

            internal enum PortSwitching : byte
            {
                /// <summary>
                /// Enable mux switch on DCD loss
                /// </summary>
                MuxDcdSwitch = 0x00,
                /// <summary>
                /// Enable Baseboard-to-BMC switch on  [MSVT]<ESC>(
                /// </summary>
                MSVT_ESC = 0x01,
                /// <summary>
                /// Enable BMC-to-Baseboard switch on [MSVT] <ESC>Q
                /// </summary>
                MSVT_ESC_Q = 0x02,
                /// <summary>
                /// Enable switch on PPP IPMI-RMCP pattern 
                /// </summary>
                PPP_IPMI_RMCP = 0x03,
                /// <summary>
                ///  Enable Baseboard-to-BMC switch on detecting basic mode Get Channel 
                ///  Authentication Capabilities message pattern in serial stream. 
                /// </summary>
                Get_Channel_Authentication = 0x04,
                /// <summary>
                /// Enable hard reset on [MSVT] <ESC>R<ESC>r<ESC>R escape sequence 
                /// </summary>
                Reset_ESC_R_ESC_R = 0x05,
                /// <summary>
                /// Enable system power-up/wakeup via [MSVT] <ESC>^ escape sequence 
                /// </summary>
                WakeUpOnLan = 0x06
            }

            internal enum PortSharing : byte
            {
                /// <summary>
                /// Serial/Modem Connection Active message, with retry and response.
                /// </summary>
                SendActive = 0x00,
                /// <summary>
                /// Enable Serial/Modem Connection Active message during direct-call 
                /// </summary>
                SendActiveDirectCall = 0x01,
                /// <summary>
                /// Enable Serial/Modem Connection Active message during Callback connection
                /// </summary>
                SendActiveCallback = 0x02,
                /// <summary>
                /// Enable Serial Port Sharing (can force mux setting using Set Serial/Modem Mux command) 
                /// </summary>
                SerialPortSharing = 0x03
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal MuxSwitchControl(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - Serial Port Switching 
                // [7] -  reserved 
                // [6] -  0b = Disable system power-up/wakeup via [MSVT] <ESC>^ escape sequence 
                //        1b = Enable system power-up/wakeup via [MSVT] escape sequence
                // [5] -  0b = Disable hard reset on [MSVT] <ESC>R<ESC>r<ESC>R escape sequence 
                //        1b = Enable hard reset on [MSVT] escape sequence
                // [4] -  0b =  Disable Baseboard-to-BMC switch on detecting basic mode Get Channel 
                //              Authentication Capabilities message pattern in serial stream. 
                //        1b =  Enable Baseboard-to-BMC switch on detecting basic mode Get Channel 
                //              Authentication Capabilities message pattern in serial stream. 
                // [3] -  0b =  Disable switch to BMC on PPP IPMI-RMCP pattern 
                //        1b =  Enable switch on PPP IPMI-RMCP pattern 
                // [2] -  0b =  Disable BMC-to-Baseboard switch on [MSVT] <ESC>Q 
                //        1b =  Enable BMC-to-Baseboard switch on [MSVT] <ESC>Q
                // [1] -  0b =  Disable Baseboard-to-BMC switch on [MSVT] <ESC>( 
                //        1b =  Enable Baseboard-to-BMC switch on  [MSVT]<ESC>(
                // [0] -  Following only used in Direct Connect Mode (ignored in Modem Mode) 
                //        0b =  Disable mux switch to BMC on DCD loss 
                //        1b =  Enable mux switch on DCD loss 

                // flush list.
                _switching.Clear();

                BitArray data1 = SerialBaseSharedFunctions.ByteToBits(data[0]);

                for (int i = 0; i < 7; i++)
                {
                    if (data1[i])
                        _switching.Add((PortSwitching)i);
                }


                // data 2 
                // [7:4] -  reserved 
                // [3] -  0b =   Disable Serial Port Sharing. (cannot force mux setting via Set Serial/Modem Mux command)  
                //        1b =   Enable Serial Port Sharing (can force mux setting using Set Serial/Modem Mux command) 
                // [2] -  0b =   Disable Serial/Modem Connection Active message during Callback connection. 
                //        1b =   Enable Serial/Modem Connection Active message during Callback connection. 
                // [1] -  0b =   Disable Serial/Modem Connection Active message during direct-call 
                //        1b =   Enable Serial/Modem Connection Active message during direct-call 
                // [0] -  0b =   Send Serial/Modem Connection Active message only once before switching mux to system 
                //        1b =   Mux switch acknowledge. Retry Serial/Modem Connection Active 

                // flush the list
                _sharing.Clear();

                // create bit array
                BitArray data2 = SerialBaseSharedFunctions.ByteToBits(data[1]);

                for (int i = 0; i < 4; i++)
                {
                    if (data2[i])
                        _sharing.Add((PortSharing)i);
                }



                // set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal MuxSwitchControl(List<PortSwitching> switching, List<PortSharing> portSharing)
                : this()
            {
                // Create bit array for byte
                BitArray arr = new BitArray(8, false);
                byte index = 0;

                // iterate list for PortSwitching items,
                // if present enable the item based
                // on it's index
                foreach (PortSwitching item in switching)
                {
                    index = (byte)item;
                    arr.Set(index, true);
                }

                // get the byte value
                byte data1 = SerialBaseSharedFunctions.BitsToByte(arr);

                // flusht the bits
                arr.SetAll(false);
                // iterate list for PortSwitching items,
                // if present enable the item based
                // on it's index

                foreach (PortSharing item in portSharing)
                {
                    index = (byte)item;
                    arr.Set(index, true);
                }

                // get the byte value
                byte data2 = SerialBaseSharedFunctions.BitsToByte(arr);

                // set the base payload
                base.Payload = new byte[2] { data1, data2 };

            }
        }

        /// <summary>
        /// Modem Ring Time Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class ModemRingTime : SerialConfigBase
        {

            private byte _duration;

            private byte _deadTime;

            /// <summary>
            /// Ring duration in 500 ms increments. 1 based. 
            //  00_0000b = BMC switches mux immediately on first detected transition of RI. 
            //  11_1111b (3Fh) = reserved 
            /// </summary>
            internal byte Duration
            {
                get { return this._duration; }
            }

            /// <summary>
            /// Amount of time, in 500 ms increments, that the RI signal must be deasserted 
            //  before the BMC determines that ringing has stopped. 0h = 500 ms. 
            /// </summary>
            internal byte DeadTime
            {
                get { return this._deadTime; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal ModemRingTime()
            {
                base.Selector = 0x09;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal ModemRingTime(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:

                // data 1 - Ring Duration 
                // [7:6] -  reserved 
                // [5:0] -  Ring duration in 500 ms increments. 1 based. 
                //          00_0000b = BMC switches mux immediately on first detected transition of RI. 
                //          11_1111b (3Fh) = reserved 
                _duration = (byte)((byte)(data[0] << 2) >> 2);

                //data 2 - Ring Dead Time 
                // [7:4] -  reserved 
                // [3:0] -  Amount of time, in 500 ms increments, that the RI signal must be deasserted 
                //          before the BMC determines that ringing has stopped. 0h = 500 ms. 

                _deadTime = (byte)((byte)(data[1] << 4) >> 4);

                // set the base payload
                base.Payload = new byte[2] { this._duration, this._deadTime };
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal ModemRingTime(ushort duration, ushort deadTime)
                : this()
            {
                base.Payload = new byte[2] { (byte)(duration & byte.MaxValue), (byte)(deadTime & byte.MaxValue) };
            }
        }

        /// <summary>
        /// Modem Init Str Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class ModemInitStr : SerialConfigBase
        {

            private string _initStr = string.Empty;

            internal string ModemInitializeString
            {
                get { return this._initStr; }
            }


            /// <summary>
            /// Initialize class
            /// </summary>
            internal ModemInitStr()
            {
                base.Selector = 0x0A;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal ModemInitStr(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // Sets the modem initialization string data. The BMC automatically follows this string with 
                // an <enter> character when sending it to the modem. 
                // data 1 -   set selector = 16-byte block number to set, 1 based. Two blocks required, at 
                // least three recommended. 
                // data 2:N -   Modem Init string data. String is stored as null terminated ASCII string.

                _initStr = ASCIIEncoding.ASCII.GetString(data, 1, Convert.ToInt32(data[0]));

                // set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal ModemInitStr(int size, string initString)
                : this()
            {
                // add 1 byte
                int lenght = Convert.ToInt32((byte)(size & byte.MaxValue)) + 1;

                byte[] strArr = ASCIIEncoding.ASCII.GetBytes(initString);

                byte[] payload = new byte[lenght];
                payload[0] = (byte)(size & byte.MaxValue);
                Buffer.BlockCopy(strArr, 0, payload, 1, size);

                base.Payload = payload;
            }
        }

        /// <summary>
        /// Modem Escape Seq Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class ModemEscapeSeq : SerialConfigBase
        {
            private string _escapeSeq = string.Empty;

            internal string EscapeSeq
            {
                get { return this._escapeSeq; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal ModemEscapeSeq()
            {
                base.Selector = 0x0B;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal ModemEscapeSeq(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data1:5- Null terminated ASCII string for the Escape string to be sent to the modem. If 
                // this parameter is empty, or this configuration option is not implemented, the default ‘+++’ 
                // sequence will be used. [If a full five characters are provided, the last character does not 
                // need to be null]

                _escapeSeq = ASCIIEncoding.ASCII.GetString(data);

                // set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal ModemEscapeSeq(string escapeSeq)
                : this()
            {

                byte[] payload = ASCIIEncoding.ASCII.GetBytes(escapeSeq);

                if (payload.Length != 5)
                    Array.Resize<byte>(ref payload, 5);

                base.Payload = payload;
            }
        }

        /// <summary>
        /// Modem HangUp Seq Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class ModemHangUpSeq : SerialConfigBase
        {
            private string _hangUpSeq = string.Empty;

            internal string ModemHangSeq
            {
                get { return this._hangUpSeq; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal ModemHangUpSeq()
            {
                base.Selector = 0x0C;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal ModemHangUpSeq(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data1:8 - Null terminated ASCII string for the hang-up string to be sent to the modem. 
                // The BMC automatically follows this string with an <enter> character when sending it to 
                // the modem. If this parameter is empty, or this configuration option is not implemented, 
                // the default ‘ATH’ sequence will be used. [If a full eight characters are provided, the last 
                // character does not need to be null] 

                _hangUpSeq = ASCIIEncoding.ASCII.GetString(data);

                // set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal ModemHangUpSeq(string hangUpSeq)
                : this()
            {

                byte[] payload = ASCIIEncoding.ASCII.GetBytes(hangUpSeq);

                if (payload.Length != 8)
                    Array.Resize<byte>(ref payload, 8);

                base.Payload = payload;
            }

        }

        /// <summary>
        /// Modem Dial Cmd Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class ModemDialCmd : SerialConfigBase
        {
            private string _dialSeq = string.Empty;

            internal string ModemDialSeq
            {
                get { return this._dialSeq; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal ModemDialCmd()
            {
                base.Selector = 0x0D;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal ModemDialCmd(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data1:8 - Null terminated ASCII string for the modem string used to initiate a dial 
                // sequence with the modem. If this parameter is empty, or this configuration option is not 
                // implemented, the default ‘ATD’ sequence will be used. [If a full eight characters are 
                // provided, the last character does not need to be null]

                _dialSeq = ASCIIEncoding.ASCII.GetString(data);

                // set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal ModemDialCmd(string dialSeq)
                : this()
            {

                byte[] payload = ASCIIEncoding.ASCII.GetBytes(dialSeq);

                if (payload.Length != 8)
                    Array.Resize<byte>(ref payload, 8);

                base.Payload = payload;
            }
        }

        /// <summary>
        /// Page Blackout Interval Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class PageBlackoutInterval : SerialConfigBase
        {
            private byte _minutes;

            internal byte IntervaleMinutes
            {
                get { return this._minutes; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal PageBlackoutInterval()
            {
                base.Selector = 0x0E;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal PageBlackoutInterval(byte data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - Dial Page, Directed Alert, or TAP Blackout Interval in minutes. 1 based. 00h = no 
                // blackout
                this._minutes = data;

                // set the base payload
                base.Payload = new byte[1] { data };
            }
        }

        /// <summary>
        /// Community String Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class CommunityStr : SerialConfigBase
        {
            private string _community = string.Empty;

            internal string Community
            {
                get { return this._community; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal CommunityStr()
            {
                base.Selector = 0x0F;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal CommunityStr(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data1:8 - Null terminated ASCII string for the modem string used to initiate a dial 
                // sequence with the modem. If this parameter is empty, or this configuration option is not 
                // implemented, the default ‘ATD’ sequence will be used. [If a full eight characters are 
                // provided, the last character does not need to be null]

                _community = ASCIIEncoding.ASCII.GetString(data);

                // set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal CommunityStr(string community)
                : this()
            {

                byte[] payload = ASCIIEncoding.ASCII.GetBytes(community);

                if (payload.Length != 8)
                    Array.Resize<byte>(ref payload, 8);

                base.Payload = payload;
            }
        }

        /// <summary>
        /// Number of Alert Destination Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class NumberofAlertDest : SerialConfigBase
        {
            private byte _data;

            internal int NumberOfDestinations
            {
                get { return Convert.ToInt32(this._data); }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal NumberofAlertDest()
            {
                base.Selector = 0x10;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal NumberofAlertDest(byte data)
                : this()
            {
                // Ipmi specification [25-3]:
                // [7:5] -   reserved. 
                // [3:0] -  Number of non-volatile alert destinations. One minimum, fifteen non-volatile 
                //          destinations maximum. It is recommended that an implementation provide at 
                //          least two destination numbers for each page/alert type supported, plus two for 
                //          callback if callback is supported. 
                //          0h = Page Alerting not supported. 

                _data = (byte)((byte)(data << 4) >> 4); ;

                // set the base payload
                base.Payload = new byte[1] { _data };
            }
        }

        /// <summary>
        /// Destination Info Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class DestinationInfo : SerialConfigBase
        {
            // Destination selector
            private byte _selector;

            // alert timeout
            private byte _timeOut;

            // Destination acknowledgement
            private DestinationAckType _desAckType;

            // Desination Type byte 2 [3:0]
            private DestinationType _desType;

            private byte _alertRetry;

            private byte _callRetry;

            private byte _hiPayload;

            private byte _loPayload;

            /// <summary>
            /// Data 5: Destination Type Specific: 
            /// For Destination Type = Dial Page: 
            ///     Dial String Selector 
            /// For Destination Type = TAP Page: 
            ///     reserved 
            /// For Destination Type = PPP Alert: 
            ///     Destination IP Address Selector 
            /// For Destination Type = PPP Mode Callback or Basic Mode Callback: 
            ///     Destination IP Address Selector for PPP Mode Callback (The IP Address
            ///     used to enable the BMC to send a Serial/Modem Connection Active 
            ///     message once the connection has been established.) 
            /// </summary>
            internal byte TypeSpecificHiByte
            {
                get { return this._hiPayload; }
            }

            /// <summary>
            /// Data 5: Destination Type Specific: 
            /// For Destination Type = Dial Page: 
            ///     Dial String Selector 
            /// For Destination Type = TAP Page: 
            ///     TAP Account Selector 
            /// For Destination Type = PPP Alert: 
            ///     PPP Account Set Selector 
            /// For Destination Type = PPP Mode Callback or Basic Mode Callback: 
            ///     PPP Account Set Selector (PPP Mode Callback only, reserved otherwise) 
            /// </summary>
            internal byte TypeSpecificLoByte
            {
                get { return this._loPayload; }
            }

            internal int AlertRetries
            {
                get { return Convert.ToInt32(this._alertRetry); }
            }

            internal int CallRetry
            {
                get { return Convert.ToInt32(this._callRetry); }
            }

            /// <summary>
            /// Alert Acknowledge Timeout, in seconds, 0-based (i.e. minimum timeout = 1 
            /// second). Recommended factory default = 5 seconds. Value is ignored if alert type does 
            /// not support acknowledge, or if the Alert Acknowledge bit (above) is 0b.
            /// </summary>
            internal int AlertTimeOut
            {
                get { return Convert.ToInt32(this._timeOut); }
            }

            /// <summary>
            /// Destination acknowledgement
            /// </summary>
            internal DestinationAckType DestinationAck
            {
                get { return this._desAckType; }
            }

            /// <summary>
            /// Destination Type
            /// </summary>
            internal DestinationType Destination
            {
                get { return this._desType; }
            }

            /// <summary>
            /// Destination Selector
            /// </summary>
            internal byte DestinationSelector
            {
                get { return this._selector; }
            }

            /// <summary>
            /// Destination Acknoledgements
            /// </summary>
            internal enum DestinationAckType : byte
            {
                Unacknowledged = 0x00,
                Acknowledged = 0x01
            }

            /// <summary>
            /// Destination Types
            /// </summary>
            internal enum DestinationType : byte
            {
                /// <summary>
                /// 0000b = Dial Page 
                /// </summary>
                Dial_Page = 0x00,
                /// <summary>
                /// 0001b = TAP Page 
                /// </summary>
                TAP_Page = 0x01,
                /// <summary>
                /// 0010b = PPP Alert (PET Alert delivered via a PPP-to-LAN connection) 
                /// </summary>
                PPP_Alert = 0x02,
                /// <summary>
                /// 0011b = Basic Mode Callback 
                /// </summary>
                Basic_Mode_Callback = 0x03,
                /// <summary>
                /// 0100b = PPP Mode Callback 
                /// </summary>
                PPP_Mode_Callback = 0x04,
                /// <summary>
                /// 1110b = OEM 1 
                /// </summary>
                OEM1 = 0x0E,
                /// <summary>
                /// 1111b = OEM 2 
                /// </summary>
                OEM2 = 0x0F,
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal DestinationInfo()
            {
                base.Selector = 0x11;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal DestinationInfo(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - Destination Selector 
                // A minimum of one and a maximum of fifteen non-volatile destinations are supported in 
                // the specification. If callback is supported, the callback number is also a type of 
                // destination. Destination 0 is always present as a volatile destination that is used with 
                // the Alert Immediate command. 
                // [7:4] - reserved 
                // [3:0] - destination selector. 
                //      0h = volatile destination. 
                //      1-Fh = non-volatile destination. 

                _selector = (byte)((byte)(data[0] << 4) >> 4);

                // data 2 - Destination Type 
                //  [7] -   Alert Acknowledge. Note, some alert types, such as Dial Page, do not support 
                //          acknowledge, in which case this bit is ignored and should be written as 0b. 
                //  0b =    Unacknowledged. Alert is assumed successful if transmission occurs 
                //          without error. This value is also used with Callback numbers. 
                //  1b =    Acknowledged. Alert is assumed successful only if acknowledged is 
                //          returned. 
                byte ack = (byte)(data[1] >> 7);

                if (Enum.IsDefined(typeof(DestinationAckType), ack))
                {
                    _desAckType = (DestinationAckType)ack;
                }

                byte dest = (byte)((byte)(data[1] << 4) >> 4);

                if (Enum.IsDefined(typeof(DestinationType), dest))
                {
                    _desType = (DestinationType)dest;
                }

                // data 3 - Alert Acknowledge Timeout, in seconds, 0-based (i.e. minimum timeout = 1 
                //          second). Recommended factory default = 5 seconds. Value is ignored if alert type does 
                //          not support acknowledge, or if the Alert Acknowledge bit (above) is 0b. 
                _timeOut = data[2];

                // data 4: Retries 
                // [7] -  reserved 
                // [6:4] -  Number of times to retry alert once call connection has been made. (Does not 
                //          apply to TAP Page or Dial Page alerts) 
                //          1-based. 000b = no retries (alert is only sent once).  
                // [3] -    reserved 
                // [2:0] -  Number of times to retry call to given destination.  (See below for Call Retry 
                //          Interval parameter) 1-based. 000b = no retries (call is only tried once). 

                // alert retries
                _alertRetry = (byte)((byte)(data[3] >> 4) & 0x07);
                // destination call retries
                _callRetry = (byte)(data[3] & 0x07);


                // data 5: Destination Type Specific: 

                // For Destination Type = Dial Page: 
                // [7:4] -  Dial String Selector 
                // [3:0] -  reserved 

                // For Destination Type = TAP Page: 
                // Indicates which set of TAP Service Settings should be used for communication with
                // this destination. 
                // [7:4] -   reserved 
                // [3:0] - TAP Account Selector 

                // For Destination Type = PPP Alert: 
                // Indicates which set of PPP Account settings should be used for communication with
                // the selected destination. 
                // [7:4] - Destination IP Address Selector 
                // [3:0] - PPP Account Set Selector 

                //For Destination Type = PPP Mode Callback or Basic Mode Callback: 
                // [7:4] -  =   Destination IP Address Selector for PPP Mode Callback (The IP Address
                //              used to enable the BMC to send a Serial/Modem Connection Active 
                //              message once the connection has been established.) 
                //           = Dial String Selector for Basic Mode Callback 
                //[3:0] -  PPP Account Set Selector (PPP Mode Callback only, reserved otherwise) 

                _hiPayload = (byte)((byte)(data[4] >> 4) << 4);
                _loPayload = (byte)((byte)(data[4] << 4) >> 4);

                // set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal DestinationInfo(byte selector, DestinationAckType ack, DestinationType destType, byte timeout, byte callRetries, byte alertRetries, byte typePayload)
                : this()
            {
                byte[] payload = new byte[5];
                payload[0] = (byte)((byte)(selector << 4) >> 4);
                payload[1] = (byte)destType;
                if (ack == DestinationAckType.Acknowledged)
                    payload[1] = (byte)(payload[1] | 0x80);
                payload[2] = timeout;

                byte cretry = (byte)((byte)(callRetries & 0x07) << 4);
                byte aretry = (byte)(alertRetries & 0x07);

                payload[3] = (byte)(cretry | aretry);

                payload[4] = typePayload;

                base.Payload = payload;
            }
        }

        /// <summary>
        /// Call Retry Interval Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class CallRetryInterval : SerialConfigBase
        {
            private byte _interval;

            internal int Interval
            {
                get { return Convert.ToInt32(this._interval); }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal CallRetryInterval()
            {
                base.Selector = 0x12;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal CallRetryInterval(byte interval)
                : this()
            {
                _interval = interval;

                base.Payload = new byte[1] { interval };
            }
        }

        /// <summary>
        /// Destination Comm Setting Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class DestinationCommSetting : SerialConfigBase
        {

            private byte _destination;

            private StopBits _stop;

            private Parity _parity;

            private CharSize _char;

            /// <summary>
            /// Destination Flow Control
            /// </summary>
            private FlowControl _flowCtrl;

            /// <summary>
            /// Destination Serial Bit Rate
            /// </summary>
            private BitRate _bitRate;

            /// <summary>
            /// Destination Selector
            /// </summary>
            internal byte DestinationSelector
            {
                get { return this._destination; }
            }

            /// <summary>
            /// Destination Serial Stop Bit
            /// </summary>
            internal StopBits StopBit
            {
                get { return this._stop; }
            }

            /// <summary>
            /// Destination Serial Parity
            /// </summary>
            internal Parity SerialParity
            {
                get { return this._parity; }
            }

            /// <summary>
            /// Destination Serial Charactor Size
            /// </summary>
            internal CharSize CharactorSize
            {
                get { return this._char; }
            }

            /// <summary>
            /// Serial Speed
            /// </summary>
            internal BitRate Speed
            {
                get { return this._bitRate; }
            }

            /// <summary>
            /// Flow Control
            /// </summary>
            internal FlowControl FlowCtrl
            {
                get { return this._flowCtrl; }
            }

            /// <summary>
            ///  0b = 1 stop bit (default) 
            ///  1b = 2 stop bits 
            /// </summary>
            internal enum StopBits : byte
            {
                One = 0x00,
                Two = 0x01,
                Unknown = 0xA0
            }

            /// <summary>
            ///  000b = no parity. 
            ///  001b = odd parity. 
            ///  010b = even parity 
            /// </summary>
            internal enum Parity : byte
            {
                None = 0x00,
                Odd = 0x01,
                Even = 0x03,
                Unknown = 0xA0
            }

            /// <summary>
            /// 0b = 8 bits (must be 8-bit for PPP) 
            /// 1b = 7-bits (most TAP services use 7-bit) 
            /// </summary>
            internal enum CharSize
            {
                EightBit = 0x00,
                SevenBit = 0x01,
                Unknown = 0xA0
            }

            /// <summary>
            /// Flow Control
            /// </summary>
            internal enum FlowControl : byte
            {
                None = 0x00,
                RTS_CTS = 0x01,
                XON_XOFF = 0x02,
                Unknown = 0xA0
            }

            /// <summary>
            /// Bit Rate
            /// </summary>
            internal enum BitRate : byte
            {
                Unknown = 0x00,
                Baud_9600_bps = 0x06,
                Baud_19_2_kbps = 0x07,
                Baud_38_4_kbps = 0x08,
                Baud_57_6_kbps = 0x09,
                Baud_115_2_kbps = 0xA0
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal DestinationCommSetting()
            {
                base.Selector = 0x13;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal DestinationCommSetting(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - Destination Selector 
                // Note that each destination has
                // [7:4] - reserved 
                // [3:0] - Destination Selector. 
                _destination = (byte)(((byte)(data[0] << 4)) >> 4);

                // data 2 - flow control, DTR hang-up, asynch format 
                // [7:6] - Flow control 
                //          00b = No flow control 
                //          01b = RTS/CTS flow control
                //          10b = XON/XOFF flow control
                //          11b = Reserved. 
                // [5] -  reserved 
                // [4] -  stop bits 
                //          0b = 1 stop bit (default) 
                //          1b = 2 stop bits 
                // [3] -  character size 
                //          0b = 8 bits (must be 8-bit for PPP) 
                //          1b = 7-bits (most TAP services use 7-bit) 
                // [2:0] -  parity 
                //          000b = no parity. 
                //          001b = odd parity. 
                //          010b = even parity 
                byte flowctrl = (byte)(data[1] >> 6);
                byte stopBit = (byte)((byte)(data[1] << 3) >> 4);
                byte charSize = (byte)((byte)(data[1] << 4) >> 3);
                byte parity = (byte)((byte)(data[1] << 5) >> 5);

                // data 2 - bit rate 
                // [7:4] -  reserved 
                // [3:0] -  0-5h = reserved. 
                //          6h = 9600 bps 
                //          7h = 19.2 kbps (required) 
                //          8h = 38.4 kbps 
                //          9h = 57.6 kbps 
                //          Ah = 115.2 kbps 
                byte bitRate = (byte)(((byte)(data[2] << 4)) >> 4);

                // cast flow control to enum value.
                if (Enum.IsDefined(typeof(FlowControl), flowctrl))
                {
                    // Flow Control
                    _flowCtrl = (FlowControl)flowctrl;
                }
                else
                {
                    _flowCtrl = FlowControl.Unknown;
                }

                // Stop Bit
                if (Enum.IsDefined(typeof(StopBits), stopBit))
                {
                    _stop = (StopBits)stopBit;
                }
                else
                {
                    _stop = StopBits.Unknown;
                }

                // Char Size
                if (Enum.IsDefined(typeof(CharSize), charSize))
                {
                    _char = (CharSize)charSize;
                }
                else
                {
                    _char = CharSize.Unknown;
                }

                // Parity
                if (Enum.IsDefined(typeof(Parity), parity))
                {
                    _parity = (Parity)parity;
                }
                else
                {
                    _parity = Parity.Unknown;
                }

                // Bit Rate
                if (Enum.IsDefined(typeof(BitRate), bitRate))
                {
                    _bitRate = (BitRate)bitRate;
                }
                else
                {
                    _bitRate = BitRate.Unknown;
                }

                //set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data byte
            /// </summary>
            internal DestinationCommSetting(byte desintation, FlowControl flow, StopBits stop, CharSize charsize, Parity parity, BitRate rate)
                : this()
            {
                // shave top bits.
                byte bDesintation = (byte)(((byte)(desintation << 4)) >> 4);

                byte bFlow = (byte)((byte)flow << 6);
                byte bStop = (byte)((byte)stop << 4);
                byte bCharsize = (byte)((byte)charsize << 3);
                byte bParity = (byte)(((byte)((byte)parity << 5)) >> 5);

                byte data2 = (byte)(bFlow | bStop | bCharsize | bParity);

                byte bRate = (byte)(((byte)((byte)rate << 4)) >> 4);

                byte[] payload = new byte[3] { bDesintation, data2, bRate };

                // set payload.
                base.Payload = payload;
            }


        }

        /// <summary>
        /// Number of Dial String Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class NumberofDialStr : SerialConfigBase
        {
            private byte _data;

            internal int NumberOfDialStrings
            {
                get { return Convert.ToInt32(this._data); }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal NumberofDialStr()
            {
                base.Selector = 0x14;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal NumberofDialStr(byte data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - Number of non-volatile Dial Strings for this channel. Dial String 0 is always 
                //          present and is typically used as a volatile destination that is used with the Alert 
                //Immediate command. 
                //[7:5] -   reserved. 
                //[3:0] -   Number of non-volatile dial strings. One minimum, fifteen non-volatile dial 
                //          strings maximum. An implementation should support one dial string for each 
                //          destination. 
                //          0h = Serial/Modem Alerting and Callback not supported. 

                _data = (byte)((byte)(data << 4) >> 4); ;

                // set the base payload
                base.Payload = new byte[1] { _data };
            }
        }

        /// <summary>
        /// Destination of Dial String Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class DestinationDialStr : SerialConfigBase
        {
            private byte _data;

            private string _number;

            private byte _blockNum;

            internal int DialStringSelector
            {
                get { return Convert.ToInt32(this._data); }
            }

            internal int BlockNumber
            {
                get { return Convert.ToInt32(this._blockNum); }
            }

            internal string Number
            {
                get { return this._number; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal DestinationDialStr()
            {
                base.Selector = 0x15;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal DestinationDialStr(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - destination selector 
                // [7:4] - reserved 
                // [3:0] - Dial String Selector. 
                //      0 = volatile dial string 
                //      1-Fh = non-volatile dial string. 
                _data = (byte)((byte)(data[0] << 4) >> 4); ;


                // data 2 - block number to set, 1 based. 
                // Blocks are 16-bytes. At least two blocks are required per number, supporting a dial 
                // string of 31 characters plus terminator. 
                _blockNum = data[1];

                int lenght = (data.Length - 2);

                // check the payload is an even number of
                // 16 byte strings
                if (unchecked(lenght % 16) == 0)
                    lenght = ((lenght / 16) * 16); // shave off errand charactors

                _number = ASCIIEncoding.ASCII.GetString(data, 2, lenght);

                // set the base payload
                base.Payload = data;
            }

            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal DestinationDialStr(byte selector, byte blocknumber, string number)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - destination selector 
                // [7:4] - reserved 
                // [3:0] - Dial String Selector. 
                //      0 = volatile dial string 
                //      1-Fh = non-volatile dial string. 
                _data = (byte)((byte)(selector << 4) >> 4); ;


                // data 2 - block number to set, 1 based. 
                // Blocks are 16-bytes. At least two blocks are required per number, supporting a dial 
                // string of 31 characters plus terminator. 
                _blockNum = blocknumber;

                // set the number
                _number = number;

                byte[] numbArr = ASCIIEncoding.ASCII.GetBytes(_number);

                // check the payload is an even number of
                // 32 (2 * required 16 bytes blocks)
                Array.Resize<byte>(ref numbArr, 32);

                byte[] payload = new byte[34];
                payload[0] = _data;
                payload[1] = _blockNum;

                Buffer.BlockCopy(numbArr, 0, payload, 2, 32);

                // set the base payload
                base.Payload = payload;
            }

        }

        /// <summary>
        /// Number of Alert IP Address Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class NumberofAlertDestIPAddr : SerialConfigBase
        {
            private byte _data;

            internal int NumberOfIpAddresses
            {
                get { return Convert.ToInt32(this._data); }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal NumberofAlertDestIPAddr()
            {
                base.Selector = 0x16;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal NumberofAlertDestIPAddr(byte data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - Number of non-volatile Alert Destination IP Addresses for this channel. Address 
                // 0 is always present and is typically used as a volatile destination that is used with the 
                // Alert Immediate command. It is recommended that there be at least one destination IP 
                // Address per PPP Account. 
                _data = (byte)((byte)(data << 4) >> 4); ;

                // set the base payload
                base.Payload = new byte[1] { _data };
            }
        }

        /// <summary>
        /// Destination of Alert IP Address Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class DestinationIPAddr : SerialConfigBase
        {
            private byte _data;

            private uint _ipAddress;

            internal int NumberOfIpAddresses
            {
                get { return Convert.ToInt32(this._data); }
            }

            /// <summary>
            /// Ip Address in String format
            /// </summary>
            internal string IpAddress
            {
                get { return ToIpAddress<uint>(_ipAddress); }
            }

            /// <summary>
            /// Convert DataType to IpAddress
            /// </summary>
            /// <typeparam name="T"></typeparam>
            private static string ToIpAddress<T>(T address)
            {
                return IPAddress.Parse(address.ToString()).ToString();
            }


            /// <summary>
            /// Initialize class
            /// </summary>
            internal DestinationIPAddr()
            {
                base.Selector = 0x17;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal DestinationIPAddr(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - destination selector 
                // [7:4] - reserved 
                // [3:0] - Destination IP Address Selector. 
                //         0 = volatile IP Address location 
                //         1-Fh = non-volatile IP Address 
                _data = (byte)((byte)(data[0] << 4) >> 4); ;

                _ipAddress = BitConverter.ToUInt32(data, 1);

                // set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal DestinationIPAddr(byte data, IPAddress ipaddress)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - destination selector 
                // [7:4] - reserved 
                // [3:0] - Destination IP Address Selector. 
                //         0 = volatile IP Address location 
                //         1-Fh = non-volatile IP Address 
                this._data = (byte)((byte)(data << 4) >> 4); ;

                byte[] ipBytes = ipaddress.GetAddressBytes();

                _ipAddress = BitConverter.ToUInt32(ipBytes, 0);

                byte[] payload = new byte[5];
                payload[0] = this._data;

                Buffer.BlockCopy(BitConverter.GetBytes(_ipAddress), 0, payload, 1, 4);

                // set the base payload
                base.Payload = payload;
            }

        }

        /// <summary>
        /// Number of TAP Accounts Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class NumberofTAPAccounts : SerialConfigBase
        {
            private byte _data;

            internal int NumberOfTapAddresses
            {
                get { return Convert.ToInt32(this._data); }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal NumberofTAPAccounts()
            {
                base.Selector = 0x18;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal NumberofTAPAccounts(byte data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - Number of non-volatile TAP Accounts for this channel. Acc
                // present and is typically used as a volatile destination that is used w
                // Immediate command. It is not included in the count. 
                // [7:5] -   reserved. 
                // [3:0] - Number of TAP Accounts. 0h = TAP not supported. 
                _data = (byte)((byte)(data << 4) >> 4); ;

                // set the base payload
                base.Payload = new byte[1] { _data };
            }
        }

        /// <summary>
        /// TAP Account Paramaters for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class TapAccount : SerialConfigBase
        {
            private byte _setSelector;

            private byte _tapDialSelector;

            private byte _tapServiceSettingsSelector;

            internal int SetSelector
            {
                get { return Convert.ToInt32(this._setSelector); }
            }

            internal int TapDialSelector
            {
                get { return Convert.ToInt32(this._tapDialSelector); }
            }

            internal int TapServiceSettingsSelector
            {
                get { return Convert.ToInt32(this._tapServiceSettingsSelector); }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal TapAccount()
            {
                base.Selector = 0x19;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal TapAccount(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - set selector = TAP Account Selector, 1-based. At least one set of TAP Account 
                // parameters must be provided for each TAP destination supported. Account 0 is always 
                // present and is typically used as a volatile destination that is used with the Alert 
                // Immediate command. 
                _setSelector = (byte)((byte)(data[0] << 4) >> 4);

                // data 2 - TAP Dial String and Service Setting selectors 
                // [7:4] - Dial String Selector 
                // [3:0] -   TAP Service Settings Selector. 1-based. 0h if Destination Type is not ‘TAP Page’ 
                _tapDialSelector = (byte)(data[1] >> 4);
                _tapServiceSettingsSelector = (byte)((byte)(data[1] << 4) >> 4);

                // set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal TapAccount(byte setSelector, byte dialSelector, byte serviceSelector)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - set selector = TAP Account Selector, 1-based. At least one set of TAP Account 
                // parameters must be provided for each TAP destination supported. Account 0 is always 
                // present and is typically used as a volatile destination that is used with the Alert 
                // Immediate command. 
                _setSelector = (byte)((byte)(setSelector << 4) >> 4);

                // data 2 - TAP Dial String and Service Setting selectors 
                // [7:4] - Dial String Selector 
                // [3:0] -   TAP Service Settings Selector. 1-based. 0h if Destination Type is not ‘TAP Page’ 
                _tapDialSelector = (byte)(dialSelector << 4);
                _tapServiceSettingsSelector = (byte)((byte)(serviceSelector << 4) >> 4);

                byte data2 = (byte)(_tapDialSelector | _tapServiceSettingsSelector);

                byte[] payload = new byte[2] { _setSelector, data2 };

                // set the base payload
                base.Payload = payload;
            }
        }

        /// <summary>
        /// TAP Password for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class TapPasswords : SerialConfigBase
        {
            private byte _setSelector;

            private string _password;

            internal int SetSelector
            {
                get { return Convert.ToInt32(this._setSelector); }
            }

            internal string Password
            {
                get { return this._password; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal TapPasswords()
            {
                base.Selector = 0x1A;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal TapPasswords(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - set selector = TAP Account selector, 1 based. 
                // data 2:8 - Password. This string is up to six ASCII characters. Null terminated if fewer 
                // than six characters are used. 
                _setSelector = data[0];

                _password = ASCIIEncoding.ASCII.GetString(data, 1, (data.Length - 1));

                // set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal TapPasswords(byte setSelector, string passsword)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - set selector = TAP Account selector, 1 based. 
                // data 2:8 - Password. This string is up to six ASCII characters. Null terminated if fewer 
                // than six characters are used. 
                _setSelector = setSelector;

                if (passsword.Length > 16)
                    passsword = passsword.Remove(16);

                _password = passsword;

                byte[] pwArr = ASCIIEncoding.ASCII.GetBytes(_password);

                int len = pwArr.Length;

                byte[] payload = new byte[len + 1];
                payload[0] = _setSelector;
                Buffer.BlockCopy(pwArr, 0, payload, 1, len);

                // set the base payload
                base.Payload = payload;
            }

        }

        /// <summary>
        /// TAP Pager Id String for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class TapPagerIdStr : SerialConfigBase
        {
            private byte _setSelector;

            private string _pagerString;

            internal int SetSelector
            {
                get { return Convert.ToInt32(this._setSelector); }
            }

            internal string PagerIdString
            {
                get { return this._pagerString; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal TapPagerIdStr()
            {
                base.Selector = 0x1B;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal TapPagerIdStr(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - set selector = TAP Account selector, 1 based. 
                // data 2:17 - Pager ID String. This string is up to 16 ASCII characters. Null terminated if 
                // fewer than 16 characters are used. The string will be transmitted with escaping as 
                // specified by the control-character escaping mask for the given destination
                _setSelector = data[0];

                _pagerString = ASCIIEncoding.ASCII.GetString(data, 1, (data.Length - 1));

                // set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal TapPagerIdStr(byte setSelector, string pagerString)
                : this()
            {
                _setSelector = setSelector;

                if (pagerString.Length > 16)
                    pagerString = pagerString.Remove(16);

                _pagerString = pagerString;

                byte[] pagerArr = ASCIIEncoding.ASCII.GetBytes(_pagerString);

                int len = pagerArr.Length;

                byte[] payload = new byte[len + 1];
                payload[0] = _setSelector;
                Buffer.BlockCopy(pagerArr, 0, payload, 1, len);

                // set the base payload
                base.Payload = payload;
            }
        }

        /// <summary>
        /// Tap Service Setting for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class TapServiceSetting : SerialConfigBase
        {
            private byte _setSelector;

            private byte _confirmation;

            private string _tapServiceType;

            private byte[] _tapControlCharacters = new byte[4];

            private byte _t2Timeout;

            private byte _t1Timeout;

            private byte _t4Timeout;

            private byte _t3Timeout;

            private byte _t6Timeout;

            private byte _t5Timeout;

            private byte _n2Retry;

            private byte _n1Retry;

            private byte _n4Retry;

            private byte _n3Retry;

            internal int SetSelector
            {
                get { return Convert.ToInt32(this._setSelector); }
            }

            internal int TapConfirmation
            {
                get { return Convert.ToInt32(this._confirmation); }
            }

            internal string TapServiceTypeField
            {
                get { return this._tapServiceType; }
            }

            internal BitArray TapControlCharactors
            {
                get { return new BitArray(_tapControlCharacters); }
            }

            internal byte TapT2Timeout
            {
                get { return this._t2Timeout; }
            }

            internal byte TapT1Timeout
            {
                get { return this._t1Timeout; }
            }

            internal byte TapT4Timeout
            {
                get { return this._t4Timeout; }
            }

            internal byte TapT3Timeout
            {
                get { return this._t3Timeout; }
            }

            internal byte TapT6Timeout
            {
                get { return this._t6Timeout; }
            }

            internal byte TapT5Timeout
            {
                get { return this._t5Timeout; }
            }

            internal byte TapN2Retries
            {
                get { return this._n2Retry; }
            }

            internal byte TapN1Retries
            {
                get { return this._n1Retry; }
            }

            internal byte TapN4Retries
            {
                get { return this._n4Retry; }
            }

            internal byte TapN3Retries
            {
                get { return this._n3Retry; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal TapServiceSetting()
            {
                base.Selector = 0x1C;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal TapServiceSetting(byte[] data)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - set selector = TAP Service Setting Selector 
                // There is a 1:1 association between the TAP Parameter selector in this row, and the 
                // selector in the previous row. Parameter fields that share the same parameter selector 
                // form a parameter set. 
                // [7:4] - reserved 
                // [3:0] - TAP Parameter selector. 1-based. (0 = volatile paramters) 
                _setSelector = (byte)((byte)(data[0] << 4) >> 4);

                // data 2 - TAP Confirmation 
                // [7:2] -  reserved. 
                // [1:0] -  confirmation. This parameter determines what criteria is used by PEF and the 
                //          Alert Immediate command to determine that a TAP Page was successfully 
                //          delivered to the paging service. 
                //  00b = ACK received after end-of-transaction only 
                //  01b = code 211 and ACK received after ETX 
                //  10b = code 211or 213, and ACK, received after ETX 
                //  11b = reserved 
                _confirmation = (byte)((byte)(data[1] << 6) >> 6);

                // data 3:5 - TAP ‘SST’ Service Type field characters, in ASCII. Default = “PG1”. 
                // Three characters must be provided. 
                _tapServiceType = ASCIIEncoding.ASCII.GetString(data, 2, 3);

                // data 6:9  - TAP Control-character escaping mask. (Default = FFFF_FFFFh) 
                // [31:0] - each bit position represents escaping for corresponding control characters 31h 
                //          through 00h. A bit value of 1b = escape the character. 0b = don’t escape the character. 
                //          This bit value is ignored for characters that a required to be escaped by TAP. By default, 
                //          all control characters are escaped. 
                Buffer.BlockCopy(data, 5, _tapControlCharacters, 0, 4);

                // data 10 - timeout parameters 1 
                // [7:4]  TAP T2 - timeout in 500 ms. 0-based (0h = 500 ms). Default = 1h (1 second) 
                // [3:0]  TAP T1 - timeout in seconds. 0-based (0h = 1 second). Default = 1h (2 seconds
                _t2Timeout = (byte)(data[9] >> 4);
                _t1Timeout = (byte)((byte)(data[9] << 4) >> 4);

                // data 11 - timeout parameters 2 
                // [7:4]  TAP T4 - timeout in seconds. 0-based (0h = 1 second). Default = 3h (4 seconds
                // [3:0]  TAP T3 - timeout in 2 second increments. 0-based (0h = 2 seconds). Default = 
                //                 4h (10 seconds) 
                _t4Timeout = (byte)(data[10] >> 4);
                _t3Timeout = (byte)((byte)(data[10] << 4) >> 4);

                // data 12 - timeout parameters 3 
                // [7:4]  IPMI T6 - IPMI timeout waiting for end-of-transaction acknowledge, in seconds. 
                //                  0-based (0 = 1 second). Default = 1h (2 seconds). 
                // [3:0]  TAP T5 -  timeout in 2 second increments. 0-based (0h = 2 seconds). Default = 
                //                  3h (4 seconds) 
                _t6Timeout = (byte)(data[11] >> 4);
                _t5Timeout = (byte)((byte)(data[11] << 4) >> 4);

                // data 13 - retry parameters 1 
                // [7:4]  TAP N2 - retries. 1-based. (0 = no retry). Default = 3. 
                // [3:0]  TAP N1 - retries. 1-based. (0 = no retry). Default = 3. 
                //                 data 14 - retry parameters 2 
                _n2Retry = (byte)(data[12] >> 4);
                _n1Retry = (byte)((byte)(data[12] << 4) >> 4);


                // [7:4]  IPMI N4 - number of retries for end-of-transaction. Default = 3. 
                // [3:0]  TAP N3 - retries. 1-based. (0 = no retry). Default = 3. 
                _n4Retry = (byte)(data[13] >> 4);
                _n3Retry = (byte)((byte)(data[13] << 4) >> 4);

                // set the base payload
                base.Payload = data;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal TapServiceSetting(byte setSelector, byte confirmation, string tapServiceType, byte[] tapCtrlEsc, byte[] timeouts, byte[] retries)
                : this()
            {
                // Ipmi specification [25-3]:
                // data 1 - set selector = TAP Service Setting Selector 
                // There is a 1:1 association between the TAP Parameter selector in this row, and the 
                // selector in the previous row. Parameter fields that share the same parameter selector 
                // form a parameter set. 
                // [7:4] - reserved 
                // [3:0] - TAP Parameter selector. 1-based. (0 = volatile paramters) 
                _setSelector = (byte)((byte)(setSelector << 4) >> 4);

                // data 2 - TAP Confirmation 
                // [7:2] -  reserved. 
                // [1:0] -  confirmation. This parameter determines what criteria is used by PEF and the 
                //          Alert Immediate command to determine that a TAP Page was successfully 
                //          delivered to the paging service. 
                //  00b = ACK received after end-of-transaction only 
                //  01b = code 211 and ACK received after ETX 
                //  10b = code 211or 213, and ACK, received after ETX 
                //  11b = reserved 
                _confirmation = (byte)((byte)(confirmation << 6) >> 6);

                // data 3:5 - TAP ‘SST’ Service Type field characters, in ASCII. Default = “PG1”. 
                // Three characters must be provided. 
                if (tapServiceType.Length > 3)
                    tapServiceType = tapServiceType.Remove(3);
                _tapServiceType = tapServiceType;

                byte[] tapService = ASCIIEncoding.ASCII.GetBytes(_tapServiceType);

                // data 6:9  - TAP Control-character escaping mask. (Default = FFFF_FFFFh) 
                // [31:0] - each bit position represents escaping for corresponding control characters 31h 
                //          through 00h. A bit value of 1b = escape the character. 0b = don’t escape the character. 
                //          This bit value is ignored for characters that a required to be escaped by TAP. By default, 
                //          all control characters are escaped. 
                Buffer.BlockCopy(tapCtrlEsc, 0, _tapControlCharacters, 0, 4);

                // data 10 - timeout parameters 1 
                // [7:4]  TAP T2 - timeout in 500 ms. 0-based (0h = 500 ms). Default = 1h (1 second) 
                // [3:0]  TAP T1 - timeout in seconds. 0-based (0h = 1 second). Default = 1h (2 seconds
                _t2Timeout = (byte)((byte)(timeouts[1] << 4) >> 4);
                _t1Timeout = (byte)((byte)(timeouts[0] << 4) >> 4);

                byte data10 = (byte)((byte)(_t2Timeout << 4) | _t1Timeout);

                // data 11 - timeout parameters 2 
                // [7:4]  TAP T4 - timeout in seconds. 0-based (0h = 1 second). Default = 3h (4 seconds
                // [3:0]  TAP T3 - timeout in 2 second increments. 0-based (0h = 2 seconds). Default = 
                //                 4h (10 seconds) 
                _t4Timeout = (byte)((byte)(timeouts[3] << 4) >> 4);
                _t3Timeout = (byte)((byte)(timeouts[2] << 4) >> 4);

                byte data11 = (byte)((byte)(_t4Timeout << 4) | _t3Timeout);

                // data 12 - timeout parameters 3 
                // [7:4]  IPMI T6 - IPMI timeout waiting for end-of-transaction acknowledge, in seconds. 
                //                  0-based (0 = 1 second). Default = 1h (2 seconds). 
                // [3:0]  TAP T5 -  timeout in 2 second increments. 0-based (0h = 2 seconds). Default = 
                //                  3h (4 seconds) 
                _t6Timeout = (byte)((byte)(timeouts[5] << 4) >> 4);
                _t5Timeout = (byte)((byte)(timeouts[4] << 4) >> 4);

                byte data12 = (byte)((byte)(_t6Timeout << 4) | _t5Timeout);

                // data 13 - retry parameters 1 
                // [7:4]  TAP N2 - retries. 1-based. (0 = no retry). Default = 3. 
                // [3:0]  TAP N1 - retries. 1-based. (0 = no retry). Default = 3. 
                //                 data 14 - retry parameters 2 
                _n2Retry = (byte)((byte)(retries[1] << 4) >> 4);
                _n1Retry = (byte)((byte)(retries[0] << 4) >> 4);

                byte data13 = (byte)((byte)(_n2Retry << 4) | _n1Retry);

                // [7:4]  IPMI N4 - number of retries for end-of-transaction. Default = 3. 
                // [3:0]  TAP N3 - retries. 1-based. (0 = no retry). Default = 3. 
                _n4Retry = (byte)((byte)(retries[3] << 4) >> 4);
                _n3Retry = (byte)((byte)(retries[2] << 4) >> 4);

                byte data14 = (byte)((byte)(_n4Retry << 4) | _n3Retry);

                // Assemble payload packet.
                byte[] payload = new byte[14];
                payload[0] = _setSelector;
                payload[1] = _confirmation;
                Buffer.BlockCopy(tapService, 0, payload, 2, 3);
                Buffer.BlockCopy(_tapControlCharacters, 0, payload, 5, 4);
                payload[9] = data10;
                payload[10] = data11;
                payload[11] = data12;
                payload[12] = data13;
                payload[13] = data14;

                base.Payload = payload;
            }
        }

        /// <summary>
        /// Terminal Mode Config for Serial/Modem 
        /// Coniguration command [IPMI 25-3] 
        /// </summary>
        internal class TerminalModeConfig : SerialConfigBase
        {
            /// <summary>
            /// Echo Character Settings
            ///     0b = no echo
            ///     1b = echo (BMC echoes characters it receives) 
            /// </summary>
            private bool _echo;

            /// <summary>
            /// HandShake
            ///     0b = disable handshake (See  14.7.7, Terminal Mode Packet Handshake) 
            ///     1b = enable handshake
            /// </summary>
            private bool _handShake;

            /// <summary>
            /// Line Editing
            ///     0b = disable line editing 
            ///     1b = enable line editing 
            /// </summary>
            private bool _lineEditing;

            /// <summary>
            /// Operation Peratmater
            /// </summary>
            private Operation _operation;

            private DeleteCtrl _delete;

            private OutputNewLineSeq _outLineSeq;

            private InputNewLineSeq _inLineSeq;

            /// <summary>
            /// Delete Control
            /// </summary>
            internal enum DeleteCtrl : byte
            {
                /// <summary>
                /// 00b = BMC outputs a <del> character  when <bksp> or <del> is received 
                /// </summary>
                Delete = 0x00,

                /// <summary>
                //  01b = BMC outputs a <bksp><sp><bksp> sequence  when <bksp> or <del> is received
                /// </summary>
                DelAndBackSpace = 0x01,
            }

            /// <summary>
            /// New Line Sequence
            /// Terminal Mode. 
            ///  0h = no termination sequence 
            ///  1h = <cr-lf> (default) 
            ///  2h = <NULL> 
            ///  3h = <CR> 
            ///  4h = <LF-CR> 
            ///  5h = <LF> 
            ///  all other = reserved. 
            /// </summary>
            internal enum OutputNewLineSeq : byte
            {
                /// <summary>
                /// 0h = no termination sequence 
                /// </summary>
                None = 0x00,

                /// <summary>
                //  1h = cr-lf (default) 
                /// </summary>
                Crlf = 0x01,

                /// <summary>
                ///  2h = NULL
                /// </summary>
                Null = 0x02,

                /// <summary>
                ///  3h = CR
                /// </summary>
                CR = 0x03,

                /// <summary>
                ///  4h = LF-CR 
                /// </summary>
                LFCR = 0x04,

                /// <summary>
                ///  5h = LF
                /// </summary>
                LF = 0x05,

                /// <summary>
                /// Injected value to singal 
                /// enum could not resolve the byte
                /// </summary>
                Unknown = 0xA0
            }

            /// <summary>
            /// Input Line Sequence
            /// Terminal Mode. 
            ///     0h = reserved  
            ///     1h = <cr> (default) 
            ///     2h = <NULL> 
            ///     all other = reserved.  
            /// </summary>
            internal enum InputNewLineSeq : byte
            {
                /// <summary>
                /// 0h = reserved 
                /// </summary>
                reserved = 0x00,

                /// <summary>
                //  1h = <cr> (default) 
                /// </summary>
                CR = 0x01,

                /// <summary>
                ///  2h = NULL
                /// </summary>
                Null = 0x02,

                /// <summary>
                /// Injected value to singal 
                /// enum could not resolve the byte
                /// </summary>
                Unknown = 0xA0
            }

            /// <summary>
            /// Paramater Operation
            ///          00b = Set volatile version of data 1 bits 5:0 and data 2 
            ///          01b = Set non-volatile version of data 1 bits 5:0 and data 2 
            ///          10b = Copy non-volatile setting to volatile setting (restore default). 
            ///          11b = reserved 
            /// </summary>
            internal enum Operation : byte
            {
                SetVolatile = 0x00,
                SetNonVolatile = 0x01,
                CopyNonVolatile = 0x02,
                Unknown = 0xA0
            }

            internal bool Echo
            {
                get { return this._echo; }
            }

            internal bool HandShake
            {
                get { return this._handShake; }
            }

            internal bool LineEditing
            {
                get { return this._lineEditing; }
            }

            /// <summary>
            /// Operation Paramater
            /// </summary>
            internal Operation OperationParamater
            {
                get { return this._operation; }
            }

            /// <summary>
            /// Console Delete Control
            /// </summary>
            internal DeleteCtrl DeleteControl
            {
                get { return this._delete; }
            }

            /// <summary>
            /// Console New Line Sequence Editing
            /// </summary>
            internal OutputNewLineSeq NewLineOutSequence
            {
                get { return this._outLineSeq; }
            }

            internal InputNewLineSeq NewLineInSequence
            {
                get { return this._inLineSeq; }
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            internal TerminalModeConfig()
            {
                base.Selector = 0x1D;
            }

            /// <summary>
            /// Initialize class with data array of bytes
            /// representing payload
            /// </summary>
            internal TerminalModeConfig(byte[] data)
                : this()
            {

                // Ipmi specification [25-3]:
                // data 1 
                // Parameter Operation  
                // [7:6] -  00b = Set volatile version of data 1 bits 5:0 and data 2 
                //          01b = Set non-volatile version of data 1 bits 5:0 and data 2 
                //          10b = Copy non-volatile setting to volatile setting (restore default). 
                //          11b = reserved 
                byte operation = (byte)((byte)(data[0] << 6) >> 6);

                if (Enum.IsDefined(typeof(Operation), operation))
                {
                    _operation = (Operation)operation;
                }
                else
                {
                    _operation = Operation.Unknown;
                }

                // Terminal mode options 
                // [5] -    0b = disable line editing 
                //          1b = enable line editing 
                // [4] -  reserved 
                // [3:2] -  delete control (only applies when line editing is enabled) 
                //          00b = BMC outputs a <del> character  when <bksp> or <del> is received 
                //          01b = BMC outputs a <bksp><sp><bksp> sequence  when <bksp> or <del> is received 
                // [1] -    0b = no echo 
                //          1b = echo (BMC echoes characters it receives) 
                // [0] -    0b = disable handshake (See  14.7.7, Terminal Mode Packet Handshake) 
                //          1b = enable handshake 

                // convert byte into bit array.
                BitArray bitArr = SerialBaseSharedFunctions.ByteToBits(data[0]);
                // set line editing
                _lineEditing = bitArr[5];
                // set echo
                _echo = bitArr[1];
                // set handshake
                _handShake = bitArr[0];
                if (!bitArr[2])
                    _delete = DeleteCtrl.Delete;
                else if (bitArr[3])
                    _delete = DeleteCtrl.DelAndBackSpace;

                // data 2 - newline sequences 
                // [7:4] -  output newline sequence (BMC to console). Selects what characters the BMC 
                //          uses as a <newline> sequence when the BMC writes a line to the console in 
                // Terminal Mode. 
                //  0h = no termination sequence 
                //  1h = <cr-lf> (default) 
                //  2h = <NULL> 
                //  3h = <CR> 
                //  4h = <LF-CR> 
                //  5h = <LF> 
                //  all other = reserved. 
                byte lineSeq = (byte)(data[1] >> 4);

                if (Enum.IsDefined(typeof(OutputNewLineSeq), lineSeq))
                {
                    _outLineSeq = (OutputNewLineSeq)lineSeq;
                }
                else
                {
                    _outLineSeq = OutputNewLineSeq.Unknown;
                }

                // [3:0] -  input newline sequence (Console to BMC). Selects what characters the console 
                //          uses as the <newline> sequence when writing to the BMC in Terminal Mode. 
                //  0h = reserved  
                //  1h = <cr> (default) 
                //  2h = <NULL> 
                //  all other = reserved. 
                byte inputLineSeq = (byte)((byte)(data[1] << 4) >> 4);

                if (Enum.IsDefined(typeof(InputNewLineSeq), inputLineSeq))
                {
                    _inLineSeq = (InputNewLineSeq)inputLineSeq;
                }
                else
                {
                    _inLineSeq = InputNewLineSeq.Unknown;
                }

            }
        }

        //internal class pppProtocolOptions : SerialConfigBase
        //{
        //    /// <summary>
        //    /// Initialize class
        //    /// </summary>
        //    internal pppProtocolOptions()
        //    {
        //        base.Selector = 0x1E;
        //    }

        //}

        //internal class pppPrimaryRmcpPort : SerialConfigBase
        //{

        //}

        //internal class pppSecondaryRmcpPort : SerialConfigBase
        //{

        //}

        //internal class PppLinkAuthentication : SerialConfigBase
        //{

        //}

        //internal class ChapName : SerialConfigBase
        //{

        //}

        //internal class pppAccm : SerialConfigBase
        //{

        //}

        //internal class pppSnoopAccm : SerialConfigBase
        //{

        //}

        //internal class NumberofpppAccount : SerialConfigBase
        //{

        //}

        //internal class pppAccountDialStr : SerialConfigBase
        //{

        //}

        //internal class pppAccountIPAddr : SerialConfigBase
        //{

        //}

        //internal class pppAccountUserNames : SerialConfigBase
        //{

        //}

        //internal class pppAccountUserDomains : SerialConfigBase
        //{

        //}

        //internal class pppAccountUserPasswords : SerialConfigBase
        //{

        //}

        //internal class pppAccountAuthSetting : SerialConfigBase
        //{

        //}

        //internal class pppAccountConnection : SerialConfigBase
        //{

        //}

        //internal class pppUdpProxyIpHeaderData : SerialConfigBase
        //{

        //}

        //internal class pppUdpProxyTransmitBuffer : SerialConfigBase
        //{

        //}

        //internal class pppUdpProxyReceiveBuffer : SerialConfigBase
        //{

        //}

        //internal class pppRemoteConsoleIp : SerialConfigBase
        //{

        //}

        //internal class SystemPhoneNumber : SerialConfigBase
        //{

        //}

        //internal class BitRateSupport : SerialConfigBase
        //{

        //}

        //internal class SystemSerialPortAssociation : SerialConfigBase
        //{

        //}

        //internal class SystemConnectorNames : SerialConfigBase
        //{

        //}

        //internal class SystemSerialChannelNames : SerialConfigBase
        //{

        //}

        //internal class BadPasswordThreshold : SerialConfigBase
        //{

        //}
    }

}
