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

# define TRACE_LOG
# define USER_LOG

namespace Microsoft.WCS.ChassisManager
{
    /// <summary>
    /// ChassisLog.userLog(myString): User events logging - string input 
    /// ChassisLog.traceLog(myString): Full debug tracing including method-name/file-name/line-number - string input
    /// </summary>
    static internal class ChassisLog
    {
        // TODO: Move this to constants class
        const string traceLogFilePath = @"C:\ChassisManagerTraceLog.txt";
        const string userLogFilePath = @"C:\ChassisManagerUserLog.txt";

        private static System.Diagnostics.TextWriterTraceListener ChassisManagerTraceLog;
        private static System.Diagnostics.TextWriterTraceListener ChassisManagerUserLog;
        private static System.IO.FileStream cmLogTraceFile;
        private static System.IO.FileStream cmLogUserFile;

        // static constructor 
        static ChassisLog()
        {
            // Creates the text file that the trace listener will write to
            // Creates the new trace listener.
            try
            {
# if TRACE_LOG
                cmLogTraceFile = new System.IO.FileStream(traceLogFilePath, System.IO.FileMode.Append);
                ChassisManagerTraceLog = new System.Diagnostics.TextWriterTraceListener(cmLogTraceFile);
# endif
            }
            catch (System.Security.SecurityException e)
            {
                System.Console.WriteLine("Trace Logging cannot be done. Security Exception " + e);
            }
            catch (System.IO.IOException e)
            {
                System.Console.WriteLine("Trace Logging cannot be done. IO Exception " + e);
            }
            catch (System.ArgumentNullException e)
            {
                System.Console.WriteLine("Trace Logging cannot be done. Exception " + e);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Trace Logging cannot be done. Exception " + e);
            }

            try
            {
# if USER_LOG
                cmLogUserFile = new System.IO.FileStream(userLogFilePath, System.IO.FileMode.Append);
                ChassisManagerUserLog = new System.Diagnostics.TextWriterTraceListener(cmLogUserFile);
# endif
            }
            catch (System.Security.SecurityException e)
            {
                System.Console.WriteLine("User Logging cannot be done. Security Exception " + e);
            }
            catch (System.IO.IOException e)
            {
                System.Console.WriteLine("User Logging cannot be done. IO Exception " + e);
            }
            catch (System.ArgumentNullException e)
            {
                System.Console.WriteLine("User Logging cannot be done. Exception " + e);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("User Logging cannot be done. Exception " + e);
            }
        }

        static internal void traceLog(string logString)
        {
            try
            {
# if TRACE_LOG
                // Logs a trace of called method(s) name, filename(s), and, line number(s) information.
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                string stackIndent = "";
                for (int i = 1; i < st.FrameCount; i++)
                {
                    System.Diagnostics.StackFrame sf = st.GetFrame(i);
                    ChassisManagerTraceLog.WriteLine("");
                    ChassisManagerTraceLog.WriteLine(stackIndent + " Method: " + sf.GetMethod().ToString());
                    ChassisManagerTraceLog.WriteLine(stackIndent + " File: " + sf.GetFileName());
                    ChassisManagerTraceLog.WriteLine(stackIndent + " Line Number: " + sf.GetFileLineNumber().ToString());
                    stackIndent += "  ";
                }
                ChassisManagerTraceLog.WriteLine("");
                // Print the passed input string
                ChassisManagerTraceLog.WriteLine(System.DateTime.Now.ToString() + " " + logString);
                ChassisManagerTraceLog.Flush();
# endif
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("Trace Logging cannot be done. Exception " + e);
            }
        }

        static internal void userLog(string logString)
        {
            try
            {
            # if USER_LOG
                            ChassisManagerUserLog.WriteLine(System.DateTime.Now.ToString() + " " + logString);
                            ChassisManagerUserLog.Flush();
            # endif
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine("User Logging cannot be done. Exception " + e);
            }
        }
    }
}

