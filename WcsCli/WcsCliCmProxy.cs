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

namespace Microsoft.GFS.WCS.WcsCli
{
    internal static class WcsCliCmProxy
    {
        /// <summary>
        /// This dictionary object maps the command name to the command object
        /// Note that command objects for every command is created during initialization - CommandInitializer()
        /// </summary>
        static Dictionary<String, command> commandMap = new Dictionary<String, command>(StringComparer.OrdinalIgnoreCase);
        static List<command> commandsWithSledIdAndSledName = new List<command>();

        /// <summary>
        /// Static constructor for initializing the command class objects
        /// </summary>
        static WcsCliCmProxy()
        {
            CommandInitializer();
        }

        /* Exposed methods */

        /// <summary>
        /// Get WCSCLI user commands, translate them to corresponding CM REST API calls 
        /// (via the command class), prints CM Response output
        /// Figure out if this is a local machine command or a CM service command
        /// Prompt for CM service credentials to talk to the CM if required
        /// Uses ConnectionManager class to establish WCF connection
        /// </summary>
        /// <param name="inputString"></param>
        internal static void InteractiveParseUserCommandGetCmResponse(bool isSerialClient, string inputString)
        {
            char[] delimiters = { ' ' };
            string[] inputSubString = null;
            command mappedCommand = null;

            try
            {
                // get all individual user-entered arguments as separate string
                // StringSplitOptions.RemoveEmptyEntries removes extra spaces 
                inputSubString = inputString.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                // Prevents parsing more than maxArgCount arguments - DOS attack scenario
                if (inputSubString.Length > command.maxArgCount)
                {
                    Console.Write(WcsCliConstants.consoleString + " " + WcsCliConstants.invalidCommandString);
                    return;
                }

                // Command string should have at least two argument strings 
                if (inputSubString.Length <= 1)
                {
                    if (inputSubString.Length == 0)
                    {
                        return;
                    }
                    Console.WriteLine(WcsCliConstants.invalidCommandString);
                    return;
                }

                // The command string should start with "WcsCli"
                if (!inputSubString[0].Equals(WcsCliConstants.WcsCli, StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine(WcsCliConstants.invalidCommandString);
                    return;
                }

                // The first argument which is the command name length must be smaller than indicated by command.maxArgLength
                // The first argument which is the command name should be preceded with a '-'
                if (inputSubString[1].Length > command.maxArgLength || inputSubString[1][0] != WcsCliConstants.argIndicatorVar)
                {
                    Console.WriteLine(WcsCliConstants.invalidCommandString);
                    return;
                }

                // Map the user-entered command name to the corresponding command object created by CommandInitializer() function
                mappedCommand = new command();
                if (commandMap.TryGetValue(inputSubString[1].Remove(0, 1), out mappedCommand) != true)
                {
                    // Handle unknown command
                    Console.WriteLine(WcsCliConstants.invalidCommandString);
                    return;
                }
                mappedCommand.isSerialClient = isSerialClient;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            try
            {
                // (re)Allocate argVal for the mappedCommand - it has to get a new space for each time command is executed
                mappedCommand.argVal = new Dictionary<char, dynamic>();

                // Execute the commandImplementation() function corresponding to the command entered by the user
                // Command is executed only when the argument syntax is correct as checked by isArgSyntaxCorrect()
                if (IsArgSyntaxCorrect(mappedCommand, inputSubString))
                {
                    if (mappedCommand.argVal.ContainsKey('h'))
                    {
                        // If the command is establishCmConnection and it is a console client.. do not display help.. since the h option is used for hostname in the console.. 
                        if (!(mappedCommand.name.Equals(WcsCliConstants.establishCmConnection, StringComparison.InvariantCultureIgnoreCase) 
                            && isSerialClient == false))
                        {
                            Console.WriteLine(mappedCommand.helpString);    
                        }
                    }
                    else
                    {
                        // If this is a CM command, check if a connection has already been established, otherwise prompt for connection
                        if (mappedCommand.isCmServiceCommand)
                        {
                            if (!WcsCli2CmConnectionManager.IsCmServiceConnectionActive)
                            {
                                Console.WriteLine("Please connect to CM service using the \"{0}\" command and try again.", WcsCliConstants.establishCmConnection);
                                Console.WriteLine(WcsCliConstants.establishCmConnectionHelp);
                                return;
                            }
                        }
                        // Resolve sled id and sled name only if the command has those parameters
                        if (commandsWithSledIdAndSledName.Contains(mappedCommand))
                        {
                            ResolveSledName(mappedCommand);
                        }
                        if (AreAllArgsPresent(mappedCommand))
                        {
                            // Execute the command
                            mappedCommand.commandImplementation();

                            // Let us allow batch only for the internal gethostportssloption command to avoid unintentional/intentional recursion
                            if (mappedCommand.name.Equals(WcsCliConstants.establishCmConnection, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // If the command has a batch parameter - process the commands in the batch file
                                if (mappedCommand.argVal.ContainsKey('b'))
                                {
                                    if (isSerialClient)
                                    {
                                        Console.WriteLine("The batch -b option is not supported in serial mode..");
                                        return;
                                    }
                                    dynamic batchFile = null;
                                    if (mappedCommand.argVal.TryGetValue('b', out batchFile))
                                    {
                                        uint batchInputFileLinesIndex = 0;
                                        string[] batchInputFileLines = System.IO.File.ReadAllLines((string)batchFile);
                                        while (batchInputFileLinesIndex < batchInputFileLines.Length)
                                        {
                                            try
                                            {
                                                // Read one command at a time
                                                inputString = batchInputFileLines[batchInputFileLinesIndex];
                                                batchInputFileLinesIndex++;
                                                if (inputString == null)
                                                    continue;

                                                // Recursive call for executing command in the batch file
                                                InteractiveParseUserCommandGetCmResponse(isSerialClient, inputString);
                                            }
                                            catch (Exception)
                                            {
                                                // skip this entry in the batch file
                                                Console.WriteLine("Error in parsing batch file. Skipped entries");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error in getting batch file name.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            // If the command is establishCmConnection and it is a console client.. do not display help.. since the h option is used for hostname in the console.. 
                            if (!(mappedCommand.name.Equals(WcsCliConstants.establishCmConnection, StringComparison.InvariantCultureIgnoreCase) && 
                                isSerialClient == false))
                            {
                                Console.WriteLine(WcsCliConstants.argsMissingString);
                                Console.WriteLine(mappedCommand.helpString);
                            }
                        }
                    }
                }
                else
                {
                    // If the command is establishCmConnection and it is a console client.. do not display help.. since the h option is used for hostname in the console.. 
                    if (!(mappedCommand.name.Equals(WcsCliConstants.establishCmConnection, StringComparison.InvariantCultureIgnoreCase) && 
                        isSerialClient == false))
                    {
                        Console.WriteLine(WcsCliConstants.invalidCommandString);
                        Console.WriteLine(mappedCommand.helpString);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(WcsCliConstants.unknownError + ex);
            }
        }

        /* Private memeber functions */
        /// <summary>
        /// Creates object for each individual command
        /// Corresponding command constructor initializes command specific parameters
        /// Also adds each command name and command object to command dictionary
        /// </summary>
        private static void CommandInitializer()
        {
            getinfo getinfoCmd = new getinfo();
            commandMap.Add(WcsCliConstants.getinfo, getinfoCmd);

            getscinfo getscinfoCmd = new getscinfo();
            commandMap.Add(WcsCliConstants.getscinfo, getscinfoCmd);
            commandsWithSledIdAndSledName.Add(getscinfoCmd);

            help helpCmd = new help();
            helpCmd.isCmServiceCommand = false;
            commandMap.Add(WcsCliConstants.help, helpCmd);

            ncidon ncidonCmd = new ncidon();
            commandMap.Add(WcsCliConstants.ncidon, ncidonCmd);

            ncidoff ncidoffCmd = new ncidoff();
            commandMap.Add(WcsCliConstants.ncidoff, ncidoffCmd);

            ncidstatus ncidstatusCmd = new ncidstatus();
            commandMap.Add(WcsCliConstants.ncidstatus, ncidstatusCmd);

            scidon scidonCmd = new scidon();
            commandMap.Add(WcsCliConstants.scidon, scidonCmd);
            commandsWithSledIdAndSledName.Add(scidonCmd);

            scidoff scidoffCmd = new scidoff();
            commandMap.Add(WcsCliConstants.scidoff, scidoffCmd);
            commandsWithSledIdAndSledName.Add(scidoffCmd);

            setscponstate setscponstateCmd = new setscponstate();
            commandMap.Add(WcsCliConstants.setscponstate, setscponstateCmd);
            commandsWithSledIdAndSledName.Add(setscponstateCmd);

            getscponstate getscponstateCmd = new getscponstate();
            commandMap.Add(WcsCliConstants.getscponstate, getscponstateCmd);
            commandsWithSledIdAndSledName.Add(getscponstateCmd);

            poweron poweronCmd = new poweron();
            commandMap.Add(WcsCliConstants.poweron, poweronCmd);
            commandsWithSledIdAndSledName.Add(poweronCmd);

            poweroff poweroffCmd = new poweroff();
            commandMap.Add(WcsCliConstants.poweroff, poweroffCmd);
            commandsWithSledIdAndSledName.Add(poweroffCmd);

            bladeon bladeOnCmd = new bladeon();
            commandMap.Add(WcsCliConstants.bladeon, bladeOnCmd);
            commandsWithSledIdAndSledName.Add(bladeOnCmd);

            bladeoff bladeOffCmd = new bladeoff();
            commandMap.Add(WcsCliConstants.bladeoff, bladeOffCmd);
            commandsWithSledIdAndSledName.Add(bladeOffCmd);

            getbladestate bladeStateCmd = new getbladestate();
            commandMap.Add(WcsCliConstants.getbladestate, bladeStateCmd);
            commandsWithSledIdAndSledName.Add(bladeStateCmd);

            powercycle powercycleCmd = new powercycle();
            commandMap.Add(WcsCliConstants.powercycle, powercycleCmd);
            commandsWithSledIdAndSledName.Add(powercycleCmd);

            getscpowerstate getscpowerstateCmd = new getscpowerstate();
            commandMap.Add(WcsCliConstants.getscpowerstate, getscpowerstateCmd);
            commandsWithSledIdAndSledName.Add(getscpowerstateCmd);

            powerinton powerintonCmd = new powerinton();
            commandMap.Add(WcsCliConstants.powerinton, powerintonCmd);

            powerintoff powerintoffCmd = new powerintoff();
            commandMap.Add(WcsCliConstants.powerintoff, powerintoffCmd);

            getpowerintstate getpowerintstateCmd = new getpowerintstate();
            commandMap.Add(WcsCliConstants.getpowerintstate, getpowerintstateCmd);

            startBladeSerialSession startBladeSerialSessionCmd = new startBladeSerialSession();
            commandMap.Add(WcsCliConstants.startBladeSerialSession, startBladeSerialSessionCmd);
            commandsWithSledIdAndSledName.Add(startBladeSerialSessionCmd);

            startPortSerialSession startPortSerialSessionCmd = new startPortSerialSession();
            commandMap.Add(WcsCliConstants.startPortSerialSession, startPortSerialSessionCmd);
            commandsWithSledIdAndSledName.Add(startBladeSerialSessionCmd);

            StopPortSerialSession stopsersessionCmd = new StopPortSerialSession();
            commandMap.Add(WcsCliConstants.stopPortSerialSession, stopsersessionCmd);
            commandsWithSledIdAndSledName.Add(stopsersessionCmd);

            StopBladeSerialSession stopBladeSerialSessionCmd = new StopBladeSerialSession();
            commandMap.Add(WcsCliConstants.stopBladeSerialSession, stopBladeSerialSessionCmd);
            commandsWithSledIdAndSledName.Add(stopBladeSerialSessionCmd);

            readnclog readnclogCmd = new readnclog();
            commandMap.Add(WcsCliConstants.readnclog, readnclogCmd);

            clrnclog clrnclogCmd = new clrnclog();
            commandMap.Add(WcsCliConstants.clrnclog, clrnclogCmd);

            readsclog readsclogCmd = new readsclog();
            commandMap.Add(WcsCliConstants.readsclog, readsclogCmd);
            commandsWithSledIdAndSledName.Add(readsclogCmd);

            clrsclog clrsclogCmd = new clrsclog();
            commandMap.Add(WcsCliConstants.clrsclog, clrsclogCmd);
            commandsWithSledIdAndSledName.Add(clrsclogCmd);

            adduser adduserCmd = new adduser();
            commandMap.Add(WcsCliConstants.adduser, adduserCmd);

            ChangeUserRole changeuserCmd = new ChangeUserRole();
            commandMap.Add(WcsCliConstants.changeuserrole, changeuserCmd);

            ChangeUserPassword changeUserPwd = new ChangeUserPassword();
            commandMap.Add(WcsCliConstants.changeuserpassword, changeUserPwd);

            removeuser removeuserCmd = new removeuser();
            commandMap.Add(WcsCliConstants.removeuser, removeuserCmd);

            getnic getnicCmd = new getnic();
            getnicCmd.isCmServiceCommand = false;
            commandMap.Add(WcsCliConstants.getnic, getnicCmd);

            setnic setnicCmd = new setnic();
            setnicCmd.isCmServiceCommand = false;
            commandMap.Add(WcsCliConstants.setnic, setnicCmd);

            getpowerreading getpowerreadingCmd = new getpowerreading();
            commandMap.Add(WcsCliConstants.getpowerreading, getpowerreadingCmd);

            getpowerlimit getpowerlimitCmd = new getpowerlimit();
            commandMap.Add(WcsCliConstants.getpowerlimit, getpowerlimitCmd);

            setpowerlimit setpowerlimitCmd = new setpowerlimit();
            commandMap.Add(WcsCliConstants.setpowerlimit, setpowerlimitCmd);

            activatepowerlimit activatepowerlimitCmd = new activatepowerlimit();
            commandMap.Add(WcsCliConstants.activatepowerlimit, activatepowerlimitCmd);

            deactivatepowerlimit deactivatepowerlimitCmd = new deactivatepowerlimit();
            commandMap.Add(WcsCliConstants.deactivatepowerlimit, deactivatepowerlimitCmd);

            GetChassisHealth getChassisHealthCmd = new GetChassisHealth();
            commandMap.Add(WcsCliConstants.getChassisHealth, getChassisHealthCmd);

            GetBladeHealth getBladeHealthCmd = new GetBladeHealth();
            commandMap.Add(WcsCliConstants.getBladeHealth, getBladeHealthCmd);

            SetNextBoot setNextBootCmd = new SetNextBoot();
            commandMap.Add(WcsCliConstants.setnextboot, setNextBootCmd);

            GetNextBoot getNextBootCmd = new GetNextBoot();
            commandMap.Add(WcsCliConstants.getnextboot, getNextBootCmd);

            GetServiceVersion getServiceVersionCmd = new GetServiceVersion();
            commandMap.Add(WcsCliConstants.getserviceversion, getServiceVersionCmd);

            EstablishConnectionToCm establishCmConnectionCmd = new EstablishConnectionToCm();
            establishCmConnectionCmd.isCmServiceCommand = false;
            commandMap.Add(WcsCliConstants.establishCmConnection, establishCmConnectionCmd);

            TerminateCmConnection terminateCmConnectionCmd = new TerminateCmConnection();
            terminateCmConnectionCmd.isCmServiceCommand = false;
            commandMap.Add(WcsCliConstants.terminateCmConnection, terminateCmConnectionCmd);

            StartChassisManagerService startCMServiceCmd = new StartChassisManagerService();
            startCMServiceCmd.isCmServiceCommand = false;
            commandMap.Add(WcsCliConstants.startchassismanager, startCMServiceCmd);

            StopChassisManagerService stopCMServiceCmd = new StopChassisManagerService();
            stopCMServiceCmd.isCmServiceCommand = false;
            commandMap.Add(WcsCliConstants.stopchassismanager, stopCMServiceCmd);

            GetCMServiceStatus getCMServiceStatusCmd = new GetCMServiceStatus();
            getCMServiceStatusCmd.isCmServiceCommand = false;
            commandMap.Add(WcsCliConstants.getchassismanagerstatus, getCMServiceStatusCmd);

            EnableSSL enableSslCmd = new EnableSSL();
            enableSslCmd.isCmServiceCommand = false;
            commandMap.Add(WcsCliConstants.enablessl, enableSslCmd);

            DisableSSL disableSslCmd = new DisableSSL();
            disableSslCmd.isCmServiceCommand = false;
            commandMap.Add(WcsCliConstants.disablessl, disableSslCmd);

            return;
        }

        /// <summary>
        /// Parsing the arguments for correct syntax 
        /// Also checks for argument parameter syntax type
        /// </summary>
        /// <param name="mappedCommand"> class object corresponding to user entered command </param>
        /// <param name="inputSubString"> represents all user-entered arguments and their values </param>
        /// <returns></returns>
        private static bool IsArgSyntaxCorrect(command mappedCommand, string[] inputSubString)
        {
            uint index = 2;
            bool isSyntaxCorrect = true;

            // Iterate over all arguments and check for correctness
            while (index < inputSubString.Length && inputSubString[index] != null)
            {
                isSyntaxCorrect = true;
                // All argument indicators start with a '-'
                if (inputSubString[index][0] != WcsCliConstants.argIndicatorVar)
                {
                    Console.Write("Hyphen missing. ");
                    isSyntaxCorrect = false;
                    break;
                }
                // Argument indicator must be '-' followed by a single character
                if (inputSubString[index].Length != 2)
                {
                    Console.Write("Argument Indicator missing. ");
                    isSyntaxCorrect = false;
                    break;
                }

                try
                {
                    // Extract the argument indicator character
                    char charIndicator = inputSubString[index][1];

                    // If 'i' or 'c' is already present in argVal, then do not process the current argument
                    if (charIndicator == 'i' || charIndicator == 'c')
                    {
                        if (mappedCommand.argVal.ContainsKey('c') || mappedCommand.argVal.ContainsKey('i'))
                        {
                            index++; // skip the current arg indicator
                            index++; // since 'i' and 'c' both take one arg value, skip the next item as well
                            continue;
                        }
                    }

                    Type argType;
                    // Find the type, 'argType' of the argument indicated by charIndicator
                    // The if block will be execute for correct argument Indicators 
                    if (mappedCommand.argSpec != null && mappedCommand.argSpec.TryGetValue(charIndicator, out argType))
                    {
                        // All argument has either zero or one parameter 
                        // if argType is null for this indicator, then this argument has zero/no parameter 
                        if (argType == null)
                        {
                            if (!mappedCommand.argVal.ContainsKey(charIndicator))
                            {
                                mappedCommand.argVal.Add(charIndicator, null);
                            }
                            index++;
                            continue;
                        }
                        else // this else block is executed for arguments that carry a single parameter
                        {
                            index++;
                            if (index < inputSubString.Length && inputSubString[index] != null)
                            {
                                if (inputSubString[index].Length > command.maxArgLength)
                                {
                                    isSyntaxCorrect = false;
                                    break;
                                }

                                if (!mappedCommand.argVal.ContainsKey(charIndicator))
                                {
                                    // Convert the argument parameter to the corresponding type indicated by argType and add it to argVal
                                    mappedCommand.argVal.Add(charIndicator, Convert.ChangeType(inputSubString[index], argType));
                                }

                                // notice index is incremented twice in this block - one for the argument indicator and another for the argument parameter
                                index++;
                                continue;
                            }
                            else
                            {
                                Console.Write("Required parameters missing. ");
                                isSyntaxCorrect = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        Console.Write("Invalid Argument Indicator. ");
                        isSyntaxCorrect = false;
                        break;
                    }
                } //try block ends
                // Exception handling in case of convert failure - say string is entered in place of a interger argument
                // Exception must also be called if argVal.Add is done with duplicate entries
                catch (InvalidCastException ex)
                {
                    isSyntaxCorrect = false;
                    Console.Write("Invalid argument type. " + ex.Message);
                    break;
                }
                catch (ArgumentException ex)
                {
                    isSyntaxCorrect = false;
                    Console.Write("Invalid argument. " + ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    isSyntaxCorrect = false;
                    Console.Write("Invalid argument. " + ex.Message);
                    break;
                }
            } // While loop ends
            return isSyntaxCorrect;
        }

        /// <summary>
        /// When we reach this code, sled id is already not provided
        /// Convert sled name to sled id and send only id to chassis manager
        /// </summary>
        /// <param name="myCommand"></param>
        private static void ResolveSledName(command myCommand)
        {
            uint flag = 0;

            if (myCommand == null)
                return;

            if (myCommand.argVal == null)
                return;

            int tempId = -1;

            foreach (KeyValuePair<char, dynamic> pair in myCommand.argVal)
            {
                if (pair.Key == 'c')
                {
                    if (((string)pair.Value).Length >= 5)
                    {
                        try
                        {
                            string tempString = null;
                            tempString = ((string)pair.Value).Substring(0, 4);

                            if (tempString == WcsCliConstants.sledNamePrefix)
                            {
                                if (((string)pair.Value).Length == 5)
                                {
                                    tempId = Convert.ToInt32(((string)pair.Value).Substring(4, 1));
                                    flag = 1;
                                    break;
                                }
                                else if (((string)pair.Value).Length == 6)
                                {
                                    tempId = Convert.ToInt32(((string)pair.Value).Substring(4, 2));
                                    flag = 1;
                                    break;
                                }
                                else
                                {
                                    // invalid id number - more than 2 digits 
                                    myCommand.argVal['c'] = "";
                                    return;
                                }
                            }
                            else
                            {
                                // invalid logical sled id
                                myCommand.argVal['c'] = "";
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            myCommand.argVal['c'] = "";
                            return;
                        }
                    }
                    else
                    {
                        // invalid sled name - less than 5 digits 
                        myCommand.argVal['c'] = "";
                        return;
                    }
                }
            }
            // Convert sled name to sled id
            if (flag == 1)
            {
                myCommand.argVal['c'] = "";
                myCommand.argVal.Add('i', tempId);
            }
            return;
        }

        /// <summary>
        /// checks if all mandatory and conditionally optional arguments present
        /// </summary>
        /// <param name="myCommand"> class object corresponding to user entered command </param>
        /// <returns>true if user entered parameters adhere to the command specification</returns>
        private static bool AreAllArgsPresent(command myCommand)
        {
            bool found = true;

            // this should not happen
            if (myCommand == null)
                return false;

            // Nothing to check here
            if (myCommand.argVal == null || myCommand.conditionalOptionalArgs == null)
                return true;

            foreach (KeyValuePair<char, char[]> pair in myCommand.conditionalOptionalArgs)
            {
                found = false;
                if (myCommand.argVal.ContainsKey(pair.Key))
                {
                    found = true;
                }
                else if (pair.Value != null) // If there are other alternative args for this arg
                {
                    foreach (char optArg in pair.Value)
                    {
                        if (myCommand.argVal.ContainsKey(optArg))
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (found == false)
                    break;
            }
            return found;
        } // function areAllArgsPresent ends
    }
}
