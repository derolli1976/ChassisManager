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
using System.ServiceProcess;
using System.IO;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Threading;

namespace Microsoft.GFS.WCS.WcsCli
{
    internal class Program
    {
        // Log file for tracing - TODO extend tracer class
        public static StreamWriter logWriter;

        public const string ServiceName = "WcscliSerialService";
        
        public class WcscliSerialService : ServiceBase
        {
            private String _defaultCliServiceComPort = "COM2";
            private int _defaultCliServiceBaudRate = 115200;
            
            private Thread _serviceThread = new Thread(ContinuousSerialUserInputCapture);
            internal static bool _continue = true;

            public WcscliSerialService()
            {
                try
                {
                    _defaultCliServiceComPort = ConfigurationManager.AppSettings.Get("COMPortName");
                }
                catch
                {
                    // Cannot determine the COM port to start the service.. Exiting
                    Environment.Exit(-1);
                }

                try
                {
                    ServiceName = ConfigurationManager.AppSettings.Get("ServiceName") + _defaultCliServiceComPort;
                }
                catch
                {
                    // if service name is not present in the app.config, use the default service name
                    ServiceName = "wcscli" + _defaultCliServiceComPort;
                }

                try
                {
                    logWriter = new StreamWriter(@"C:\" + ServiceName + "_TraceLog.txt");
                    logWriter.AutoFlush = true;
                }
                catch
                {
                    // Cannot open log file
                }

                try
                {
                    _defaultCliServiceBaudRate = Convert.ToInt32(ConfigurationManager.AppSettings.Get("COMPortBaudRate"));
                }
                catch(Exception ex)
                {
                    if (logWriter != null)
                    {
                        logWriter.WriteLine("Failure reading config File to determine the baud rate.. Using the default 115200" + ex.Message);
                    }
                    _defaultCliServiceBaudRate = 115200;
                }
            }

            protected override void OnStart(string[] args)
            {
                // Wcscli Serial Service specific code goes here - starts
                try
                {
                    if (!CliSerialPort.CliSerialOpen(_defaultCliServiceComPort, _defaultCliServiceBaudRate))
                    {
                        // Print failure and exit the service
                        if (logWriter != null)
                        {
                            logWriter.WriteLine("Fault when serial port CliSerialOpen is called..");
                        }
                        Environment.Exit(-1);
                    }
                    logWriter.WriteLine("Succesfully started service with name ({0}) at Port ({1})", ServiceName, _defaultCliServiceComPort);
                }
                catch (IOException e)
                {
                    if (logWriter != null)
                    {
                        logWriter.WriteLine("Fault (IOException) at service start " + e.Message);
                    }
                }
                catch (Exception ex)
                {
                    if (logWriter != null)
                    {
                        logWriter.WriteLine("Fault at service start " + ex.ToString());
                    }
                }
                // Wcscli Serial Service specific code goes here - ends

                // onstart code here
                _serviceThread.Start();
                if (logWriter != null)
                {
                    logWriter.WriteLine("Service thread started.. ");
                }
            }

            /// <summary>
            /// Parses the input entered by the user over serial
            /// Calls WcsCliCmProxy class for processing the input
            /// </summary>
            private static void ContinuousSerialUserInputCapture()
            {
                String inputString = null;

                while (_continue)
                {
                    // Blocking call that will wait until the user enters a command delimited by carriage return or new line
                    inputString = CliSerialPort.ReadUserInputStringFromSerial(new char[] { '\n', '\r' });

                    if (inputString != null)
                    {
                        if (inputString.Equals("exit", StringComparison.InvariantCultureIgnoreCase) || inputString.Equals("quit", StringComparison.InvariantCultureIgnoreCase))
                        {
                            inputString = "wcscli ";
                            inputString += "-" + WcsCliConstants.terminateCmConnection;
                        }

                        // Execute the command
                        WcsCliCmProxy.InteractiveParseUserCommandGetCmResponse(true, inputString);
                    }
                    Console.Write(WcsCliConstants.consoleString + " " + "");

                    // Write response data from console out to serial
                    CliSerialPort.WriteConsoleOutToSerial();
                } // While loop ends
            }

            protected override void OnStop()
            {
                CliSerialPort.CliSerialClose();
                
                _continue = false;
                if (_serviceThread != null)
                    _serviceThread.Join(2000);
                if (_serviceThread != null)
                    _serviceThread.Abort();
                
                if (logWriter != null)
                {
                    logWriter.WriteLine("The END!");
                    logWriter.Close();
                }
            }
        }

        // If '-b' or '-v' is specified as WCSCLI command line paramters.. then do not enter continuous interactive mode.. 
        // Instead quit the application after executing the respective command..
        internal static bool isBatchOrVersionCmd = false;

        /// <summary>
        /// Main program where the console command-line user interface starts
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
            {
                // running as service
                using (var service = new WcscliSerialService())
                    ServiceBase.Run(service);
            }
            else 
            {
                // Command-line specific code goes here 
                Console.Title = "Chassis Manager Command-line Interface.";
                WcsCliCmProxy.InteractiveParseUserCommandGetCmResponse(false, ConvertConsoleCommandLineArgsToCommandInput(args));
                if (isBatchOrVersionCmd)
                {
                    // If batch file is specified as input then do not get in to interactive mode.. quit.. 
                    return;
                }
                if (!WcsCli2CmConnectionManager.TestConnectionToCmService())
                {
                    Console.WriteLine("Please try again by executing \"{0}\" \n", WcsCliConstants.wcscliConsoleParameterHelp);
                    return;
                }
                ContinuousConsoleUserInputCapture();
            }
        }

        /// <summary>
        /// Parses the input entered by the user over console
        /// Calls WcsCliCmProxy class for processing the input
        /// </summary>
        private static void ContinuousConsoleUserInputCapture()
        {
            String inputString = null;
            bool _tobreak = false;

            while (true)
            {
                Console.Write(WcsCliConstants.consoleString + " " + "");
                inputString = Console.ReadLine();

                if (inputString == null)
                        continue;

                if (inputString.Equals("exit", StringComparison.InvariantCultureIgnoreCase) || 
                        inputString.Equals("quit", StringComparison.InvariantCultureIgnoreCase))
                {
                    inputString = "wcscli";
                    inputString += " ";
                    inputString += "-" + WcsCliConstants.terminateCmConnection;
                    _tobreak = true;
                }

                WcsCliCmProxy.InteractiveParseUserCommandGetCmResponse(false,inputString);

                if (_tobreak)
                    break;
            } // While loop ends
            return;
        }

        /// <summary>
        /// Method to convert command line arguments to a string array for command validation
        /// </summary>
        /// <param name="args">arguments for host, port, ssl otipn, batch input</param>
        /// <returns></returns>
        private static string ConvertConsoleCommandLineArgsToCommandInput(string[] args)
        {
            string inputString;

            if(args != null)
            {
                inputString = "wcscli";
                inputString += " ";
                inputString += "-" + WcsCliConstants.establishCmConnection;
                inputString += " ";
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Trim().Equals(@"-h", StringComparison.InvariantCultureIgnoreCase))
                    {
                        args[i] = "-m";
                    }
                    if (args[i].Trim().Equals(@"-b", StringComparison.InvariantCultureIgnoreCase) ||
                            args[i].Trim().Equals(@"-v", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isBatchOrVersionCmd = true;
                    }
                    inputString += args[i];
                    inputString += " ";
                }
                
                return inputString;
            }
            else
            {
                return null;
            }
        }
    } // class program ends
} // namespace WcsCli ends

