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

using System.Diagnostics;
using System.IO;
using System;
using Microsoft.GFS.WCS.ChassisManager;
using System.Threading;

namespace Microsoft.GFS.WCS.ChassisManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceModel;
    using System.Security.Principal;

    internal static class Tracer
    {
        /// <summary>
        /// Changed in the App config.  Signal for checking if tracing is enabled.  Application usage:
        ///     Tracer.Trace.WriteLineIf(traceEnabled.Enabled, "Content to trace");
        /// </summary>
        public static BooleanSwitch TraceEnabled = new BooleanSwitch("TraceEnabled", "On/Off signal for trace checking");

        /// <summary>
        /// Get source switch information for debug tracing.Defined in app.config.
        /// </summary>
        public static SourceSwitch DebugSourceSwitch = new SourceSwitch("TraceSourceSwitch");

        /// <summary>
        /// Trace file path
        /// </summary>
        private static string _tracefileName = ConfigLoaded.TraceLogFilePath;

        /// <summary>
        /// User log file path
        /// </summary>
        private static string _userlogfileName = ConfigLoaded.UserLogFilePath;

        /// <summary>
        /// Define a trace source for user logging, 
        /// as we need only user specific data written to this log file
        /// </summary>
        private static TraceSource UserSource =
            new TraceSource("TraceUserSource");

        /// <summary>
        /// Define a trace source for debug log, 
        /// as we need only debugging specific data written to this log file
        /// </summary>
        private static TraceSource DebugSource =
            new TraceSource("TraceDebugSource");

        /// <summary>
        /// Define a circular tracelistener for Debug trace log
        /// </summary>
        private static CircularTraceListener DebugTraceLog;

        /// <summary>
        /// Define a circular tracelistener for User trace log
        /// </summary>
        private static CircularTraceListener UserTraceLog;

        /// <summary>
        /// EventLog is for tracing this trace log
        /// </summary> 
        public static EventLog chassisManagerEventLog;


        /// <summary>
        /// Exception flag for Trace log
        /// </summary>
        internal static bool chassisTraceLogException = false;

        /// <summary>
        /// Exception flag for User log
        /// </summary>
        internal static bool chassisUserLogException = false;

        /// <summary>
        /// Constructor for initialization
        /// </summary>
        static Tracer()
        {
            try
            {
                if (!EventLog.SourceExists("ChassisTraceLogSource"))
                {
                    // TODO : Move event source creation to service install script.
                    EventLog.CreateEventSource("ChassisTraceLogSource", "ChassisManagerTracerEvent");
                    System.Threading.Thread.Sleep(10000); // Sleep for 10 seconds to allow time for OS to refresh its event list (MSDN)
                }
                chassisManagerEventLog = new EventLog();
                chassisManagerEventLog.Source = "ChassisTraceLogSource";

                // Initialize trace log
                traceLogInit();

                // Initialize user log
                userLogInit();
            }
            catch (Exception)
            {
                chassisManagerEventLog = null;
                return;
            }
        }

        /// <summary>
        /// Initialize the trace log.
        /// </summary>
        public static void traceLogInit()
        {
            try
            {
                //Create a new instance of CircularTraceListener class for debug log.
                DebugSource.Switch = DebugSourceSwitch;
                DebugTraceLog = new CircularTraceListener(new CircularStream(_tracefileName, ConfigLoaded.TraceLogFileSize));
                DebugSource.Listeners.Add(DebugTraceLog);
                
            }
            catch (Exception e)
            {
                chassisManagerEventLog.WriteEntry("CM Trace Logging cannot be done. Exception: " + e.ToString());
               
            }
        }

        /// <summary>
        /// Initialize user log
        /// </summary>
        public static void userLogInit()
        {
            try
            {               
                //Create a new instance of CircularTraceListener class for Userlog.
                UserSource.Switch.Level = SourceLevels.All;
                UserTraceLog = new CircularTraceListener(new CircularStream(_userlogfileName, ConfigLoaded.UserLogFileSize));
                UserSource.Listeners.Add(UserTraceLog);
            }
            catch (Exception e)
            {
                chassisManagerEventLog.WriteEntry("CM User Logging cannot be done. Exception: " + e.ToString());
            }
        }

        /// <summary>
        /// User logging is always done and not taken as config parameter input  
        /// </summary>
        /// <param name="message"></param>
        public static void WriteUserLog(string message, Object obj1 = null, Object obj2 = null, Object obj3 = null)
        {
            // Getting the username from the operation context
            // Initialize the username as anonymous
            string currentUsername = "Anonymous";
            try
            {
                currentUsername = OperationContext.Current.ServiceSecurityContext.WindowsIdentity.Name;

                UserSource.TraceInformation(string.Format("{0},{1},{2},{3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    currentUsername,"ThreadID:" + Thread.CurrentThread.ManagedThreadId, String.Format(message, obj1, obj2, obj3)));
                    
                UserSource.Flush();
            }
            catch (Exception ex)
            {
                 chassisManagerEventLog.WriteEntry(ex.ToString());
            }
        }

        /// <summary>
        /// Clear user log
        /// </summary>
        /// <returns>Status success/failure</returns>
        public static bool ClearUserLog()
        {
            bool success = false;
            try
            {
                success = UserTraceLog.ClearTrace();
            }
            catch (Exception ex)
            {   
                chassisManagerEventLog.WriteEntry(ex.ToString());
            }

            return success;
        }

        /// <summary>
        /// System Trace Write Output if log level is enabled in the app config.
        /// </summary>
        public static void WriteError(string message, Object obj1 = null, Object obj2 = null, Object obj3 = null)
        {
            try
            {
                DebugSource.TraceEvent(TraceEventType.Error, 1, message, obj1, obj2, obj3);
            }
            catch (Exception ex)
            {
                chassisManagerEventLog.WriteEntry(ex.ToString());               
            }
        }

        /// <summary>
        /// System Trace Write Output if log level is enabled in the app config.
        /// </summary>
        public static void WriteError(Exception ex)
        {
            WriteError(ex.ToString());
        }

        /// <summary>
        /// Write warnings to debug trace log
        /// </summary>
        /// <param name="message">log message</param>
        public static void WriteWarning(string message, Object obj1 = null, Object obj2 = null, Object obj3 = null)
        {
            try
            {
                DebugSource.TraceEvent(TraceEventType.Warning, 1, message, obj1, obj2, obj3);

            }
            catch (Exception ex)
            {
                chassisManagerEventLog.WriteEntry(ex.ToString());               
            }
        }

        /// <summary>
        /// Write info to debug trace log
        /// </summary>
        /// <param name="message">log message</param>
        public static void WriteInfo(string message, Object obj1 = null, Object obj2 = null, Object obj3 = null)
        {
            try
            {
                 DebugSource.TraceEvent(TraceEventType.Information, 1, message, obj1, obj2, obj3);
            }
            catch (Exception ex)
            {
                 chassisManagerEventLog.WriteEntry(ex.ToString());
            }
        }

        /// <summary>
        /// Get user log file path.
        /// </summary>
        /// <returns>User log file path</returns>
        public static string GetCurrentUserLogFilePath()
        {
            string filePath = null;
            try
            {
                 filePath = UserTraceLog.GetFilePath();
            }
            catch (Exception ex)
            {
                chassisManagerEventLog.WriteEntry(ex.ToString());
            }

            return filePath;
        }
    }
}
