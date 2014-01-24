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
    using System.Reflection;
    using System.Collections;
    using System.Globalization;
    using System.Collections.Generic;

    internal abstract class IpmiClientBasic : IpmiClientBase
    {
        #region Send/Receive

        /// <summary>
        /// Send Receive Ipmi messages
        /// </summary>
        internal override abstract IpmiResponse IpmiSendReceive(IpmiRequest ipmiRequest, Type responseType, bool allowRetry = true);

        #endregion

        #region Ipmi Commands

        /// <summary>
        /// Queries BMC for the currently set boot device.
        /// </summary>
        /// <returns>Flags indicating the boot device.</returns>
        public virtual NextBoot GetNextBoot()
        {
            GetSystemBootOptionsRequest req = new GetSystemBootOptionsRequest(
                (byte)SystemBootOptionsParameter.BootFlags, 0);

            GetSystemBootOptionsResponse response =
                (GetSystemBootOptionsResponse)this.IpmiSendReceive(req, typeof(GetSystemBootOptionsResponse));

            NextBoot nextboot = new NextBoot(response.CompletionCode);

            if (response.CompletionCode == 0x00)
                nextboot.SetParamaters(response.ParameterData);

            return nextboot;
        }

        /// <summary>
        /// The helper for several boot type setting methods, as they
        /// essentially send the same sequence of messages.
        /// </summary>
        /// <param name="bootType">The desired boot type.</param>
        public virtual NextBoot SetNextBoot(BootType bootType, bool uefi, bool persistent, byte instance, bool requireCommit = false)
        {

            byte completionCode = 0x00;

            SsboSetInProgress req = new SsboSetInProgress(
                                    false, SboSetInProgress.SetInProgress);

            SetSystemBootOptionsResponse response =
                                        (SetSystemBootOptionsResponse)this.IpmiSendReceive(
                                        req, typeof(SetSystemBootOptionsResponse));

            completionCode = response.CompletionCode;

            SsboBootInfoAcknowledge ack = new SsboBootInfoAcknowledge(
                                          false,
                                          SboBootInfoAcknowledgeMask.EnableWriteBiosFlag,
                                          SboBootInfoAcknowledgeData.BiosHandlingFlag);

            response = (SetSystemBootOptionsResponse)this.IpmiSendReceive(
                        ack, typeof(SetSystemBootOptionsResponse));

            if (completionCode == 0x00)
                completionCode = response.CompletionCode;


            BootFlags bootFlags = BootFlags.BootFlagsValid;

            if (persistent)
                bootFlags = (bootFlags | BootFlags.AllSubsequentBoots);

            if (uefi)
                bootFlags = (bootFlags | BootFlags.EfiBootType);

            SsboBootFlags flags = new SsboBootFlags(
                                    false, bootFlags, bootType, 0, 0, instance);

            response = (SetSystemBootOptionsResponse)this.IpmiSendReceive(
                                    flags, typeof(SetSystemBootOptionsResponse));


            if (completionCode == 0x00)
                completionCode = response.CompletionCode;

            if (requireCommit)
            {
                req = new SsboSetInProgress(false, SboSetInProgress.CommitWrite);

                response = (SetSystemBootOptionsResponse)this.IpmiSendReceive(
                            req, typeof(SetSystemBootOptionsResponse));

                if (completionCode == 0x00)
                    completionCode = response.CompletionCode;
            }

            req = new SsboSetInProgress(false, SboSetInProgress.SetComplete);

            response = (SetSystemBootOptionsResponse)this.IpmiSendReceive(
                         req, typeof(SetSystemBootOptionsResponse));

            if (completionCode == 0x00)
                completionCode = response.CompletionCode;

            NextBoot nextboot = new NextBoot(completionCode);
            nextboot.BootDevice = bootType;

            return nextboot;
        }

        #region Identify

        /// <summary>
        /// Physically identify the computer by using a light or sound.
        /// </summary>
        /// <param name="interval">Identify interval in seconds or 255 for indefinite.</param>
        public virtual bool Identify(byte interval)
        {
            ChassisIdentifyResponse response =
                (ChassisIdentifyResponse)this.IpmiSendReceive(
                    new ChassisIdentifyRequest(interval),
                    typeof(ChassisIdentifyResponse));

            if (response.CompletionCode == 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Queries BMC for the GUID of the system.
        /// </summary>
        /// <returns>GUID reported by Baseboard Management Controller.</returns>
        public virtual DeviceGuid GetSystemGuid(bool retry = false)
        {
            GetSystemGuidRequest req = new GetSystemGuidRequest();

            GetSystemGuidResponse response =
                (GetSystemGuidResponse)this.IpmiSendReceive(req, typeof(GetSystemGuidResponse), retry);

            DeviceGuid responseObj = new DeviceGuid(response.CompletionCode);

            if (response.CompletionCode == 0x00)
                responseObj.SetParamaters(response.Guid);

            return responseObj;
        }

        #endregion

        #region Power

        /// <summary>
        /// Set the computer power state.
        /// </summary>
        /// <param name="powerState">Power state to set.</param>
        public virtual byte SetPowerState(IpmiPowerState powerState)
        {
            byte chassisOperation;

            switch (powerState)
            {
                case IpmiPowerState.Off:
                    chassisOperation = ChassisControlRequest.OperationPowerDown;
                    break;

                case IpmiPowerState.On:
                    chassisOperation = ChassisControlRequest.OperationPowerUp;
                    break;

                case IpmiPowerState.Cycle:
                    chassisOperation = ChassisControlRequest.OperationPowerCycle;
                    break;

                case IpmiPowerState.Reset:
                    chassisOperation = ChassisControlRequest.OperationHardReset;
                    break;

                case IpmiPowerState.SoftOff:
                    chassisOperation = ChassisControlRequest.OperationSoftShutdown;
                    break;
                default:
                    chassisOperation = 6;
                    break;
            }

            if (chassisOperation <= 5)
            {
                ChassisControlResponse response = (ChassisControlResponse)this.IpmiSendReceive(
                   new ChassisControlRequest(chassisOperation),
                   typeof(ChassisControlResponse));

                return response.CompletionCode;
            }
            else
            {
                // Invalid data field in Request
                return 0xCC;
            }
        }

        /// <summary>
        /// Sets the Power-On time
        /// </summary>
        /// <param name="interval">00 interval is none, other integers are interpretted as seconds.</param>
        public virtual bool SetPowerOnTime(byte interval)
        {
            SetPowerCycleIntervalResponse response =
                (SetPowerCycleIntervalResponse)this.IpmiSendReceive(
                    new SetPowerCycleIntervalRequest(interval),
                    typeof(SetPowerCycleIntervalResponse));

            if (response.CompletionCode == 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Get the current power state of the host computer.
        /// </summary>
        /// <returns>ImpiPowerState enumeration.</returns>
        /// <devdoc>
        /// Originally used the 'Get ACPI Power State' message to retrieve the power state but not supported
        /// by the Arima's Scorpio IPMI card with firmware 1.10.00610100.  The 'Get Chassis Status' message
        /// returns the correct information for all IPMI cards tested.
        /// </devdoc>
        public virtual SystemStatus GetChassisState()
        {
            GetChassisStatusResponse response =
                (GetChassisStatusResponse)this.IpmiSendReceive(
                    new GetChassisStatusRequest(),
                    typeof(GetChassisStatusResponse));

            SystemStatus responseObj = new SystemStatus(response.CompletionCode);

            if (response.CompletionCode == 0)
            {

                responseObj.SetParamaters(response.CurrentPowerState, response.LastPowerEvent,
                    response.MiscellaneousChassisState);
            }
            else
            {
                responseObj.PowerState = IpmiPowerState.Invalid;
            }

            return responseObj;
        }

        /// <summary>
        /// Set Chassis Power Restore Policy.
        /// </summary>
        public virtual PowerRestorePolicy SetPowerRestorePolicy(PowerRestoreOption policyOption)
        {
            // Set/Get Policy
            SetPowerRestoreResponse response = (SetPowerRestoreResponse)this.IpmiSendReceive(new SetPowerRestoreRequest(policyOption),
              typeof(SetPowerRestoreResponse));

            PowerRestorePolicy responseObj = new PowerRestorePolicy(response.CompletionCode);

            byte[] data = new byte[1];

            if (response.CompletionCode == 0)
            {
                data[0] = response.PowerRestorePolicy;
            }

            responseObj.SetParamaters(data);

            return responseObj;
        }

        /// <summary>
        /// Get the Power-On-Hours (POH) of the host computer.
        /// </summary>
        /// <returns>System Power On Hours.</returns>
        /// <remarks> Specification Note: Power-on hours shall accumulate whenever the system is in 
        /// the operational (S0) state. An implementation may elect to increment power-on hours in the S1 
        /// and S2 states as well.
        /// </remarks>
        public virtual PowerOnHours PowerOnHours()
        {
            GetPohCounterResponse response =
                    (GetPohCounterResponse)this.IpmiSendReceive(
                    new GetPohCounterRequest(), typeof(GetPohCounterResponse));

            PowerOnHours responseObj = new PowerOnHours(response.CompletionCode);

            if (response.CompletionCode == 0)
            {
                responseObj.SetParamaters(response.Counter);
            }

            return responseObj;
        }

        #endregion

        #region Firmware

        /// <summary>
        /// Gets BMC firmware revision.  Returns HEX string.
        /// </summary>
        /// <returns>firmware revision</returns>
        public virtual BmcFirmware GetFirmware()
        {
            // Get Device Id
            GetDeviceIdResponse response = (GetDeviceIdResponse)this.IpmiSendReceive(new GetDeviceIdRequest(),
              typeof(GetDeviceIdResponse));

            BmcFirmware responseObj = new BmcFirmware(response.CompletionCode);

            if (response.CompletionCode == 0)
            {
                responseObj.SetParamaters(response.MajorFirmware, response.MinorFirmware);
            }

            return responseObj;
        }

        /// <summary>
        /// Gets Device Id.  Returns HEX string.
        /// </summary>
        /// <returns>firmware revision</returns>
        public virtual BmcDeviceId GetDeviceId()
        {
            // Get Device Id
            GetDeviceIdResponse response = (GetDeviceIdResponse)this.IpmiSendReceive(new GetDeviceIdRequest(),
              typeof(GetDeviceIdResponse));

            BmcDeviceId responseObj = new BmcDeviceId(response.CompletionCode);

            if (response.CompletionCode == 0)
            {
                responseObj.SetParamaters(response.MajorFirmware, response.MinorFirmware,
                    response.ManufactureId, response.ProductId);
            }

            return responseObj;
        }

        #endregion

        #region User

        /// <summary>
        /// Get Users. Returns dictionary of User Ids and corresponding User names
        /// </summary>
        public virtual Dictionary<int, string> GetUsers()
        {
            // create return object
            Dictionary<int, string> results = new Dictionary<int, string>();

            // maximum users
            int maxUsers = 0;

            GetUserAccessResponse userAccess = GetUserAccess(defaultUser, defaultChannel);

            if (userAccess.CompletionCode == 0)
            {
                // get maximum user Ids allowed on BMC
                maxUsers = Convert.ToInt32(userAccess.MaxUsers);
            }

            // loop through all User Id slots on BMC
            // start for loop at defaultUser(1)
            for (int i = defaultUser; i <= maxUsers; i++)
            {
                byte userByte = Convert.ToByte(i);

                // Get User Name assigned to User Id
                GetUserNameResponse UserNameResponse = (GetUserNameResponse)this.IpmiSendReceive(
                  new GetUserNameRequest(userByte),
                  typeof(GetUserNameResponse));

                if (UserNameResponse != null)
                {
                    if (UserNameResponse.CompletionCode == 0)
                    {
                        string userNameStr = System.Text.ASCIIEncoding.ASCII.GetString(UserNameResponse.UserName).TrimEnd('\0');

                        if (!results.ContainsKey(i))
                        {

                            results.Add(i, userNameStr);
                        }
                    }
                }

                UserNameResponse = null;
            }

            return results;
        }

        /// <summary>
        /// Set Password
        /// </summary>
        /// <param name="userId">User Id.</param>
        /// <param name="operation">operation. setPassword, testPassword, disable\enable User</param>
        /// <param name="password">password to be set, 16 byte max for IPMI V1.5 and 20 byte max for V2.0</param>
        public virtual bool SetUserPassword(int userId, IpmiAccountManagment operation, string password)
        {

            if (password == null)
            {
                password = string.Empty;
            }

            // always 16
            int byteArraySize = defaultMaxPasswordSize;

            // set proper byte array size based on IPMI version
            if (IpmiVersion == IpmiVersion.V20)
            {
                byteArraySize = enhancedPasswordMaxSize;

                // userId
                // [7] password lenght, 
                // [5:0] userId
                // Set bit 7 and signal 20 byte password
                userId = (userId | 128);
            }

            if (password.Length > byteArraySize)
            {
                password = password.Remove(byteArraySize);
            }

            // Byte[] passed to BMC must be padded with 0's for empty slots
            byte[] paddedPwArray = new byte[byteArraySize];

            // Get ASCII encoding for password
            byte[] tempArray = System.Text.ASCIIEncoding.ASCII.GetBytes(password);

            tempArray.CopyTo(paddedPwArray, 0);

            byte accountRequest;

            switch (operation)
            {
                case IpmiAccountManagment.SetPassword:
                    accountRequest = SetUserPasswordRequest.OperationSetPassword;
                    break;
                case IpmiAccountManagment.TestPassword:
                    accountRequest = SetUserPasswordRequest.OperationTestPassword;
                    break;
                case IpmiAccountManagment.EnableUser:
                    accountRequest = SetUserPasswordRequest.OperationEnableUser;
                    break;
                case IpmiAccountManagment.DisableUser:
                    accountRequest = SetUserPasswordRequest.OperationDisableUser;
                    break;
                default:
                    accountRequest = 5;
                    break;
            }

            if (accountRequest <= 4)
            {
                SetUserPasswordResponse response =
                   (SetUserPasswordResponse)this.IpmiSendReceive(
                       new SetUserPasswordRequest(Convert.ToByte(userId), accountRequest, paddedPwArray),
                       typeof(SetUserPasswordResponse));

                if (response.CompletionCode == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set the User Name for a given User Id
        /// </summary>       
        public virtual bool SetUserName(int userId, string userName)
        {

            // Byte[] passed to BMC must be padded with 0's for empty slots
            byte[] userNameAscii = new byte[16];

            byte[] tempArray = System.Text.ASCIIEncoding.ASCII.GetBytes(userName);

            tempArray.CopyTo(userNameAscii, 0);

            SetUserNameResponse response =
               (SetUserNameResponse)this.IpmiSendReceive(
                   new SetUserNameRequest(Convert.ToByte(userId), userNameAscii),
                   typeof(SetUserNameResponse));

            if (response.CompletionCode == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get User Name
        /// </summary>
        /// <param name="userId">User Id.</param>
        public virtual UserName GetUserName(int userId)
        {
            byte userByte = Convert.ToByte(userId);

            GetUserNameResponse response =
               (GetUserNameResponse)this.IpmiSendReceive(
                   new GetUserNameRequest(userByte),
                   typeof(GetUserNameResponse));

            UserName responseObj = new UserName(response.CompletionCode);

            if (response.CompletionCode == 0)
            {
                responseObj.SetParamaters(response.UserName);
            }

            return responseObj;
        }

        /// <summary>
        /// Set User Access
        /// </summary>
        /// <param name="userId">User Id.</param>
        /// <param name="userLmit">User Privilege Level.</param>
        /// <param name="allowBitMod">True|False, allow modification of bits in request byte</param>
        /// <param name="callBack">True|False, allow callbacks, usually set to False</param>
        /// <param name="linkAuth">True|False, allow link authoriation, usually set to True</param>
        /// <param name="ipmiMessage">allow Impi messaging, usually set to True</param>
        /// <param name="channel">channel used to communicate with BMC, 1-7</param>
        public virtual bool SetUserAccess(int userId, PrivilegeLevel priv, bool allowBitMod, bool callback, bool linkAuth, bool ipmiMessage, int channel)
        {
            // TODO:  implement Set Channel Access command to be comsistent with 
            // the user settings.  Temp workaround, when setting user access
            // set allowBitMod to false.

            byte[] tempArray = new byte[1];

            // intialize 8 bit byte request for byte #1 of SetUserAccessRequest
            BitArray bitArray = new BitArray(8);

            byte[] channelByte = BitConverter.GetBytes(channel);

            // inialize BitArray with channel number
            BitArray channelBitArray = new BitArray(channelByte);

            // set BitArray values for supplied parameters
            bitArray.Set(7, allowBitMod); // true|false, Allow changing of bits in the request byte
            bitArray.Set(6, callback); // true|false, User Restricted to Callback
            bitArray.Set(5, linkAuth); // true|false, Link Auth
            bitArray.Set(4, ipmiMessage); // true|false, IPMI Messaging
            bitArray.Set(3, channelBitArray[3]); // channel number
            bitArray.Set(2, channelBitArray[2]); // channel number
            bitArray.Set(1, channelBitArray[1]); // channel number
            bitArray.Set(0, channelBitArray[0]); // channel number

            // copy BitArray to Byte[] array
            bitArray.CopyTo(tempArray, 0);

            // convert Byte value to base 16
            byte requestByte1 = Convert.ToByte(string.Format(CultureInfo.InvariantCulture, "{0:x}", tempArray[0]), 16);

            SetUserAccessResponse response = (SetUserAccessResponse)this.IpmiSendReceive(
                  new SetUserAccessRequest(Convert.ToByte(userId, CultureInfo.InvariantCulture), Convert.ToByte(priv, CultureInfo.InvariantCulture), requestByte1, 0x00),
                  typeof(SetUserAccessResponse));

            if (response.CompletionCode == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get user privilege level
        /// </summary>
        public virtual UserPrivilege GetUserPrivlige(byte userId, byte channel)
        {
            GetUserAccessResponse accessResp = GetUserAccess(userId, channel);

            UserPrivilege privilege = new UserPrivilege(accessResp.CompletionCode);

            if (accessResp.CompletionCode == 0)
            {
                privilege.SetParamaters(new byte[1] { accessResp.AccessLevel });
            }

            return privilege;
        }

        #endregion

        #region Session

        /// <summary>
        /// Ipmi Get Session Info Command.
        /// </summary>
        internal GetSessionInfoResponse GetSessionInfo()
        {
            GetSessionInfoResponse response = (GetSessionInfoResponse)this.IpmiSendReceive(
                new GetSessionInfoRequest(0x00, 0), typeof(GetSessionInfoResponse));

            return response;
        }

        /// <summary>
        /// Ipmi Get Session Info Command.
        /// </summary>
        internal SetChannelAccessResponse SetChannelAccess(byte channel, bool enablePef, bool disableUserAuth, bool AccessMode)
        {
            SetChannelAccessResponse response = (SetChannelAccessResponse)this.IpmiSendReceive(
                new SetChannelAccessRequest(channel, enablePef, disableUserAuth, AccessMode), typeof(SetChannelAccessResponse));

            return response;
        }

        /// <summary>
        /// Negotiates the ipmi version and sets client accordingly. Also sets the authentication type for V1.5
        /// </summary>
        public virtual ChannelAuthenticationCapabilities GetAuthenticationCapabilities(PrivilegeLevel privilegeLevel, bool retry = false)
        {
            // Get Channel Authentication Capabilities
            GetChannelAuthenticationCapabilitiesResponse response =
                (GetChannelAuthenticationCapabilitiesResponse)this.IpmiSendReceive(
                    new GetChannelAuthenticationCapabilitiesRequest(0x0E, privilegeLevel),
                    typeof(GetChannelAuthenticationCapabilitiesResponse), retry);

            ChannelAuthenticationCapabilities authCapabilities = new ChannelAuthenticationCapabilities(response.CompletionCode);

            if (response.CompletionCode == 0)
            {
                authCapabilities.SetParamaters(response.ChannelNumber, 
                    response.AuthenticationTypeSupport1,
                    response.AuthenticationTypeSupport2, 
                    response.ExtendedCapabilities,
                    response.OemId, response.OemData);
            }

            return authCapabilities;
        }

        /// <summary>
        /// Send an IPMI Set Session Privilege Level request message and return the response.
        /// </summary>
        /// <param name="privilegeLevel">Privilege level for this session.</param>
        /// <returns>GetSessionChallengeResponse instance.</returns>
        public void SetSessionPrivilegeLevel(PrivilegeLevel privilegeLevel, bool retry = false)
        {
            this.IpmiSendReceive(
           new SetSessionPrivilegeLevelRequest(privilegeLevel),
           typeof(SetSessionPrivilegeLevelResponse), retry);

        }

        #endregion

        #endregion

        #region Ipmi Bridge Commands

        /// <summary>
        /// Send sync Bridge Command
        /// </summary>
        /// <param name="channel">Channel to send command (Intel ME = 6)</param>
        /// <param name="slaveId">Channel Slave Id</param>
        /// <param name="messageData">Message payload</param>
        public virtual BridgeMessage SendMessage(byte channel, byte slaveId, byte[] requestMessage)
        {            
            SendMessageResponse sendmsg = (SendMessageResponse)this.IpmiSendReceive(
              new SendMessageRequest(channel, requestMessage),
              typeof(SendMessageResponse));

            BridgeMessage response = new BridgeMessage(sendmsg.CompletionCode);

            if (sendmsg.CompletionCode == 0x00)
                response.SetParamaters(sendmsg.MessageData);

            return response;
        }

        /// <summary>
        /// Get Message Flags
        /// </summary>
        public virtual MessageFlags GetMessageFlags()
        {
            GetMessageFlagsResponse flags = (GetMessageFlagsResponse)this.IpmiSendReceive(
              new GetMessageFlagsRequest(),
              typeof(GetMessageFlagsResponse));

            MessageFlags resposne = new MessageFlags(flags.CompletionCode);

            if(flags.CompletionCode == 0x00)
                resposne.SetParamaters(flags.MessageAvailable, flags.BufferFull, 
                    flags.WatchDogTimeout, flags.OEM1, flags.OEM2, flags.OEM3);

            return resposne;
        }

        /// <summary>
        /// Read Event Message Buffer
        /// </summary>
        public virtual BridgeMessage ReadEventMessageBuffer()
        {
            ReadEventMessageBufferResponse getMsg = (ReadEventMessageBufferResponse)this.IpmiSendReceive(
            new ReadEventMessageBufferRequest(), typeof(ReadEventMessageBufferResponse));

            BridgeMessage response = new BridgeMessage(getMsg.CompletionCode);

            if (getMsg.CompletionCode == 0x00)
                response.SetParamaters(getMsg.MessageData);

            return response;
        }

        /// <summary>
        /// Get Message Response
        /// </summary>
        public virtual BridgeMessage GetMessage()
        {
            GetMessageResponse getMsg = (GetMessageResponse)this.IpmiSendReceive(
            new GetMessageRequest(), typeof(GetMessageResponse));

            BridgeMessage response = new BridgeMessage(getMsg.CompletionCode);

            if (getMsg.CompletionCode == 0x00)
                response.SetParamaters(getMsg.MessageData);

            return response;
        }

        /// <summary>
        /// Get the channel state for bridging commands
        /// </summary>
        /// <param name="channel">Channel number to check</param>
        /// <param name="enabled">Channel Disabled = 0x00, Channel Enabled = 0x001</param>
        public virtual BridgeChannelReceive BridgeChannelEnabled(byte channel)
        {
            EnableMessageChannelReceiveResponse channelRec = (EnableMessageChannelReceiveResponse)this.IpmiSendReceive(
            new EnableMessageChannelReceiveRequest(channel), typeof(EnableMessageChannelReceiveResponse));

            BridgeChannelReceive resposne = new BridgeChannelReceive(channelRec.CompletionCode);

            if (channelRec.CompletionCode == 0x00)
                resposne.SetParamaters(channelRec.Channel, channelRec.ChannelState);

            return resposne;
        }

        /// <summary>
        /// Enable or Disable the Ipmi Bridge Channel
        /// </summary>
        /// <param name="channel">Channel number to enable</param>
        /// <param name="enabled">Enabled = true, Disabled = false</param>
        public virtual BridgeChannelReceive EnableDisableBridgeChannel(byte channel, bool enabled)
        {
            EnableMessageChannelReceiveResponse channelRec = (EnableMessageChannelReceiveResponse)this.IpmiSendReceive(
            new EnableMessageChannelReceiveRequest(channel, enabled), typeof(EnableMessageChannelReceiveResponse));

            BridgeChannelReceive resposne = new BridgeChannelReceive(channelRec.CompletionCode);

            if (channelRec.CompletionCode == 0x00)
                resposne.SetParamaters(channelRec.Channel, channelRec.ChannelState);
            
            return resposne;
        }

        #endregion

        #region Ipmi Command Support

        #region Command Support: User

        /// <summary>
        /// Get User Access, returns user access level and maximum User Ids allowed on BMC
        /// </summary>
        private GetUserAccessResponse GetUserAccess(byte userId, byte channel)
        {

            GetUserAccessResponse UserAccessResponse = (GetUserAccessResponse)this.IpmiSendReceive(
                  new GetUserAccessRequest(channel, userId),
                  typeof(GetUserAccessResponse));

            return UserAccessResponse;
        }

        #endregion

        #endregion
    }
}
