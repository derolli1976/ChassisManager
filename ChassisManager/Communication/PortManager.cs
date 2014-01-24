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
using System.Threading;
using System.Collections;
using System.IO.Ports;
using System.Diagnostics;

namespace Microsoft.GFS.WCS.ChassisManager
{
    /// <summary>
    /// A base class that abstracts communication ports available in the system
    /// </summary>
    class PortManager : IDisposable
    {
        /// <summary>
        /// The work queues that contain the work items.
        /// Each work item is inserted into one of the work queue based on its
        /// priority.
        /// The lower the index of the queue, the higher priority
        /// </summary>
        protected Queue[] workQueues;

        // To specify the maximum number of work items that each work queue
        // can contain
        protected int maxQueueLength = ConfigLoaded.MaxPortManagerWorkQueueLength;

        // Event variable to signal the device-thread whenever a new work item is
        // inserted in one of the work queues
        protected AutoResetEvent autoEvent;

        // 0: SerialPort - Other hardware components (Fan, PSU, Power)
        // 1: SerialPort - Servers
        // 2: SerialPort - serial port console 1
        protected int logicalPortId;

        // TODO: more ports can be added in the future (e.g., COM2, 5, 6)
        static private string[] physicalPortNames = { "COM3", "COM4", "COM1", "COM2", "COM5", "COM6" };

        // Each communication port has a dedicated device-level (worker) thread.
        // Once a new work item is inserted, the thread wakes up, serves the request,
        // wakes up the waiting user-level thread with a response, and sleeps until
        // a new work item is inserted
        protected Thread workerThread;

        // If this flag is set, the worker thread should terminate
        volatile private bool shouldTerminateWorkerThread = false;

        // The time to wait (in ms) for the worker thread to join
        const int timeToWaitInMsToJoinWorkerThread = 7000;

        private bool disposed = false;

        /// <summary>
        /// This flag is set if CM is running in the safe mode
        /// </summary>
        volatile protected bool isSafeModeEnabled = false;

        internal PortManager(int lpId, int numPriorityLevels)
        {
            int i;
            logicalPortId = lpId;

            // TODO: Currently assuming one-to-one mapping from priorityLevel to workQueue
            // Need to be generalized
            workQueues = new Queue[numPriorityLevels];

            for (i = 0; i < numPriorityLevels; i++)
            {
                workQueues[i] = new Queue();
            }

            // Create an event variable to signal the worker thread
            autoEvent = new AutoResetEvent(false);

            // Create the worker thread dedicated for this port
            workerThread = new Thread(WorkerThreadFunction);
        }

        ~PortManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Get the work queue associated a given priority level
        /// Return value can be null and the caller must check
        /// </summary>
        /// <param name="priorityLevel"></param>
        /// <returns></returns>
        private Queue GetWorkQueue(PriorityLevel priorityLevel)
        {
            if (priorityLevel == PriorityLevel.System)
            {
                return workQueues[0];
            }
            else if (priorityLevel == PriorityLevel.User)
            {
                return workQueues[1];
            }
            else
            {
                Tracer.WriteError("[Error] Invalid prioirty level ({0})", priorityLevel);
                return null;
            }
        }

        /// <summary>
        /// Add a work item into a work queue.
        /// Return fail if the parameter is not valid and/or
        /// the target queue is currently full
        /// The caller must check the return value
        /// </summary>
        /// <param name="priorityLevel"></param>
        /// <param name="workItem"></param>
        /// <returns></returns>
        internal bool SendReceive(PriorityLevel priorityLevel, WorkItem workItem)
        {
            Queue workQueue = GetWorkQueue(priorityLevel);

            // Invalid priority level
            if (workQueue == null)
            {
                return false;
            }

            // Must be thread-safe as other user-level thread or the device-level thread
            // can concurrently access the queue
            lock (workQueue.SyncRoot)
            {
                if (workQueue.Count > maxQueueLength)
                {
                    // Invalid execution path: should not reach here
                    Tracer.WriteError("[Error, PortManager: {0}, priorityLevel: {1}] Queue (size: {2}) overflowed",
                        logicalPortId, priorityLevel, workQueue.Count);
                    return false;
                }
                else if (workQueue.Count == maxQueueLength)
                {
                    // The work queue is current full and cannot serve the request
                    Tracer.WriteWarning("[PortManager: {0}, priorityLevel: {1}] Full", logicalPortId, priorityLevel);
                    return false;
                }

                // Insert the work item
                workQueue.Enqueue(workItem);
            }

            // Signal the worker thread to process the work item that has been just inserted
            autoEvent.Set();

            // The work item has been successfully inserted
            return true;
        }

        /// <summary>
        /// The worker thread must call this method to get a new work item.
        /// If all the work queues are empty, null is returned
        /// </summary>
        /// <returns></returns>
        protected WorkItem GetWorkItem()
        {
            int i;
            WorkItem workItem = null;

            // The lower index, the higher priority
            // TODO: low-priority requests can starve
            for (i = 0; i < workQueues.Length; i++)
            {
                // Must be thread-safe as other threads can concurrently access the queue
                lock (workQueues[i].SyncRoot)
                {
                    if (workQueues[i].Count != 0)
                    {
                        workItem = (WorkItem)workQueues[i].Dequeue();
                        return workItem;
                    }
                }
            }

            return workItem;
        }
                       
        /// <summary>
        /// The worker thread must call this method to wait for a new item
        /// </summary>
        protected void WaitForWorkItem()
        {
            // TODO: determine a proper value
            const int timeToWaitInMs = 1000;
            autoEvent.WaitOne(timeToWaitInMs);
        }

        /// <summary>
        /// Initalizes the port manager.
        /// 1. Initialize port-specific data structures in derived methods
        /// 2. Start the worker thread
        /// </summary>
        internal virtual CompletionCode Init()
        {
            shouldTerminateWorkerThread = false;

            // Safe mode is disabled by default
            isSafeModeEnabled = false;

            workerThread.Start();

            // Success path
            return CompletionCode.Success;
        }

        /// <summary>
        /// The code that is executed by the worker thread
        /// 1. Wait for a new work item
        /// 2. Generate a data packet using the request information
        /// 3. Send the generated packet over the port
        /// 4. Receive the response from the target hardware device
        /// 5. Generate/copy a response packet
        /// 6. Signal the waiting user-level thread
        /// 7. Repeat above
        /// Note: For serviceability purpose, continue to execute
        /// the main loop even with an unhandled exception
        /// </summary>
        private void WorkerThreadFunction()
        {
            while (true)
            {
                try
                {
                    while (true)
                    {
                        WorkItem workItem;

                        // Get a new work item
                        workItem = GetWorkItem();

                        if (workItem != null)
                        {
                            Tracer.WriteInfo("[Worker] logicalPortId: {0}, Signaled", logicalPortId);
                            if (shouldTerminateWorkerThread == true)
                            {
                                Tracer.WriteWarning("[Worker] Draining the remaining work item before terminating CM");
                            }
                            SendReceive(ref workItem);
                        }
                        else
                        {
                            // If CM is about to gracefully terminate and there is no more work item in the
                            // queues (i.e., all the queues have been drained), stop this worker thread 
                            // by exiting from the main loop 
                            if (shouldTerminateWorkerThread == true)
                            {
                                Tracer.WriteInfo("[Worker] Stopping worker threads", logicalPortId);
                                return;
                            }
                            WaitForWorkItem();
                        }
                    }
                }
                catch (Exception e)
                {
                    Tracer.WriteError("Unhandled Exception in PortManager worker thread main loop");
                    Tracer.WriteError(e);
                }
            }
        }

        /// <summary>          
        /// Send and receive the data through the actual physical port/channel
        /// Inherited classes should implement this as it is physical port/channel specific
        /// </summary>
        /// <param name="workItem"></param>
        protected virtual void SendReceive(ref WorkItem workItem)
        {
            // Inherited classes should implement
        }

        /// <summary>
        /// Get the physical port name from a logical port ID.
        /// Returns null if an invalid logical port ID
        /// </summary>
        /// <param name="lpId"></param>
        /// <returns></returns>
        internal static protected string GetPhysicalPortNameFromLogicalId(int lpId)
        {
            if (lpId >= 0 && lpId < physicalPortNames.Length)
            {
                Tracer.WriteInfo("GetPhysicalPortNameFromLogicalId({0})", physicalPortNames[lpId]);
                return physicalPortNames[lpId];
            }
            else
            {
                Tracer.WriteInfo("GetPhysicalPortNameFromLogicalId: Error Invalid port");
                return null;
            }
        }

        /// <summary>
        /// Terminate the worker thread
        /// </summary>
        protected void TerminateWorkerThread()
        {
            // First, set this flag to stop the worker thread
            shouldTerminateWorkerThread = true;

            // Wait until the worker thread joins
            if (workerThread != null)
            {
                if (workerThread.IsAlive)
                {
                    try
                    {
                        workerThread.Join(timeToWaitInMsToJoinWorkerThread);
                    }
                    catch (Exception e)
                    {
                        Tracer.WriteError(e);
                    }
                }
            }

            workerThread = null;
        }

        /// <summary>
        /// Release all the resource
        /// </summary>
        public virtual void Release()
        {
            // Implemented by derived classes
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (autoEvent != null)
                    {
                        autoEvent.Dispose();
                        autoEvent = null;
                    }
                }
                disposed = true;
            }
        }

        /// <summary>
        /// Check if in safe mode
        /// </summary>
        internal bool IsSafeMode()
        {
            Tracer.WriteInfo("Safe mode status (logicalPortId: {0})", logicalPortId);
            return isSafeModeEnabled;
        }

        /// <summary>
        /// Disable safe mode
        /// </summary>
        internal void DisableSafeMode()
        {
            isSafeModeEnabled = false;
            Tracer.WriteInfo("Safe mode has been disabled (logicalPortId: {0})", logicalPortId);
        }

        /// <summary>
        /// Enable safe mode
        /// </summary>
        internal void EnableSafeMode()
        {
            isSafeModeEnabled = true;
            Tracer.WriteInfo("Safe mode has been enabled (logicalPortId: {0})", logicalPortId);
        }
    }
}
