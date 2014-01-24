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
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Microsoft.GFS.WCS.ChassisManager
{
    /// <summary>
    /// This class defines a circular trace listener
    /// </summary>
    public class CircularTraceListener : XmlWriterTraceListener
    {
        /// <summary>
        /// Circular stream
        /// </summary>
        private CircularStream m_stream = null;

        /// <summary>
        /// lock object to make the trace listener thread safe
        /// </summary>
        private Object TraceLockObject = new Object();

        #region Member Functions

        /// <summary>
        /// Determine if number of bytes written is greater than max size, if yes switch stream.
        /// </summary>
        private void DetermineOverQuota()
        {
            //If we're past the Quota, flush, then switch files
      
            if (m_stream.IsOverQuota)
            {
                base.Flush();
                m_stream.SwitchFiles();
            }
        }

        #endregion

        #region XmlWriterTraceListener Functions

        public CircularTraceListener(CircularStream stream)
            : base(stream)
        {
            this.m_stream = stream;
        }

        /// <summary>
        /// Tracelistener is thread safe here -all key operations are performed from a lock.
        /// Trace class does a get on this property, if listener is not thread safe it takes a global lock 
        /// which can be a performance bottleneck. hence to avoid that set UseGlobalLock property to false in app.config and 
        /// make tracelistener thread safe instead.
        /// </summary>
        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            lock (this.TraceLockObject)
            {
                this.DetermineOverQuota();
                base.TraceEvent(eventCache, source, eventType, id);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            lock (this.TraceLockObject)
            {
                this.DetermineOverQuota();
                base.TraceEvent(eventCache, source, eventType, id, format, args);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            lock (this.TraceLockObject)
            {
                this.DetermineOverQuota();
                base.TraceEvent(eventCache, source, eventType, id, message);
            }
        }

        /// <summary>
        /// Clear trace log
        /// </summary>
        /// <returns></returns>
        public bool ClearTrace()
        {
            lock (TraceLockObject)
            {
                return (m_stream.Clear());
            }
        }

        /// <summary>
        /// Get current file path
        /// </summary>
        /// <returns></returns>
        public string GetFilePath()
        {
            lock (TraceLockObject)
            {
                return m_stream.GetCurrentFilePath();
            }
        }

        protected override void Dispose(bool disposing)
        {
            lock (TraceLockObject)
            {
                m_stream.Dispose();
                base.Dispose(disposing);
            }
        }

        #endregion

    }

    public class CircularStream : System.IO.Stream
    {
        private FileStream[] FStream = null;
        private String[] FPath = null;
        private long DataWritten = 0;
        private int FileQuota = 0;
        private int CurrentFile = 0;
        private string stringWritten = string.Empty;

        /// <summary>
        /// Inititialize a new filestream, using provided filename or default.
        /// </summary>
        /// <param name="FileName"></param>
        public CircularStream(string FileName, int maxFileSize)
        {
            try
            {
                // MaxFileSize is in KB in the configuration file, convert to bytes
                this.FileQuota = maxFileSize * 1024;

                string filePath = Path.GetDirectoryName(FileName);
                string fileBase = Path.GetFileNameWithoutExtension(FileName);
                string fileExt = Path.GetExtension(FileName);

                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = AppDomain.CurrentDomain.BaseDirectory;
                }

                FPath = new String[2];

                //Add 00 and 01 to FileNames and open streams
                FPath[0] = Path.Combine(filePath, fileBase + "00" + fileExt);
                FPath[1] = Path.Combine(filePath, fileBase + "01" + fileExt);

                FStream = new FileStream[2];
                FStream[0] = new FileStream(FPath[0], FileMode.Create);

                if (Tracer.chassisManagerEventLog != null)
                    Tracer.chassisManagerEventLog.WriteEntry("Circular stream created");
            }
            catch (IOException ex)
            {
                if (Tracer.chassisManagerEventLog != null)
                    Tracer.chassisManagerEventLog.WriteEntry("Trace/user Logging cannot be done. Exception: " + ex);
            }
        }

        /// <summary>
        /// Switch files
        /// </summary>
        public void SwitchFiles()
        {
            try
            {
                //Close current file, open next file (deleting its contents)                         
                DataWritten = 0;
                FStream[CurrentFile].Dispose();

                CurrentFile = (CurrentFile + 1) % 2;

                FStream[CurrentFile] = new FileStream(FPath[CurrentFile], FileMode.Create);
            }
            catch (Exception ex)
            {
                if (Tracer.chassisManagerEventLog != null)
                    Tracer.chassisManagerEventLog.WriteEntry("Trace/user Logging cannot be done. Exception: " + ex);
            }
        }

        /// <summary>
        /// Get trace current file path
        /// </summary>
        /// <returns>Current trace file path</returns>
        public string GetCurrentFilePath()
        {
            try
            {
                return FPath[CurrentFile];
            }
            catch (Exception ex)
            {
                Trace.TraceError("exception occured while fetching current File path" + ex);
                return null;
            }
        }

        /// <summary>
        /// Property IsOverQuota
        /// </summary>
        public bool IsOverQuota
        {
            get
            {
                return (DataWritten >= FileQuota);
            }

        }

        /// <summary>
        /// Property CanRead
        /// </summary>
        public override bool CanRead
        {
            get
            {
                try
                {
                    return FStream[CurrentFile].CanRead;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("exception occured while getting Filestream CanRead property" + ex);
                    return true;
                }
            }
        }

        /// <summary>
        /// Property CanSeek
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                try
                {
                    return FStream[CurrentFile].CanSeek;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("exception occured while getting Filestream CanSeek property" + ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// Get Filestream Length
        /// </summary>
        public override long Length
        {
            get
            {
                try
                {
                    return FStream[CurrentFile].Length;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("exception occured while getting Filestream length property" + ex);
                    return -1;
                }
            }
        }

        /// <summary>
        /// Get/set Filestream position
        /// </summary>
        public override long Position
        {
            get
            {
                try
                {
                    return FStream[CurrentFile].Position;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("exception occured while getting Filestream Position property" + ex);
                    return -1;
                }
            }
            set
            {
                try
                {
                    FStream[CurrentFile].Position = Position;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("exception occured while setting Filestream Position property" + ex);
                }
            }
        }

        /// <summary>
        /// Property CanWrite
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                try
                {
                    return FStream[CurrentFile].CanWrite;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("exception occured while getting Filestream CanWrite property" + ex);
                    return true;
                }
            }
        }

        /// <summary>
        /// Flush filestream
        /// </summary>
        public override void Flush()
        {
            try
            {
                 FStream[CurrentFile].Flush();
            }
            catch (Exception ex)
            {
                Trace.TraceError("exception occured while Filestream Flush " + ex);
            }
        }

        /// <summary>
        /// Clear filestream
        /// </summary>
        public bool Clear()
        {
            bool success = false;
            try
            {
                if (FStream[CurrentFile] != null)
                {
                    FStream[CurrentFile].SetLength(0);
                    this.DataWritten = 0;
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("exception occured while clearing trace log " + ex);
            }
            return success;
        }

        /// <summary>
        /// Filestream seek operation
        /// </summary>
        /// <param name="offset">offset</param>
        /// <param name="origin">start</param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            try
            {
                return FStream[CurrentFile].Seek(offset, origin);
            }
            catch (Exception ex)
            {
                Trace.TraceError("exception occured while Filestream seek operation " + ex);
                return -1;
            }
        }

        /// <summary>
        /// Filestream set length to given value
        /// </summary>
        /// <param name="value">given length value</param>
        public override void SetLength(long value)
        {
            try
            {
               FStream[CurrentFile].SetLength(value);
            }
            catch (Exception ex)
            {
                Trace.TraceError("exception occured while Filestream set length operation " + ex);
            }
        }

        /// <summary>
        /// Write to filestream
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                FStream[CurrentFile].Write(buffer, offset, count);
                DataWritten += count;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to write to filestream" + ex);
            }
        }

        /// <summary>
        /// Read from filestream
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                 return FStream[CurrentFile].Read(buffer, offset, count);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to read from filestream" + ex);
                return -1;
            }
        }

        /// <summary>
        /// Close filestream
        /// </summary>
        public override void Close()
        {
            try
            {
                FStream[CurrentFile].Close();
            }
            catch (Exception ex)
            {
                Tracer.chassisManagerEventLog.WriteEntry("Failed to close trace log. Exception: " + ex);

            }
        }

        /// <summary>
        /// Dispose the filestream
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (FStream != null)
            {
                FStream[CurrentFile].Dispose();
                FStream = null;
            }

            base.Dispose();
        }

    }

}
