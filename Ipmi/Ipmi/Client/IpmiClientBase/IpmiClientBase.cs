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

    /// <summary>
    /// Client Base Class.  Used to derive Ipmi Client Classes
    /// </summary>
    internal abstract class IpmiClientBase
    {
        #region Private Variables

        /// <summary>
        /// Number of milliseconds to wait for the response.  Default is 100ms.
        /// </summary>
        private uint _timeout = 100;

        /// <summary>
        /// Current state of the IPMI client session.
        /// </summary>
        private IpmiClientState _clientState = IpmiClientState.Disconnected;

        /// <summary>
        /// Account userid for authenticating with the remote BMC.
        /// </summary>
        private string _userid;

        /// <summary>
        /// Account password for authenticating with the remote BMC.  Must be keep around in string form as
        /// used computing the MD5 digest of each packet when Udp client is using encryption.
        /// </summary>
        private string _password;

        /// <summary>
        /// RMCP+ rqSeq message tracker.
        /// </summary>
        private byte _ipmiRqSeq;

        /// <summary>
        /// Represents the current V1.5 authentication type used for IPMI messages.
        /// </summary>
        /// <remarks>
        /// if unset during initialization, this initially starts with None and 
        /// switches to proposed authentication type during activation.
        /// </remarks>
        private AuthenticationType _ipmiAuthenticationType;

        /// <summary>
        /// Represents the proposed V1.5 authentication type during session setup.
        /// </summary>
        /// <remarks>
        /// used during session creation to negociate an authentication type 
        /// </remarks>
        private AuthenticationType _ipmiProposedAuthenticationType;

        /// <summary>
        /// Represents the current privlege level used for IPMI messages.
        /// </summary>
        private PrivilegeLevel _ipmiPrivilegeLevel;

        #endregion

        #region Internal Variables


        /// <summary>
        /// default user is always present on IPMI BMC.
        /// </summary>
        internal byte defaultUser = 0x01;

        /// <summary>
        /// default channel is always avaialble for IPMI BMC.
        /// </summary>
        internal byte defaultChannel = 0x01;

        /// <summary>
        /// default Ipmi password maximum size for V1.5.
        /// </summary>
        internal int defaultMaxPasswordSize = 16;

        /// <summary>
        /// Ipmi password maximum size for V2.0.
        /// </summary>
        internal int enhancedPasswordMaxSize = 20;

        /// <summary>
        /// Gets and RMCP+ rqSeq used for mapping sent/received messages.
        /// </summary>
        /// <value>RMCP+ .</value>
        internal byte IpmiRqSeq
        {
            // rqSeq is a 6 bit number, the byte is shared with the 2 bit LUN.
            // permitted values include 1h to 30h. 31h -> 3Fh are reserved for 
            // future functions in this library. 
            get { return this._ipmiRqSeq; }
            set { this._ipmiRqSeq = (value > (byte)0x30 ? (byte)0x01 : value); }
        }

        /// <summary>
        /// Gets the IPMI password required for authenticating the IPMI messages.
        /// </summary>
        /// <value>Password of the current logged in userid.</value>
        internal string IpmiPassword
        {
            get { return this._password; }
            set { this._password = value; }
        }

        /// <summary>
        /// Gets the IPMI User required for authenticating the IPMI messages.
        /// </summary>
        internal string IpmiUserId
        {
            get { return this._userid; }
            set { this._userid = value; }
        }

        /// <summary>
        /// IPMI version which determines if RMCP or RMCP+ is used as the wire protocol.
        /// </summary>
        internal IpmiVersion ipmiVersion;

        /// <summary>
        /// Gets and sets the privilege level used for IPMI messages.
        /// </summary>
        /// <value>IPMI Privilege Level.</value>
        internal PrivilegeLevel IpmiPrivilegeLevel
        {
            get { return this._ipmiPrivilegeLevel; }
            set { this._ipmiPrivilegeLevel = value; }
        }

        /// <summary>
        /// Gets and sets the current authentication type used for IPMI messages.
        /// </summary>
        /// <value>IPMI Authentication Type.</value>
        internal AuthenticationType IpmiAuthenticationType
        {
            get { return this._ipmiAuthenticationType; }
            set { this._ipmiAuthenticationType = value; }
        }

        /// <summary>
        /// Gets and sets the bmc proposed authentication type used for IPMI messages.
        /// </summary>
        /// <value>IPMI Authentication Type.</value>
        internal AuthenticationType IpmiProposedAuthenticationType
        {
            get { return this._ipmiProposedAuthenticationType; }
            set { this._ipmiProposedAuthenticationType = value; }
        }

        /// <summary>
        /// Number of retries for the current IPMI request message (if any).
        /// </summary>
        internal uint ipmiResponseRetries;

        #endregion

        #region Public Variables

        /// <summary>
        /// Gets and sets the number of milliseconds to wait for a request.  Default is 500ms.
        /// </summary>
        /// <value>Number of milliseconds to wait for a request.  Default is 500ms.</value>
        public uint Timeout
        {
            get { return this._timeout; }
            set { this._timeout = value; }
        }

        /// <summary>
        /// Gets the IPMI version this session is under.
        /// </summary>
        /// <value>IpmiVersion value.</value>
        public IpmiVersion IpmiVersion
        {
            get { return this.ipmiVersion; }
        }

        #endregion

        #region Client State Control

        protected virtual IpmiClientState ClientState
        {
            get { return this._clientState; }
        }

        /// <summary>
        /// Set the current ClientState and notify any observers.
        /// </summary>
        /// <param name="newClientState">New ClientState.</param>
        protected virtual void SetClientState(IpmiClientState newClientState)
        {
            if (this.ClientState != newClientState)
            {
                this._clientState = newClientState;
            }
        }

        #endregion

        #region Send/Receive

        /// <summary>
        /// Generics method IpmiSendReceive for easier use
        /// </summary>
        internal virtual T IpmiSendReceive<T>(IpmiRequest ipmiRequest) where T : IpmiResponse
        {
            return (T)IpmiSendReceive(ipmiRequest, typeof(T));
        }

        /// <summary>
        /// Send Receive Ipmi messages
        /// </summary>
        internal abstract IpmiResponse IpmiSendReceive(IpmiRequest ipmiRequest, Type responseType, bool allowRetry = true);

        #endregion

    }
}
