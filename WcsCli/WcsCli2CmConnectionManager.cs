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
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Xml;
using System.ServiceProcess;

namespace Microsoft.GFS.WCS.WcsCli
{
    /// <summary>
    /// Static class that defines the global config values for WcsCli.
    /// These are set at runtime from user input
    /// </summary>
    internal static class WcsCli2CmConnectionManager
    {
        private static ChannelFactory<Contracts.IChassisManager> serviceChannel {get; set;}


        internal static bool IsCmServiceConnectionActive { get; private set; }

        // Command class uses this channel to talk to the CM
        internal static Contracts.IChassisManager channel {get; set;}

        private const int TimeoutInSecs = 180; // Timeout value for service connection

        internal static void CreateConnectionToService(string cmHostname, int cmServicePortno, bool sslEnabled, string cmServiceUsername, string cmServicePassword, int timeout = TimeoutInSecs)
        {
            try
            {
                WebHttpBinding bd = new WebHttpBinding();
                bd.UseDefaultWebProxy = false;
                bd.SendTimeout = TimeSpan.FromSeconds(timeout);
                bd.ReceiveTimeout = TimeSpan.FromSeconds(timeout);

                bd.MaxBufferSize = 1024 * 1024 * 10;
                bd.MaxReceivedMessageSize = 10 * 1024 * 1024;
                bd.MaxBufferPoolSize = 10 * 1024 * 1024;
                bd.ReaderQuotas.MaxStringContentLength = 128 * 1024;

                if (!ValidateHostNameAndPort(cmHostname, cmServicePortno))
                    return;

                // If enable encryption is true
                if (sslEnabled)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(ValidateServerCertificate);
                    bd.Security.Mode = WebHttpSecurityMode.Transport;
                    bd.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                    String uri = "https://" + cmHostname + ":" + cmServicePortno;
                    serviceChannel = new ChannelFactory<Contracts.IChassisManager>(bd, uri);
                }
                else
                {
                    bd.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
                    bd.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                    String uri = "http://" + cmHostname + ":" + cmServicePortno;
                    serviceChannel = new ChannelFactory<Contracts.IChassisManager>(bd, uri);
                }

                serviceChannel.Endpoint.Behaviors.Add(new WebHttpBehavior());

                // Check if user credentials are specified, if not use default
                if (cmServiceUsername!=null && cmServicePassword!=null)
                {
                    // Set user credentials specified
                    serviceChannel.Credentials.Windows.ClientCredential =
                            new System.Net.NetworkCredential(cmServiceUsername, cmServicePassword);
                } 

                channel = serviceChannel.CreateChannel();
            }
            catch (Exception)
            {
                //Exception occured when creating service channel with given config params, return false
                Console.WriteLine("failed to connect to service");
            }
        }

        // The following method is invoked by the RemoteCertificateValidationDelegate if EnableSSLEncryption is true. 
        internal static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Test if we can establish a connection to service with given hostname, port and SSL option
        /// </summary>
        /// <returns></returns>
        internal static bool TestConnectionToCmService()
        {
            try
            {
                if (serviceChannel!=null && serviceChannel.State != System.ServiceModel.CommunicationState.Faulted)
                {
                    // Call chassis manager service API to check connection and get service version information
                    Contracts.ServiceVersionResponse verResponse = channel.GetServiceVersion();
                    if (verResponse != null)
                    {
                        // If the CLI version and the CM version do not match, return false
                        if (!String.Equals(verResponse.serviceVersion, GetCLIVersion(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            Console.WriteLine("WCSCLI version ({0})does not match the Service version({1}).Please install correct version of WCSCLI and retry.", GetCLIVersion(), verResponse.serviceVersion);
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    
                    // Connection established, return true
                    IsCmServiceConnectionActive = true;
                    return true;
                }
                else
                {
                    // channel is faulted, can't establish connection
                    return false;
                }

            }
            catch (Exception)
            {
                //Exception occured when communicating with service, return false
                return false;
            }
        }

        internal static void TerminateConnectionToCmService()
        {
            IsCmServiceConnectionActive = false;
        }

        /// <summary>
        /// Validate hostname and port enetered by user
        /// </summary>
        /// <param name="hostname">Chassis Manager hostname</param>
        /// <param name="port">Chassis Manager port number</param>
        private static bool ValidateHostNameAndPort(string hostname, int port)
        {
            // Check if host name is valid
            if (hostname == null || Uri.CheckHostName(hostname) == System.UriHostNameType.Unknown)
            {
                Console.WriteLine("Host Name is invalid. Please enter a valid host name");
                return false;

            }
            // Check if port is non-zero and not a negative value
            else if (port <= 0)
            {
                Console.WriteLine("Port number is invalid. Please enter a valid port number");
                return false;
            }
            else
            {
                // Check if we can establish a connection and port is listening
                if (!CheckIfPortIsListening(hostname, port))
                {
                    Console.WriteLine("\nFailed to establish a connection with the given host name and port.Try again.");
                    return false;
                }

                // If no errors return success
                return true;
            }
        }

        /// <summary>
        /// Check if the port is listening, and we can establish a connection
        /// </summary>
        /// <param name="hostname">Chassis Manager host name</param>
        /// <param name="port">Chassis Manager port number</param>
        /// <returns>bool true/false for success/failure</returns>
        private static bool CheckIfPortIsListening(string hostname, int port)
        {
            bool status = false;
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(hostname, port);
                status = true;
            }
            // catch all exceptions
            catch (Exception)
            {
                status = false;
            }
            finally
            {
                socket.Close();
            }

            return status;
        }

        /// <summary>
        /// Get CLI assembly version
        /// </summary>
        /// <returns></returns>
        internal static string GetCLIVersion()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                string cliVersion = fileVersionInfo.ProductVersion;
                return cliVersion;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Starts Windows chassismanager service
        /// </summary>
        /// <returns>Boolean status of service start</returns>
        public static bool StartChassisManager()
        {
            bool status = false;

            // Secified timeout to wait for the service to reach the start status
            int timeoutMilliseconds = 30000;
            ServiceController controller = new ServiceController("chassismanager");
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

            // Start the service only when it is already stopped
            if (controller.Status == ServiceControllerStatus.Stopped)
            {
                Console.WriteLine("Starting the chassismanager service ...");
                try
                {
                    // Start the service, and wait until its status is "Running"
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    status = true;
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Could not start the chassismanager service");
                }
            }
            else
            {
                Console.WriteLine("chassismanager service is already running.");
            }

            return status;
        }

        /// <summary>
        /// Stops Windows chassismanager service
        /// </summary>
        /// <returns>Boolean status of service stop</returns>
        public static bool StopChassisManager()
        {
            bool status = false;

            // Secified timeout to wait for the service to reach the stop status
            int timeoutMilliseconds = 30000;
            ServiceController controller = new ServiceController("chassismanager");
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

            // Stop service only when it is already started and running
            if (controller.Status == ServiceControllerStatus.Running)
            {
                Console.WriteLine("Stopping the chassismanager service ...");
                try
                {
                    // Stop the service, and wait until its status is "Stopped"
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    status = true;
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Could not stop the chassismanager service");
                }
            }
            else
            {
                Console.WriteLine("chassismanager service is already stopped.");
                status = false;
            }

            return status;
        }

        // <summary>
        /// Establish SSL based connection to chassis manager service
        /// </summary>
        /// <returns>Boolean status indicating the success/failure of the connection</returns>
        public static bool SetSSL(bool enable)
        {
            try
            {
                // Stop chassismanager service
                StopChassisManager();

                // Change EnableSsyEncryption to 1 in ChassisManager config.
                WriteSSL(enable);

                // Start chassismanager service
                StartChassisManager();
            }
            catch (Exception e)
            {
                Console.WriteLine("The SetSSL failed with message: "
                    + e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Change EnableSslEncryption value to 0 or 1 (based on user input) in ChassisManager config file.
        /// </summary>
        private static void WriteSSL(bool enable)
        {
            // This is a fixed file location for chassismanager config.
            string fileLoc = @"C:\ChassisManager\Microsoft.GFS.WCS.ChassisManager.exe.config";

            XmlDocument doc = new XmlDocument();
            doc.Load(fileLoc);

            System.Xml.XmlElement Root = doc.DocumentElement;
            XmlNode appNode = Root["appSettings"];

            int sslEnable = 1;

            if(enable)
                sslEnable = 1;
            else
                sslEnable = 0;

            foreach (XmlNode node in appNode.ChildNodes)
            {
                if (node.Attributes != null)
                {
                    try
                    {
                        string key = node.Attributes.GetNamedItem("key").Value;
                        // Change the value of EnableSslEncryption key based on input parameter 
                        if (key == "EnableSslEncryption")
                        {
                            node.Attributes.GetNamedItem("value").Value = 
                                sslEnable.ToString();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception occurred during XML parsing: "
                            + e.Message);
                    }
                }
            }
            doc.Save(fileLoc);
            doc = null;
        }
    }
}
