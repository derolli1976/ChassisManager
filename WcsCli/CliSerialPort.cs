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
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Microsoft.GFS.WCS.WcsCli
{
    internal static class CliSerialPort
    {
        private static SerialPort _serialPort = new SerialPort();

        // Variables for serial input/output 
        private static List<byte> _userCommandInput = new List<byte>();
        private static MemoryStream _memoryStreamConsoleOut = new MemoryStream();

        // lock variable for protecting the read/writes to _userCommandInput
        private static Object _lockObject = new object();

        // Signal variable 
        private static AutoResetEvent _waitSerialUserInput = new AutoResetEvent(false);

        // Timeout value for serial read/write
        private static int _timeout = 500;

        internal static bool CliSerialOpen(string _comPort, int _baudRate)
        {
            try
            {
                // Make these serial port paramters configurable
                _serialPort.PortName = _comPort;
                _serialPort.BaudRate = _baudRate;
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;

                // Set the read/write timeouts
                _serialPort.ReadTimeout = _timeout; // Since we are using ReadExisting this timeout value does not matter
                _serialPort.WriteTimeout = _timeout;
                _serialPort.Open();

                // Set up the event handler when we receive data on the serial port
                _serialPort.DataReceived += SerialInputReceivedHandler;
            }
            catch (Exception ex)
            {
                if (Program.logWriter != null)
                {
                    Program.logWriter.WriteLine("Failure while opening serial port: " + ex.ToString());
                }
                return false;
            }

            try
            {
                // Redirect console output to a memory stream object
                StreamWriter serialWriter = new StreamWriter(_memoryStreamConsoleOut);
                serialWriter.AutoFlush = true;
                Console.SetOut(serialWriter);
            }
            catch (Exception ex)
            {
                if (Program.logWriter != null)
                {
                    Program.logWriter.WriteLine("Failure while console redirect: " + ex.ToString());
                }
                return false;
            }

            return true;
        }

        internal static void CliSerialClose()
        {
            try
            {
                _serialPort.DataReceived -= SerialInputReceivedHandler;

                // Recover the standard output stream so that a completion message can be displayed.
                StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);

                _waitSerialUserInput.Dispose();
                _memoryStreamConsoleOut.Dispose();
                _serialPort.Close();
            }
            catch (Exception ex)
            {
                if (Program.logWriter != null)
                {
                    Program.logWriter.WriteLine("Failure while closing serial port: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// This is a blocking call, which waits until a serial user input is received 
        /// </summary>
        /// <returns>Return the user entered byte array</returns>
        internal static Byte[] ReadUserInputBytesFromSerial()
        {
            while (true)
            {
                Byte[] outData = null;

                // If the requested user input data is already available, return it
                // Take a lock since we are accessing the shared _userCommandInput byte array
                lock (_lockObject)
                {
                    if (_userCommandInput!= null && _userCommandInput.Count>0)
                    {
                        outData = _userCommandInput.ToArray();
                        _userCommandInput.Clear();
                        return outData;
                    }
                }
                // Else wait for the data to become available
                // Wait until the user enters a command on the serial line
                _waitSerialUserInput.WaitOne();
            }
        }

        /// <summary>
        /// This is a blocking call, which waits until a serial user input command is received 
        /// </summary>
        /// <returns>Return the user entered command string </returns>
        internal static string ReadUserInputStringFromSerial(char[] delimiter)
        {
            while (true)
            {
                String outData = null;

                try
                {
                    // If the requested user input data is already available, return it
                    // Take a lock since we are accessing the shared _userCommandInput byte array
                    lock (_lockObject)
                    {
                        if (_userCommandInput != null && _userCommandInput.Count>0)
                        {
                            // Remove backspace characters from the command input
                            string tempString = Encoding.ASCII.GetString(_userCommandInput.ToArray());
                            tempString = HandleBackspace(tempString);
                            outData = grabRequestedData(ref tempString, delimiter);
                            
                            // Since you have consumed the data, clear the incoming buffer
                            _userCommandInput.Clear();

                            // If you have not fully consumed the data, leave the rest in the buffer
                            if (tempString != null)
                            {
                                Byte[] tempByteArray  = Encoding.ASCII.GetBytes(tempString);
                                for(int i =0;i<tempByteArray.Length;i++)
                                {
                                    _userCommandInput.Add(tempByteArray[i]);
                                }
                            }
                        }
                        if (outData != null)
                        {
                            // When a valid user entered input command line is obtained.. move the serial console to the next line..
                            // This will prevent the serial client terminal to overwrite on the same command line.. 
                            _serialPort.WriteLine("");
                            return outData;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (Program.logWriter != null)
                    {
                        Program.logWriter.WriteLine("Failure at ReadUserInputFromSerial.. " + ex.Message);
                    }
                    return null;
                }
                // Else wait for the data to become available
                // Wait until the user enters a command on the serial line
                _waitSerialUserInput.WaitOne();
            }
        }

        /// <summary>
        /// Method for writing the input byte array to serial
        /// </summary>
        /// <param name="myData"></param>
        internal static void WriteBytestoSerial(byte[] myData)
        {
                _serialPort.Write(myData, 0, myData.Length);
        }

        /// <summary>
        /// Write console output (from the memory stream object) to serial  
        /// </summary>
        internal static void WriteConsoleOutToSerial()
        {
            if (_memoryStreamConsoleOut != null)
            {
                var bytes = _memoryStreamConsoleOut.ToArray();
                
                try
                {
                    // Sending 256 bytes at a time.. It looks like writing a larger buffer in serail.write will not work
                    int index = 0;
                    int length = 256;
                    while(true)
                    {
                        if (index + length < bytes.Length)
                        {
                            _serialPort.Write(bytes, index, length);
                        }
                        else
                        {
                            _serialPort.Write(bytes, index, bytes.Length - index);
                            break;
                        }
                        index += length;
                    }

                    // Clear the memory stream object and set the pointer to the beginning of the stream
                    _memoryStreamConsoleOut.SetLength(0);
                    _memoryStreamConsoleOut.Seek(0, SeekOrigin.Begin);
                }
                catch (Exception ex)
                {
                    if (Program.logWriter != null)
                    {

                        Program.logWriter.WriteLine("Failure while writing console out to serial. " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Event handler will be called when any data is available on the serial port
        /// Note that we may have received one or more bytes when this event handler is called
        /// We need to make sure that we consume the data at the granularity we are interested in (either a carriage return or new line)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SerialInputReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Append the currentUserInputString to the already existing and unconsumed user input data    
                lock (_lockObject)
                { 
                    // Get everything that the user has entered on the serial line
                    for (int i = 0; i < (sender as SerialPort).BytesToRead; i++)
                    {
                        _userCommandInput.Add((byte)(sender as SerialPort).ReadByte());
                    }
                }
            }
            catch (TimeoutException)
            {
                if (Program.logWriter != null)
                {
                    Program.logWriter.WriteLine("Event handler: Serial read timeout..");
                }
            }
            catch (Exception ex)
            {
                if (Program.logWriter != null)
                {
                    Program.logWriter.WriteLine("Event handler: Failure while reading from serial port: " + ex.ToString());
                }
            }
            
            // Signal to let the program consume the user input data
            _waitSerialUserInput.Set();
        }

        private static string HandleBackspace(string inString)
        {
            if (string.IsNullOrEmpty(inString))
                return inString;

            StringBuilder result = new StringBuilder(inString.Length);
            foreach (char c in inString)
            {
                if (c == '\b')
                {
                    if (result.Length > 0)
                        result.Length--;
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }
        
        /// <summary>
        /// Grab the first occurrence of the delimited string/data and return it
        /// Remove the returned string/data from the "data" argument
        /// </summary>
        /// <param name="data"></param>
        /// <param name="delimit"></param>
        /// <returns></returns>
        private static string grabRequestedData(ref string data, char[] delimit)
        {
            try
            {
                if (data == null)
                {
                    return null;
                }

                if (delimit.Length == 0)
                {
                    return data;
                }

                string[] parts = data.Split(delimit);

                // If the delimiter is not present in the input string, return null
                if (parts.Length == 1 && parts[0].Equals(data, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }
                else // else return the first occurrence of the delimited string
                {
                    string tempString =null;
                    for (int i = 1; i < parts.Length; i++)
                    {
                        tempString += parts[i];
                    }
                    
                    if (tempString.Length > 0)
                        data = tempString;
                    else
                        data = null;

                    return parts[0];
                }
            }
            catch (Exception ex)
            {
                if (Program.logWriter != null)
                {
                    Program.logWriter.WriteLine("Failure in grabRequestedData().. " + ex.Message);
                }
                return null;
            }
        }
    }
}
