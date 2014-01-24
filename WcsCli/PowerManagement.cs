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
using System.Linq;
using Microsoft.GFS.WCS.Contracts;

namespace Microsoft.GFS.WCS.WcsCli
{
    internal class getpowerreading : command
    {
        internal getpowerreading()
        {
            this.name = WcsCliConstants.getpowerreading;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.getbladepowerreadingHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });
        }

        internal override void commandImplementation()
        {
            uint sledId = 1;
            BladePowerReadingResponse myResponse = new BladePowerReadingResponse();
            GetAllBladesPowerReadingResponse myResponses = new GetAllBladesPowerReadingResponse();
            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.GetAllBladesPowerReading();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.GetBladePowerReading((int)mySledId);
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
                for (int index = 0; index < myResponses.bladePowerReadingCollection.Count(); index++)
                {
                    if (myResponses.bladePowerReadingCollection[index].completionCode == Contracts.CompletionCode.Success)
                    {
                        Console.WriteLine("Blade Power Reading" + myResponses.bladePowerReadingCollection[index].bladeNumber + ": " + myResponses.bladePowerReadingCollection[index].powerReading + " Watts");
                    }
                    else if (myResponses.bladePowerReadingCollection[index].completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade " + myResponses.bladePowerReadingCollection[index].bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                    }
                    else
                    {
                        // Display error if other than success/unknown
                        Console.WriteLine("Blade Power Reading for blade " + myResponses.bladePowerReadingCollection[index].bladeNumber + " failed with completion code: " + myResponses.bladePowerReadingCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    Console.WriteLine("Blade Power Reading" + myResponse.bladeNumber + ": " + myResponse.powerReading + " Watts");
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    // Display error if other than success/unknown
                    Console.WriteLine("Blade Power Reading " + myResponse.bladeNumber + ": " + myResponse.completionCode.ToString());
                }
            }
        }
    }

    internal class getpowerlimit : command
    {
        internal getpowerlimit()
        {
            this.name = WcsCliConstants.getpowerlimit;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.getbladebpowerlimitHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });
        }

        internal override void commandImplementation()
        {
            uint sledId = 1;
            BladePowerLimitResponse myResponse = new BladePowerLimitResponse();
            GetAllBladesPowerLimitResponse myResponses = new GetAllBladesPowerLimitResponse();
            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.GetAllBladesPowerLimit();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.GetBladePowerLimit((int)mySledId);
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
                for (int index = 0; index < myResponses.bladePowerLimitCollection.Count(); index++)
                {
                    if (myResponses.bladePowerLimitCollection[index].completionCode == Contracts.CompletionCode.Success)
                    {
                        Console.WriteLine("Blade Power Limit" + myResponses.bladePowerLimitCollection[index].bladeNumber + ": " + myResponses.bladePowerLimitCollection[index].powerLimit + " Watts");
                    }
                    else if (myResponses.bladePowerLimitCollection[index].completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade " + myResponses.bladePowerLimitCollection[index].bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                    }
                    else
                    {
                        // Display error if other than success/unknown
                        Console.WriteLine("Blade Power Limit for blade " + myResponses.bladePowerLimitCollection[index].bladeNumber + " failed with completion code: " + myResponses.bladePowerLimitCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    Console.WriteLine("Blade Power Limit" + myResponse.bladeNumber + ": " + myResponse.powerLimit + " Watts");
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    // Display error if other than success/unknown
                    Console.WriteLine("Blade Power Limit " + myResponse.bladeNumber + ": " + myResponse.completionCode.ToString());
                }
            }
        }

    }

    internal class setpowerlimit : command
    {
        internal setpowerlimit()
        {
            this.name = WcsCliConstants.setpowerlimit;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('l', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setbladepowerlimitHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });
            this.conditionalOptionalArgs.Add('l', null);
        }

        internal override void commandImplementation()
        {
            BladeResponse myResponse = new BladeResponse();
            AllBladesResponse myResponses = new AllBladesResponse();
            dynamic myLimit = null;
            dynamic mySledId = null;
            this.argVal.TryGetValue('l', out myLimit);

            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.SetAllBladesPowerLimit((double)myLimit);
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    this.argVal.TryGetValue('i', out mySledId);
                    myResponse = WcsCli2CmConnectionManager.channel.SetBladePowerLimit((int)mySledId, (double)myLimit);
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
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": Power Limit Set");
                    }
                    else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                    }
                    else
                    {
                        // Display error if other than success/unknown
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + " failed with completion code: " + myResponses.bladeResponseCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": Power Limit Set");
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    // Display error if other than success/unknown
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + myResponse.completionCode.ToString());
                }
            }
        }
    }

    internal class activatepowerlimit : command
    {
        internal activatepowerlimit()
        {
            this.name = WcsCliConstants.activatepowerlimit;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setbladepowerlimitOnHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });
        }

        internal override void commandImplementation()
        {
            uint sledId = 1;
            BladeResponse myResponse = new BladeResponse();
            AllBladesResponse myResponses = new AllBladesResponse();
            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.SetAllBladesPowerLimitOn();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.SetBladePowerLimitOn((int)mySledId);
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
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": Power Limit Active");
                    }
                    else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                    }
                    else
                    {
                        // Display error if other than success/unknown
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + " failed with completion code: " + myResponses.bladeResponseCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": Power Limit Active");
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    // Display error if other than success/unknown
                    Console.WriteLine("Blade " + myResponse.bladeNumber + " failed with completion code: " + myResponse.completionCode.ToString());
                }
            }
        }
    }

    internal class deactivatepowerlimit : command
    {
        internal deactivatepowerlimit()
        {
            this.name = WcsCliConstants.deactivatepowerlimit;
            this.argSpec.Add('i', Type.GetType("System.UInt32"));
            this.argSpec.Add('c', Type.GetType("System.String"));
            this.argSpec.Add('a', null);
            this.argSpec.Add('h', null);
            this.helpString = WcsCliConstants.setbladepowerlimitoffHelp;

            this.conditionalOptionalArgs.Add('i', new char[] { 'c', 'a' });
            this.conditionalOptionalArgs.Add('c', new char[] { 'a', 'i' });
            this.conditionalOptionalArgs.Add('a', new char[] { 'i', 'c' });
        }

        internal override void commandImplementation()
        {
            uint sledId = 1;
            BladeResponse myResponse = new BladeResponse();
            AllBladesResponse myResponses = new AllBladesResponse();
            try
            {
                if (this.argVal.ContainsKey('a'))
                {
                    myResponses = WcsCli2CmConnectionManager.channel.SetAllBladesPowerLimitOff();
                }
                else if (this.argVal.ContainsKey('i'))
                {
                    dynamic mySledId = null;
                    this.argVal.TryGetValue('i', out mySledId);
                    sledId = (uint)mySledId;
                    myResponse = WcsCli2CmConnectionManager.channel.SetBladePowerLimitOff((int)mySledId);
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
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": Power Limit Deactive");
                    }
                    else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                    {
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                    }
                    else
                    {
                        // Display error if other than success/unknown
                        Console.WriteLine("Blade " + myResponses.bladeResponseCollection[index].bladeNumber + " failed with completion code: " + myResponses.bladeResponseCollection[index].completionCode.ToString());
                    }
                }
            }
            else
            {
                if (myResponse.completionCode == Contracts.CompletionCode.Success)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": Power Limit Deactive");
                }
                else if (myResponse.completionCode == Contracts.CompletionCode.Unknown)
                {
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + WcsCliConstants.bladeStateUnknown);
                }
                else
                {
                    // Display error if other than success/unknown
                    Console.WriteLine("Blade " + myResponse.bladeNumber + ": " + myResponse.completionCode.ToString());
                }
            }
        }

    }

   
    }
