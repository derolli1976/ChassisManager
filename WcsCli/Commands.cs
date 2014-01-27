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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using Microsoft.GFS.WCS.Contracts;
using System.Net.NetworkInformation;
using System.Net;
using System.Management;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;

namespace Microsoft.GFS.WCS.WcsCli
{
    /// <summary>
    /// Parent generic command class
    /// </summary>
    internal class command
    {
        internal string name; // Command name
        
        // Specification for the command argument/values. 
        // Includes a character to indicate the argument and Type of the expected parameter values
        internal Dictionary<char, Type> argSpec; 
        
        internal Dictionary<char, dynamic> argVal; // Actual user entered command arguments and values
        internal Dictionary<char, char[]> conditionalOptionalArgs; // Set of arg indicators among which at least one has to be specified
        internal static uint maxArgCount = 64; // hardcoded maximum number of arguments - includes argument indicator and the value
        internal static uint maxArgLength = 64; // hardcoded maximum length of arguments parameters and command  
        internal string helpString;

        // Indicates whether the client is serial or console - will be provided at runtime by Program.cs class
        internal bool isSerialClient = false; 
        
        // Indicates whether this command is a chassis manager command (true) or a local command (false). 
        // This needs to be statically populated in commandInitializer in WcsCliCmProxy.cs 
        internal bool isCmServiceCommand = true; 
        
        // virtual command specific implementation function - will get overrided by inherited individual commands
        internal virtual void commandImplementation()
        {
        }

        // constructor of the parent command class
        internal command()
        {
            this.argSpec = new Dictionary<char, Type>();
            this.argVal = new Dictionary<char, dynamic>();
            this.conditionalOptionalArgs = new Dictionary<char, char[]>();
            this.helpString = "";
        }

        internal static void printTabSeperatedStrings(List<String> inputStrings)
        {
            for (int myIndex = 0; myIndex < inputStrings.Count; myIndex++)
            {
                if (myIndex != 0)
                {
                    Console.Write(" ");
                }
                Console.Write(inputStrings[myIndex]);
                Console.Write("\t");
                if (myIndex < inputStrings.Count - 1)
                {
                    Console.Write("|");
                }
            }
            Console.WriteLine();
        }

        /// <summary>
        /// This method displays error message based on the completion code returned by user.
        /// </summary>
        /// <param name="completionCode">API completion code</param>
        protected void printErrorMessage(Contracts.CompletionCode completionCode)
        {
            if (completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
                return;
            }
            if (completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
                return;
            }
            if (completionCode != Contracts.CompletionCode.Success)
            {
                Console.WriteLine("Command failed with completion code : {0}", completionCode);
                return;
            }
        }
    }

    /// <summary>
    /// GetChassisInfo command class - derives from parent command class
    /// Constructor initializes command argument indicators and argument type
    /// </summary>
    internal class getinfo : command
    {
        internal getinfo()
        {
            this.name = WcsCliConstants.getinfo;
            this.argSpec.Add('s', null);
            this.argSpec.Add('p', null);
            this.argSpec.Add('c', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.getchassisinfoHelp;
            this.conditionalOptionalArgs = null;
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            bool noArgSpecified = false;
            if (!(this.argVal.ContainsKey('c') || this.argVal.ContainsKey('s') || this.argVal.ContainsKey('p')))
            {
                noArgSpecified = true;
            }
            Contracts.ChassisInfoResponse myPacket = new Contracts.ChassisInfoResponse();
            try
            {
                if (noArgSpecified)
                {
                    myPacket = WcsCli2CmConnectionManager.channel.GetChassisInfo(true, true, true);
                }
                else
                {
                    myPacket = WcsCli2CmConnectionManager.channel.GetChassisInfo(this.argVal.ContainsKey('s'), this.argVal.ContainsKey('p'), this.argVal.ContainsKey('c'));
                }
            }

            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myPacket == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }
            if (myPacket.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
                return;
            }
            if (myPacket.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
                return;
            }
            if (myPacket.completionCode != Contracts.CompletionCode.Success)
            {
                Console.WriteLine("Command failed with completion code : {0}", myPacket.completionCode);
                return;
            }
            // Display output 
            List<string> myStrings = new List<string>();

            if (noArgSpecified == true || this.argVal.ContainsKey('s'))
            {
                // bladeCollections output
                if (myPacket.bladeCollections == null)
                {
                    Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                    return;
                }
                Console.WriteLine();
                Console.WriteLine(WcsCliConstants.getinfoComputeNodesHeader);
                myStrings.Add("#"); myStrings.Add("Name"); myStrings.Add("GUID\t\t\t\t"); myStrings.Add("State"); myStrings.Add("BMC MAC");
                myStrings.Add("Completion Code");
                printTabSeperatedStrings(myStrings);

                foreach (BladeInfo gs in myPacket.bladeCollections)
                {
                    myStrings.RemoveAll(item => (1 == 1));
                    if (gs != null)
                    {
                        myStrings.Add(gs.bladeNumber.ToString());

                        if (gs.bladeName != null)
                        {
                            if (gs.bladeName.ToString().Length < 6)
                            {
                                myStrings.Add(gs.bladeName.ToString() + " ");
                            }
                            else
                            {
                                myStrings.Add(gs.bladeName.ToString());
                            }
                        }
                        else
                        {
                            myStrings.Add("");
                        }

                        if (gs.bladeGuid != null)
                        {
                            myStrings.Add(gs.bladeGuid.ToString());
                        }
                        else
                        {
                            myStrings.Add("");
                        }

                        if (gs.powerState == Contracts.PowerState.ON)
                        {
                            myStrings.Add("On");
                        }
                        else if (gs.powerState == Contracts.PowerState.OFF)
                        {
                            myStrings.Add("Off");
                        }
                        else
                        {
                            myStrings.Add("--");
                        }

                        if (gs.bladeMacAddress != null)
                        {
                            foreach (NicInfo info in gs.bladeMacAddress)
                            {
                                myStrings.Add("DeviceID: " + info.deviceId + "MAC Address: " + info.macAddress);
                            }
                        }
                        else
                        {
                            myStrings.Add("");
                        }

                        myStrings.Add(gs.completionCode.ToString());

                    }
                    printTabSeperatedStrings(myStrings);
                }
            }

            if (noArgSpecified == true || this.argVal.ContainsKey('p'))
            {
                // psuCollections output
                if (myPacket.psuCollections == null)
                {
                    Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                    return;
                }
                Console.WriteLine();
                Console.WriteLine(WcsCliConstants.getinfoPowerSuppliesHeader);
                myStrings.RemoveAll(item => (1 == 1));
                myStrings.Add("#"); myStrings.Add("Serial Num"); myStrings.Add("State"); myStrings.Add("Pout (W)");
                myStrings.Add("Completion Code");
                printTabSeperatedStrings(myStrings);

                foreach (PsuInfo gp in myPacket.psuCollections)
                {
                    myStrings.RemoveAll(item => (1 == 1));
                    if (gp != null)
                    {
                        myStrings.Add(gp.id.ToString());

                        if (gp.serialNumber != null)
                        {
                            myStrings.Add(gp.serialNumber.ToString());
                        }
                        else
                        {
                            myStrings.Add("");
                        }

                        if (gp.serialNumber != null)
                        {
                            if (gp.state == Contracts.PowerState.ON)
                            {
                                myStrings.Add("On");
                            }
                            else if (gp.state == Contracts.PowerState.OFF)
                            {
                                myStrings.Add("Off");
                            }
                            else
                            {
                                myStrings.Add("NA");
                            }
                        }
                        else
                        {
                            myStrings.Add("");
                        }

                        myStrings.Add(gp.powerOut.ToString());

                        myStrings.Add(gp.completionCode.ToString());
                    }
                    printTabSeperatedStrings(myStrings);
                }
            }

            if (noArgSpecified == true || this.argVal.ContainsKey('c'))
            {
                // Chassis Manager
                if (myPacket.chassisController == null)
                {
                    Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                    return;
                }
                Console.WriteLine();
                Console.WriteLine(WcsCliConstants.getinfoChassisControllerHeader);
                Console.WriteLine("Firmware Version\t: " + ((myPacket.chassisController.firmwareVersion != null) ? myPacket.chassisController.firmwareVersion : ""));
                Console.WriteLine("Hardware Version\t: " + ((myPacket.chassisController.hardwareVersion != null) ? myPacket.chassisController.hardwareVersion : ""));
                Console.WriteLine("Serial Number\t\t: " + ((myPacket.chassisController.serialNumber != null) ? myPacket.chassisController.serialNumber : ""));
                Console.WriteLine("Asset Tag\t\t: " + ((myPacket.chassisController.assetTag != null) ? myPacket.chassisController.assetTag : ""));
                if (myPacket.chassisController.systemUptime != null)
                {
                    Console.WriteLine("System Uptime\t\t: " + myPacket.chassisController.systemUptime);
                }
                else
                {
                    Console.WriteLine("System Uptime\t\t: " + " ");
                }
                if (myPacket.chassisController.networkProperties != null)
                {
                    for (int i = 0; i < myPacket.chassisController.networkProperties.chassisNetworkPropertyCollection.Count; i++)
                    {
                        Console.WriteLine("N/w Interface {0}:", i);
                        Console.WriteLine("\tIP Address\t\t:" + myPacket.chassisController.networkProperties.chassisNetworkPropertyCollection[i].ipAddress);
                        Console.WriteLine("\tMAC Address\t\t:" + myPacket.chassisController.networkProperties.chassisNetworkPropertyCollection[i].macAddress);
                        Console.WriteLine("\tDHCP Server\t\t:" + myPacket.chassisController.networkProperties.chassisNetworkPropertyCollection[i].dhcpServer);
                        Console.WriteLine("\tDHCP Enabled\t\t:" + myPacket.chassisController.networkProperties.chassisNetworkPropertyCollection[i].dhcpEnabled.ToString());
                        Console.WriteLine("\tDNS Address\t\t:" + myPacket.chassisController.networkProperties.chassisNetworkPropertyCollection[i].dnsAddress);
                        Console.WriteLine("\tDNS Domain\t\t:" + myPacket.chassisController.networkProperties.chassisNetworkPropertyCollection[i].dnsDomain);
                        Console.WriteLine("\tDNS Hostname\t\t:" + myPacket.chassisController.networkProperties.chassisNetworkPropertyCollection[i].dnsHostName);
                        Console.WriteLine("\tGateway Address\t\t:" + myPacket.chassisController.networkProperties.chassisNetworkPropertyCollection[i].gatewayAddress);
                        Console.WriteLine("\tSubnet Mask\t\t:" + myPacket.chassisController.networkProperties.chassisNetworkPropertyCollection[i].subnetMask);
                        Console.WriteLine("\tCompletion Code\t\t:" + myPacket.chassisController.networkProperties.chassisNetworkPropertyCollection[i].completionCode);
                    }
                }
                Console.WriteLine("Completion Code\t\t:" + myPacket.chassisController.completionCode.ToString());
            }
        }
    }

    /// <summary>
    /// getscinfo command class - derives from parent command class
    /// Constructor initializes command argument indicators and argument type
    /// </summary>
    internal class getscinfo : command
    {
        internal getscinfo()
        {
            this.name = WcsCliConstants.getscinfo;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('h', null);

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });

            this.helpString = WcsCliConstants.getbladeinfoHelp;
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            BladeInfoResponse myPacket = new BladeInfoResponse();
            GetAllBladesInfoResponse myPackets = new GetAllBladesInfoResponse();

            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myPackets = WcsCli2CmConnectionManager.channel.GetAllBladesInfo();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    myPacket = WcsCli2CmConnectionManager.channel.GetBladeInfo((int)mySledId);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if ((this.argVal.ContainsKey('a') && myPackets == null) || (myPacket == null))
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (this.argVal.ContainsKey('a'))
            {
                for (int index = 0; index < myPackets.bladeInfoResponseCollection.Count(); index++)
                {
                    Console.WriteLine("======================= Blade {0} ========================", myPackets.bladeInfoResponseCollection[index].bladeNumber);
                    printGetscinfoPacket(myPackets.bladeInfoResponseCollection[index]);
                }
            }
            else
            {
                printGetscinfoPacket(myPacket);
            }
        }

        void printGetscinfoPacket(BladeInfoResponse myPacket)
        {
            if (myPacket.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myPacket.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else if (myPacket.completionCode == Contracts.CompletionCode.Unknown)
            {
                Console.WriteLine(WcsCliConstants.bladeStateUnknown);
            }
            else if (myPacket.completionCode != Contracts.CompletionCode.Success)
            {
                Console.WriteLine("Command failed with the completion code :{0}", myPacket.completionCode.ToString());
            }
            else if (myPacket.completionCode == Contracts.CompletionCode.Success)
            {
                // Display output 
                List<string> myStrings = new List<string>();

                // bladeCollections output
                if (myPacket == null)
                {
                    Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                    return;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(WcsCliConstants.getscinfoComputeNodeHeader);
                    Console.WriteLine("Firmware Version\t: " + myPacket.firmwareVersion);
                    Console.WriteLine("Hardware Version\t: " + myPacket.hardwareVersion);
                    Console.WriteLine("Serial Number\t\t: " + myPacket.serialNumber);
                    Console.WriteLine("Asset Tag\t\t: " + myPacket.assetTag);
                    Console.WriteLine("");
                    if (myPacket.macAddress != null)
                    {
                        Console.WriteLine(WcsCliConstants.macAddressesInfoHeader);

                        foreach (NicInfo ni in myPacket.macAddress)
                        {
                            Console.WriteLine("Device Id\t\t\t: " + ni.deviceId);
                            Console.WriteLine("MAC Address\t\t: " + ni.macAddress + "\n");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine(WcsCliConstants.dataFetchError);
            }
        }
    }

    /// <summary>
    /// GetChassisHealth command class - derives from parent command class.
    /// </summary>
    internal class GetChassisHealth : command
    {
        internal GetChassisHealth()
        {
            this.name = WcsCliConstants.getChassisHealth;
            this.argSpec.Add('b', null);
            this.argSpec.Add('p', null);
            this.argSpec.Add('f', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.getChassisHealthHelp;
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            bool noArgSpecified = false;
            Contracts.ChassisHealthResponse myPacket = new Contracts.ChassisHealthResponse();

            if (!(this.argVal.ContainsKey('b') || this.argVal.ContainsKey('p') || this.argVal.ContainsKey('f')))
            {
                noArgSpecified = true;
            }
            try
            {
                if (noArgSpecified)
                {
                    myPacket = WcsCli2CmConnectionManager.channel.GetChassisHealth(true, true, true);
                }
                else
                {
                    myPacket = WcsCli2CmConnectionManager.channel.GetChassisHealth(this.argVal.ContainsKey('b'), this.argVal.ContainsKey('p'), this.argVal.ContainsKey('f'));
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            // If service response is null display error message to user & return
            if (myPacket == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            // If completion code is failure/timeout/!Success display appropriate error message and return
            if (myPacket.completionCode == Contracts.CompletionCode.Failure
                || myPacket.completionCode == Contracts.CompletionCode.Timeout
                || myPacket.completionCode != Contracts.CompletionCode.Success)
            {
                this.printErrorMessage(myPacket.completionCode);
                return;
            }

            // if no arguments are specified or if Blade health is requested
            if (noArgSpecified == true || this.argVal.ContainsKey('b'))
            {
                if (myPacket.bladeShellCollection != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(WcsCliConstants.bladeHeathHeader);

                    // For each blade shell object in the collection, display blade health data
                    foreach (BladeShellResponse bsr in myPacket.bladeShellCollection)
                    {
                        Console.WriteLine("Blade Id\t: " + bsr.bladeNumber);
                        Console.WriteLine("Blade State\t: " + bsr.bladeState);
                        Console.WriteLine("Blade Type\t: " + bsr.bladeType);
                        Console.WriteLine("");
                    }
                }
                else
                {
                    // If blade response from service is empty display error message 
                    Console.WriteLine("Blade Health: " + WcsCliConstants.serviceResponseEmpty);
                }

            }
            // if no arguments are specified or if Psu health is requested
            if (noArgSpecified == true || this.argVal.ContainsKey('p'))
            {
                if (myPacket.psuInfoCollection != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(WcsCliConstants.psuHealthHeader);

                    // For each Psu info object in the collection, display Psu data
                    foreach (PsuInfo pi in myPacket.psuInfoCollection)
                    {
                        Console.WriteLine("Psu Id\t: " + pi.id);
                        Console.WriteLine("Psu Serial Number\t: " + pi.serialNumber);
                        Console.WriteLine("Psu State\t\t: " + pi.state.ToString());
                        Console.WriteLine("PSU Power Out\t\t: " + pi.powerOut);
                        Console.WriteLine("Psu Completion code: " + pi.completionCode);
                        Console.WriteLine("");
                    }
                }
                else
                {
                    // If Psu response from service is empty display error message 
                    Console.WriteLine("PSU Health: " + WcsCliConstants.serviceResponseEmpty);
                }
            }

            // if no arguments are specified or if Fan health is requested
            if (noArgSpecified == true || this.argVal.ContainsKey('f'))
            {
                if (myPacket.fanInfoCollection != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(WcsCliConstants.fanHealthHeader);

                    // If there are no fans present, print this message
                    if (myPacket.fanInfoCollection.Count() == 0)
                    {
                        Console.WriteLine("No fan data available");
                        return;
                    }

                    // For each Fan info object in the collection, display Fan data
                    foreach (FanInfo fi in myPacket.fanInfoCollection)
                    {
                        if (fi.completionCode == Contracts.CompletionCode.Success)
                        {
                            Console.WriteLine("Fan Id\t: " + fi.fanId);
                            Console.WriteLine("Fan Speed: " + fi.fanSpeed);
                            Console.WriteLine("Fan status: " + fi.isFanHealthy);
                            Console.WriteLine("");
                        }
                        else if (fi.completionCode == Contracts.CompletionCode.FanlessChassis)
                        {
                            Console.WriteLine("Fan Id\t: " + fi.fanId);
                            Console.WriteLine("Fanless Chassis");
                            Console.WriteLine("");
                        }
                    }
                }
                else
                {
                    // If Fan response from service is empty display error message 
                    Console.WriteLine("Fan Health: " + WcsCliConstants.serviceResponseEmpty);
                    Console.WriteLine("No fan data found.");
                }
            }
        }
    }

    /// <summary>
    /// This command is called to terminate connection to CM
    /// </summary>
    internal class TerminateCmConnection : command
    {
        internal TerminateCmConnection()
        {
            this.name = WcsCliConstants.terminateCmConnection;
            this.helpString = WcsCliConstants.terminateCmConnectionHelp;
            this.argSpec.Add('h', null);
        }

        internal override void commandImplementation()
        {
            try
            {
                WcsCli2CmConnectionManager.TerminateConnectionToCmService();
                Console.WriteLine("Connection to CM terminated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection to CM failed.");
                Console.WriteLine("Exception is " + ex);
            }
        }
    }

    // <summary>
    /// This command is called at the start of application to get host, port, 
    /// SSL encryption option, batch file option from user.
    /// This method sets the config parameters.
    /// </summary>
    internal class EstablishConnectionToCm : command
    {
        internal EstablishConnectionToCm()
        {
            this.name = WcsCliConstants.establishCmConnection;
            this.argSpec.Add('m', Type.GetType("System.String"));
            this.argSpec.Add('p', Type.GetType("System.UInt32"));
            this.argSpec.Add('s', Type.GetType("System.UInt32"));
            this.argSpec.Add('u', Type.GetType("System.String"));
            this.argSpec.Add('x', Type.GetType("System.String"));
            this.argSpec.Add('b', Type.GetType("System.String"));
            this.argSpec.Add('v', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.establishCmConnectionHelp;
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            dynamic hostname = null;
            dynamic port = null;
            dynamic sslOption = null;
            dynamic username = null;
            dynamic password = null;

            string tempHostname = null;
            int tempPortno;
            bool tempSslEnabled = false;
            string tempUsername = null;
            string tempPassword = null;

            try
            {
                // version information is requested
                if (this.argVal.ContainsKey('v'))
                {
                    // display CLI version
                    Console.WriteLine("WCSCLI version: " + WcsCli2CmConnectionManager.GetCLIVersion());
                    // If version info is requested, do not process anything else.. just exit the command.. 
                    return;
                }
                // If this is a serial (local) client, CM hostname is assumed as localhost
                if(this.isSerialClient)
                {
                    this.argVal['m'] = "localhost";
                }
                // If hostname is not provided, CM hostname is assumed as localhost
                if(!this.argVal.TryGetValue('m', out hostname))
                    {
                    hostname = "localhost";
                }
                if (this.argVal.TryGetValue('p', out port) && this.argVal.TryGetValue('s', out sslOption))
                {
                    tempHostname = (string)hostname;
                    tempPortno = (int)port;
                        if ((int)sslOption == 1)
                        {
                        tempSslEnabled = true;
                        }
                        else
                        {
                        tempSslEnabled = false;
                        }

                        // check if both username and password are specified, else use default credentials
                        if (this.argVal.TryGetValue('u', out username) && this.argVal.TryGetValue('x', out password))
                        {
                        tempUsername = (string)username;
                        tempPassword = (string)password;
                        }
                        else
                        {
                            Console.WriteLine("Using current user context as credentials");
                        }
                    // Try to establish a connection
                    WcsCli2CmConnectionManager.CreateConnectionToService(tempHostname, tempPortno, tempSslEnabled, tempUsername, tempPassword);
                    if (!WcsCli2CmConnectionManager.TestConnectionToCmService())
                    {
                        Console.WriteLine("Connection to CM is not successful. \n");
                    }
                    else
                    {
                        Console.WriteLine("Connection to CM succeeded.. \n");
                    }
                }
                    else
                    {
                        Console.WriteLine(WcsCliConstants.invalidCommandString);
                    }
                }
            catch (Exception ex)
            {
                Console.WriteLine(WcsCliConstants.invalidCommandString + ex.Message);
                return;
            }
        }
    }

    /// <summary>
    /// GetBladeHealth command class - derives from parent command class
    /// Constructor initializes command argument indicators and argument type
    /// </summary>
    internal class GetBladeHealth : command
    {
        internal GetBladeHealth()
        {
            this.name = WcsCliConstants.getBladeHealth;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('m', null);
            this.argSpec.Add('d', null);
            this.argSpec.Add('p', null);
            this.argSpec.Add('s', null);
            this.argSpec.Add('t', null);
            this.argSpec.Add('f', null);
            this.argSpec.Add('h', null);

            this.conditionalOptionalArgs.Add('i', new char[] { 'a', 'h'});

            this.helpString = WcsCliConstants.getBladeHealthHelp;
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            BladeHealthResponse myPacket = new BladeHealthResponse();
            bool noArgSpecified = false;
            dynamic bladeId = null;

            if (!(this.argVal.ContainsKey('i')))
            {
                Console.WriteLine("No blade ID specified, please look command help");
                return;
            }

            if (!(this.argVal.ContainsKey('a') || this.argVal.ContainsKey('m') || this.argVal.ContainsKey('d') || this.argVal.ContainsKey('p')
                || this.argVal.ContainsKey('s') || this.argVal.ContainsKey('t') || this.argVal.ContainsKey('f')))
            {
                noArgSpecified = true;
            }

            try
            {
                this.argVal.TryGetValue('i', out bladeId);

                if (noArgSpecified)
                {
                    myPacket = WcsCli2CmConnectionManager.channel.GetBladeHealth((int)bladeId, true, true, true, true, true, true, true);
                }
                else
                {
                    myPacket = WcsCli2CmConnectionManager.channel.GetBladeHealth((int)bladeId, this.argVal.ContainsKey('a'), this.argVal.ContainsKey('m'),
                       this.argVal.ContainsKey('d'), this.argVal.ContainsKey('p'), this.argVal.ContainsKey('s'), this.argVal.ContainsKey('t'), this.argVal.ContainsKey('f'));
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            // If service response is null display error message to user & return
            if (myPacket == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }
            // If completion code is failure/timeout/!Success display appropriate error message and return
            if (myPacket.completionCode == Contracts.CompletionCode.Failure
                || myPacket.completionCode == Contracts.CompletionCode.Timeout
                || myPacket.completionCode != Contracts.CompletionCode.Success)
            {
                this.printErrorMessage(myPacket.completionCode);
                return;
            }

            Console.WriteLine();
            Console.WriteLine("== Blade " + bladeId + " Health Information ==");
            Console.WriteLine("Blade ID\t: " + myPacket.bladeShell.bladeNumber);
            Console.WriteLine("Blade State\t: " + myPacket.bladeShell.bladeState);
            Console.WriteLine("Blade Type\t: " + myPacket.bladeShell.bladeType);

            // Show additional information if JBOD
            if (myPacket.bladeShell.bladeType == WcsCliConstants.BladeTypeJBOD && myPacket.jbodInfo != null)
            {
                Console.WriteLine("Unit\t: " + myPacket.jbodInfo.unit);
                Console.WriteLine("Reading\t: " + myPacket.jbodInfo.reading);
            }

            // if no arguments are specified or if Processor health is requested
            if (noArgSpecified == true || this.argVal.ContainsKey('a'))
            {
                if (myPacket.processorInfo != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(WcsCliConstants.cpuInfo);

                    // For each processor object in the collection, display processor health data
                    foreach (ProcessorInfo pri in myPacket.processorInfo)
                    {
                        Console.WriteLine("Processor Id\t: " + pri.procId);
                        Console.WriteLine("Processor Type\t: " + pri.procType);
                        Console.WriteLine("Processor Frequence\t: " + pri.frequency);
                        Console.WriteLine("");
                    }
                }
                else if (myPacket.processorInfo == null && myPacket.bladeShell.bladeType == WcsCliConstants.BladeTypeCompute)
                {
                    // If blade response from service is empty display error message 
                    Console.WriteLine("Blade Processor Information: " + WcsCliConstants.serviceResponseEmpty);
                }

            }
            // if no arguments are specified or if Memory health is requested
            if (noArgSpecified == true || this.argVal.ContainsKey('m'))
            {
                if (myPacket.memoryInfo != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(WcsCliConstants.memoryInfo);

                    // For each memoey info object in the collection, display Memory data
                    foreach (MemoryInfo mi in myPacket.memoryInfo)
                    {
                        Console.WriteLine("Dimm\t: " + mi.dimm);
                        Console.WriteLine("Dimm Type\t: " + mi.dimmType);
                        Console.WriteLine("Memory Voltage\t\t: " + mi.memVoltage);
                        Console.WriteLine("Size\t\t: " + mi.size);
                        Console.WriteLine("Speed\t\t: " + mi.speed);
                        Console.WriteLine("Memory Status\t\t: " + mi.status);
                        Console.WriteLine("Memory Completion code: " + mi.completionCode.ToString());
                        Console.WriteLine("");
                    }
                }
                else if (myPacket.processorInfo == null && myPacket.bladeShell.bladeType == WcsCliConstants.BladeTypeCompute)
                {
                    // If response from service is empty display error message 
                    Console.WriteLine("Blade memory Information: " + WcsCliConstants.serviceResponseEmpty);
                }
            }
            // if no arguments are specified or if Disk health is requested
            if (noArgSpecified == true || this.argVal.ContainsKey('d'))
            {
                if (myPacket.bladeShell.bladeType == WcsCliConstants.BladeTypeCompute && myPacket.bladeDisk != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(WcsCliConstants.diskInfo);

                    // For each disk info object in the collection, display disk data
                    foreach (DiskInfo di in myPacket.bladeDisk)
                    {
                        Console.WriteLine("Disk Id\t: " + di.diskId);
                        Console.WriteLine("Disk Speed\t: " + di.diskStatus);
                        Console.WriteLine("Disk CompletionCode\t\t: " + di.completionCode.ToString());
                        Console.WriteLine("");
                    }
                }
                else if (myPacket.bladeShell.bladeType == WcsCliConstants.BladeTypeJBOD && myPacket.jbodDiskInfo != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(WcsCliConstants.diskInfo);
                    Console.WriteLine("JBOD Disk Count\t: " + myPacket.jbodDiskInfo.diskCount);
                    Console.WriteLine("JBOD Disk Channel\t: " + myPacket.jbodDiskInfo.channel);
                    Console.WriteLine("Disk CompletionCode\t\t: " + myPacket.jbodDiskInfo.completionCode.ToString());
                    Console.WriteLine("");
                    foreach (DiskInfo di in myPacket.jbodDiskInfo.diskInfo)
                    {
                        Console.WriteLine("== Disk " + di.diskId + " ==");
                        Console.WriteLine("JBOD Disk ID\t: " + di.diskId);
                        Console.WriteLine("JBOD Disk Status\t: " + di.diskStatus);
                        Console.WriteLine("");
                    }
                }
                else if ((myPacket.bladeShell.bladeType == WcsCliConstants.BladeTypeCompute && myPacket.bladeDisk == null) ||
                        (myPacket.bladeShell.bladeType == WcsCliConstants.BladeTypeJBOD && myPacket.jbodDiskInfo == null))
                {
                    // If disk response from service is empty display error message 
                    Console.WriteLine("Blade health : Disk Information: " + WcsCliConstants.serviceResponseEmpty);
                }
            }
            // if no arguments are specified or if PCIE health is requested
            if (noArgSpecified == true || this.argVal.ContainsKey('p'))
            {
                if (myPacket.pcieInfo != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(WcsCliConstants.pcieInfo);

                    // For each PCIE info object in the collection, display PCIE data
                    foreach (PCIeInfo pci in myPacket.pcieInfo)
                    {
                        Console.WriteLine("PCIE Id\t: " + pci.deviceId);
                        Console.WriteLine("PCIE Number\t: " + pci.pcieNumber);
                        Console.WriteLine("PCIE Sub System Id\t\t: " + pci.subSystemId);
                        Console.WriteLine("PCIE System Id\t\t: " + pci.systemId);
                        Console.WriteLine("PCIE Vendor Id\t\t: " + pci.vendorId);
                        Console.WriteLine("PCIE Completion Code\t\t: " + pci.completionCode.ToString());
                        Console.WriteLine("");
                    }
                }
                else if (myPacket.processorInfo == null && myPacket.bladeShell.bladeType == WcsCliConstants.BladeTypeCompute)
                {
                    // If PCIE response from service is empty display error message 
                    Console.WriteLine("Blade PCIE Information: " + WcsCliConstants.serviceResponseEmpty);
                }
            }

            // if no arguments are specified or if Sensor info is requested
            if (noArgSpecified == true || this.argVal.ContainsKey('s'))
            {
                if (myPacket.sensors != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(WcsCliConstants.sensorInfo);

                    // For each Sensor info object in the collection, display Sensor data
                    foreach (SensorInfo si in myPacket.sensors)
                    {
                        Console.WriteLine("Sensor Number\t: " + si.sensorNumber);
                        Console.WriteLine("Sensor Type\t: " + si.sensorType);
                        Console.WriteLine("Sensor Reading\t\t: " + si.reading);
                        Console.WriteLine("Sensor Description\t\t: " + si.description);
                        Console.WriteLine("Sensor Entity\t\t: " + si.entity);
                        Console.WriteLine("Sensor Entity Instance\t\t: " + si.entityInstance);
                        Console.WriteLine("Sensor Status\t\t: " + si.status);
                        Console.WriteLine("Sensor CompletionCode\t\t: " + si.completionCode.ToString());
                        Console.WriteLine("");
                    }
                    Console.WriteLine("");
                }
                else if (myPacket.sensors == null && myPacket.bladeShell.bladeType == WcsCliConstants.BladeTypeCompute)
                {
                    // If Sensor response from service is empty display error message 
                    Console.WriteLine("Blade Sensor Information: " + WcsCliConstants.serviceResponseEmpty);
                }
            }

            // if no arguments are specified or if temp Sensor info is requested
            if (noArgSpecified == true || this.argVal.ContainsKey('t'))
            {
                if (myPacket.sensors != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(WcsCliConstants.tempSensorInfo);

                    // For each Sensor info object in the collection, display Sensor data
                    foreach (SensorInfo si in myPacket.sensors)
                    {
                        if (si.sensorType == WcsCliConstants.SensorTypeTemp)
                        {
                            Console.WriteLine("Sensor Number\t: " + si.sensorNumber);
                            Console.WriteLine("Sensor Type\t: " + si.sensorType);
                            Console.WriteLine("Sensor Reading\t\t: " + si.reading);
                            Console.WriteLine("Sensor Description\t\t: " + si.description);
                            Console.WriteLine("Sensor Entity\t\t: " + si.entity);
                            Console.WriteLine("Sensor Entity Instance\t\t: " + si.entityInstance);
                            Console.WriteLine("Sensor Status\t\t: " + si.status);
                            Console.WriteLine("Sensor CompletionCode\t\t: " + si.completionCode.ToString());
                            Console.WriteLine("");
                        }
                    }
                    Console.WriteLine("");
                }
                else if (myPacket.sensors == null && myPacket.bladeShell.bladeType == WcsCliConstants.BladeTypeCompute)
                {
                    // If Sensor response from service is empty display error message 
                    Console.WriteLine("Blade Temprature Sensor Information: " + WcsCliConstants.serviceResponseEmpty);
                }
            }

            // if no arguments are specified or if FRU information is requested
            if (noArgSpecified == true || this.argVal.ContainsKey('f'))
            {
                if (myPacket != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(WcsCliConstants.fruInfo);
                    Console.WriteLine("Blade Serial Number\t: " + myPacket.serialNumber);
                    Console.WriteLine("Blade Asset Tag\t: " + myPacket.assetTag);
                    Console.WriteLine("Blade Product Type\t: " + myPacket.productType);
                    Console.WriteLine("Blade Hardware Version\t: " + myPacket.hardwareVersion);
                    Console.WriteLine("");
                }
                else if (myPacket.processorInfo == null && (myPacket.bladeShell.bladeType == WcsCliConstants.BladeTypeCompute ||
                    myPacket.bladeShell.bladeType == WcsCliConstants.BladeTypeJBOD))
                {
                    // If FRU response from service is empty display error message 
                    Console.WriteLine("Blade FRU Info: " + WcsCliConstants.serviceResponseEmpty);
                }
            }
        }
    }

    /// <summary>
    /// help command class - derives from parent command class
    /// Constructor initializes command argument indicators and argument type
    /// </summary>
    internal class help : command
    {
        internal help()
        {
            this.name = WcsCliConstants.help;
            this.argSpec = null;
            this.conditionalOptionalArgs = null;
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            Console.WriteLine(WcsCliConstants.WcsCliHelp);
        }
    }

    internal class ncidstatus : command
    {
        internal ncidstatus()
        {
            this.name = WcsCliConstants.ncidstatus;
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.getchassisattentionledstatusHelp;
            this.conditionalOptionalArgs = null;
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            LedStatusResponse myResponse = new LedStatusResponse();
            try
            {
                myResponse = WcsCli2CmConnectionManager.channel.GetChassisAttentionLEDStatus();
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine("Chassis LED: " + myResponse.ledState);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode.ToString());
            }
        }
    }

    internal class ncidon : command
    {
        internal ncidon()
        {
            this.name = WcsCliConstants.ncidon;
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setchassisattentionledonHelp;
            this.conditionalOptionalArgs = null;
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            Contracts.ChassisResponse myResponse = new Contracts.ChassisResponse();
            try
            {
                myResponse = WcsCli2CmConnectionManager.channel.SetChassisAttentionLEDOn();
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine(WcsCliConstants.commandSucceeded);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode.ToString());
            }
        }
    }

    internal class ncidoff : command
    {
        internal ncidoff()
        {
            this.name = WcsCliConstants.ncidoff;
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setchassisattentionledoffHelp;
            this.conditionalOptionalArgs = null;
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            Contracts.ChassisResponse myResponse = new Contracts.ChassisResponse();
            try
            {
                myResponse = WcsCli2CmConnectionManager.channel.SetChassisAttentionLEDOff();
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine(WcsCliConstants.commandSucceeded);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode.ToString());
            }
        }
    }

    internal class scidon : command
    {
        internal scidon()
        {
            this.name = WcsCliConstants.scidon;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setbladeattentionledonHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            uint sledId = 1;
            BladeResponse myResponse = new BladeResponse();
            AllBladesResponse myResponses = new AllBladesResponse();
            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.SetAllBladesAttentionLEDOn();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.SetBladeAttentionLEDOn((int)mySledId);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if ((this.argVal.ContainsKey('a') && myResponses == null) || myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (this.argVal.ContainsKey('a'))
            {
                for (int index = 0; index < myResponses.bladeResponseCollection.Count(); index++)
                {
                    if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Success)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": Attention LED ON");
                    }
                    else if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                    }
                    else
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + myResponses.bladeResponseCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": Attention LED ON");
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + myResponse.completionCode.ToString());
                }
            }
        }
    }

    internal class scidoff : command
    {
        internal scidoff()
        {
            this.name = WcsCliConstants.scidoff;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setbladeattentionledoffHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            uint sledId = 1;
            BladeResponse myResponse = new BladeResponse();
            AllBladesResponse myResponses = new AllBladesResponse();
            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.SetAllBladesAttentionLEDOff();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.SetBladeAttentionLEDOff((int)mySledId);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if ((this.argVal.ContainsKey('a') && myResponses == null) || myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (this.argVal.ContainsKey('a'))
            {
                for (int index = 0; index < myResponses.bladeResponseCollection.Count(); index++)
                {
                    if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Success)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": Attention LED OFF");
                    }
                    else if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                    }
                    else
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + myResponses.bladeResponseCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": Attention LED OFF");
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + myResponse.completionCode.ToString());
                }
            }
        }
    }

    internal class setscponstate : command
    {
        internal setscponstate()
        {
            this.name = WcsCliConstants.setscponstate;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('s', Type.GetType("System.UInt32"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setbladedefaultpowerstateHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });
            this.conditionalOptionalArgs.Add('s', null);
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            BladeResponse myResponse = new BladeResponse();
            AllBladesResponse myResponses = new AllBladesResponse();
            dynamic myState = null;
            dynamic mySledId = null;
            this.argVal.TryGetValue('s', out myState);

            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    if ((uint)myState == 0)
                    {
                        myResponses = WcsCli2CmConnectionManager.channel.SetAllBladesDefaultPowerStateOff();
                    }
                    else if ((uint)myState == 1)
                    {
                        myResponses = WcsCli2CmConnectionManager.channel.SetAllBladesDefaultPowerStateOn();
                    }
                    else
                    {
                        Console.WriteLine("Invalid power state.");
                        return;
                    }
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    this.argVal.TryGetValue('i', out mySledId);
                    if ((uint)myState == 0)
                    {
                        myResponse = WcsCli2CmConnectionManager.channel.SetBladeDefaultPowerStateOff((int)mySledId);
                    }
                    else if ((uint)myState == 1)
                    {
                        myResponse = WcsCli2CmConnectionManager.channel.SetBladeDefaultPowerStateOn((int)mySledId);
                    }
                    else
                    {
                        Console.WriteLine("Invalid power state.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if ((this.argVal.ContainsKey('a') && myResponses == null) || myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (this.argVal.ContainsKey('a'))
            {
                for (int index = 0; index < myResponses.bladeResponseCollection.Count(); index++)
                {
                    if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Success)
                    {
                        if ((uint)myState == 0)
                        {
                            Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ":OFF");
                        }
                        else if ((uint)myState == 1)
                        {
                            Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ":ON");
                        }
                    }
                    else if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                    }
                    else
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + myResponses.bladeResponseCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    if ((uint)myState == 0)
                    {
                        Console.WriteLine("Blade " + myResponse.bladeNumber + ":OFF");
                    }
                    else if ((uint)myState == 1)
                    {
                        Console.WriteLine("Blade " + myResponse.bladeNumber + ":ON");
                    }
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + myResponse.completionCode.ToString());
                }
            }
        }
    }

    internal class getscponstate : command
    {
        internal getscponstate()
        {
            this.name = WcsCliConstants.getscponstate;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.getbladedefaultpowerstateHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            dynamic mySledId = null;
            BladeStateResponse myResponse = new BladeStateResponse();
            GetAllBladesStateResponse myResponses = new GetAllBladesStateResponse();
            uint sledId = 1;
            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.GetAllBladesDefaultPowerState();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.GetBladeDefaultPowerState((int)mySledId);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if ((this.argVal.ContainsKey('a') && myResponses == null) || myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (this.argVal.ContainsKey('a'))
            {
                for (int index = 0; index < myResponses.bladeStateResponseCollection.Count(); index++)
                {
                    if (myResponses.bladeStateResponseCollection[index].completionCode == Contracts.CompletionCode.Success)
                    {
                        if (myResponses.bladeStateResponseCollection[index].bladeState == Contracts.PowerState.ON)
                        {
                            Console.WriteLine("Blade Default Power State" + myResponses.bladeStateResponseCollection[index].bladeNumber + ": ON");
                        }
                        else if (myResponses.bladeStateResponseCollection[index].bladeState == Contracts.PowerState.OFF)
                        {
                            Console.WriteLine("Blade Default Power State" + myResponses.bladeStateResponseCollection[index].bladeNumber + ": OFF");
                        }
                        else
                        {
                            Console.WriteLine("Blade Default Power State" + myResponses.bladeStateResponseCollection[index].bladeNumber + ": --");
                        }
                    }
                    else if (myResponses.bladeStateResponseCollection[index].completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade Default Power State" + myResponses.bladeStateResponseCollection[index].bladeNumber + ": "
                            + WcsCliConstants.bladeStateUnknown);
                    }
                    else
                    {
                        // If not success/unknown display the exact error code returned from service
                        Console.WriteLine("Blade Default Power State" + myResponses.bladeStateResponseCollection[index].bladeNumber + ": "
                            + myResponses.bladeStateResponseCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    if (myResponse.bladeState == Contracts.PowerState.ON)
                    {
                        Console.WriteLine("Blade Default Power State" + myResponse.bladeNumber + ": ON");
                    }
                    else if (myResponse.bladeState == Contracts.PowerState.OFF)
                    {
                        Console.WriteLine("Blade Default Power State" + myResponse.bladeNumber + ": OFF");
                    }
                    else
                    {
                        Console.WriteLine("Blade Default Power State" + myResponse.bladeNumber + ": --");
                    }
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade Default Power State" + myResponse.bladeNumber + ": "
                        + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    // If not success/unknown display the exact error code returned from service
                    Console.WriteLine("Blade Default Power State" + sledId + ": " + myResponse.completionCode.ToString());
                }
            }
        }
    }

    // Try catch in all functions - code gets bloated
    internal class poweron : command
    {
        internal poweron()
        {
            this.name = WcsCliConstants.poweron;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('n', Type.GetType("System.UInt32"));
            this.argSpec.Add('w', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setpoweronHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            uint sledId = 1;
            BladeResponse myResponse = new BladeResponse();
            AllBladesResponse myResponses = new AllBladesResponse();
            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.SetAllPowerOn();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.SetPowerOn((int)mySledId);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if ((this.argVal.ContainsKey('a') && myResponses == null) || myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (this.argVal.ContainsKey('a'))
            {
                for (int index = 0; index < myResponses.bladeResponseCollection.Count(); index++)
                {
                    if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Success)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": ON");
                    }
                    else if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                    }
                    else
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + myResponses.bladeResponseCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": ON");
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + myResponse.completionCode.ToString());
                }
            }
        }
    }

    internal class bladeon : command
    {
        internal bladeon()
        {
            this.name = WcsCliConstants.bladeon;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setbladeonHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'a' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i' });
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            uint sledId = 1;
            BladeResponse myResponse = new BladeResponse();
            AllBladesResponse myResponses = new AllBladesResponse();
            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.SetAllBladesOn();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.SetBladeOn((int)mySledId);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if ((this.argVal.ContainsKey('a') && myResponses == null) || myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (this.argVal.ContainsKey('a'))
            {
                for (int index = 0; index < myResponses.bladeResponseCollection.Count(); index++)
                {
                    if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Success)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": ON");
                    }
                    else if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                    }
                    else if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Failure)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": Failed to set Blade soft power ON");
                    }
                    else
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + myResponse.completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": ON");
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": Failed to set Blade soft power ON");
                }
                else
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + myResponse.completionCode.ToString());
                }
            }
        }
    }

    internal class poweroff : command
    {
        internal poweroff()
        {
            this.name = WcsCliConstants.poweroff;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('n', Type.GetType("System.UInt32"));
            this.argSpec.Add('w', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setpoweroffHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            uint sledId = 1;
            BladeResponse myResponse = new BladeResponse();
            AllBladesResponse myResponses = new AllBladesResponse();
            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.SetAllPowerOff();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.SetPowerOff((int)mySledId);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if ((this.argVal.ContainsKey('a') && myResponses == null) || myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (this.argVal.ContainsKey('a'))
            {
                for (int index = 0; index < myResponses.bladeResponseCollection.Count(); index++)
                {
                    if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Success)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": OFF");
                    }
                    else if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                    }
                    else
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + myResponses.bladeResponseCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": OFF");
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + myResponse.completionCode.ToString());
                }
            }
        }
    }

    internal class bladeoff : command
    {
        internal bladeoff()
        {
            this.name = WcsCliConstants.bladeoff;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setbladeoffHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'a' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i' });
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            uint sledId = 1;
            BladeResponse myResponse = new BladeResponse();
            AllBladesResponse myResponses = new AllBladesResponse();
            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.SetAllBladesOff();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.SetBladeOff((int)mySledId);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if ((this.argVal.ContainsKey('a') && myResponses == null) || myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (this.argVal.ContainsKey('a'))
            {
                for (int index = 0; index < myResponses.bladeResponseCollection.Count(); index++)
                {
                    if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Success)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": OFF");
                    }
                    else if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                    }
                    else if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Failure)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": Failed to set Blade soft power OFF");
                    }
                    else
                    {
                        // Display error code retruned from service if not Success/Unknown
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ":" + myResponses.bladeResponseCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": OFF");
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": Failed to set Blade soft power OFF");
                }
                else
                {
                    // Display error code retruned from service if not Success/Unknown
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + myResponse.completionCode.ToString());
                }
            }
        }
    }

    //powercycle(int sledId, string sledName, bool doForAllSleds, uint offTime)
    internal class powercycle : command
    {
        internal powercycle()
        {
            this.name = WcsCliConstants.powercycle;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('n', Type.GetType("System.UInt32"));
            this.argSpec.Add('t', Type.GetType("System.UInt32"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setbladeactivepowercycleHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            uint sledId = 1;
            BladeResponse myResponse = new BladeResponse();
            AllBladesResponse myResponses = new AllBladesResponse();
            dynamic myOffTime = null;

            if (this.argVal.ContainsKey('t'))
            {
                this.argVal.TryGetValue('t', out myOffTime);
            }
            else
            {
                myOffTime = WcsCliConstants.powercycleOfftime;
            }

            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.SetAllBladesActivePowerCycle((uint)myOffTime);
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.SetBladeActivePowerCycle((int)mySledId, (uint)myOffTime);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if ((this.argVal.ContainsKey('a') && myResponses == null) || myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (this.argVal.ContainsKey('a'))
            {
                for (int index = 0; index < myResponses.bladeResponseCollection.Count(); index++)
                {
                    if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Success)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": OK");
                    }
                    else if (myResponses.bladeResponseCollection[index].completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                    }
                    else
                    {
                        // Display error if not Success/Unknown
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + myResponses.bladeResponseCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": OK");
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    // Display error if not Success/Unknown
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + myResponse.completionCode.ToString());
                }
            }
        }
    }

    //getscpowerstate(int sledId, string sledName, bool doForAllSleds)
    internal class getscpowerstate : command
    {
        internal getscpowerstate()
        {
            this.name = WcsCliConstants.getscpowerstate;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('n', Type.GetType("System.UInt32"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.getpowerstateHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            uint sledId = 1;
            PowerStateResponse myResponse = new PowerStateResponse();
            GetAllPowerStateResponse myResponses = new GetAllPowerStateResponse();

            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.GetAllPowerState();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.GetPowerState((int)mySledId);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if ((this.argVal.ContainsKey('a') && myResponses == null) || myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (this.argVal.ContainsKey('a'))
            {
                for (int index = 0; index < myResponses.powerStateResponseCollection.Count(); index++)
                {
                    if (myResponses.powerStateResponseCollection[index].completionCode == Contracts.CompletionCode.Success)
                    {
                        if (myResponses.powerStateResponseCollection[index].powerState == Contracts.PowerState.ON)
                        {
                            Console.WriteLine("Blade Active Power State " + myResponses.powerStateResponseCollection[index].bladeNumber + ": ON");
                        }
                        else if (myResponses.powerStateResponseCollection[index].powerState == Contracts.PowerState.OFF)
                        {
                            Console.WriteLine("Blade Active Power State " + myResponses.powerStateResponseCollection[index].bladeNumber + ": OFF");
                        }
                        else
                        {
                            Console.WriteLine("Blade Active Power State " + myResponses.powerStateResponseCollection[index].bladeNumber + ": --");
                        }
                    }
                    else if (myResponses.powerStateResponseCollection[index].completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade Active Power State " + myResponses.powerStateResponseCollection[index].bladeNumber + ": "
                            + WcsCliConstants.bladeStateUnknown);
                    }
                    else
                    {
                        // diaplay the error message if not success/unknown
                        Console.WriteLine("Blade Active Power State " + myResponses.powerStateResponseCollection[index].bladeNumber + ": "
                            + myResponses.powerStateResponseCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    if (myResponse.powerState == Contracts.PowerState.ON)
                    {
                        Console.WriteLine("Blade Active Power State " + myResponse.bladeNumber + ": ON");
                    }
                    else if (myResponse.powerState == Contracts.PowerState.OFF)
                    {
                        Console.WriteLine("Blade Active Power State " + myResponse.bladeNumber + ": OFF");
                    }
                    else
                    {
                        Console.WriteLine("Blade Active Power State " + myResponse.bladeNumber + ": --");
                    }
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade Active Power State " + myResponse.bladeNumber + ": "
                        + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    // diaplay the error message if not success/unknown
                    Console.WriteLine("Blade Active Power State " + sledId + ": "
                        + myResponse.completionCode.ToString());
                }
            }
        }
    }

    internal class getbladestate : command
    {
        internal getbladestate()
        {
            this.name = WcsCliConstants.getbladestate;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.getbladestateHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'a' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i' });
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            uint sledId = 1;
            BladeStateResponse myResponse = new BladeStateResponse();
            GetAllBladesStateResponse myResponses = new GetAllBladesStateResponse();

            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.GetAllBladesState();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.GetBladeState((int)mySledId);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if ((this.argVal.ContainsKey('a') && myResponses == null) || myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (this.argVal.ContainsKey('a'))
            {
                for (int index = 0; index < myResponses.bladeStateResponseCollection.Count(); index++)
                {
                    if (myResponses.bladeStateResponseCollection[index].completionCode == Contracts.CompletionCode.Success)
                    {
                        if (myResponses.bladeStateResponseCollection[index].bladeState == Contracts.PowerState.ON)
                        {
                            Console.WriteLine("Blade State " + myResponses.bladeStateResponseCollection[index].bladeNumber + ": ON");
                        }
                        else if (myResponses.bladeStateResponseCollection[index].bladeState == Contracts.PowerState.OFF)
                        {
                            Console.WriteLine("Blade State " + myResponses.bladeStateResponseCollection[index].bladeNumber + ": OFF");
                        }
                        else
                        {
                            Console.WriteLine("Blade State " + myResponses.bladeStateResponseCollection[index].bladeNumber + ": --");
                        }
                    }
                    else
                    {
                        // If not success, return the exact error state
                        Console.WriteLine("Blade State " + myResponses.bladeStateResponseCollection[index].bladeNumber + ": "
                            + myResponses.bladeStateResponseCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    if (myResponse.bladeState == Contracts.PowerState.ON)
                    {
                        Console.WriteLine("Blade State " + myResponse.bladeNumber + ": ON");
                    }
                    else if (myResponse.bladeState == Contracts.PowerState.OFF)
                    {
                        Console.WriteLine("Blade State " + myResponse.bladeNumber + ": OFF");
                    }
                    else
                    {
                        Console.WriteLine("Blade State " + myResponse.bladeNumber + ": --");
                    }
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade State " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    // Display error if other than success/unknown
                    Console.WriteLine("Blade State " + sledId + ": " + myResponse.completionCode.ToString());
                }
            }
        }
    }

    //powerinton(uint portNo)
    internal class powerinton : command
    {
        internal powerinton()
        {
            this.name = WcsCliConstants.powerinton;
            this.argSpec.Add('p', Type.GetType("System.UInt32"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setacsocketpowerstateonHelp;

            this.conditionalOptionalArgs.Add('p', null);
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            Contracts.ChassisResponse myResponse = new Contracts.ChassisResponse();
            dynamic myPortNo = null;
            uint portNo = 0;

            try
            {
                this.argVal.TryGetValue('p', out myPortNo);
                portNo = (uint)myPortNo;
                myResponse = WcsCli2CmConnectionManager.channel.SetACSocketPowerStateOn(portNo);
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine(WcsCliConstants.commandSucceeded);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode.ToString());
            }
        }
    }

    //powerintoff(uint portNo)
    internal class powerintoff : command
    {
        internal powerintoff()
        {
            this.name = WcsCliConstants.powerintoff;
            this.argSpec.Add('p', Type.GetType("System.UInt32"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setacsocketpowerstateoffHelp;

            this.conditionalOptionalArgs.Add('p', null);
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            Contracts.ChassisResponse myResponse = new Contracts.ChassisResponse();
            dynamic myPortNo = null;
            uint portNo = 0;

            try
            {
                this.argVal.TryGetValue('p', out myPortNo);
                portNo = (uint)myPortNo;
                myResponse = WcsCli2CmConnectionManager.channel.SetACSocketPowerStateOff((uint)myPortNo);
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine(WcsCliConstants.commandSucceeded);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode.ToString());
            }
        }
    }

    //getpowerintstate(uint portNo)
    internal class getpowerintstate : command
    {
        internal getpowerintstate()
        {
            this.name = WcsCliConstants.getpowerintstate;
            this.argSpec.Add('p', Type.GetType("System.UInt32"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.getacsocketpowerstateHelp;

            this.conditionalOptionalArgs.Add('p', null);
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            ACSocketStateResponse myResponse = new ACSocketStateResponse();
            dynamic myPortNo = null;
            uint portNo = 0;
            try
            {
                this.argVal.TryGetValue('p', out myPortNo);
                portNo = (uint)myPortNo;
                myResponse = WcsCli2CmConnectionManager.channel.GetACSocketPowerState(portNo);
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }


            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                int index = (int)myPortNo;
                if (myResponse.powerState == PowerState.ON)
                {
                    Console.WriteLine("ON");
                }
                else if (myResponse.powerState == PowerState.OFF)
                {
                    Console.WriteLine("OFF");
                }
                else
                {
                    Console.WriteLine("--");
                }
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode.ToString());
            }
        }
    }

    /// <summary>
    /// Abstract class for VT100 serial session commands
    /// </summary>
    internal abstract class SerialSession : command
    {
        /// <summary>
        /// local class cache locker object
        /// </summary>
        protected readonly object _locker = new object();

        /// <summary>
        /// session string / serial session cookie.
        /// </summary>
        protected string _sessionString = string.Empty;

        /// <summary>
        /// Access session string.
        /// </summary>
        protected string SessionString
        {
            get { lock (_locker) { return this._sessionString; } }
            set { lock (_locker) { this._sessionString = value; } }
        }

        // notice that read thread has ended.
        protected ManualResetEvent ended = new ManualResetEvent(false);

        /// <summary>
        /// Terminates polling thread for VT100
        /// </summary>
        protected void TerminateSession()
        {
            SharedFunc.SetSerialSession(false);
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ErrorMessage(string message, bool flush, bool terminate = false)
        {
            // terminate the vt100
            if (terminate)
            {
                // blow the house down.
                TerminateSession();

                if (!this.isSerialClient)
                {
                if (flush)
                {
                    Console.Clear();
                }
                }

                terminationMessage = message;
            }
            else
            {
                // display the message on screen.
                Console.WriteLine(message);
            }
        }

        protected string terminationMessage = string.Empty;

        internal abstract void Send(byte[] payload);

        protected abstract void Receive();
    }

    internal class startBladeSerialSession : SerialSession, IDisposable
    {
        // Track whether Dispose has been called. 
        private bool _disposed = false;

        internal startBladeSerialSession()
        {
            this.name = WcsCliConstants.startBladeSerialSession;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('s', Type.GetType("System.Int32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.startBladeSerialSessionHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'i' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'i' });
        }

        private AnsiEscape<startBladeSerialSession> _vt100;

        /// <summary>
        /// WCS Blade Id
        /// </summary>
        private int bladeId = 0;

        /// <summary>
        /// VT100 encoding for Enter key. True: CR+LF for Enter. False: CR for Enter.
        /// </summary>
        private bool enterEncodeCRLF = true;

        /// <summary>
        /// Switches to VT100 mode.
        /// enterEncodeCRLF: VT100 encoding for Enter key. True: CR+LF for Enter. False: CR for Enter.
        /// </summary>
        private void SetupVt100(bool enterEncodeCRLF)
        {
            _vt100 = new AnsiEscape<startBladeSerialSession>(this);

            // start the read on another thread
            Thread receiver = new Thread(new ThreadStart(Receive));
            receiver.Start();

            // This method blocks.  The only way to exit is kill the process
            // or press Ctrl + C
            _vt100.ReadConsole(enterEncodeCRLF);

            // wait at most for 3 seconds for the read thread to end.
            ended.WaitOne(3000);

            // undo all VT100 console changes. fall through will reach this method.
            _vt100.RevertConsole();

            // dispose of the Vt100 class.
            _vt100 = null;

            // write the tare down message
            Console.WriteLine();
            Console.WriteLine(terminationMessage);
            Console.WriteLine();
        }

        /// <summary>
        /// Establish blade serial session over Serial
        /// </summary>
        private void EstablishBladeSerialSessionOverSerialClient()
        {
            // start the read on another thread.
            Thread receiver = new Thread(new ThreadStart(Receive));
            receiver.Start();

            // Read continually until activeRead signal escapes.
            while (SharedFunc.ActiveSerialSession)
            {
                // This method blocks. Get user input and send it to the blade
                Byte[] userEnteredData = CliSerialPort.ReadUserInputBytesFromSerial();
                // Check if the data has ctrl-c and exit 
                for (int i = 0; i < userEnteredData.Length; i++)
                {
                    // Snoop to see if the user entered Ctrl+C 
                    if ((int)userEnteredData[i] == 3)
                    {
                        // Signal the receive thread to stop
                        SharedFunc.SetSerialSession(false);

                        // Stop the serial session
                        StopBladeSerialSession stopCmd = new StopBladeSerialSession();
                        stopCmd.isSerialClient = true;
                        stopCmd.argVal.Add('i', bladeId);
                        stopCmd.commandImplementation();

                        // Display that serial session has ended
                        Console.WriteLine("Blade serial session ended..\n");
                        break;
                    }
                }
                Send(userEnteredData);
            }
        }

        internal override void Send(byte[] payload)
        {
            // check session string and active write permission
            if (SessionString != string.Empty && SharedFunc.ActiveSerialSession && payload!=null && payload.Length>0)
            {
                ChassisResponse response = WcsCli2CmConnectionManager.channel.SendBladeSerialData(bladeId, SessionString, payload);

                if ((int)response.completionCode != 0)
                {
                    // signals the session failed.
                    string msg = string.Format("Data Send Error failed with Completion Code: {0}.  See User Log for futher information"
                       , response.completionCode);

                    ErrorMessage(msg, true, true);
                }
            }
        }

        protected override void Receive()
        {
            // check session string
            if (SessionString != string.Empty)
            {
                // Read continually until activeRead singal escapes.
                while (SharedFunc.ActiveSerialSession)
                {
                    try
                    {
                        SerialDataResponse response = WcsCli2CmConnectionManager.channel.ReceiveBladeSerialData(bladeId, SessionString);

                        if (response.completionCode == 0 && response.data != null)
                        {
                            if (this.isSerialClient)
                            {
                                CliSerialPort.WriteBytestoSerial(response.data);   
                            }
                            else
                            {
                            _vt100.SplitAnsiEscape(response.data);
                        }
                        }
                        else if (((int)response.completionCode != 0) // return an error message and kill the serial session
                        && ((int)response.completionCode != (163))) // if completion code is anything other than a timeout (which are expected).
                        {
                            // signals the session failed.
                            string msg = string.Format("Receiving Data failed with Completion Code: {0}.  See User Log for futher information"
                                , response.completionCode);
                            ErrorMessage(msg, true, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        // signals the session failed.
                        ErrorMessage(string.Format("Receiving Data failed with Exception: {0}.  "
                            , ex.Message.ToString()), true, true);
                    }
                }
            }
            // signal the read thread has ended.
            ended.Set();
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            StartSerialResponse myResponse = new StartSerialResponse();
            BladeInfoResponse bladeInfo = new BladeInfoResponse();

            try
            {
                if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    dynamic sessionTimeoutInSecs = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    
                    // Determine blade type
                    bladeInfo = WcsCli2CmConnectionManager.channel.GetBladeInfo((int)mySledId);
                    if (bladeInfo.completionCode == Contracts.CompletionCode.Success)
                    {
                        if (bladeInfo.bladeType == WcsCliConstants.BladeTypeCompute)
                        {
                            // Compute blade needs CR+LF for Enter key
                            enterEncodeCRLF = true;
                        }
                        else if (bladeInfo.bladeType == WcsCliConstants.BladeTypeJBOD)
                        {
                            // JBOD only needs CR for Enter key
                            enterEncodeCRLF = false;
                        }

                        // Open serial session
                        if (this.argVal.TryGetValue('s', out sessionTimeoutInSecs))
                        {
                            myResponse = WcsCli2CmConnectionManager.channel.StartBladeSerialSession(
                            (int)mySledId, (int)sessionTimeoutInSecs);
                        }
                        else
                        {
                            myResponse = WcsCli2CmConnectionManager.channel.StartBladeSerialSession(
                            (int)mySledId, 0);
                        }

                    }
                    else
                    {
                        myResponse.completionCode = Contracts.CompletionCode.Failure;
                    }

                    // set blade Id
                    bladeId = (int)mySledId;
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                if (myResponse.serialSessionToken != null)
                {
                    // set the serial cache
                    SessionString = myResponse.serialSessionToken;

                    if (bladeId > 0)
                    {
                        // Console client
                        if (!this.isSerialClient)
                        {
                            // Automatically start into VT100 mode.
                            // This is a blocking method.
                            SetupVt100(enterEncodeCRLF);
                        }
                        else // Serial client
                        {
                            SharedFunc.SetSerialSession(true);
                            EstablishBladeSerialSessionOverSerialClient();
                        }
                        // When the setup ends, call a close session by default
                        // as the session should be destroyed.
                        WcsCli2CmConnectionManager.channel.StopBladeSerialSession(bladeId, SessionString);
                    }
                    else
                    {
                        ErrorMessage("failed to start serial session due to conversion of blade Id", false, false);
                    }
                    return;
                }
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
                return;
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
                return;
            }
            else
            {
                Console.WriteLine(WcsCliConstants.unknownError);
            }
        }

        // Use C# destructor syntax for finalization code. 
        // This destructor will run only if the Dispose method 
        // does not get called. 
        // It gives your base class the opportunity to finalize. 
        // Do not provide destructors in types derived from this class.
        ~startBladeSerialSession()
        {
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability.
            Dispose(false);
        }

        // Implement IDisposable. 
        // A derived class should not be able to override this method. 
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed. 
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    if (_vt100 != null)
                        _vt100 = null;
                }

                if (ended != null)
                    ended.Dispose();

                // Note disposing has been done.
                _disposed = true;
            }
        }


        public int sessionTimeoutInSecs { get; set; }
    }

    internal class StopPortSerialSession : command
    {
        internal StopPortSerialSession()
        {
            this.name = WcsCliConstants.stopPortSerialSession;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.stopPortSerialSessionHelp;

            this.conditionalOptionalArgs.Add('i', null);
        }

        /// <summary>
        /// command specific implementation 
        /// </summary>
        internal override void commandImplementation()
        {
            dynamic portId = null;

            try
            {
                if (this.argVal.TryGetValue('i', out portId))
                {
                    WcsCli2CmConnectionManager.channel.StopSerialPortConsole((int)portId, null, true);
                }
                else
                {
                    Console.WriteLine(WcsCliConstants.invalidCommandString);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }
        }
    }

    /// <summary>
    /// Class StopBladeSerialSession.
    /// </summary>
    internal class StopBladeSerialSession : command
    {
        internal StopBladeSerialSession()
        {
            this.name = WcsCliConstants.stopBladeSerialSession;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.stopBladeSerialSessionHelp;
        }

        /// <summary>
        /// command specific implementation
        /// </summary>
        internal override void commandImplementation()
        {
            dynamic bladeId = null;

            try
            {
                if (this.argVal.TryGetValue('i', out bladeId))
                {
                    WcsCli2CmConnectionManager.channel.StopBladeSerialSession((int)bladeId, null, true);
                }
                else
                {
                    WcsCli2CmConnectionManager.channel.StopBladeSerialSession(0, null, true);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }
        }
    }

    //public string startsersession(int sledId, string sledName)
    internal class startPortSerialSession : SerialSession, IDisposable
    {
        // Track whether Dispose has been called. 
        private bool _disposed = false;

        internal startPortSerialSession()
        {
            this.name = WcsCliConstants.startPortSerialSession;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('s', Type.GetType("System.Int32"));
            this.argSpec.Add('d', Type.GetType("System.Int32"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.startPortSerialSessionHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'i' });
        }

        private AnsiEscape<startPortSerialSession> _vt100;

        /// <summary>
        /// WCS swtich port Id
        /// </summary>
        private int portId = 0;

        /// <summary>
        /// VT100 encoding for Enter key. True: CR+LF for Enter. False: CR for Enter.
        /// </summary>
        private const bool enterEncodeCRLF = true;  // Only one encoding used for port serial session.

        /// <summary>
        /// Switches to VT100 mode.
        /// enterEncodeCRLF: VT100 encoding for Enter key. True: CR+LF for Enter. False: CR for Enter.
        /// </summary>
        private void SetupVt100(bool enterEncodeCRLF)
        {
            _vt100 = new AnsiEscape<startPortSerialSession>(this);

            // start the read on another threa.
            Thread receiver = new Thread(new ThreadStart(Receive));
            receiver.Start();

            // This method blocks.  The only way to exit is kill the process
            // or press Ctrl + C
            _vt100.ReadConsole(enterEncodeCRLF);

            // wait at most for 3 seconds for the read thread to end.
            ended.WaitOne(3000);

            // undo all VT100 console changes. fall through will reach this method.
            _vt100.RevertConsole();

            // dispose of the Vt100 class.
            _vt100 = null;

            // write the tare down message
            Console.WriteLine();
            Console.WriteLine(terminationMessage);
            Console.WriteLine();

        }

        /// <summary>
        /// Establish port serial session over Serial
        /// </summary>
        private void EstablishPortSerialSessionOverSerialClient()
        {
            // start the read on another thread.
            Thread receiver = new Thread(new ThreadStart(Receive));
            receiver.Start();

            // Read continually until activeRead signal escapes.
            while (SharedFunc.ActiveSerialSession)
            {
                // This method blocks. Get user input and send it to the blade
                Byte[] userEnteredData = CliSerialPort.ReadUserInputBytesFromSerial();
                // Check if the data has ctrl-c and exit 
                for (int i = 0; i < userEnteredData.Length; i++)
                {
                    // Snoop to see if the user entered Ctrl+C 
                    if ((int)userEnteredData[i] == 3)
                    {
                        // Signal the receive thread to stop
                        SharedFunc.SetSerialSession(false);

                        // Stop the serial session
                        StopPortSerialSession stopCmd = new StopPortSerialSession();
                        stopCmd.isSerialClient = true;
                        stopCmd.argVal.Add('i', portId);
                        stopCmd.commandImplementation();

                        // Display that serial session has ended
                        Console.WriteLine("Blade serial session ended..\n");
                        break;
                    }
                }
                Send(userEnteredData);
            }
        }

        internal override void Send(byte[] payload)
        {
            // check session string and active write permission
            if (SessionString != string.Empty && SharedFunc.ActiveSerialSession && payload != null && payload.Length > 0)
            {
                ChassisResponse response = WcsCli2CmConnectionManager.channel.SendSerialPortData(portId, SessionString, payload);

                if ((int)response.completionCode != 0)
                {
                    // signals the session failed.
                    string msg = string.Format("Data Send Error failed with Completion Code: {0}.  See User Log for futher information"
                        , response.completionCode);
                    ErrorMessage(msg, true, true);
                }

            }
            else
            {
                // signals the session failed.
                ErrorMessage(string.Format("Serial Session was ended")
                    , true, true);
            }

        }

        protected override void Receive()
        {
            // check session string
            if (SessionString != string.Empty)
            {
                // Read continually until activeRead singal escapes.
                while (SharedFunc.ActiveSerialSession)
                {
                    try
                    {
                        SerialDataResponse response = WcsCli2CmConnectionManager.channel.ReceiveSerialPortData(portId, SessionString);

                        if (response.completionCode == 0 && response.data != null)
                        {
                            if (this.isSerialClient)
                            {
                                CliSerialPort.WriteBytestoSerial(response.data);
                            }
                            else
                            {
                            _vt100.SplitAnsiEscape(response.data);
                        }
                        }
                        else if (((int)response.completionCode != 0) // return an error message and kill the serial session
                        && ((int)response.completionCode != (163))) // if completion code is anything other than a timeout (which are expected).
                        {
                            // signals the session failed.
                            ErrorMessage(string.Format("Receiving Data failed with Completion Code: {0}.  See User Log for futher information"
                                , response.completionCode), true, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        // signals the session failed.
                        ErrorMessage(string.Format("Receiving Data failed with Exception: {0}.  "
                            , ex.Message.ToString()), true, true);
                    }
                }
            }
            else
            {
                // signals the session failed.
                ErrorMessage("Serial Session was not correctly activated"
                    , true, true);
            }

            // signal the read thread has ended.
            ended.Set();
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            StartSerialResponse myResponse = new StartSerialResponse();
            try
            {
                if (this.argVal.ContainsKey('i'))
                {
                    dynamic myportId = null;
                    dynamic sessionTimeoutInSecs = null;
                    dynamic deviceTimeoutInMSecs = null;
                    this.argVal.TryGetValue('i', out myportId);
                    this.argVal.TryGetValue('s', out sessionTimeoutInSecs);
                    this.argVal.TryGetValue('d', out deviceTimeoutInMSecs);

                    if (this.argVal.TryGetValue('s', out sessionTimeoutInSecs) &&
                        this.argVal.TryGetValue('d', out deviceTimeoutInMSecs))
                    {
                        myResponse = WcsCli2CmConnectionManager.channel.StartSerialPortConsole((int)myportId,
                            (int)sessionTimeoutInSecs, (int)deviceTimeoutInMSecs);
                    }
                    else
                    {
                        myResponse = WcsCli2CmConnectionManager.channel.StartSerialPortConsole(
                            (int)myportId, 0, 0);
                    }

                    // set port Id
                    portId = (int)myportId;
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                if (myResponse.serialSessionToken != null)
                {
                    Console.WriteLine(myResponse.serialSessionToken);

                    // set the serial cache
                    SessionString = myResponse.serialSessionToken;

                    if (portId > 0)
                    {
                        // Console client
                        if (!this.isSerialClient)
                        {
                            // Automatically start into VT100 mode.
                            // This is a blocking method.
                            SetupVt100(enterEncodeCRLF);
                        }
                        else // Serial client
                        {
                            SharedFunc.SetSerialSession(true);
                            EstablishPortSerialSessionOverSerialClient();
                        }

                        // When the setup ends, call a close session by default
                        // as the session should be destroyed.
                        WcsCli2CmConnectionManager.channel.StopSerialPortConsole(portId, SessionString, false);
                    }
                    else
                    {
                        ErrorMessage("failed to start serial session due to conversion of port Id", false, false);
                    }

                    return;
                }
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
                return;
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
                return;
            }
            else
            {
                Console.WriteLine(WcsCliConstants.unknownError);
            }
        }

        // Use C# destructor syntax for finalization code. 
        // This destructor will run only if the Dispose method 
        // does not get called. 
        // It gives your base class the opportunity to finalize. 
        // Do not provide destructors in types derived from this class.
        ~startPortSerialSession()
        {
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability.
            Dispose(false);
        }

        // Implement IDisposable. 
        // A derived class should not be able to override this method. 
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed. 
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    if (_vt100 != null)
                        _vt100 = null;
                }

                if (ended != null)
                    ended.Dispose();

                // Note disposing has been done.
                _disposed = true;
            }
        }

    }

    internal class readnclog : command
    {
        internal readnclog()
        {
            this.name = WcsCliConstants.readnclog;
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.readchassislogHelp;

            this.conditionalOptionalArgs = null;
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            ChassisLogResponse myResponse = new ChassisLogResponse();
            try
            {
                myResponse = WcsCli2CmConnectionManager.channel.ReadChassisLog();
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                List<string> myStrings = new List<string>();
                Console.WriteLine(WcsCliConstants.readnclogHeader);
                myStrings.Add("Timestamp"); myStrings.Add("Entry");
                printTabSeperatedStrings(myStrings);
                foreach (LogEntry lg in myResponse.logEntries)
                {
                    myStrings.RemoveAll(item => (1 == 1));
                    myStrings.Add(lg.eventTime.ToString());
                    myStrings.Add(lg.eventDescription.ToString());
                    printTabSeperatedStrings(myStrings);
                }
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode.ToString());
            }
        }
    }

    //public uint clrnclog()
    internal class clrnclog : command
    {
        internal clrnclog()
        {
            this.name = WcsCliConstants.clrnclog;
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.clearchassislogHelp;

            this.conditionalOptionalArgs = null;
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            Contracts.ChassisResponse myResponse = new Contracts.ChassisResponse();
            try
            {
                myResponse = WcsCli2CmConnectionManager.channel.ClearChassisLog();
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine(WcsCliConstants.commandSucceeded);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode.ToString());
            }
        }
    }

    //public Microsoft.WCS.WcsCli.logPacket readsclog(int sledId, string sledName, uint logType, uint noEntries)
    internal class readsclog : command
    {
        internal readsclog()
        {
            this.name = WcsCliConstants.readsclog;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('n', Type.GetType("System.UInt32"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.readbladelogHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'i' });
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            ChassisLogResponse myResponse = new ChassisLogResponse();
            dynamic myNumberEntries = null;
            if (!this.argVal.TryGetValue('n', out myNumberEntries))
            {
                myNumberEntries = WcsCliConstants.readsclogNumberEntries;
            }

            try
            {
                if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    myResponse = WcsCli2CmConnectionManager.channel.ReadBladeLog((int)mySledId);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                List<string> myStrings = new List<string>();
                Console.WriteLine(WcsCliConstants.readsclogHeader);
                myStrings.Add("Timestamp"); myStrings.Add("Entry");
                printTabSeperatedStrings(myStrings);
                uint index = 0;
                foreach (LogEntry lg in myResponse.logEntries)
                {
                    if (index > myNumberEntries)
                    {
                        break;
                    }
                    myStrings.RemoveAll(item => (1 == 1));
                    myStrings.Add(lg.eventTime.ToString());
                    myStrings.Add(lg.eventDescription);
                    printTabSeperatedStrings(myStrings);
                    index++;
                }
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode.ToString());
            }
        }
    }

    //public uint clrsclog(int sledId, string sledName, bool doForAllSleds)
    internal class clrsclog : command
    {
        internal clrsclog()
        {
            this.name = WcsCliConstants.clrsclog;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.clearbladelogHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'i' });
        }

        /// <summary>
        /// command specific implementation 
        /// argVal command class member has all user-entered command argument indicators and parameter values
        /// Currently just prints all argument indicators and argument values
        /// </summary>
        internal override void commandImplementation()
        {
            BladeResponse myResponse = new BladeResponse();

            try
            {
                dynamic mySledId = null;
                this.argVal.TryGetValue('i', out mySledId);
                myResponse = WcsCli2CmConnectionManager.channel.ClearBladeLog((int)mySledId);
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine(WcsCliConstants.commandSucceeded);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
            {
                Console.WriteLine("Blade: " + WcsCliConstants.bladeStateUnknown);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode);
            }
        }
    }

    internal class adduser : command
    {
        internal adduser()
        {
            this.name = WcsCliConstants.adduser;
            this.argSpec.Add('u', Type.GetType("System.String"));
            this.argSpec.Add('p', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('o', null);
            this.argSpec.Add('r', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.adduserHelp;

            this.conditionalOptionalArgs.Add('u', null);
            this.conditionalOptionalArgs.Add('p', null);
            this.conditionalOptionalArgs.Add('a', new char[] { 'r', 'o' });
            this.conditionalOptionalArgs.Add('o', new char[] { 'a', 'r' });
            this.conditionalOptionalArgs.Add('r', new char[] { 'a', 'o' });
        }

        /// <summary>
        /// command specific implementation 
        /// </summary>
        internal override void commandImplementation()
        {
            Contracts.ChassisResponse myResponse = new Contracts.ChassisResponse();
            dynamic uname = null;
            dynamic pword = null;

            try
            {
                if (this.argVal.TryGetValue('u', out uname) && this.argVal.TryGetValue('p', out pword) && this.argVal.ContainsKey('a'))
                {
                    Console.WriteLine(WCSSecurityRole.WcsCmAdmin.ToString());
                    myResponse = WcsCli2CmConnectionManager.channel.AddChassisControllerUser((string)uname, (string)pword, WCSSecurityRole.WcsCmAdmin);

                }
                else if (this.argVal.TryGetValue('u', out uname) && this.argVal.TryGetValue('p', out pword) && this.argVal.ContainsKey('o'))
                {
                    myResponse = WcsCli2CmConnectionManager.channel.AddChassisControllerUser((string)uname, (string)pword, WCSSecurityRole.WcsCmOperator);
                }
                else if (this.argVal.TryGetValue('u', out uname) && this.argVal.TryGetValue('p', out pword) && this.argVal.ContainsKey('u'))
                {
                    myResponse = WcsCli2CmConnectionManager.channel.AddChassisControllerUser((string)uname, (string)pword, WCSSecurityRole.WcsCmUser);
                }
                else
                {
                    Console.WriteLine(WcsCliConstants.invalidCommandString);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine(WcsCliConstants.commandSucceeded);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.UserAccountExists)
            {
                Console.WriteLine(WcsCliConstants.commandUserAccountExists);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.UserPasswordDoesNotMeetRequirement)
            {
                Console.WriteLine(WcsCliConstants.commandUserPwdDoesNotMeetReq);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode);
            }
        }
    }

    internal class ChangeUserRole : command
    {
        internal ChangeUserRole()
        {
            this.name = WcsCliConstants.changeuserrole;
            this.argSpec.Add('u', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('o', null);
            this.argSpec.Add('r', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.changeuserroleHelp;

            this.conditionalOptionalArgs.Add('u', null);
            this.conditionalOptionalArgs.Add('a', new char[] { 'r', 'o' });
            this.conditionalOptionalArgs.Add('o', new char[] { 'a', 'r' });
            this.conditionalOptionalArgs.Add('r', new char[] { 'a', 'o' });
        }

        /// <summary>
        /// command specific implementation 
        /// </summary>
        internal override void commandImplementation()
        {
            Contracts.ChassisResponse myResponse = new Contracts.ChassisResponse();
            dynamic uname = null;

            try
            {
                if (this.argVal.TryGetValue('u', out uname) && this.argVal.ContainsKey('a'))
                {
                    myResponse = WcsCli2CmConnectionManager.channel.ChangeChassisControllerUserRole((string)uname, WCSSecurityRole.WcsCmAdmin);
                }
                else if (this.argVal.TryGetValue('u', out uname) && this.argVal.ContainsKey('o'))
                {
                    myResponse = WcsCli2CmConnectionManager.channel.ChangeChassisControllerUserRole((string)uname, WCSSecurityRole.WcsCmOperator);
                }
                else if (this.argVal.TryGetValue('u', out uname) && this.argVal.ContainsKey('r'))
                {
                    myResponse = WcsCli2CmConnectionManager.channel.ChangeChassisControllerUserRole((string)uname, WCSSecurityRole.WcsCmUser);
                }
                else
                {
                    Console.WriteLine(WcsCliConstants.invalidCommandString);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine(WcsCliConstants.commandSucceeded);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.UserNotFound)
            {
                Console.WriteLine(WcsCliConstants.commandUserNotFound);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode.ToString());
            }
        }
    }

    internal class ChangeUserPassword : command
    {
        internal ChangeUserPassword()
        {
            this.name = WcsCliConstants.changeuserpassword;
            this.argSpec.Add('u', Type.GetType("System.String"));
            this.argSpec.Add('p', Type.GetType("System.String"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.changeUserPwdHelp;

            this.conditionalOptionalArgs.Add('u', null);
            this.conditionalOptionalArgs.Add('p', null);
        }

        /// <summary>
        /// command specific implementation 
        /// </summary>
        internal override void commandImplementation()
        {
            Contracts.ChassisResponse myResponse = new Contracts.ChassisResponse();
            dynamic uname = null;
            dynamic newpword = null;

            try
            {
                if (this.argVal.TryGetValue('u', out uname) && this.argVal.TryGetValue('p', out newpword))
                {
                    myResponse = WcsCli2CmConnectionManager.channel.ChangeChassisControllerUserPassword((string)uname, (string)newpword);
                }
                else
                {
                    Console.WriteLine(WcsCliConstants.invalidCommandString);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine(WcsCliConstants.commandSucceeded);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.UserNotFound)
            {
                Console.WriteLine(WcsCliConstants.commandUserNotFound);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.UserPasswordDoesNotMeetRequirement)
            {
                Console.WriteLine(WcsCliConstants.commandUserPwdDoesNotMeetReq);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode.ToString());
            }
        }
    }

    internal class removeuser : command
    {
        internal removeuser()
        {
            this.name = WcsCliConstants.removeuser;
            this.argSpec.Add('u', Type.GetType("System.String"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.removeuserHelp;

            this.conditionalOptionalArgs.Add('u', null);
        }

        /// <summary>
        /// command specific implementation 
        /// </summary>
        internal override void commandImplementation()
        {
            Contracts.ChassisResponse myResponse = new Contracts.ChassisResponse();
            dynamic uname = null;

            try
            {
                if (this.argVal.TryGetValue('u', out uname))
                {
                    myResponse = WcsCli2CmConnectionManager.channel.RemoveChassisControllerUser((string)uname);
                }
                else
                {
                    Console.WriteLine(WcsCliConstants.invalidCommandString);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine(WcsCliConstants.commandSucceeded);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.UserNotFound)
            {
                Console.WriteLine(WcsCliConstants.commandUserNotFound);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.UserPasswordDoesNotMeetRequirement)
            {
                Console.WriteLine(WcsCliConstants.commandUserPwdDoesNotMeetReq);
            }
            else
            {
                Console.WriteLine("Command failed with completion code: {0}", myResponse.completionCode.ToString());
            }
        }
    }

    static internal class NetworkInterfaces
    {
        static uint numberInterfaces = 2;

        public class NetworkPropertiesResponse
        {
            public List<NetworkProperty> NetworkPropertyCollection = new List<NetworkProperty>();
            public int completionCode; // 1 means success, 0 means failure 
        }

        public class NetworkProperty
        {
            public string macAddress = String.Empty;
            public string ipAddress = String.Empty;
            public string subnetMask = String.Empty;
            public string gatewayAddress = String.Empty;
            public string dnsAddress = String.Empty;
            public string dhcpServer = String.Empty;
            public string dnsDomain = String.Empty;
            public string dnsHostName = String.Empty;
            public bool dhcpEnabled;
            public string primary = String.Empty;
            public string secondary = String.Empty;
            public int completionCode; // 1 means success, 0 means failure
        }

        /// <summary>
        /// Get Network parameters
        /// </summary>
        static internal NetworkPropertiesResponse GetNetworkProperties()
        {
            string[] ipAddresses = null;
            string[] subnets = null;
            string[] gateways = null;
            string dnsHostName = null;
            string dhcpServer = null;
            string dnsDomain = null;
            string macAddress = null;
            bool dhcpEnabled = true;
            string primary = null;
            string secondary = null;
            string[] DNSServerAddress = null;

            NetworkPropertiesResponse response = new NetworkPropertiesResponse();
            response.NetworkPropertyCollection = new List<NetworkProperty>();

            // Create management class object using the Win32_NetworkAdapterConfiguration
            // class to retrieve different attributes of the network adapters
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");

            // Create ManagementObjectCollection to retrieve the attributes
            ManagementObjectCollection moc = mc.GetInstances();

            // Set default completion code to unknown.
            response.completionCode = 0;

            try
            {
                foreach (ManagementObject mo in moc)
                {
                    // If interface has IP enabled
                    if ((bool)mo["ipEnabled"])
                    {
                        ipAddresses = (string[])mo["IPAddress"];
                        subnets = (string[])mo["IPSubnet"];
                        gateways = (string[])mo["DefaultIPGateway"];
                        dnsHostName = (string)mo["DNSHostName"];
                        dhcpServer = (string)mo["DHCPServer"];
                        dnsDomain = (string)mo["DNSDomain"];
                        macAddress = (string)mo["MACAddress"];
                        dhcpEnabled = (bool)mo["DHCPEnabled"];
                        primary = (string)mo["WINSPrimaryServer"];
                        secondary = (string)mo["WINSSecondaryServer"];
                        DNSServerAddress = (string[])mo["DNSServerSearchOrder"];

                        int index = 0;
                        foreach (string ip in ipAddresses)
                        {
                            if (NetworkInterfaces.checkIpFormat(ip))
                            {
                                NetworkProperty cr = new NetworkProperty();
                                cr.ipAddress = ipAddresses.ToArray()[index];
                                if (subnets != null)
                                {
                                    cr.subnetMask = subnets.ToArray()[index];
                                }
                                if (gateways != null)
                                {
                                    cr.gatewayAddress = gateways.ToArray()[index];
                                }
                                if (DNSServerAddress != null)
                                {
                                    cr.dnsAddress = DNSServerAddress.ToArray()[index];
                                }
                                cr.dhcpServer = dhcpServer;
                                cr.dnsDomain = dnsDomain;
                                cr.dnsHostName = dnsHostName;
                                cr.macAddress = macAddress;
                                cr.dhcpEnabled = dhcpEnabled;
                                cr.primary = primary;
                                cr.secondary = secondary;
                                cr.completionCode = 1;
                                response.NetworkPropertyCollection.Add(cr);
                            }
                            index++;
                        }
                    }
                    else // all other interfaces (with ip not enables)
                    {
                        macAddress = (string)mo["MACAddress"];
                        // Populating interfaces only with valid mac addresses - ignoring loopback and other virtual interfaces
                        if (macAddress != null)
                        {
                            NetworkProperty cr = new NetworkProperty();
                            cr.macAddress = macAddress;
                            cr.completionCode = 1;
                            response.NetworkPropertyCollection.Add(cr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.completionCode = 0;
                Console.WriteLine("Get NIC properties failed with message " + ex.Message);
                return response;
            }

            response.completionCode = 1;
            return response;
        }

        static bool checkIpFormat(string IpAddresses)
        {
            System.Net.IPAddress ipAdd;
            if (System.Net.IPAddress.TryParse(IpAddresses, out ipAdd))
            {
                if (ipAdd.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set Network parameters
        /// </summary>
        static internal NetworkPropertiesResponse SetNetworkProperties(
            string IpAddresses, string hostName, string IPSubnet, string SubnetMask, string Gateway, 
            string DNSPrimary, string DNSSecondary, string ipSource, int interfaceNumber = 1)
        {
            Console.WriteLine(ipSource);

            if (interfaceNumber > numberInterfaces)
            {
                Console.WriteLine(
                    "Invalid Interface Number. Network Interface properties cannot be changed.");
            }

            NetworkPropertiesResponse response = new NetworkPropertiesResponse();
            
            // Set default completion code to unknown.
            response.completionCode = 0;
            
            try
            {
                IpAddresses = IpAddresses.Trim();
                SubnetMask = SubnetMask.Trim();
                if (!string.IsNullOrEmpty(Gateway))
                {
                    Gateway = Gateway.Trim();
                }

                Console.WriteLine(IpAddresses);
                Console.WriteLine(SubnetMask);

                if (!(checkIpFormat(IpAddresses) && checkIpFormat(SubnetMask) && 
                    checkIpFormat(DNSPrimary) && checkIpFormat(DNSSecondary)))
                {
                    Console.WriteLine(@"Invalid Input Network Parameters.
                             Network interface properties cannot be changed.");
                }

                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                int index = 1;

                foreach (ManagementObject mo in moc)
                {
                    Console.WriteLine("index: " + index);

                    if ((bool)mo["IPEnabled"])
                    {
                        if (index == interfaceNumber)
                        {
                            try
                            {
                                Console.WriteLine("index" + index);
                                Console.WriteLine(ipSource);

                                ManagementBaseObject newGateWay =
                                    mo.GetMethodParameters("SetGateways");
                                ManagementBaseObject newDNS =
                                    mo.GetMethodParameters("SetDNSServerSearchOrder");
                                
                                if (string.Equals(ipSource, "DHCP",
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    // Set DNS
                                    newDNS["DNSServerSearchOrder"] = null;

                                    ManagementBaseObject enableDHCP =
                                        mo.InvokeMethod("EnableDHCP", null, null);
                                }
                                else if (string.Equals(ipSource, "STATIC",
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Enabling STATIC IP ...");
                                    try
                                    {
                                        mo.InvokeMethod("EnableStatic",
                                            new object[] {new string[] { IpAddresses },
                                                new string[] { SubnetMask } });

                                        // Set DNS
                                        string[] DNSServerSearchOrder =
                                            new string[] { DNSPrimary, DNSSecondary };
                                        newDNS["DNSSeverSearchOrder"] = DNSServerSearchOrder;
                                        newDNS["DNSHostName"] = hostName;
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Exception in EnableStatic: "
                                            + ex.Message);
                                    }
                                }

                                if (!string.IsNullOrEmpty(Gateway))
                                {
                                    // Set Gateway
                                    newGateWay["DefaultIPGateway"] = new string[] { Gateway };
                                    newGateWay["GatewayCostMetric"] = new int[] { 1 };
                                    ManagementBaseObject setGateways =
                                        mo.InvokeMethod("SetGateways", newGateWay, null);
                                }

                                ManagementBaseObject setDNS =
                                    mo.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error: " + e.Message);
                            }
                        }
                        index++;
                    }
                }
            }
            catch (Exception ex)
            {
                response.completionCode = 0;
                Console.WriteLine("exception encountered: " + ex.Message);
            }

            response.completionCode = 1;
            return response;
        }
    }



    internal class getnic : command
    {
        internal getnic()
        {
            this.name = WcsCliConstants.getnic;
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.getnicHelp;
        }

        /// <summary>
        /// command specific implementation 
        /// </summary>
        internal override void commandImplementation()
        {
            if (!this.isSerialClient)
            {
                Console.WriteLine("Command only supported in serial wcscli client mode.. Use native windows APIs for getting NIC info..");
                return;
            }

           NetworkInterfaces.NetworkPropertiesResponse myResponse = new NetworkInterfaces.NetworkPropertiesResponse();
            try
            {
                myResponse = NetworkInterfaces.GetNetworkProperties();       
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            uint index = 1;
            foreach (NetworkInterfaces.NetworkProperty res in myResponse.NetworkPropertyCollection)
            {
                if (res.completionCode == 1)
                {
                    Console.WriteLine("N/w Interface {0}:", index);
                    Console.WriteLine("\tIP Address\t\t: " + res.ipAddress);
                    Console.WriteLine("\tHostname\t\t: " + res.dnsHostName);
                    Console.WriteLine("\tMAC Address\t\t: " + res.macAddress);
                    Console.WriteLine("\tSubnet Mask\t\t: " + res.subnetMask);
                    Console.WriteLine("\tDHCP Enabled\t\t: " + res.dhcpEnabled.ToString());
                    Console.WriteLine("\tDHCP Server\t\t: " + res.dhcpServer);
                    Console.WriteLine("\tDNS Domain\t\t: " + res.dnsDomain);
                    Console.WriteLine("\tGateway Address\t\t: " + res.gatewayAddress);
                    Console.WriteLine("\tPrimary\t\t\t: " + res.primary);
                    Console.WriteLine("\tSecondary\t\t: " + res.secondary);
                    Console.WriteLine("\tDNS Server\t\t: " + res.dnsAddress);
                    Console.WriteLine("\tCompletion Code\t\t: " + res.completionCode);
                    Console.WriteLine();
                    index++;
                }
                else if (res.completionCode == 0)
                {
                    Console.WriteLine(WcsCliConstants.commandFailure);
                }
                else
                {
                    Console.WriteLine("Command failed with completion code: {0}",
                        res.completionCode.ToString());
                }
            }
        }
    }

    internal class setnic : command
    {
        internal setnic()
        {
            this.name = WcsCliConstants.setnic;
            this.argSpec.Add('i', Type.GetType("System.String"));
            this.argSpec.Add('n', Type.GetType("System.String"));
            this.argSpec.Add('s', Type.GetType("System.String"));
            this.argSpec.Add('m', Type.GetType("System.String"));
            this.argSpec.Add('p', Type.GetType("System.String"));
            this.argSpec.Add('d', Type.GetType("System.String"));
            this.argSpec.Add('g', Type.GetType("System.String"));
            this.argSpec.Add('a', Type.GetType("System.String"));
            this.argSpec.Add('t', Type.GetType("System.UInt32"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setnicHelp;

            this.conditionalOptionalArgs.Add('a', null);
        }

        /// <summary>
        /// command specific implementation 
        /// </summary>
        internal override void commandImplementation()
        {
            if (!this.isSerialClient)
            {
                Console.WriteLine("Command only supported in serial wcscli client mode");
                return;
            }

            NetworkInterfaces.NetworkPropertiesResponse myResponse = new NetworkInterfaces.NetworkPropertiesResponse();
            dynamic newIp = null;
            dynamic hostName = null;
            dynamic newSubnet = null;
            dynamic netMask = null;
            dynamic newGateway = null;
            dynamic primaryDNS = null;
            dynamic secondaryDNS = null;
            dynamic interfaceId = null;
            dynamic ipSource = null;
            try
            {
                this.argVal.TryGetValue('a', out ipSource);
                if (!this.argVal.TryGetValue('t', out interfaceId))
                {
                    interfaceId = 1; // set the default interface id as 1
                }

                if (string.Equals((string)ipSource, "STATIC",
                    StringComparison.OrdinalIgnoreCase))
                {
                    if ( this.argVal.TryGetValue('i', out newIp) && 
                         this.argVal.TryGetValue('p', out primaryDNS) && 
                         this.argVal.TryGetValue('d', out secondaryDNS) &&
                         this.argVal.TryGetValue('m', out netMask) ) 
                    {
                         this.argVal.TryGetValue('s', out newSubnet); 
                         this.argVal.TryGetValue('g', out newGateway);
                         this.argVal.TryGetValue('n', out hostName);
                    }
                    else
                {
                        Console.WriteLine(WcsCliConstants.invalidCommandString);
                        Console.WriteLine("All required parameters not supplied");
                        return;
                    }
                }
                // Set network properties
                myResponse = NetworkInterfaces.SetNetworkProperties(
                    (string)newIp, (string)hostName, (string)newSubnet, 
                    (string)netMask, (string)newGateway, (string)primaryDNS,
                    (string) secondaryDNS, (string)ipSource, (int)interfaceId);
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == 1)
            {
                Console.WriteLine("Command succeeded with completion code: "
                    + myResponse.completionCode);
                }
            else if (myResponse.completionCode == 0)
            {
                Console.WriteLine("Command failed with completion code: "
                    + myResponse.completionCode);
            }
        }
    }

    internal class GetServiceVersion : command
    {
        internal GetServiceVersion()
        {
            this.name = WcsCliConstants.getserviceversion;
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.getServiceVersionHelp;

            this.conditionalOptionalArgs = null;
        }

        /// <summary>
        /// command specific implementation 
        /// </summary>
        internal override void commandImplementation()
        {
            ServiceVersionResponse myResponse = new ServiceVersionResponse();
            try
            {
                myResponse = WcsCli2CmConnectionManager.channel.GetServiceVersion();
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine("Chassis Manager Service version: " + myResponse.serviceVersion);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code: {0}", myResponse.completionCode.ToString());
            }
        }


    }

    internal class GetNextBoot : command
    {
        internal GetNextBoot()
        {
            this.name = WcsCliConstants.getnextboot;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.getnextbootHelp;

            this.conditionalOptionalArgs.Add('i', null);
        }

        /// <summary>
        /// command specific implementation 
        /// </summary>
        internal override void commandImplementation()
        {
            Contracts.BootResponse myResponse = new Contracts.BootResponse();
            dynamic bladeId = null;

            try
            {
                if (this.argVal.TryGetValue('i', out bladeId))
                {
                    myResponse = WcsCli2CmConnectionManager.channel.GetNextBoot((int)bladeId);
                }
                else
                {
                    Console.WriteLine(WcsCliConstants.invalidCommandString);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine("Next boot is " + myResponse.nextBoot);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code : {0}", myResponse.completionCode);
            }
        }
    }
        
    internal class SetNextBoot : command
    {
        internal SetNextBoot()
        {
            this.name = WcsCliConstants.setnextboot;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('t', Type.GetType("System.UInt32"));
            this.argSpec.Add('m', Type.GetType("System.UInt32"));
            this.argSpec.Add('p', Type.GetType("System.UInt32"));
            this.argSpec.Add('n', Type.GetType("System.UInt32"));
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setnextbootHelp;

            this.conditionalOptionalArgs.Add('i', null);
            this.conditionalOptionalArgs.Add('t', null);
            this.conditionalOptionalArgs.Add('m', null);
            this.conditionalOptionalArgs.Add('p', null);
            this.conditionalOptionalArgs.Add('n', null);
        }

        internal Contracts.BladeBootType getBootType(int varType)
        {
            //1. NoOverRide = 0x00,
            if (varType == 1)
                return Contracts.BladeBootType.NoOverride;
            //2. ForcePxe = 0x04,
            else if (varType == 2)
                return Contracts.BladeBootType.ForcePxe;
            //3. ForceDefaultHdd = 0x08,
            else if (varType == 3)
                return Contracts.BladeBootType.ForceDefaultHdd;   
            //4. ForceIntoBiosSetup = 0x0c,
            else if (varType == 4)
                return Contracts.BladeBootType.ForceIntoBiosSetup;
            //5. ForceFloppyOrRemovable = 0x10,
            else if (varType == 5)
                return Contracts.BladeBootType.ForceFloppyOrRemovable;
            //X. Unknown = 0xff
            else
                return Contracts.BladeBootType.Unknown;
        }

        internal bool getIsPersistent(int varPersist)
        {
            if (varPersist <= 0)
                return false;
            else
                return true;
        }

        internal bool getIsUefi(int uefi)
        {
            if (uefi <= 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// command specific implementation 
        /// </summary>
        internal override void commandImplementation()
        {
            Contracts.BootResponse myResponse = new Contracts.BootResponse();
            dynamic bootType = null;
            dynamic bladeId = null;
            dynamic uefi = null;
            dynamic isPersistent = null;
            dynamic bootInstance = null;

            try
            {
                if (this.argVal.TryGetValue('i', out bladeId) && this.argVal.TryGetValue('t', out bootType) && this.argVal.TryGetValue('m', out uefi) && this.argVal.TryGetValue('p', out isPersistent) && this.argVal.TryGetValue('n', out bootInstance))
                {
                    myResponse = WcsCli2CmConnectionManager.channel.SetNextBoot((int)bladeId, getBootType((int)bootType), getIsUefi((int)uefi), (bool)getIsPersistent((int)isPersistent), (int)bootInstance);
                }
                else
                {
                    Console.WriteLine(WcsCliConstants.invalidCommandString);
                }
            }
            catch (Exception ex)
            {
                SharedFunc.ExceptionOutput(ex);
                return;
            }

            if (myResponse == null)
            {
                Console.WriteLine(WcsCliConstants.serviceResponseEmpty);
                return;
            }

            if (myResponse.completionCode == Contracts.CompletionCode.Success)
            {
                Console.WriteLine(WcsCliConstants.commandSucceeded + ". Next boot is " + myResponse.nextBoot);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Failure)
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
            else if (myResponse.completionCode == Contracts.CompletionCode.Timeout)
            {
                Console.WriteLine(WcsCliConstants.commandTimeout);
            }
            else
            {
                Console.WriteLine("Command failed with the completion code : {0}", myResponse.completionCode);
            }
        }
    }
 
    internal class StartChassisManagerService : command
    {
        internal StartChassisManagerService()
        {
            this.name = WcsCliConstants.startchassismanager;
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.startchassismanagerHelp;

            this.conditionalOptionalArgs = null;
        }

        internal override void commandImplementation()
        {
            if (!this.isSerialClient)
            {
                Console.WriteLine("Command only supported in serial wcscli client mode..");
                return;
            }

            bool status = WcsCli2CmConnectionManager.StartChassisManager();
            if (status)
            {
                Console.WriteLine("chassismanager service successfully started");
            }
            else
            {
                    Console.WriteLine(WcsCliConstants.commandFailure);
            }
        }
    }

    internal class StopChassisManagerService : command
    {
        internal StopChassisManagerService()
        {
            this.name = WcsCliConstants.stopchassismanager;
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.stopchassismanagerHelp;

            this.conditionalOptionalArgs = null;
        }

        internal override void commandImplementation()
        {
            if (!this.isSerialClient)
            {
                Console.WriteLine("Command only supported in serial wcscli client mode..");
                return;
            }

            bool status = WcsCli2CmConnectionManager.StopChassisManager();
            if (status == true)
            {
                Console.WriteLine("chassismanager service successfully stopped.");
            }
            else
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }        
        }
    }

    internal class GetCMServiceStatus : command
    {
        internal GetCMServiceStatus()
        {
            this.name = WcsCliConstants.getchassismanagerstatus;
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.getchassismanagerstatusHelp;

            this.conditionalOptionalArgs = null;
        }

        internal override void commandImplementation()
        {
            if (!this.isSerialClient)
            {
                Console.WriteLine("Command only supported in serial wcscli client mode..");
                return;
            }

            try
            {
                ServiceController controller = new ServiceController("chassismanager");
                Console.WriteLine("chassismanager service status: " + controller.Status);
                Console.WriteLine(WcsCliConstants.commandSucceeded);

            }
            catch (Exception)
            {
                Console.WriteLine("chassismanager service status query failed");
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
        }
    }

    internal class EnableSSL : command
    {
        internal EnableSSL()
        {
            this.name = WcsCliConstants.enablessl;
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.enablesslHelp;

            this.conditionalOptionalArgs = null;
        }

        internal override void commandImplementation()
        {
            if (!this.isSerialClient)
            {
                Console.WriteLine("Command only supported in serial wcscli client mode..");
                return;
            }

            bool status = WcsCli2CmConnectionManager.SetSSL(true);
            if (status)
            {
                Console.WriteLine("Successfully enabled SSL in the chassismanager service.");
                Console.WriteLine("");
                Console.WriteLine("You will need to establish connection to the CM again via ({0}) command to run any commands..", WcsCliConstants.establishCmConnection);
                Console.WriteLine("");
                Console.WriteLine(WcsCliConstants.establishCmConnectionHelp);
            }
            else
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
        }
    }

    internal class DisableSSL : command
    {
        internal DisableSSL()
        {
            this.name = WcsCliConstants.disablessl;
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.disablesslHelp;

            this.conditionalOptionalArgs = null;
        }

        internal override void commandImplementation()
        {
            if (!this.isSerialClient)
            {
                Console.WriteLine("Command only supported in serial wcscli client mode..");
                return;
            }

            bool status = WcsCli2CmConnectionManager.SetSSL(false);
            if (status)
            {
                Console.WriteLine("Successfully disabled SSL in the chassismanager service.");
                Console.WriteLine("");
                Console.WriteLine("You will need to establish connection to the CM again via ({0}) command to run any commands..", WcsCliConstants.establishCmConnection);
                Console.WriteLine("");
                Console.WriteLine(WcsCliConstants.establishCmConnectionHelp);
            }
            else
            {
                Console.WriteLine(WcsCliConstants.commandFailure);
            }
        }
    }

}
