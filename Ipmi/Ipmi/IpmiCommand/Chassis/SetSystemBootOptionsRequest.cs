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

    /// <summary>
    /// IPMI 'Set System Boot Options' message works very differently depending
    /// on the parameter selector, so there is a set of subclasses for actual
    /// message subtypes and the base class.
    /// </summary>
    [IpmiMessageRequest(IpmiFunctions.Chassis,
        IpmiCommand.SetSystemBootOptions)]
    internal abstract class SetSystemBootOptionsRequest : IpmiRequest
    {
        /// <summary>
        /// Flag which is 0 to mark parameter valid/unlocked... to be
        /// understood.
        /// </summary>
        protected bool parameterLocked;

        /// <summary>
        /// Selector of parameter (valid 7 least significant bits)
        /// </summary>
        protected byte parameterSelector;

        /// <summary>
        /// Parameter data, per Table 28-14, Boot Option Parameters, IPMI Spec.
        /// </summary>
        protected byte[] parameterData;

        /// <summary>
        /// First data byte carries two fields of this class.
        /// </summary>
        [IpmiMessageData(0)]
        public byte ParameterId
        {
            get
            {
                if(parameterLocked)
                    return (byte)(0x80 & this.parameterSelector);
                else
                    return this.parameterSelector;
            }
        }

        /// <summary>
        /// The rest of data bytes depend on parameter selector and are set in
        /// the corresponding subclasses.
        /// </summary>
        [IpmiMessageData(1)]
        public byte[] ParameterData
        {
            get { return this.parameterData; }
        }
    }

    /// <summary>
    /// Parameter for setting the system boot options.
    /// The SetSystemBootOptions is used, e.g., for boot control. To send a
    /// command, the parameter have to be specified, according to IPMI spec,
    /// 28.13 .
    /// </summary>
    internal enum SystemBootOptionsParameter
    {
        SetInProgress = 0,
        ServicePartitionSelector = 1,
        ServicePartitionScan = 2,
        BootFlagValidBitClearing = 3,
        BootInfoAcknowledge = 4,
        BootFlags = 5,
        BootInitiatorInfo = 6,
        BootInitiatorMailbox = 7
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Indicates the state of the updating parameters.
    /// </summary>
    internal enum SboSetInProgress
    {
        SetComplete = 0,
        SetInProgress = 1,
        CommitWrite = 2
    }

    /// <summary>
    /// This class allows to set the state of parameter updating. Data field is
    /// 1 byte long.
    /// </summary>
    internal class SsboSetInProgress : SetSystemBootOptionsRequest
    {
        internal SsboSetInProgress(bool parameterLocked,
            SboSetInProgress state)
        {
            this.parameterLocked = parameterLocked;

            this.parameterSelector =
                (byte)SystemBootOptionsParameter.SetInProgress;

            this.parameterData = new byte[] { (byte)state };
        }
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// This mask bits allow to write corresponding bits of data.
    /// </summary>
    internal enum SboBootInfoAcknowledgeMask
    {
        EnableWriteOemFlag = 0x10,
        EnableWriteSmsFlag = 0x08,
        EnableWriteOsServiceFlag = 0x04,
        EnableWriteOsLoaderFlag = 0x02,
        EnableWriteBiosFlag = 0x01
    }

    /// <summary>
    /// Specifies which party - OEM, SMS etc. - should ignore the boot info.
    /// These bits are sent inverted - all should be 1s except those bits
    /// corresponding to parties, which should ignore the boot info.
    /// </summary>
    internal enum SboBootInfoAcknowledgeData
    {
        OemHandlingFlag = 0x10,
        SmsHandlingFlag = 0x08,
        OsServiceHandlingFlag = 0x04,
        OsLoaderHandlingFlag = 0x02,
        BiosHandlingFlag = 0x01
    }

    /// <summary>
    /// Allows individual parties to track whether thay've already handled the
    /// boot information. There are two bytes of data, the second data byte
    /// bits are sent inverted.
    /// </summary>
    internal class SsboBootInfoAcknowledge : SetSystemBootOptionsRequest
    {
        internal SsboBootInfoAcknowledge(bool parameterLocked,
            SboBootInfoAcknowledgeMask mask, SboBootInfoAcknowledgeData data)
        {
            this.parameterLocked = parameterLocked;

            this.parameterSelector =
                (byte)SystemBootOptionsParameter.BootInfoAcknowledge;

            this.parameterData = new byte[] { (byte)mask, (byte)~data };
        }
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Flags for 1st data byte.
    /// </summary>
    internal enum BootFlags
    {
        BootFlagsValid = 0x80,
        AllSubsequentBoots = 0x40,
        EfiBootType = 0x20
    }

    /// <summary>
    /// Several calls in this API set the next boot into one of the following
    /// types. The boot should follow soon (within one minute) after the boot
    /// type is set.
    /// </summary>
    public enum BootType
    {
        NoOverride = 0x00,
        ForcePxe = 0x04,
        ForceDefaultHdd = 0x08,
        ForceDefaultHddSafeMode = 0x0c,
        ForceDefaultDiagPartition = 0x10,
        ForceDefaultDvd = 0x14,
        ForceIntoBiosSetup = 0x18,
        ForceFloppyOrRemovable = 0x3c,
        Unknown = 0xff
    }

    /// <summary>
    /// Flags for 3rd byte. Bits 6:5 are one field, bits 1:0 are also one
    /// field.
    /// </summary>
    internal enum BootLocks
    {
        LockOutPowerButton = 0x80,
        VerbosityQuietDisplay = 0x20, // don't OR this flag and next
        VerbosityVerboseDisplay = 0x40,
        ForceTraps = 0x10,
        UserPasswordBypass = 0x08,
        LockOutSleepButton = 0x40,
        SuppressConsoleRedir = 0x01, // don't OR this flag and next
        RequestConsoleRedir = 0x02
    }

    /// <summary>
    /// Flags for 4th byte. Bits 2:0 are one field.
    /// </summary>
    internal enum 
        BootOverrides
    {
        BiosSharedOverride = 0x04,
        BiosMuxForceBmc = 0x01, // don't OR this flag and next
        BiosMuxForceSystem = 0x02
    }

    internal class SsboBootFlags : SetSystemBootOptionsRequest
    {
        public SsboBootFlags(bool parameterLocked, BootFlags bootFlags,
            BootType bootType, BootLocks bootLocks,
            BootOverrides bootOverrides, byte bootInstance)
        {
            //  Ipmi Spec on Instance:
            //  10001b to 11111b = internal device instance number 1 to 15, respectively
            if(bootInstance != 0)
                bootInstance = (byte)((bootInstance ^ 0x10) & 0x1F);

            this.parameterLocked = parameterLocked;

            this.parameterSelector =
                (byte)SystemBootOptionsParameter.BootFlags;

            this.parameterData = new byte[5] { (byte)bootFlags, (byte)bootType,
                (byte)bootLocks, (byte)bootOverrides, (byte)bootInstance };
        }
    }
}
