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

    /// <summary>
    /// Chassis State class for OnStart, OnStop methods and for State management
    /// </summary>
    public static class ChassisState
    {
        /// <summary>
        /// Indicates a fan failure has occured
        /// </summary>
        internal static bool FanFailure
        {
            get {return fanFailure; }
            set {fanFailure = value; }
        }

        /// <summary>
        /// Indicates a Psu failure has occured.
        /// </summary>
        internal static bool PsuFailure
        {
            get { return psuFailure; }
            set { psuFailure = value; }
        }

        /// <summary>
        /// Indicates a Chassis Manager Service Shutdown in progress.
        /// </summary>
        internal static bool ShutDown
        {
            get { return shutDown; }
            set { shutDown = value; }
        }

        #region private vars

            private static byte[] bladeState = new byte[ConfigLoaded.Population];
            private static bool fanFailure = false;
            private static bool psuFailure = false;
            private static bool shutDown = false;

        #endregion

        #region internal vars

            // lock variable for ensuring state write and read are atomic
            internal static object[] _lock = new object[ConfigLoaded.Population];

            // serial Console Metadata.
            internal static SerialConsoleMetadata[] SerialConsolePortsMetadata;

            // chassis AC Power Socket collection
            internal static AcSocket[] AcPowerSockets;

            // psu collection
            internal static PsuBase[] Psu = new PsuBase[(uint)ConfigLoaded.NumPsus];

            // fan collection
            internal static Fan[] Fans = new Fan[(uint)ConfigLoaded.NumFans];

            // Blade Power Switch (aka. Blade_EN1)
            internal static BladePowerSwitch[] BladePower = new BladePowerSwitch[ConfigLoaded.Population];

            // Chassis WatchDogTimer
            internal static WatchDogTimer Wdt = new WatchDogTimer(1);

            
            internal static byte[] FailCount = new byte[ConfigLoaded.Population];
            internal static byte[] PowerFailCount = new byte[ConfigLoaded.Population];
            internal static byte[] BladeTypeCache = new byte[ConfigLoaded.Population];
            
            internal static StatusLed AttentionLed = new StatusLed();

        #endregion

        internal static PsuModel ConvertPsuModelNumberToPsuModel(string psuModelNmber)
        {
            switch (psuModelNmber)
            {
                case "44-50-53-2D-31-32-30-30-4D-42-2D-31-20-43-20-20":
                    return PsuModel.Delta;
                case "EMERSONMODELNUMBER":
                    return PsuModel.Emerson;
                default:
                    return PsuModel.Default;
            }
        }

        internal static void Initialize()
        {
            for (int i = 0; i < ConfigLoaded.Population; i++)
            {
                _lock[i] = new object();
            }

            // Create Serial Console Port Metadata objects and initialize the sessiontoken and timestamp using the class constructor
            SerialConsolePortsMetadata = new SerialConsoleMetadata[(uint)ConfigLoaded.MaxSerialConsolePorts];
            for (int numPorts = 0; numPorts < (int)ConfigLoaded.MaxSerialConsolePorts; numPorts++)
            {
                SerialConsolePortsMetadata[numPorts] = new SerialConsoleMetadata(ChassisManagerUtil.GetSerialConsolePortIdFromIndex(numPorts), ConfigLoaded.InactiveSerialPortSessionToken, DateTime.Now);
            }

            // Create AC Power Socket objects 
            AcPowerSockets = new AcSocket[(uint)ConfigLoaded.NumPowerSwitches];
            for (int numSockets = 0; numSockets < (int)ConfigLoaded.NumPowerSwitches; numSockets++)
            {
                AcPowerSockets[numSockets] = new AcSocket((byte)(numSockets + 1));
            }

            // Create PSU objects
            for (int psuIndex = 0; psuIndex < (int)ConfigLoaded.NumPsus; psuIndex++)
            {
                // Initially create instance of the base class
                // Later.. based on the psu model number, we create the appropriate psu class object
                Psu[psuIndex] = new PsuBase((byte)(psuIndex + 1));
            }

            // Create fan objects
            for (int fanId = 0; fanId < (int)ConfigLoaded.NumPsus; fanId++)
            {
                Fans[fanId] = new Fan((byte)(fanId + 1));
            }

            // Initialize Hard Power On/Off Switches (Blade Enable)
            for (int bladeId = 0; bladeId < BladePower.Length; bladeId++)
            {
                BladePower[bladeId] = new BladePowerSwitch(Convert.ToByte(bladeId + 1));
            }

        }

        /// <summary>
        /// Gets the string value of state for the enum
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        internal static string GetStateName(byte deviceId)
        {
            return Enum.GetName(typeof(BladeState), ChassisState.bladeState[deviceId - 1]);
        }

        /// <summary>
        /// Gets the string name of the Blade Type (compute, JBOD, Xbox etc)
        /// </summary>
        /// <param name="bladeType"></param>
        /// <returns></returns>
        internal static string GetBladeTypeName(byte bladeType)
        {
            if (Enum.IsDefined(typeof(BladeTypeName), bladeType))
            {
                return Enum.GetName(typeof(BladeTypeName), bladeType);
            }
            else
            {
                return BladeTypeName.Unknown.ToString();
            }
        }

        /// <summary>
        /// gets the blade state for each blade
        /// </summary>
        /// <param name="bladeId"></param>
        /// <returns></returns>
        internal static byte GetBladeState(byte bladeId)
        {
            return bladeState[bladeId - 1];
        }

        /// <summary>
        /// Sets the blade state for each blade
        /// </summary>
        /// <param name="bladeId"></param>
        /// <param name="state"></param>
        internal static void SetBladeState(byte bladeId, byte state)
        {
            bladeState[bladeId - 1] = state;
        }

        /// <summary>
        /// Gets the blade type (compute, JBOD etc)
        /// </summary>
        /// <param name="bladeId"></param>
        /// <returns></returns>
        internal static byte GetBladeType(byte bladeId)
        {
            return BladeTypeCache[bladeId - 1];
        }
    }
}
