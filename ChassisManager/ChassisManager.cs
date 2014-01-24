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

namespace Microsoft.GFS.WCS.ChassisManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.ServiceModel.Description;
    using System.Diagnostics;
    using System.Threading;
    using System.ComponentModel;
    using System.ServiceProcess;
    using System.Configuration;
    using System.Configuration.Install;
    using Microsoft.GFS.WCS.Contracts;
    using System.DirectoryServices;
    using System.Net.NetworkInformation;
    using System.Net;
    using System.Net.Sockets;
    using System.Management;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using System.Web.Security;
    using System.Security.Principal;
    using System.DirectoryServices.ActiveDirectory;
    using System.DirectoryServices.AccountManagement;
    using System.Xml;


    /// <summary>
    /// This class creates and initializes the Chassis Manager service
    /// </summary>
    public class ChassisManagerWindowsService : ServiceBase
    {
        Thread getThread;
        Thread setThread;

        public WebServiceHost serviceHost = null;

        public ChassisManagerWindowsService()
        {
            // Name the Windows Service
            ServiceName = "ChassisManager";
        }

        /// <summary>
        /// Chassis manager release function
        /// </summary>
        public void Release()
        {
            // Stop the internal get and set threads by setting this global variable
            ChassisState.ShutDown = true;

            // Wait for threads to complete their current logic before stopping
            if (getThread != null)
            {
                try
                {
                    getThread.Join();
                    Tracer.WriteInfo("OnStop: Get thread joined");
                }
                catch (Exception e)
                {
                    Tracer.WriteError(e);
                }
            }

            if (setThread != null)
            {
                try
                {
                    setThread.Join();
                    Tracer.WriteInfo("OnStop: Set thread joined");
                }
                catch (Exception e)
                {
                    Tracer.WriteError(e);
                }
            }
        }

        /// <summary>
        /// Chassis Manager initialize function
        /// </summary>
        public void Initialize()
        {
            Tracer.WriteInfo("Chassis Manager Initialization started");
            ChassisManagerInternal CM = new ChassisManagerInternal();
            byte success = CM.Initialize();

            Tracer.WriteInfo("Chassis Manager Internal initialized");

            if (success == (byte)CompletionCode.Success)
            {
                Tracer.WriteInfo("Starting Monitoirng Threads");
                getThread = new Thread(new ThreadStart(CM.RunGetAllBladeRequirements));
                setThread = new Thread(new ThreadStart(CM.RunSetDeviceCommands));

                getThread.Start();
                setThread.Start();
            }
            else
            {
                Tracer.WriteError("Chassis manager failed to initialize at {0}", DateTime.Now);
                this.Stop();
            }
            Tracer.WriteInfo("Chassis Manager initialization completed");
        }

        public static void Main()
        {
            ServiceBase.Run(new ChassisManagerWindowsService());
        }

        // Start the Windows service.
        protected override void OnStart(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CustomUnhandledExceptionEventHandler);
            try
            {
                if (serviceHost != null)
                {
                    serviceHost.Close();
                }

                // Creating a Restful binding
                WebHttpBinding bind = new WebHttpBinding();
                bind.ReceiveTimeout = TimeSpan.FromMinutes(ConfigLoaded.ServiceTimeoutInMinutes);

                Tracer.WriteInfo("CM Service: PortNo: {0}, Encryption:{1}", ConfigLoaded.CmServicePortNumber, ConfigLoaded.EnableSslEncryption);

                if (!ConfigLoaded.EnableSslEncryption)
                {
                    // Http url endpoint for the service
                    serviceHost = new WebServiceHost(typeof(ChassisManager), new Uri("http://localhost:" + ConfigLoaded.CmServicePortNumber.ToString() + "/"));

                    bind.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
                }
                else
                {
                    // Https url endpoint for the service
                    serviceHost = new WebServiceHost(typeof(ChassisManager), new Uri("https://localhost:" + ConfigLoaded.CmServicePortNumber.ToString() + "/"));

                    // Self-signed certificate located in standard certifcate store location in local machine 
                    // TODO: Change this to use remote active directory based certificate signed by Microsoft Certificate Authority
                    serviceHost.Credentials.ServiceCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My, X509FindType.FindBySubjectName, ConfigLoaded.SslCertificateName);

                    // Specify transport level security (SSL ENCRYPTION)
                    bind.Security.Mode = WebHttpSecurityMode.Transport;
                }

                // Client AUTHENTICATION is done using Windows credentials (Active directory)
                bind.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;

                // Establish a service endpoint
                ServiceEndpoint ep = serviceHost.AddServiceEndpoint(typeof(IChassisManager), bind, "");

                // Add a custom authorization manager to the service authorization behavior.
                serviceHost.Authorization.ServiceAuthorizationManager = new MyServiceAuthorizationManager();

                ServiceDebugBehavior sdb = serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
                sdb.HttpHelpPageEnabled = false;
            }
            catch (InvalidOperationException ex)
            {
                Tracer.chassisManagerEventLog.WriteEntry(ex.Message + ". You may try disabling encryption through app.config or install certificate with the name provided in app.config.");
                Tracer.WriteError(ex.Message + ". You may try disabling encryption through app.config or install certificate with the name provided in app.config.");
                Environment.Exit(-1);
            }
            catch (Exception ex)
            {
                Tracer.chassisManagerEventLog.WriteEntry("Exception in starting CM service : " + ex.Message);
                Tracer.WriteError("Exception in starting CM service " + ex.Message);
                Environment.Exit(-1);
            }

            int requiredTime = (ConfigLoaded.Population * 60000);

            // CM intialization
            RequestAdditionalTime(requiredTime); // This time period is based on sled population (might need to be tuned)

            Tracer.WriteInfo(string.Format("Additional Time Requeted: {0}", (requiredTime)));

            this.Initialize();

            Tracer.WriteInfo("Internal Initialize Complete. Attempting to open WCF host for Business");

            // Service open for connections
            serviceHost.Open();

            Tracer.WriteInfo("WCF opened for Business");
        }

        private static void CustomUnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            if (Tracer.chassisManagerEventLog != null)
            {
                Tracer.chassisManagerEventLog.WriteEntry("CustomUnhandledExceptionEventHandler caught : " + e.Message);
            }
            Environment.Exit(-1);
        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }

            Tracer.WriteInfo("OnStop: Service closed");

            RequestAdditionalTime(60 * 1000); // This is to prevent Windows service from timeouts

            // Release Chassis Manager threads
            this.Release();
            Tracer.WriteInfo("OnStop: Chassis Manager threads stopped");

            // Try to gracefully Close Open Ipmi sessions
            WcsBladeFacade.Release();
            Tracer.WriteInfo("OnStop: WcsBladeFacade released");

            // Release the communication device layer holds
            CommunicationDevice.Release();
            Tracer.WriteInfo("OnStop: Communication Device released");

        }
    }

    // Provide the ProjectInstaller class which allows 
    // the service to be installed by the Installutil.exe tool
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;

        public ProjectInstaller()
        {
            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            service = new ServiceInstaller();
            service.ServiceName = "ChassisManager";
            this.service.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            Installers.Add(process);
            Installers.Add(service);
        }
    }

    /// <summary>
    /// This class implements the service contract.
    /// </summary>
    public class ChassisManager : IChassisManager
    {
        // Class constructor
        public ChassisManager()
        {
            // Sets Web Response to be not-cache-able by client.
            WebOperationContext.Current.OutgoingResponse.Headers.Add(Constants.CacheControl, Constants.NoCache);
        }

        // Implement functionality for the service operations.

        /// <summary>
        /// Get Chassis Information
        /// </summary>
        /// <param name="bladeInfo">Set to True to get blade info </param>
        /// <param name="psuInfo">Set to True to get PSU info</param>
        /// <param name="chassisControllerInfo">Set to True to get chassis controller info</param>
        /// <returns>Response packet for Chassis Info</returns>
        public ChassisInfoResponse GetChassisInfo(bool bladeInfo, bool psuInfo, bool chassisControllerInfo)
        {
            byte MaxbladeCount = (byte)ConfigLoaded.Population;
            byte MaxPsuCount = (byte)ConfigLoaded.NumPsus;

            Tracer.WriteInfo("Received GetChassisInfo({0},{1},{2})", bladeInfo, psuInfo, chassisControllerInfo);

            Tracer.WriteUserLog("Invoked GetChassisInfo({0},{1},{2})", bladeInfo, psuInfo, chassisControllerInfo);

            // Check for the scenario where none of the params are specified or where all params are set to false 
            // return everything in that case.
            if (bladeInfo == false && psuInfo == false && chassisControllerInfo == false)
            {
                bladeInfo = true;
                psuInfo = true;
                chassisControllerInfo = true;
            }

            // Server side class structure to populate blade and psu information
            ChassisInfoResponse cip = new ChassisInfoResponse();
            cip.completionCode = Contracts.CompletionCode.Unknown;
            cip.statusDescription = String.Empty;

            // Initialize to empty collections to begin with
            cip.bladeCollections = new List<BladeInfo>();
            cip.chassisController = null;

            if (BladeSerialSessionMetadata.ApiGreenSignalling() != CompletionCode.Success)
            {
                Tracer.WriteError("GetChassisInfo() API: Failed to get green signal");
                cip.completionCode = Contracts.CompletionCode.SerialSessionActive;
                cip.statusDescription = "Device busy.Another serial session active.";

                return cip;
            }


            if (bladeInfo)
            {
                // Loop to populate blade information for requested number of blades
                for (int loop = 1; loop <= MaxbladeCount; loop++)
                {
                    try
                    {
                        BladeInfo bladeInstance = new BladeInfo();
                        bladeInstance.bladeNumber = loop;

                        // initialize completion code to unknown to start with
                        bladeInstance.completionCode = Contracts.CompletionCode.Unknown;

                        BladeStateResponse bdr = new BladeStateResponse();
                        Tracer.WriteInfo("Calling Get blade active power state");
                        bdr = GetBladeState(loop);

                        bladeInstance.completionCode = bdr.completionCode;

                        // Even if one succeeds, we set the function completion code to success
                        if (bladeInstance.completionCode == Contracts.CompletionCode.Success)
                        {
                            cip.completionCode = Contracts.CompletionCode.Success;
                        }
                        else
                        {
                            // If not already set to success, set to failure, because something actually failed here
                            if (cip.completionCode != Contracts.CompletionCode.Success)
                            {
                                cip.completionCode = Contracts.CompletionCode.Failure;
                                cip.statusDescription = "Blade info could not be retrieved, for one or more blades";
                            }
                        }

                        Contracts.PowerState powerResponse = bdr.bladeState;
                        Tracer.WriteInfo("powerResponse received");

                        // Get Blade Power State 
                        if (powerResponse == PowerState.ON)
                        {
                            bladeInstance.powerState = PowerState.ON;
                        }
                        else if (powerResponse == PowerState.OFF)
                        {
                            bladeInstance.powerState = PowerState.OFF;
                        }
                        else
                        {
                            bladeInstance.powerState = PowerState.NA;
                        }

                        // Get GUID 
                        Ipmi.DeviceGuid devGuid = WcsBladeFacade.GetSystemGuid((byte)loop);

                        if (devGuid.CompletionCode == (byte)CompletionCode.Success)
                        {
                            bladeInstance.bladeGuid = devGuid.Guid;
                            cip.completionCode = Contracts.CompletionCode.Success;
                        }
                        else
                        {
                            Tracer.WriteWarning("GetSystemGuid failed with Completion Code {0}", devGuid.CompletionCode);
                            bladeInstance.bladeGuid = System.Guid.Empty;

                            // If completion code not already set to success, set to failure, because something actually failed here
                            if (cip.completionCode != Contracts.CompletionCode.Success)
                            {
                                cip.completionCode = Contracts.CompletionCode.Failure;
                                cip.statusDescription = "Blade info could not be retrieved, for one or more blades";
                            }
                        }

                        // Any success is sufficient for this function, so only if we did not succeed, we set new value to completionCode
                        if (bladeInstance.completionCode != Contracts.CompletionCode.Success)
                        {
                            bladeInstance.completionCode =
                                    ChassisManagerUtil.GetContractsCompletionCodeMapping(devGuid.CompletionCode);
                        }

                        // bladename is BladeId
                        bladeInstance.bladeName = String.Concat("BLADE", loop);

                        // BMC Mac address should be added as a list
                        bladeInstance.bladeMacAddress = new List<NicInfo>();

                        for (byte i = 0; i < ConfigLoaded.NumNicsPerBlade; i++)
                        {
                            Ipmi.NicInfo ipmiNicInfo = WcsBladeFacade.GetNicInfo((byte)loop, (byte)(i+1));

                            if (ipmiNicInfo.CompletionCode != (byte)CompletionCode.Success &&
                                ipmiNicInfo.CompletionCode != (byte)CompletionCode.IpmiInvalidDataFieldInRequest)
                            {
                                Tracer.WriteError("Nic {0} from Blade {1} returned an error code: {2}", i, loop, ipmiNicInfo.CompletionCode);
                            }
                            Contracts.NicInfo nicInfo = GetNicInfoObject(ipmiNicInfo);
                            bladeInstance.bladeMacAddress.Add(nicInfo);
                        }

                        // Add blade to list
                        cip.bladeCollections.Add(bladeInstance);
                        
                    }
                    catch (Exception ex)
                    {
                        Tracer.WriteUserLog("GetChassisInfo (Blade portion) failed for blade {0} with exception: {1}", loop, ex.Message);
                        cip.completionCode = Contracts.CompletionCode.Failure;
                        cip.statusDescription = String.Format("GetChassisInfo (Blade portion) failed for blade {0} with exception: {1}", loop, ex.Message);
                    }
                }
            }

            if (psuInfo)
            {
                // Get the PSU Info.
                cip.psuCollections = GetPsuInfo();

                // if the master object is not successful, check child objects
                if (cip.completionCode != Contracts.CompletionCode.Success)
                {
                    // if it Psu received any positive results, return success.
                    foreach (PsuInfo psu in cip.psuCollections)
                    {
                        // if any children are successful, set master to success.
                        if (psu.completionCode == Contracts.CompletionCode.Success)
                        {
                            cip.completionCode = Contracts.CompletionCode.Success;
                            break; // once a match has been found escape foreach
                        }
                    }

                    // if master completion code is still unknown, replace with failure.
                    if(cip.completionCode == Contracts.CompletionCode.Unknown)
                        cip.completionCode = Contracts.CompletionCode.Failure;
                }
            }

            // Chassis Info should be read by reading the Fru device
            if (chassisControllerInfo)
            {
                try
                {
                    //Populate chassis controller data
                    cip.chassisController = new ChassisControllerInfo();

                    // get chassis network properties
                    cip.chassisController.networkProperties = new ChassisNetworkPropertiesResponse();
                    cip.chassisController.networkProperties = GetChassisNetworkProperties();
                    // Populate chassis IP address
                    if (cip.chassisController.networkProperties != null)
                    {
                        cip.chassisController.completionCode = Contracts.CompletionCode.Success;
                        cip.completionCode = Contracts.CompletionCode.Success;
                    }
                    else
                    {
                        Tracer.WriteInfo("GetChassisInfo - failed to get chassis network properties");
                        if (cip.chassisController.completionCode != Contracts.CompletionCode.Success)
                        {
                            cip.chassisController.completionCode = Contracts.CompletionCode.Failure;
                            cip.chassisController.statusDescription = String.Format("GetChassisInfo - failed to get chassis network properties");
                        }
                        if (cip.completionCode != Contracts.CompletionCode.Success)
                        {
                            cip.completionCode = Contracts.CompletionCode.Failure;
                            cip.statusDescription = "Failed to get chassis information";
                        }
                    }

                    cip.chassisController.systemUptime = (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString();

                    // Default Chassis details before reading FRU later
                    cip.chassisController.firmwareVersion = "NA";
                    cip.chassisController.hardwareVersion = "NA";
                    cip.chassisController.serialNumber = "NA";
                    cip.chassisController.assetTag = "NA";

                    // Read CM Fru and populate data variables
                    ChassisFru CMFruData = new ChassisFru();
                    byte status = CMFruData.readChassisFru();

                    if (status == (byte)CompletionCode.Success)
                    {
                        cip.chassisController.completionCode = Contracts.CompletionCode.Success;
                        cip.completionCode = Contracts.CompletionCode.Success;

                        cip.chassisController.firmwareVersion = CMFruData.ProductInfo.ProductVersion.ToString();
                        cip.chassisController.hardwareVersion = CMFruData.ProductInfo.Version.ToString();
                        cip.chassisController.serialNumber = CMFruData.ProductInfo.SerialNumber.ToString();
                        cip.chassisController.assetTag = CMFruData.ProductInfo.AssetTag.ToString();
                    }
                    else
                    {
                        Tracer.WriteWarning("CM Fru Read failed with completion code: {0:X}", status);
                        if (cip.chassisController.completionCode != Contracts.CompletionCode.Success)
                        {
                            cip.chassisController.completionCode = Contracts.CompletionCode.Failure;
                            cip.chassisController.statusDescription = 
                                String.Format("CM Fru Read failed with completion code: {0:X}", status);
                        }
                        if (cip.completionCode != Contracts.CompletionCode.Success)
                        {
                            cip.completionCode = Contracts.CompletionCode.Failure;
                            cip.statusDescription = "Failed to get chassis information";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Tracer.WriteUserLog(" GetChassisInfo failed with exception: " + ex.Message);
                    if (cip.completionCode != Contracts.CompletionCode.Success)
                    {
                        cip.completionCode = Contracts.CompletionCode.Failure;
                        cip.statusDescription = String.Format(" GetChassisInfo failed with exception: " + ex.Message);
                    }
                }
            }

            Tracer.WriteInfo("Return: GetChassisInfo returned, Number of Blades: {0}, Number of PSUs : {1}", cip.bladeCollections.Count(),
                cip.psuCollections.Count());

            return cip;
        }

        /// <summary>
        /// Get Chassis Manager product version
        /// </summary>
        /// <returns>service product version</returns>
        public Contracts.ServiceVersionResponse GetServiceVersion()
        {
            Contracts.ServiceVersionResponse serviceVersion = new ServiceVersionResponse();
            Tracer.WriteUserLog("Invoked GetServiceVersion");
            Tracer.WriteInfo("Received GetServiceVersion");

            serviceVersion.serviceVersion = null;
            serviceVersion.completionCode = Contracts.CompletionCode.Unknown;
            serviceVersion.statusDescription = String.Empty;
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = fileVersionInfo.ProductVersion;
                serviceVersion.serviceVersion = version;
                serviceVersion.completionCode = Contracts.CompletionCode.Success;
            }
            catch (Exception ex)
            {
                serviceVersion.completionCode = Contracts.CompletionCode.Failure;
                serviceVersion.statusDescription = String.Format("GetServiceVersion failed with exception: {0}", ex);
                Tracer.WriteError("GetServiceVersion failed with exception" + ex);
            }

            return serviceVersion;

        }

        /// <summary>
        /// Get Blade info for given Blade ID
        /// </summary>
        /// <param name="bladeId">Blade ID (1-48)</param>
        /// <returns>Blade info response</returns>
        public BladeInfoResponse GetBladeInfo(int bladeId)
        {
            byte MaxBladeCount = (byte)ConfigLoaded.Population;

            // Server side class structure to populate blade information
            BladeInfoResponse bip = new BladeInfoResponse();
            bip.bladeNumber = bladeId;
            bip.statusDescription = String.Empty;
            bip.completionCode = Contracts.CompletionCode.Unknown;

            Tracer.WriteInfo("Received GetBladeInfo({0})", bladeId);

            Tracer.WriteUserLog("Invoked GetBladeInfo({0})", bladeId);
            Contracts.ChassisResponse varResponse = ValidateRequest("GetBladeInfo", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                bip.completionCode = varResponse.completionCode;
                bip.statusDescription = varResponse.statusDescription;
                return bip;
            }


            // Get the blade information from Chassis Controller
            BladeStatusInfo bladeInfo = WcsBladeFacade.GetBladeInfo((byte)bladeId);

            if (bladeInfo.CompletionCode != 0)
            {
                Tracer.WriteWarning("GetBladeInfo failed for blade: {0}, with Completion Code: {1}", bladeId,
                     Ipmi.IpmiSharedFunc.ByteToHexString((byte)bladeInfo.CompletionCode));
                bip.completionCode = ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)bladeInfo.CompletionCode);
                bip.statusDescription = bip.completionCode.ToString();
            }
            else
            {
                // Populate the returned object
                bip.completionCode = Contracts.CompletionCode.Success;
                bip.bladeNumber = bladeId;
                bip.firmwareVersion = bladeInfo.BmcFirmware;
                bip.hardwareVersion = bladeInfo.HardwareVersion;
                bip.serialNumber = bladeInfo.SerialNumber;
                bip.assetTag = bladeInfo.AssetTag;

                if (Enum.IsDefined(typeof(BladeTypeName), ChassisState.GetBladeType((byte)bladeId)))
                {
                    bip.bladeType = Enum.GetName(typeof(BladeTypeName), ChassisState.GetBladeType((byte)bladeId));
                }
                else
                {
                    bip.bladeType = BladeTypeName.Unknown.ToString();
                }

                // BMC Mac address should be added as a list
                bip.macAddress = new List<NicInfo>();

                for (int i = 0; i < ConfigLoaded.NumNicsPerBlade; i++)
                {
                    Ipmi.NicInfo ipmiNicInfo = WcsBladeFacade.GetNicInfo((byte)bladeId, (byte)(i+1));

                    if (ipmiNicInfo.CompletionCode != (byte)CompletionCode.Success &&
                        ipmiNicInfo.CompletionCode != (byte)CompletionCode.IpmiInvalidDataFieldInRequest)
                    {
                        Tracer.WriteError("Nic {0} from Blade {1} returned an error code: {2}", i, bladeId, ipmiNicInfo.CompletionCode);
                    }
                    NicInfo nicInfo = GetNicInfoObject(ipmiNicInfo);
                    bip.macAddress.Add(nicInfo);
                }
            }

            return bip;
        }

        private Contracts.ChassisResponse ValidateRequest(string cmd, int bladeId)
        {
            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();

            // Check blade ID is valid
            byte status = ChassisManagerUtil.CheckBladeId((byte)bladeId);
            if (status == (byte)CompletionCode.InvalidBladeId)
            {
                Tracer.WriteWarning("{0} failed, Invalid blade Id {1}", cmd, bladeId);

                varResponse.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                varResponse.statusDescription = Contracts.CompletionCode.ParameterOutOfRange.ToString();
                return varResponse;
            }

            if (!FunctionValidityChecker.checkBladeStateValidity((byte)bladeId))
            {
                Tracer.WriteWarning("{0} failed, Invalid blade State {1}", cmd, bladeId);

                varResponse.completionCode = Contracts.CompletionCode.DevicePoweredOff;
                varResponse.statusDescription = Contracts.CompletionCode.DevicePoweredOff.ToString();
                return varResponse;
            }

            if (!FunctionValidityChecker.checkBladeTypeValidity((byte)bladeId))
            {
                Tracer.WriteWarning("{0} failed, Invalid blade Type {1}", cmd, bladeId);

                varResponse.completionCode = Contracts.CompletionCode.CommandNotValidForBlade;
                varResponse.statusDescription = Contracts.CompletionCode.CommandNotValidForBlade.ToString();
                return varResponse;
            }

            if (BladeSerialSessionMetadata.ApiGreenSignalling() != CompletionCode.Success)
            {
                Tracer.WriteWarning("{0} failed, Device busy {1}", cmd, bladeId);

                varResponse.completionCode = Contracts.CompletionCode.SerialSessionActive;
                varResponse.statusDescription = Contracts.CompletionCode.SerialSessionActive.ToString();
                return varResponse;
            }

            varResponse.completionCode = Contracts.CompletionCode.Success;
            varResponse.statusDescription = String.Empty;
            return varResponse;
        }

        /// <summary>
        /// Get information for all blades
        /// </summary>
        /// <returns>Array of blade info response</returns>
        public GetAllBladesInfoResponse GetAllBladesInfo()
        {
            byte maxbladeCount = (byte)ConfigLoaded.Population;

            Tracer.WriteInfo("Received GetAllBladesInfo()");
            Tracer.WriteUserLog("Invoked GetAllBladesInfo()");

            // Server side class structure to populate blade information
            GetAllBladesInfoResponse responses = new GetAllBladesInfoResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeInfoResponseCollection = new List<BladeInfoResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[maxbladeCount];
            
            // Loop to populate blade information for requested number of blades
            for (int loop = 0; loop < maxbladeCount; loop++)
            {
                int bladeId = loop + 1; // we need to get for all blades
                Tracer.WriteInfo("GetAllBladesInfo : Processing BladeID " + bladeId);

                //Call getBladeInfo for the Blade ID
                responses.bladeInfoResponseCollection.Add(this.GetBladeInfo(bladeId));

                // Set the internal blade response to the blade completion code.
                bladeInternalResponseCollection[loop] = responses.bladeInfoResponseCollection[loop].completionCode;

                Tracer.WriteInfo("GetALLBladesInfo : Completed populating for BladeID " + bladeId);

            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;

            return responses;
        }

        /// <summary>
        /// Switch chassis Attention LED On
        /// </summary>
        /// <returns>Chassis Response packet</returns>
        public Contracts.ChassisResponse SetChassisAttentionLEDOn()
        {
            Tracer.WriteInfo("Received SetChassisAttentionLEDOn()");

            Tracer.WriteUserLog("Invoked SetChassisAttentionLEDOn()");

            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            // Turn On the Attention LED.
            byte status = ChassisState.AttentionLed.TurnLedOn();

            Tracer.WriteInfo("SetChassisAttentionLEDOn Return: {0}", status);

            if (status != (byte)Contracts.CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.Failure;
                Tracer.WriteError("Chassis attention LED turn on failed with Completion Code: {0:X}", status);
                response.statusDescription = String.Format("Chassis attention LED turn on failed with Completion Code: {0:X}", status);
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.Success;
                Tracer.WriteInfo("Chassis attention LED is turned ON successfully");
            }

            return response;
        }

        /// <summary>
        /// Switch chassis Attention LED Off
        /// </summary>
        /// <returns>Chassis Response Success/Failure</returns>
        public Contracts.ChassisResponse SetChassisAttentionLEDOff()
        {
            Tracer.WriteInfo("Received SetChassisAttentionLEDOff()");

            Tracer.WriteUserLog("Invoked SetChassisAttentionLEDOff()");

            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            // Turn off the Chasssis Attention LED.
            byte status = ChassisState.AttentionLed.TurnLedOff();

            Tracer.WriteInfo("Return: {0}", status);

            if (status != (byte)Contracts.CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.Failure;
                Tracer.WriteError("Chassis attention LED turn on failed with Completion Code: {0:X}", status);
                response.statusDescription = String.Format("Chassis attention LED turn on failed with Completion Code: {0:X}", status);
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.Success;
                Tracer.WriteInfo("Chassis attention LED is turned OFF successfully");
            }

            return response;
        }

        /// <summary>
        /// Switch blade Attention LED On
        /// </summary>
        /// <param name="bladeId">Blade ID (1-48)</param>
        /// <returns>Blade Response Packet with status Success/Failure.</returns>
        public BladeResponse SetBladeAttentionLEDOn(int bladeId)
        {
            byte MaxbladeCount = (byte)ConfigLoaded.Population;
            BladeResponse response = new BladeResponse();
            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = string.Empty;
            Tracer.WriteUserLog("Invoked SetBladeAttentionLEDOn({0})", bladeId);
            Tracer.WriteInfo("Received SetBladeAttentionLEDOn({0})", bladeId);

            Contracts.ChassisResponse varResponse = ValidateRequest("SetBladeAttentionLEDOn", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            if (SetStatusLedOn(bladeId))
            {
                response.completionCode = Contracts.CompletionCode.Success;
                Tracer.WriteInfo("Blade attention LED is turned ON successfully for blade: " + bladeId);
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
                Tracer.WriteError("Blade attention LED Failed to turn ON for blade:" + bladeId);
            }
            return response;
        }

        /// <summary>
        /// Switch blade Attention LED On for all blades
        /// </summary>
        /// <returns>Blade ResponsePacket with status Success/Failure.</returns>
        public AllBladesResponse SetAllBladesAttentionLEDOn()
        {
            byte maxbladeCount = (byte)ConfigLoaded.Population;

            AllBladesResponse responses = new AllBladesResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeResponseCollection = new List<BladeResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[maxbladeCount];

            Tracer.WriteUserLog("Invoked SetAllBladesAttentionLEDOn");
            Tracer.WriteInfo("Received SetAllBladesAttentionLEDOn");

            for (int loop = 0; loop < maxbladeCount; loop++)
            {
                int bladeId = loop + 1;
                responses.bladeResponseCollection.Add(SetBladeAttentionLEDOn(bladeId));

                // Set the internal blade response to the blade completion code.
                bladeInternalResponseCollection[loop] = responses.bladeResponseCollection[loop].completionCode;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Internal command to turn on blade LED for individual blade
        /// </summary>
        /// <param name="bladeId">Blade ID (1- 48)</param>
        /// <returns>Return true/false if operation was success/failure</returns>
        private bool SetStatusLedOn(int bladeId)
        {
            Tracer.WriteInfo("Received SetStatusLedOn({0})", bladeId);

            byte LEDHigh = (byte)ConfigLoaded.LEDHigh;

            bool status = WcsBladeFacade.Identify((byte)bladeId, LEDHigh);

            Tracer.WriteInfo("Return: {0}", status);

            if (status)
            {
                Tracer.WriteInfo("blade status LED turn on successfully for bladeId: " + bladeId);
                return status;
            }
            else
            {
                Tracer.WriteError("blade status LED turn on failed for bladeId: " + bladeId);
                return status;
            }
        }

        /// <summary>
        /// Switch blade Attention LED Off 
        /// </summary>
        /// <param name="bladeId">Blade ID (1-48)</param>
        /// <returns>Return blade response true/false if operation was success/failure</returns>
        public BladeResponse SetBladeAttentionLEDOff(int bladeId)
        {
            byte maxbladeCount = (byte)ConfigLoaded.Population;
            BladeResponse response = new BladeResponse();
            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;
            Tracer.WriteUserLog("Invoked SetBladeAttentionLEDOff(bladeId : {0})", bladeId);
            Tracer.WriteInfo("Received SetBladeAttentionLEDOff(bladeId : {0})", bladeId);

            Contracts.ChassisResponse varResponse = ValidateRequest("SetBladeAttentionLEDOff", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            if (SetBladeLedOff(bladeId))
            {
                response.completionCode = Contracts.CompletionCode.Success;
                Tracer.WriteInfo("Blade attention LED turn off successfully for blade:" + bladeId);
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
                Tracer.WriteError("blade attention LED turn on failed for blade: " + bladeId);
            }
            return response;
        }

        /// <summary>
        /// Switch all blades Attention LED Off 
        /// </summary>
        /// <returns>Return true/false if operation was success/failure</returns>
        public AllBladesResponse SetAllBladesAttentionLEDOff()
        {
            byte maxbladeCount = (byte)ConfigLoaded.Population;

            Tracer.WriteUserLog("Invoked SetAllBladesAttentionLEDOff()");
            Tracer.WriteInfo("Received SetAllBladesAttentionLEDOff()");

            AllBladesResponse responses = new AllBladesResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeResponseCollection = new List<BladeResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[maxbladeCount];

            for (int loop = 0; loop < maxbladeCount; loop++)
            {
                int bladeId = loop + 1;
                responses.bladeResponseCollection.Add(SetBladeAttentionLEDOff(bladeId));

                // Set the internal blade response to the blade completion code.
                bladeInternalResponseCollection[loop] = responses.bladeResponseCollection[loop].completionCode;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Internal method to set blade LED off
        /// </summary>
        /// <param name="bladeId">Blade ID (1-48)</param>
        /// <returns>true/false if operation was success/failure</returns>
        private bool SetBladeLedOff(int bladeId)
        {
            Tracer.WriteInfo("Received SetBladeLedOff()");

            byte LEDLow = (byte)ConfigLoaded.LEDLow;

            bool status = WcsBladeFacade.Identify((byte)bladeId, LEDLow);

            Tracer.WriteInfo("Return: {0}", status);

            if (status)
            {
                Tracer.WriteInfo("blade attention LED turn off succeeded for bladeId: " + bladeId);
                return status;
            }
            else
            {
                Tracer.WriteError("blade attention LED turn off failed for bladeId: " + bladeId);
                return status;
            }
        }

        /// <summary>
        /// Sets the default blade board power state ON
        /// Indicates whether the system should be powered on or kept shutdown after power comes back to the system
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <returns>Blade Response success/failure.</returns>
        public BladeResponse SetBladeDefaultPowerStateOn(int bladeId)
        {
            Tracer.WriteInfo("Received SetBladeDefaultPowerStateOn({0})", bladeId);

            Tracer.WriteUserLog("Invoked SetBladeDefaultPowerStateOn({0})", bladeId);

            BladeResponse response = new BladeResponse();
            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("SetBladeDefaultPowerStateOn", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            Ipmi.PowerRestoreOption powerState = Ipmi.PowerRestoreOption.AlwaysPowerUp;

            Tracer.WriteInfo("Set Blade Default Power State ON for Blade: ", bladeId);

            Ipmi.PowerRestorePolicy powerPolicy = WcsBladeFacade.SetPowerRestorePolicy((byte)bladeId, powerState);

            if (powerPolicy.CompletionCode != 0)
            {
                Tracer.WriteError("Set default power state failed with completion code: {0:X}", powerPolicy.CompletionCode);
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.Success;
                Tracer.WriteInfo("Set Blade Default Power State ON success for Blade: ", bladeId);
            }
            return response;
        }

        /// <summary>
        /// Sets the default blade board power state ON for all blades
        /// Indicates whether the system should be powered on or kept shutdown after power comes back to the system
        /// </summary>
        /// <returns>rray of blade responses, one for each blade. Indicates success/failure.</returns>
        public AllBladesResponse SetAllBladesDefaultPowerStateOn()
        {
            Tracer.WriteInfo("Received SetAllBladesDefaultPowerStateOn");
            Tracer.WriteUserLog("Invoked SetAllBladesDefaultPowerStateOn()");

            byte maxbladeCount = (byte)ConfigLoaded.Population;
            AllBladesResponse responses = new AllBladesResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeResponseCollection = new List<BladeResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[maxbladeCount];

            for (int loop = 0; loop < maxbladeCount; loop++)
            {
                int bladeId = loop + 1;
                responses.bladeResponseCollection.Add(SetBladeDefaultPowerStateOn(bladeId));

                // Set the internal blade response to the blade completion code.
                bladeInternalResponseCollection[loop] = responses.bladeResponseCollection[loop].completionCode;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Sets the default blade board power state Off
        /// Indicates whether the system should be powered on or kept shutdown after power comes back to the system
        /// </summary>
        /// <param name="bladeId">Blade ID ( 1-24)</param>
        /// <returns>Blade Response success/failure.</returns>
        public BladeResponse SetBladeDefaultPowerStateOff(int bladeId)
        {
            Tracer.WriteInfo("Received SetBladeDefaultPowerStateOff(BladeId: {0})", bladeId);
            Tracer.WriteUserLog("Invoked SetBladeDefaultPowerStateOff(BladeId: {0})", bladeId);

            BladeResponse response = new BladeResponse();
            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("SetBladeDefaultPowerStateOff", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            Ipmi.PowerRestoreOption powerState = Ipmi.PowerRestoreOption.StayOff;
            Tracer.WriteInfo("Set Blade Default Power State OFF for Blade: ", bladeId);
            Ipmi.PowerRestorePolicy powerPolicy = WcsBladeFacade.SetPowerRestorePolicy((byte)bladeId, powerState);

            if (powerPolicy.CompletionCode == 0)
            {
                Tracer.WriteInfo("Set default power state OFF succeded for blade: ", bladeId);
                response.completionCode = Contracts.CompletionCode.Success;
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
                Tracer.WriteError("Set default power state failed with completion code: {0:X}, for blade: {1}", powerPolicy.CompletionCode, bladeId);
            }
            return response;
        }

        /// <summary>
        /// Sets the default blade board power state Off for all blades
        /// Indicates whether the system should be powered on or kept shutdown after power comes back to the system
        /// </summary>
        /// <returns>Array of blade responses, one for each blade. Indicates success/failure.</returns>
        public AllBladesResponse SetAllBladesDefaultPowerStateOff()
        {
            Tracer.WriteInfo("Received SetAllBladesDefaultPowerStateOff");
            Tracer.WriteUserLog("Invoked SetAllBladesDefaultPowerStateOff()");
            byte maxbladeCount = (byte)ConfigLoaded.Population;

            AllBladesResponse responses = new AllBladesResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeResponseCollection = new List<BladeResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[maxbladeCount];

            for (int loop = 0; loop < maxbladeCount; loop++)
            {
                int bladeId = loop + 1;
                responses.bladeResponseCollection.Add(SetBladeDefaultPowerStateOff(bladeId));

                // Set the internal blade response to the blade completion code.
                bladeInternalResponseCollection[loop] = responses.bladeResponseCollection[loop].completionCode;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Returns the default blade board power state
        /// </summary>
        /// <param name="bladeId">Blade ID (1-24)</param>
        /// <returns>Blade State Response packet</returns>
        public BladeStateResponse GetBladeDefaultPowerState(int bladeId)
        {
            Tracer.WriteInfo("Received GetBladeDefaultPowerState({0})", bladeId);

            Tracer.WriteUserLog("Invoked GetBladeDefaultPowerState({0})", bladeId);

            BladeStateResponse response = new BladeStateResponse();
            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("GetBladeDefaultPowerState", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            Ipmi.SystemStatus powerPolicy = WcsBladeFacade.GetChassisState((byte)bladeId);

            if (powerPolicy.CompletionCode != 0)
            {
                Tracer.WriteWarning("Set default power state failed with completion code:"
                    + Ipmi.IpmiSharedFunc.ByteToHexString((byte)powerPolicy.CompletionCode));
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
                response.bladeState = PowerState.NA;
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.Success;
                Ipmi.PowerRestoreOption currentSetting = powerPolicy.PowerOnPolicy;

                switch (currentSetting)
                {
                    case Microsoft.GFS.WCS.ChassisManager.Ipmi.PowerRestoreOption.StayOff:
                        response.bladeState = PowerState.OFF;
                        Tracer.WriteInfo("GetBladeDefaultPowerState(BladeId: {0}) return is OFF", bladeId);
                        break;
                    case Microsoft.GFS.WCS.ChassisManager.Ipmi.PowerRestoreOption.AlwaysPowerUp:
                        response.bladeState = PowerState.ON;
                        Tracer.WriteInfo("GetBladeDefaultPowerState(BladeID: {0}) return is ON", bladeId);
                        break;
                    case Microsoft.GFS.WCS.ChassisManager.Ipmi.PowerRestoreOption.GetCurrentPolicy:
                        response.bladeState = PowerState.NA;
                        Tracer.WriteInfo("GetBladeDefaultPowerState(BladeID: {0}) return is curr policy", bladeId);
                        break;
                    case Microsoft.GFS.WCS.ChassisManager.Ipmi.PowerRestoreOption.PreviousState:
                        response.bladeState = PowerState.NA;
                        Tracer.WriteInfo("GetBladeDefaultPowerState(BladeID: {0}) return is prev state", bladeId);
                        break;
                    case Microsoft.GFS.WCS.ChassisManager.Ipmi.PowerRestoreOption.Unknown:
                        response.bladeState = PowerState.NA;
                        Tracer.WriteInfo("GetBladeDefaultPowerState(BladeID: {0}) return is unknown", bladeId);
                        break;
                    default:
                        response.bladeState = PowerState.NA;
                        Tracer.WriteInfo("GetBladeDefaultPowerState(BladeID: {0}) return is NA", bladeId);
                        break;
                }
            }

            return response;
        }

        /// <summary>
        /// Returns the default blade board power state (On or Off) for all blades
        /// </summary>
        /// <returns>Array of blade state response, one for each blade.</returns>
        public GetAllBladesStateResponse GetAllBladesDefaultPowerState()
        {
            byte maxbladeCount = (byte)ConfigLoaded.Population;

            Tracer.WriteInfo("Received GetAllBladesDefaultPowerState()");

            Tracer.WriteUserLog("Invoked GetAllBladesDefaultPowerState()");

            GetAllBladesStateResponse responses = new GetAllBladesStateResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeStateResponseCollection = new List<BladeStateResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[maxbladeCount];

            for (int loop = 0; loop < maxbladeCount; loop++)
            {
                int bladeId = loop + 1;
                responses.bladeStateResponseCollection.Add(GetBladeDefaultPowerState(bladeId));

                // Set the internal blade response to the blade completion code.
                bladeInternalResponseCollection[loop] = responses.bladeStateResponseCollection[loop].completionCode;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Power On specified blade
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <returns>Blade Response, indicates success/failure.</returns>
        public BladeResponse SetPowerOn(int bladeId)
        {
            Tracer.WriteInfo("Received SetPowerOn(bladeId: {0})", bladeId);
            Tracer.WriteUserLog("Invoked SetPowerOn(bladeId: {0})", bladeId);

            BladeResponse response = new BladeResponse();
            byte maxbladeCount = (byte)ConfigLoaded.Population;
            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            // Check for correct blade id
            if (ChassisManagerUtil.CheckBladeId((byte)bladeId) == (byte)CompletionCode.InvalidBladeId)
            {
                Tracer.WriteWarning("Invalid blade Id {0}", bladeId);
                response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                response.statusDescription = Contracts.CompletionCode.ParameterOutOfRange.ToString();
                return response;
            }

            if (BladeSerialSessionMetadata.ApiGreenSignalling() != CompletionCode.Success)
            {
                Tracer.WriteError("SetPowerOn({0}) API: Failed to get green signal", bladeId);
                response.completionCode = Contracts.CompletionCode.SerialSessionActive;
                response.statusDescription = "Device busy";
                return response;
            }

            if (PowerOn(bladeId))
            {
                response.completionCode = Contracts.CompletionCode.Success;
                Tracer.WriteInfo("Successfully set power to ON for blade : " + bladeId);
            }
            else
            {
                Tracer.WriteError("Failed to set power to ON for blade : " + bladeId);
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
            }
            return response;
        }

        /// <summary>
        /// Power On all blades
        /// </summary>
        /// <returns>Array of blade responses, one for each blade. Indicates success/failure.</returns>
        public AllBladesResponse SetAllPowerOn()
        {
            byte maxBladeCount = (byte)ConfigLoaded.Population;

            AllBladesResponse responses = new AllBladesResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeResponseCollection = new List<BladeResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[maxBladeCount];

            Tracer.WriteInfo("Invoked SetAllPowerOn()");
            Tracer.WriteUserLog("Invoked SetAllPowerOn()");

            for (int loop = 0; loop < maxBladeCount; loop++)
            {
                int bladeId = loop + 1;
                responses.bladeResponseCollection.Add(SetPowerOn(bladeId));

                // Set the internal blade response to the blade completion code.
                bladeInternalResponseCollection[loop] = responses.bladeResponseCollection[loop].completionCode;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Internal operation to call both hard power on (soft power on is not exposed to the user)
        /// </summary>
        /// <param name="bladeId">Blade ID</param>
        /// <returns>True/false for success/failure</returns>
        private bool PowerOn(int bladeId)
        {
            Tracer.WriteInfo("Received poweron({0})", bladeId);
            bool powerOnStatus = false;

            BladePowerStatePacket bladePowerSwitchStatePacket = new BladePowerStatePacket();
            CompletionCode status;

            // Hard Power enable
            // Serialize setting of state and actual code logic 
            lock (ChassisState._lock[bladeId - 1])
            {

                BladePowerStatePacket currState = ChassisState.BladePower[bladeId - 1].GetBladePowerState();

                if (currState.CompletionCode != CompletionCode.Success
                    || (currState.BladePowerState == (byte)Contracts.PowerState.OFF))
                {
                    // No return here, because we still want to return a BMC state on the fall through,
                    // if Blade enable read fails for whatever reason
                    Tracer.WriteWarning("Sled Power Enable state read failed (Completion Code: {0:X})", currState.CompletionCode);

                    bladePowerSwitchStatePacket = ChassisState.BladePower[bladeId - 1].SetBladePowerState((byte)PowerState.ON);
                    status = bladePowerSwitchStatePacket.CompletionCode;
                    Tracer.WriteInfo("Hard poweron status " + status);

                    if (status == CompletionCode.Success)
                    {
                        // Hard power on status is true, so Blade should be set to Initialization state on success
                        Tracer.WriteInfo("State Transition for Sled {0}: {1} -> Initialization", bladeId,
                            ChassisState.GetStateName((byte)bladeId));

                        ChassisState.SetBladeState((byte)bladeId, (byte)BladeState.Initialization);
                        powerOnStatus = true;
                    }
                    else
                    {
                        Tracer.WriteWarning("Hard Power On failed for SledId {0} with code {1:X}", bladeId, status);
                    }
                }
                else
                {
                    powerOnStatus = true; // the blade was already powered on, so we dont power it on again
                }
            }
            return powerOnStatus;
        }

        /// <summary>
        /// Power Off specified blade
        /// </summary>
        /// <param name="bladeId">Blade ID (1-48)</param>
        /// <returns>Blade Response indicating success/failure</returns>
        public BladeResponse SetPowerOff(int bladeId)
        {
            BladeResponse response = new BladeResponse();
            byte maxbladeCount = (byte)ConfigLoaded.Population;

            Tracer.WriteInfo("Received SetPowerOff(bladeId: {0})", bladeId);
            Tracer.WriteUserLog("Invoked SetPowerOff(bladeId: {0})", bladeId);

            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            // Check for correct blade id
            if (ChassisManagerUtil.CheckBladeId((byte)bladeId) == (byte)CompletionCode.InvalidBladeId)
            {
                Tracer.WriteWarning("Invalid blade Id {0}", bladeId);
                response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                response.statusDescription = Contracts.CompletionCode.ParameterOutOfRange.ToString();
                return response;
            }

            if (BladeSerialSessionMetadata.ApiGreenSignalling() != CompletionCode.Success)
            {
                Tracer.WriteError("SetPowerOff({0}) API: Failed to get green signal", bladeId);
                response.completionCode = Contracts.CompletionCode.SerialSessionActive;
                response.statusDescription = "Device busy";
                return response;
            }
            if (PowerOff(bladeId))
            {
                response.completionCode = Contracts.CompletionCode.Success;
                Tracer.WriteInfo("Successfully set power to OFF for blade: " + bladeId);
            }
            else
            {
                Tracer.WriteError("Failed to set power to OFF for blade: " + bladeId);
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
            }
            return response;
        }

        /// <summary>
        /// Power OFF all blades
        /// </summary>
        /// <returns>Array of Blade Responses indicating success/failure</returns>
        public AllBladesResponse SetAllPowerOff()
        {
            byte maxBladeCount = (byte)ConfigLoaded.Population;

            AllBladesResponse responses = new AllBladesResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeResponseCollection = new List<BladeResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[maxBladeCount];

            Tracer.WriteUserLog("Invoked SetAllPowerOff()");

            for (int loop = 0; loop < maxBladeCount; loop++)
            {
                int bladeId = loop + 1;
                responses.bladeResponseCollection.Add(SetPowerOff(bladeId));

                // Set the internal blade response to the blade completion code.
                bladeInternalResponseCollection[loop] = responses.bladeResponseCollection[loop].completionCode;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Internal method to Power off blade
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <returns>true/false if operation was success/failure</returns>
        private bool PowerOff(int bladeId)
        {
            Tracer.WriteInfo("Received poweroff({0})", bladeId);
            bool powerOffStatus = false;

            BladePowerStatePacket bladePowerSwitchStatePacket = new BladePowerStatePacket();

            // Serialize power off and power on, on the same lock variable per blade, so we prevent inconsistent power state behavior
            lock (ChassisState._lock[bladeId - 1])
            {
                bladePowerSwitchStatePacket = ChassisState.BladePower[bladeId - 1].SetBladePowerState((byte)PowerState.OFF);
                CompletionCode status = bladePowerSwitchStatePacket.CompletionCode;

                // Sleep for specified amount of time after blade hard power off to prevent hardware inconsistent state 
                // - hot-swap controller not completely draining its capacitance leading to inconsistent power state issues
                Thread.Sleep(ConfigLoaded.WaitTimeAfterBladeHardPowerOffInMsecs);

                Tracer.WriteInfo("Return: {0}", status);

                if (status != CompletionCode.Success)
                {
                    Tracer.WriteError("blade Power Off Failed with Completion code {0:X}", status);
                    powerOffStatus = false;
                }
                else
                {
                    powerOffStatus = true;
                    // set state to Hard Power Off
                    Tracer.WriteInfo("State Transition for Sled {0}: {1} -> HardPowerOff", bladeId,
                        ChassisState.GetStateName((byte)bladeId));

                    ChassisState.SetBladeState((byte)bladeId, (byte)BladeState.HardPowerOff);
                    ChassisState.PowerFailCount[bladeId - 1] = 0;
                }
            }
            return powerOffStatus;
        }

        // Soft Blade Power On and Off Commands

        /// <summary>
        /// Set Soft Blade power ON
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <returns>Blade response indicating blade operation was success/failure</returns>
        public BladeResponse SetBladeOn(int bladeId)
        {
            Tracer.WriteInfo("Received SetBladeOn(bladeId: {0})", bladeId);
            Tracer.WriteUserLog("Invoked SetBladeOn(bladeId: {0})", bladeId);

            BladeResponse response = new BladeResponse();
            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("SetBladeOn", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            if (this.BladeOn(bladeId))
            {
                Tracer.WriteInfo("SetBladeOn({0}): Blade soft power set to ON", bladeId);
                response.completionCode = Contracts.CompletionCode.Success;
            }
            else
            {
                Tracer.WriteError("SetBladeOn({0}): Failed to set Blade soft power ON", bladeId);
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
            }

            return response;
        }

        /// <summary>
        /// Set soft blade power limit ON for all blades
        /// </summary>
        /// <returns>Array of blade responses indicating blade operation was success/failure</returns>
        public AllBladesResponse SetAllBladesOn()
        {
            byte MaxbladeCount = (byte)ConfigLoaded.Population;

            AllBladesResponse responses = new AllBladesResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeResponseCollection = new List<BladeResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[MaxbladeCount];

            Tracer.WriteInfo("Received SetAllBladesOn()");
            Tracer.WriteUserLog("Invoked SetAllBladesOn()");
            try
            {
                for (int index = 0; index < ConfigLoaded.Population; index++)
                {
                    int bladeId = index + 1;
                    responses.bladeResponseCollection.Add(SetBladeOn(bladeId));

                    // Set the internal blade response to the blade completion code.
                    bladeInternalResponseCollection[index] = responses.bladeResponseCollection[index].completionCode;
                }
            }
            catch (Exception ex)
            {
                Tracer.WriteError("SetAllBladesOn failed with exception" + ex);
                responses.completionCode = Contracts.CompletionCode.Failure;
                responses.statusDescription = Contracts.CompletionCode.Failure + ": " + ex.Message;
                return responses;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Set blade soft power OFF for specified blade
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <returns>Blade response indicating blade operation was success/failure</returns>
        public BladeResponse SetBladeOff(int bladeId)
        {
            BladeResponse response = new BladeResponse();
            response.bladeNumber = bladeId;

            Tracer.WriteInfo("Received SetBladeOff(bladeId: {0})", bladeId);
            Tracer.WriteUserLog("Invoked SetBladeOff(bladeId: {0})", bladeId);

            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("SetBladeOff", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            if (this.BladeOff(bladeId))
            {
                Tracer.WriteInfo("SetBladeOff({0}): Blade soft power set to OFF", bladeId);
                response.completionCode = Contracts.CompletionCode.Success;
            }
            else
            {
                Tracer.WriteError("SetBladeOff({0}): Failed to set Blade soft power to OFF", bladeId);
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
            }

            return response;
        }

        /// <summary>
        /// Set blade soft power OFF for all blades
        /// </summary>
        /// <returns>Array of blade responses indicating blade operation was success/failure</returns>
        public AllBladesResponse SetAllBladesOff()
        {
            byte MaxbladeCount = (byte)ConfigLoaded.Population;

            AllBladesResponse responses = new AllBladesResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeResponseCollection = new List<BladeResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[MaxbladeCount];

            Tracer.WriteInfo("Received SetAllBladesOff()");
            Tracer.WriteUserLog("Invoked SetAllBladesOff()");

            try
            {
                for (int index = 0; index < ConfigLoaded.Population; index++)
                {
                    int bladeId = index + 1;
                    responses.bladeResponseCollection.Add(SetBladeOff(bladeId));

                    // Set the internal blade response to the blade completion code.
                    bladeInternalResponseCollection[index] = responses.bladeResponseCollection[index].completionCode;
                }
            }
            catch (Exception ex)
            {
                responses.completionCode = Contracts.CompletionCode.Failure;
                responses.statusDescription = responses.completionCode.ToString() + ": " + ex.Message;
                Tracer.WriteError("SetAllBladesOff failed with exception" + ex);
                return responses;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// bladeOn - turns the blade on through IPMI, once BMC is powered on (Soft power on)
        /// </summary>
        /// <param name="bladeId"></param>
        /// <returns></returns>
        private bool BladeOn(int bladeId)
        {
            bool powerOnStatus = false;

            // Soft power enable
            byte softStatus = WcsBladeFacade.SetPowerState((byte)bladeId, Ipmi.IpmiPowerState.On);
            Tracer.WriteInfo("Soft poweron status " + softStatus);

            if (softStatus != (byte)CompletionCode.Success)
            {
                Tracer.WriteWarning("Blade Soft Power On Failed with Completion Code {0:X}", softStatus);
            }
            else
            {
                powerOnStatus = true;
            }
            return powerOnStatus;
        }

        /// <summary>
        /// BladeOff commands switches off blade through IPMI (soft blade off)
        /// </summary>
        /// <param name="bladeId"></param>
        /// <returns></returns>
        private bool BladeOff(int bladeId)
        {
            bool powerOffStatus = false;

            // Soft power enable
            byte softStatus = WcsBladeFacade.SetPowerState((byte)bladeId, Ipmi.IpmiPowerState.Off);
            Tracer.WriteInfo("Soft poweroff status " + softStatus);

            if (softStatus != (byte)CompletionCode.Success)
            {
                Tracer.WriteWarning("Blade Soft Power On Failed with Completion Code {0:X}", softStatus);
            }
            else
            {
                powerOffStatus = true;
            }
            return powerOffStatus;
        }

        /// <summary>
        /// Power cycle specified blade
        /// </summary>
        /// <param name="bladeId">if bladeId id -1, then bladeId not provided</param>
        /// <param name="offTime">time for which the blades will be powered off in seconds</param>
        /// <returns>Blade response indicating if blade operation was success/failure</returns>
        public BladeResponse SetBladeActivePowerCycle(int bladeId, uint offTime)
        {
            Tracer.WriteUserLog("Invoked SetBladeActivePowerCycle(bladeId: {0}, offTime: {1})", bladeId, offTime);

            BladeResponse response = new BladeResponse();
            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("SetBladeActivePowerCycle", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            if (PowerCycle(bladeId, offTime))
            {
                response.completionCode = Contracts.CompletionCode.Success;
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
            }

            return response;
        }

        /// <summary>
        /// Power cycle all blades
        /// </summary>
        /// <param name="offTime">time for which the blades will be powered off in seconds</param>
        /// <returns>Collection of Blade responses indicating if blade operation was success/failure</returns>
        public AllBladesResponse SetAllBladesActivePowerCycle(uint offTime)
        {
            byte maxbladeCount = (byte)ConfigLoaded.Population;

            AllBladesResponse responses = new AllBladesResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeResponseCollection = new List<BladeResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[maxbladeCount];

            Tracer.WriteUserLog(" Invoked SetAllBladesActivePowerCycle(offTime: {0})", offTime);

            for (int loop = 0; loop < maxbladeCount; loop++)
            {
                int bladeId = loop + 1;

                responses.bladeResponseCollection.Add(SetBladeActivePowerCycle(bladeId, offTime));

                // Set the internal blade response to the blade completion code.
                bladeInternalResponseCollection[loop] = responses.bladeResponseCollection[loop].completionCode;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Internal method to power cycle specified blade
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <param name="offTime">time for which the blades will be powered off in seconds</param>
        /// <returns>true/false indicating if blade operation was success/failure</returns>
        private bool PowerCycle(int bladeId, uint offTime)
        {
            Tracer.WriteInfo("Received powercycle({0},{1})", bladeId, offTime);
            bool powerStatus = false;

            bool intervalStatus = WcsBladeFacade.SetPowerCycleInterval((byte)bladeId, (byte)offTime);
            if (intervalStatus != true)
            {
                Tracer.WriteWarning("blade PowerCycle Interval setting failed with Completion code {0:X}", intervalStatus);
                return powerStatus;
            }

            byte status = WcsBladeFacade.SetPowerState((byte)bladeId, Ipmi.IpmiPowerState.Cycle);

            Tracer.WriteInfo("Return: {0}", status);

            if (status != 0)
            {
                Tracer.WriteWarning("blade PowerCycle Failed with Completion code {0:X}", status);
            }
            else
            {
                powerStatus = true;
            }
            return powerStatus;
        }

        /// <summary>
        /// Get power state of all blades
        /// </summary>
        /// <returns>Collection of Blade State response packets</returns>
        public GetAllPowerStateResponse GetAllPowerState()
        {
            byte MaxbladeCount = (byte)ConfigLoaded.Population;

            GetAllPowerStateResponse responses = new GetAllPowerStateResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.powerStateResponseCollection = new List<PowerStateResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[MaxbladeCount];
            uint bladeCount = MaxbladeCount;

            Tracer.WriteUserLog("Invoked GetAllPowerState()");
            Tracer.WriteInfo("Invoked GetAllPowerState()");

            for (int loop = 0; loop < bladeCount; loop++)
            {
                responses.powerStateResponseCollection.Add(GetPowerState(loop + 1));
                Tracer.WriteInfo("Blade power state: ", responses.powerStateResponseCollection[loop].powerState);

                // Set the internal blade response to the blade completion code.
                bladeInternalResponseCollection[loop] = responses.powerStateResponseCollection[loop].completionCode;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Get power state for specified blade
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <returns>Blade active power state response</returns>
        public PowerStateResponse GetPowerState(int bladeId)
        {
            Tracer.WriteInfo("Received GetPowerState({0})", bladeId);

            Tracer.WriteUserLog("Invoked GetPowerState(bladeid: {0})", bladeId);

            PowerStateResponse responsePowerState = new PowerStateResponse();
            responsePowerState.completionCode = Contracts.CompletionCode.Unknown;
            responsePowerState.statusDescription = String.Empty;
            responsePowerState.bladeNumber = bladeId;
            responsePowerState.powerState = Contracts.PowerState.NA;

            // Check for blade id
            if (ChassisManagerUtil.CheckBladeId((byte)bladeId) == (byte)CompletionCode.InvalidBladeId)
            {
                Tracer.WriteWarning("Invalid blade Id {0}", bladeId);
                responsePowerState.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                responsePowerState.statusDescription = Contracts.CompletionCode.ParameterOutOfRange.ToString();
                return responsePowerState;
            }

            if (BladeSerialSessionMetadata.ApiGreenSignalling() != CompletionCode.Success)
            {
                Tracer.WriteError("GetPowerState({0}) API: Failed to get green signal", bladeId);
                responsePowerState.completionCode = Contracts.CompletionCode.SerialSessionActive;
                responsePowerState.statusDescription = "Device busy";
                return responsePowerState;
            }

            // Get Power State
            BladePowerStatePacket response = ChassisState.BladePower[bladeId - 1].GetBladePowerState();

            if (response.CompletionCode != CompletionCode.Success)
            {
                Tracer.WriteError("Sled Power Enable state read failed (Completion Code: {0:X})", response.CompletionCode);
                responsePowerState.powerState = Contracts.PowerState.NA;
                responsePowerState.completionCode =
                    ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)response.CompletionCode);
                responsePowerState.statusDescription = responsePowerState.completionCode.ToString();
            }
            else
            {
                responsePowerState.completionCode = Contracts.CompletionCode.Success;
                if (response.BladePowerState == (byte)Contracts.PowerState.ON)
                {
                    responsePowerState.powerState = Contracts.PowerState.ON;
                    Tracer.WriteInfo("GetPowerState: Blade is receiving AC Outlet power");
                }
                else if (response.BladePowerState == (byte)Contracts.PowerState.OFF)
                {
                    responsePowerState.powerState = Contracts.PowerState.OFF;
                    Tracer.WriteInfo("GetPowerState: Blade is NOT receiving AC Outlet power");
                }
                else
                {
                    responsePowerState.powerState = Contracts.PowerState.NA;
                    Tracer.WriteWarning("GetPowerState: Unknown power state");
                }
            }

            return responsePowerState;
        }

        /// <summary>
        /// Get Blade soft power state
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <returns>Blade state response class object</returns>
        public BladeStateResponse GetBladeState(int bladeId)
        {
            Tracer.WriteInfo("Received GetBladeState({0})", bladeId);

            Tracer.WriteUserLog("Invoked GetBladeState(bladeid: {0})", bladeId);

            BladeStateResponse stateResponse = new BladeStateResponse();
            stateResponse.bladeNumber = bladeId;
            stateResponse.bladeState = Contracts.PowerState.NA;
            stateResponse.completionCode = Contracts.CompletionCode.Unknown;
            stateResponse.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("GetBladeState", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                stateResponse.completionCode = varResponse.completionCode;
                stateResponse.statusDescription = varResponse.statusDescription;
                return stateResponse;
            }

            // Check to see if the blade enable itself is OFF - then the BMC power state does not matter
            BladePowerStatePacket response = ChassisState.BladePower[bladeId - 1].GetBladePowerState();

            if (response.CompletionCode != CompletionCode.Success)
            {
                // No return here, because we still want to return a BMC state on the fall through,
                // if Blade enable read fails for whatever reason
                Tracer.WriteWarning("Sled Power Enable state read failed (Completion Code: {0:X})", response.CompletionCode);
                stateResponse.completionCode =
                    ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)response.CompletionCode);
                stateResponse.statusDescription = stateResponse.completionCode.ToString();
            }
            else
            {
                // Only if the blade enable is OFF, we return that status, for anything else we have to read BMC status
                if (response.BladePowerState == (byte)Contracts.PowerState.OFF)
                {
                    // Since we do not know if a blade is present in that slot or not, we return NA as power state
                    // TODO: This was supposed to return OFF status, and not NA
                    stateResponse.bladeState = Contracts.PowerState.NA;

                    stateResponse.completionCode = Contracts.CompletionCode.Success;

                    return stateResponse;
                }
            }

            Tracer.WriteInfo("Calling WcsBladeClient");
            Ipmi.SystemStatus powerState = WcsBladeFacade.GetChassisState((byte)bladeId);

            Tracer.WriteInfo("Return: {0}, Blade State: {1}", powerState.CompletionCode, powerState.PowerState.ToString());

            if (powerState.CompletionCode != 0)
            {
                Tracer.WriteError("GetBladeState Failed with Completion code {0:X}", powerState.CompletionCode);
                stateResponse.bladeState = Contracts.PowerState.NA;
                stateResponse.completionCode =
                    ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)powerState.CompletionCode);
                stateResponse.statusDescription = stateResponse.completionCode.ToString();
            }
            else
            {
                stateResponse.completionCode = Contracts.CompletionCode.Success;
                if (powerState.PowerState == Ipmi.IpmiPowerState.On)
                {
                    stateResponse.bladeState = Contracts.PowerState.ON;
                }
                else
                {
                    stateResponse.bladeState = Contracts.PowerState.OFF;
                }
            }
            return stateResponse;
        }

        /// <summary>
        /// Get all blades soft power state
        /// </summary>
        /// <returns>Blade state response class object</returns>
        public GetAllBladesStateResponse GetAllBladesState()
        {
            byte MaxbladeCount = (byte)ConfigLoaded.Population;

            GetAllBladesStateResponse responses = new GetAllBladesStateResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeStateResponseCollection = new List<BladeStateResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[MaxbladeCount];
            uint bladeCount = MaxbladeCount;

            Tracer.WriteUserLog("Invoked GetAllBladesState()");
            Tracer.WriteInfo("Invoked GetAllBladesState()");

            for (int loop = 0; loop < bladeCount; loop++)
            {
                responses.bladeStateResponseCollection.Add(GetBladeState(loop + 1));
                Tracer.WriteInfo("Blade state: ", responses.bladeStateResponseCollection[loop].bladeState);

                // Set the internal blade response to the blade completion code.
                bladeInternalResponseCollection[loop] = responses.bladeStateResponseCollection[loop].completionCode;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Turn on AC socket within the chassis 
        /// </summary>
        /// <param name="portNo">Port no corresponding to the AC sockets internal to the chassis like TOR switches</param>
        /// <returns>Chassis Response success/failure</returns>
        public Contracts.ChassisResponse SetACSocketPowerStateOn(uint portNo)
        {
            Tracer.WriteInfo("Received SetACSocketPowerStateOn({0})", portNo);

            Tracer.WriteUserLog("Invoked SetACSocketPowerStateOn(portNo: {0})", portNo);

            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            AcSocket acSocket = ChassisState.AcPowerSockets[portNo - 1]; 
            byte status = acSocket.turnOnAcSocket();

            Tracer.WriteInfo("Return: {0}", status);

            if (status != 0)
            {
                Tracer.WriteWarning("blade AC Socket Turn On Failed with Completion code {0:X}", status);
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = String.Format("blade AC Socket Turn On Failed with Completion code {0:X}", status);
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.Success;
            }
            return response;
        }

        /// <summary>
        /// Turn off AC socket within the chassis 
        /// </summary>
        /// <param name="portNo">Port no corresponding to the AC sockets internal to the chassis like TOR switches</param>
        /// <returns>Chassis Response success/failure.</returns>
        public Contracts.ChassisResponse SetACSocketPowerStateOff(uint portNo)
        {
            Tracer.WriteInfo("Received SetACSocketPowerStateOff({0})", portNo);

            Tracer.WriteUserLog("Invoked SetACSocketPowerStateOff(portNo: {0})", portNo);

            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            AcSocket acSocket = ChassisState.AcPowerSockets[portNo - 1];
            byte status = acSocket.turnOffAcSocket();

            Tracer.WriteInfo("Return: {0}", status);

            if (status != 0)
            {
                Tracer.WriteWarning("blade AC Socket Turn Off Failed with Completion code {0:X}", status);
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = String.Format("blade AC Socket Turn Off Failed with Completion code {0:X}", status);
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.Success;
            }
            return response;
        }

        /// <summary>
        /// Get power status of AC socket within the chassis 
        /// </summary>
        /// <param name="portNo">Port no corresponding to the AC sockets internal to the chassis like TOR switches</param>
        /// <returns>AC Socket power state.</returns>
        public ACSocketStateResponse GetACSocketPowerState(uint portNo)
        {
            byte MaxbladeCount = (byte)ConfigLoaded.Population;

            Tracer.WriteInfo("Received GetACSocketPowerState({0})", portNo);

            Tracer.WriteUserLog("Invoked GetACSocketPowerState(portNo: {0})", portNo);

            ACSocketStateResponse response = new ACSocketStateResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;
            response.portNo = portNo;
            response.powerState = new PowerState();

            AcSocket acSocket = ChassisState.AcPowerSockets[portNo - 1];
            PowerState status = acSocket.getAcSocketStatus();

            Tracer.WriteInfo("Return: {0}", status);

            if (status == PowerState.NA)
            {
                Tracer.WriteError("blade AC Socket Get Status Failed with Completion code {0:X}", status);
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = String.Format("blade AC Socket Get Status Failed with Completion code {0:X}", status);
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.Success;
                response.powerState = status;
            }
            return response;
        }

        /// <summary>
        /// Read Chassis Log
        /// </summary>
        /// <returns>returns logPacket structure poluated. If null then failure</returns>
        public ChassisLogResponse ReadChassisLog()
        {
            ChassisLogResponse response = new ChassisLogResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;
            Tracer.WriteUserLog("Invoked ReadChassisLog()");
            try
            {
                if (Tracer.GetCurrentUserLogFilePath() != null)
                {
                    // Open a filestream to read the user log
                    FileStream fileStream = new FileStream(
                        Tracer.GetCurrentUserLogFilePath(), FileMode.Open,
                        FileAccess.Read, FileShare.ReadWrite);

                    List<string> entries = new List<string>();

                    //Read each log entry
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.ConformanceLevel = ConformanceLevel.Fragment;
                    XmlReader reader = XmlReader.Create(fileStream, settings);
                    
                    const int MAX_ENTRIES = 500;
                    int traceEventCount = 0;
                    
                    while (reader.Read() && traceEventCount < MAX_ENTRIES)
                    {
                        try
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                if (reader.Name == "E2ETraceEvent")
                                {
                                    // Read each sub-entry in E2ETraceEvent
                                    XmlReader subReader = reader.ReadSubtree();
                                    while (subReader.Read() && traceEventCount < MAX_ENTRIES)
                                    {
                                        reader.ReadToFollowing("ApplicationData");
                                        if (reader.Read())
                                        {
                                            entries.Add(reader.Value.Trim());
                                        }
                                    }
                                }
                            }
                        }
                        catch (System.Xml.XmlException xm)
                        {
                            Tracer.WriteInfo("ReadChassisLog XML exception - ignoring it: " + xm.Message);
                        }
                    }
                    
                    // dispose the filestream. dispose internally calls Close().
                    fileStream.Dispose();

                    response.logEntries = new List<LogEntry>();

                    // For each entry get the timestamp and description
                    int loopCount = 0;
                    foreach (string entry in entries)
                    {
                        System.DateTime timeStamp = new System.DateTime();
                        string[] tokens = entry.Split(new char[] { ',' });

                        if (DateTime.TryParse(tokens[0], out timeStamp))
                        {
                            LogEntry lg = new LogEntry();
                            lg.eventTime = timeStamp;
                            lg.eventDescription = entry.Replace(tokens[0] + ",", "");
                            response.logEntries.Add(lg);
                        }

                        loopCount++;
                    }

                    response.completionCode = Contracts.CompletionCode.Success;
                }
                else
                {
                    Tracer.WriteError("ReadChassisLog: Unable to get current trace file path");
                    response.completionCode = Contracts.CompletionCode.Failure;
                    response.statusDescription = "ReadChassisLog: Unable to get current trace file path";
                }
            }
            catch (Exception e)
            {
                Tracer.WriteError(e.Message);
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = String.Format("ReadChassisLog failed with exception:{0} ", e.Message);
            }

            return response;

        }

        /// <summary>
        /// ***TODO M2*** Read chassis log with Timestamp
        /// </summary>
        /// <param name="start">Start Timestamp</param>
        /// <param name="end">End Timestamp</param>
        /// <returns>Chassis Log</returns>
        public ChassisLogResponse ReadChassisLogWithTimestamp(DateTime start, DateTime end)
        {
            ChassisLogResponse response = new ChassisLogResponse();
            ChassisLogResponse filteredResponse = new ChassisLogResponse();
            filteredResponse.completionCode = Contracts.CompletionCode.Unknown;
            filteredResponse.statusDescription = String.Empty;
            response = ReadChassisLog();
            filteredResponse.logEntries = new List<LogEntry>();

            for (int i = 0; i < response.logEntries.Count(); i++)
            {
                if (response.logEntries[i].eventTime >= start && response.logEntries[i].eventTime <= end)
                {
                    filteredResponse.logEntries.Add(response.logEntries[i]);
                }
            }

            // Get the completion code & status description from response object.
            filteredResponse.completionCode = response.completionCode;
            filteredResponse.statusDescription = response.statusDescription;
            return filteredResponse;
        }

        /// <summary>
        /// ***TODO M2*** Clear chassis log
        /// </summary>
        /// <returns>1 indicates success. 0 indicates failure.</returns>
        public Contracts.ChassisResponse ClearChassisLog()
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            string filePath = ConfigLoaded.UserLogFilePath;
            Tracer.WriteUserLog("Invoked ClearChassisLog()");

            // Initialize to failure to start with
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            try
            {
                if (Tracer.ClearUserLog())
                {
                    response.completionCode = Contracts.CompletionCode.Success;
                    Tracer.WriteInfo("Cleared chassis log");
                }
                else
                {
                    response.completionCode = Contracts.CompletionCode.Failure;
                    response.statusDescription = "Failed to clear chassis log";
                    Tracer.WriteError("Failed to clear chassis log");
                }
            }
            catch (IOException ex)
            {
                Tracer.WriteError("ClearChassisLog Error " + ex.Message);
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = String.Format("ClearChassisLog Error " + ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Read Blade Log for specified blade with timestamp
        /// TO DO M2 - Modify to include timestamp
        /// </summary>
        /// <param name="bladeId">Blade ID</param>
        /// <param name="logType">log type</param>
        /// <param name="start">Start Timestamp</param>
        /// <param name="end">End Timestamp</param>
        /// <returns>Blade log for specified blade</returns>
        public ChassisLogResponse ReadBladeLogWithTimestamp(int bladeId, DateTime start, DateTime end)
        {
            Tracer.WriteUserLog("Invoked ReadBladeLogWithTimestamp(bladeId: {0}, StartTimestamp: {1}, EndTimestamp: {2}", bladeId, start, end);

            ChassisLogResponse response = new ChassisLogResponse();
            ChassisLogResponse filteredResponse = new ChassisLogResponse();
            filteredResponse.logEntries = new List<LogEntry>();
            filteredResponse.completionCode = Contracts.CompletionCode.Unknown;
            filteredResponse.statusDescription = String.Empty;

            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("ReadBladeLogWithTimestamp", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            response = ReadBladeLog(bladeId);
            try
            {
                if (response.completionCode == Contracts.CompletionCode.Success)
                {
                    for (int i = 0; i < response.logEntries.Count(); i++)
                    {
                        if (response.logEntries[i].eventTime >= start && response.logEntries[i].eventTime <= end)
                        {
                            filteredResponse.logEntries.Add(response.logEntries[i]);
                        }
                    }
                    filteredResponse.completionCode = Contracts.CompletionCode.Success;
                }
                else
                {
                    filteredResponse.completionCode = Contracts.CompletionCode.Failure;
                    filteredResponse.statusDescription = Contracts.CompletionCode.Failure.ToString();
                }
            }
            catch (Exception ex)
            {
                Tracer.WriteError("ReadLogWithTimestamp failed for BladeID :{0}, with the exception {1}", bladeId, ex.Message);
                filteredResponse.completionCode = Contracts.CompletionCode.Failure;
                filteredResponse.statusDescription = Contracts.CompletionCode.Failure.ToString() + ": " + ex.Message;
            }
            return filteredResponse;
        }

        /// <summary>
        /// Read blade log, for given blade number
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <returns>returns logPacket structure poluated. If null then failure</returns>
        public ChassisLogResponse ReadBladeLog(int bladeId)
        {
            Tracer.WriteInfo("Received Readbladelog({0})", bladeId);
            Tracer.WriteUserLog("Invoked ReadBladelog(bladeId: {0})", bladeId);

            uint noEntries = 100;

            ChassisLogResponse selLog = new ChassisLogResponse();
            selLog.logEntries = new List<LogEntry>();
            selLog.completionCode = Contracts.CompletionCode.Unknown;
            selLog.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("ReadBladeLog", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                selLog.completionCode = varResponse.completionCode;
                selLog.statusDescription = varResponse.statusDescription;
                return selLog;
            }

            Ipmi.SystemEventLog selRecord = WcsBladeFacade.GetSel((byte)bladeId);

            // Code added to write output to the console window.
            Tracer.WriteInfo("Blade {0} ReadBladeLog Return: {1}", bladeId, selRecord.CompletionCode);

            if (selRecord.CompletionCode != 0)
            {
                Tracer.WriteWarning("blade Get SEL Failed with Completion code {0:X}", selRecord.CompletionCode);
                selLog.completionCode = ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)selRecord.CompletionCode);
                selLog.statusDescription = selLog.completionCode.ToString();
            }
            else
            {
                try
                {
                    string spacer = ConfigLoaded.EventLogStrSpacer +
                                    ConfigLoaded.EventLogStrSeparator +
                                    ConfigLoaded.EventLogStrSpacer;

                    foreach (Ipmi.SystemEventLogMessage eventLog in selRecord.EventLog)
                    {
                        if (selLog.logEntries.Count() >= noEntries)
                        {
                            break;
                        }

                        EventLogData eventData = ExtractEventMessage(eventLog);

                        // Get sensor details.
                        string sensor = string.Format(ConfigLoaded.EventLogStrSensor,
                                         eventLog.SensorType.ToString(),
                                         WcsBladeFacade.GetSensorDescription((byte)bladeId, (byte)eventLog.SensorNumber),
                                         eventLog.SensorNumber);

                        // Get Event Error Message
                        string error = string.Format(ConfigLoaded.EventLogStrError, eventLog.EventPayload);

                        LogEntry logEntry = new LogEntry();
                        logEntry.eventTime = eventLog.EventDate;
                        logEntry.eventDescription = (eventLog.EventDir.ToString() +
                                                    spacer +
                                                    eventData.Description +
                                                    spacer +
                                                    eventData.EventMessage +
                                                    spacer +
                                                    sensor +
                                                    spacer +
                                                    error);
                       

                        selLog.logEntries.Add(logEntry);
                    }

                    selLog.completionCode = Contracts.CompletionCode.Success;
                    Tracer.WriteInfo("Readbladelog returned " + selLog.logEntries.Count() + " entries.");
                }
                catch (Exception ex)
                {
                    Tracer.WriteError("ReadBladeLog failed with exception: " + ex);
                    selLog.completionCode = Contracts.CompletionCode.Failure;
                    selLog.statusDescription = selLog.completionCode.ToString() + ": " + ex.Message;
                }
            }
            return selLog;
        }

        private string GetSensorDescription(byte sensorType, byte sensorNumber)
        {
            return string.Empty;
        }

        private EventLogData ExtractEventMessage(Ipmi.SystemEventLogMessage eventLog)
        {
            Ipmi.EventLogMsgType classification = eventLog.EventMessage.MessageType;

            string spacer = ConfigLoaded.EventLogStrSpacer +
                            ConfigLoaded.EventLogStrSeparator +
                            ConfigLoaded.EventLogStrSpacer;

            switch (classification)
            {
                case Microsoft.GFS.WCS.ChassisManager.Ipmi.EventLogMsgType.Threshold:
                    {
                        Ipmi.ThresholdEvent log = (Ipmi.ThresholdEvent)eventLog.EventMessage;

                        EventLogData logData = ConfigLoaded.GetEventLogData(classification, log.EventTypeCode, log.ReadingOffset);

                        logData.EventMessage = string.Format(logData.EventMessage, log.TriggerReading, log.TriggerThreshold);

                        return logData;
                    }
                case Microsoft.GFS.WCS.ChassisManager.Ipmi.EventLogMsgType.Discrete:
                    {
                        Ipmi.DiscreteEvent log = (Ipmi.DiscreteEvent)eventLog.EventMessage;

                        EventLogData logData = ConfigLoaded.GetEventLogData(classification, log.EventTypeCode, log.ReadingOffset);

                        logData.EventMessage = string.Format(logData.EventMessage, log.EventPayload[1], log.EventPayload[2]);

                        return logData;
                    }
                case Microsoft.GFS.WCS.ChassisManager.Ipmi.EventLogMsgType.SensorSpecific:
                    {
                        Ipmi.DiscreteEvent log = (Ipmi.DiscreteEvent)eventLog.EventMessage;

                        // Sensor Specific Event Types use the SensorType for indexing the TypeCode.
                        EventLogData logData = ConfigLoaded.GetEventLogData(classification, log.SensorType, 
                            log.ReadingOffset);

                        // create exceptions to logging for DIMM number lookup, as opposed to reporting DIMM index.
                        if (log.SensorType == 12 || (log.SensorType == 16 && log.ReadingOffset == 0))
                        {
                            // dimm number is unknown at first.
                            string dimmNumber = ConfigLoaded.Unknown;

                            byte dimm;

                            // get wcs dimm number.
                            if(log.SensorType == 12)
                                dimm = ConfigLoaded.GetDimmNumber(log.EventPayload[2]);
                            else
                                dimm = ConfigLoaded.GetDimmNumber(log.EventPayload[1]);
                            
                            if(dimm != 0xff)
                                dimmNumber = string.Format("{0:X}", dimm);

                            logData.EventMessage = string.Format(logData.EventMessage, dimmNumber);
                        }
                        else if (eventLog.SensorType == Ipmi.SensorType.CriticalInterrupt && (log.ReadingOffset == 0x07 || log.ReadingOffset == 0x08 || log.ReadingOffset == 0x0A))
                        {
                            // Correctable, uncorrectable and fatal bus errors
                            logData.EventMessage = string.Format(logData.EventMessage, (byte)(((byte)log.EventPayload[1] >> 3) & 0x1F), 
                                                                                       ((byte)log.EventPayload[1] & 0x07), log.EventPayload[2]);
                        }
                        else
                        {
                            logData.EventMessage = string.Format(logData.EventMessage, log.EventPayload[1],
                                log.EventPayload[2]);
                        }

                        string extension = logData.GetExtension(log.EvtByte2Reading);
                        
                        if (extension != string.Empty)
                        {
                            logData.EventMessage = (logData.EventMessage +
                                               spacer +
                                               extension);
                        }

                        return logData;
                    }
                case Microsoft.GFS.WCS.ChassisManager.Ipmi.EventLogMsgType.Oem:
                    {
                        Ipmi.OemEvent log = (Ipmi.OemEvent)eventLog.EventMessage;

                        EventLogData logData = ConfigLoaded.GetEventLogData(classification, 0, 0);

                        return logData;
                    }
                case Microsoft.GFS.WCS.ChassisManager.Ipmi.EventLogMsgType.OemTimestamped:
                    {
                        Ipmi.OemTimeStampedEvent log = (Ipmi.OemTimeStampedEvent)eventLog.EventMessage;

                        EventLogData logData = ConfigLoaded.GetEventLogData(classification, 0, 0);
                        
                        // Format OEM Timestamped SEL Record
                        logData.EventMessage = string.Format(logData.EventMessage, string.Format("0x{0:X}", log.ManufacturerID), 
                            Ipmi.IpmiSharedFunc.ByteArrayToHexString(log.OemDefined));

                        return logData;
                    }
                case Microsoft.GFS.WCS.ChassisManager.Ipmi.EventLogMsgType.OemNonTimeStamped:
                    {
                        Ipmi.OemNonTimeStampedEvent log = (Ipmi.OemNonTimeStampedEvent)eventLog.EventMessage;

                        EventLogData logData = ConfigLoaded.GetEventLogData(classification, 0, 0);

                        // Format OEM Non-timestamped SEL Record
                        logData.EventMessage = string.Format(logData.EventMessage, Ipmi.IpmiSharedFunc.ByteArrayToHexString(log.OemDefined));
                        
                        return logData;
                    }
                default:
                    {
                        Ipmi.UnknownEvent log = (Ipmi.UnknownEvent)eventLog.EventMessage;

                        EventLogData logData = ConfigLoaded.GetEventLogData(classification, 0, 0);

                        return logData;
                    }
            }
        }

        /// <summary>
        /// Clear blade log
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <returns>Blade response indicating clear log operation was success/failure</returns>
        public BladeResponse ClearBladeLog(int bladeId)
        {
            Tracer.WriteUserLog(" Invoked ClearBladeLog(bladeID: {0})", bladeId);

            byte MaxbladeCount = (byte)ConfigLoaded.Population;
            Tracer.WriteInfo("Received clearbladelog({0})", bladeId);

            BladeResponse response = new BladeResponse();
            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("ClearBladeLog", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            bool status = WcsBladeFacade.ClearSel((byte)bladeId);
            if (status != true)
            {
                Tracer.WriteWarning("Clear SEL log failed");
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
                return response;
            }
            response.completionCode = Contracts.CompletionCode.Success;
            return response;
        }

        /// <summary>
        /// Get blade power reading for specified blade
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <returns>Blade response containing the power reading</returns>
        public BladePowerReadingResponse GetBladePowerReading(int bladeId)
        {
            Tracer.WriteInfo("Invoked GetBladePowerReading(bladeId: {0})", bladeId);
            Tracer.WriteUserLog("Invoked GetBladePowerReading(bladeId: {0})", bladeId);

            BladePowerReadingResponse response = new BladePowerReadingResponse();
            response.bladeNumber = bladeId;
            response.powerReading = -1;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("GetBladePowerReading", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            List<Ipmi.PowerReading> myPowerReading = new List<Ipmi.PowerReading>();
            myPowerReading = WcsBladeFacade.GetPowerReading((byte)bladeId);

            if (myPowerReading == null || myPowerReading.Count == 0 || myPowerReading[0].CompletionCode != 0 || myPowerReading[0].PowerSupport == false)
            {
                Tracer.WriteError("GetPowerReading:(" + bladeId + ") Error reading power ");
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
                return response;
            }
            response.powerReading = myPowerReading[0].Present;
            response.completionCode = Contracts.CompletionCode.Success;
            Tracer.WriteInfo("GetPowerReading:(" + bladeId + ") Avg " + myPowerReading[0].Average + " Curr " + myPowerReading[0].Present + " Support " + myPowerReading[0].PowerSupport);

            return response;
        }

        /// <summary>
        /// Get power reading for all blades
        /// </summary>
        /// <returns>Array of blade responses containing the power reading</returns>
        public GetAllBladesPowerReadingResponse GetAllBladesPowerReading()
        {
            byte MaxbladeCount = (byte)ConfigLoaded.Population;
            GetAllBladesPowerReadingResponse responses = new GetAllBladesPowerReadingResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladePowerReadingCollection = new List<BladePowerReadingResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[MaxbladeCount];

            Tracer.WriteUserLog("Invoked GetAllBladesPowerReading()");

            try
            {
                for (int index = 0; index < ConfigLoaded.Population; index++)
                {
                    int bladeId = index + 1;
                    responses.bladePowerReadingCollection.Add(GetBladePowerReading((int)bladeId));

                    // Set the internal blade response to the blade completion code.
                    bladeInternalResponseCollection[index] = responses.bladePowerReadingCollection[index].completionCode;
                }
            }
            catch (Exception ex)
            {
                responses.completionCode = Contracts.CompletionCode.Failure;
                responses.statusDescription = responses.completionCode.ToString() + ": " + ex.Message;
                Tracer.WriteError("GetAllBladesPowerReading Exception" + ex);
                return responses;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Get blade power limit for specified blade
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <returns>Blade response containing the blade power limit value</returns>
        public BladePowerLimitResponse GetBladePowerLimit(int bladeId)
        {
            Tracer.WriteUserLog("Invoked GetBladePowerLimit(bladeId : {0})", bladeId);

            BladePowerLimitResponse response = new BladePowerLimitResponse();
            response.bladeNumber = bladeId;
            response.powerLimit = -1;

            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("GetBladePowerLimit", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            Ipmi.PowerLimit myPowerLimit = WcsBladeFacade.GetPowerLimit((byte)(bladeId));
            Tracer.WriteInfo("GetPowerLimit(" + bladeId + "):" + " CC: " + myPowerLimit.CompletionCode + " LimitVal: " + myPowerLimit.LimitValue + " Active: " + myPowerLimit.ActiveLimit);
            if (myPowerLimit.CompletionCode != 0)
            {
                Tracer.WriteError("GetBladePowerLimit failed for blade({0}) with CompletionCode({1})", bladeId, myPowerLimit.CompletionCode);
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
                return response;
            }
            response.powerLimit = (double)myPowerLimit.LimitValue;
            response.completionCode = Contracts.CompletionCode.Success;

            return response;
        }

        /// <summary>
        /// Get power limit value for all blades
        /// </summary>
        /// <returns>Array of blade responses containing the blade power limit values</returns>
        public GetAllBladesPowerLimitResponse GetAllBladesPowerLimit()
        {
            byte MaxbladeCount = (byte)ConfigLoaded.Population;

            GetAllBladesPowerLimitResponse responses = new GetAllBladesPowerLimitResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladePowerLimitCollection = new List<BladePowerLimitResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[MaxbladeCount];

            responses.bladePowerLimitCollection = new List<BladePowerLimitResponse>();

            Tracer.WriteUserLog("Invoked GetAllBladesPowerLimit()");

            try
            {
                for (int index = 0; index < ConfigLoaded.Population; index++)
                {
                    int bladeId = index + 1;
                    responses.bladePowerLimitCollection.Add(GetBladePowerLimit(bladeId));

                    // Set the internal blade response to the blade completion code.
                    bladeInternalResponseCollection[index] = responses.bladePowerLimitCollection[index].completionCode;
                }
            }
            catch (Exception ex)
            {
                responses.completionCode = Contracts.CompletionCode.Failure;
                responses.statusDescription = responses.completionCode.ToString() + ": " + ex.Message;
                Tracer.WriteError("GetAllBladesPowerLimit Exception");
                return responses;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Set blade power limit to given value for the specified blade
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <param name="powerLimitInWatts">Power limit to set</param>
        /// <returns>Blade response indicating blade operation was success/failure</returns>
        public BladeResponse SetBladePowerLimit(int bladeId, double powerLimitInWatts)
        {
            BladeResponse response = new BladeResponse();
            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("SetBladePowerLimit", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            if (ChassisManagerUtil.CheckBladePowerLimit(powerLimitInWatts))
            {
                Tracer.WriteUserLog("Invoked SetBladePowerLimit( bladeId: {0}, powerLimitInWatts: {1})", bladeId, powerLimitInWatts);

                // TODO: Remove hard-coded correction-time, action and sampling period and move it to config
                // TODO: Change this to get correction-time and sampling period from getpowerlimit
                // Note: Action is set to 0 (do nothing). Setting action to 1 will cause blade shutdown upon power limit violation
                // Note: 6 sec correction time and 1 sec sampling period is the minimum time period that works
                Ipmi.ActivePowerLimit myActiveLimit = WcsBladeFacade.SetPowerLimit((byte)(bladeId), (short)powerLimitInWatts, 6000, 0, 1);
                Tracer.WriteInfo("SetPowerLimit({0}): Set {1} CC {2}", bladeId, myActiveLimit.LimitSet, myActiveLimit.CompletionCode);

                if (myActiveLimit.CompletionCode == 0)
                {
                    response.completionCode = Contracts.CompletionCode.Success;
                }
                else
                {
                    response.completionCode = Contracts.CompletionCode.Failure;
                    response.statusDescription = response.completionCode.ToString();
                }
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                response.statusDescription = response.completionCode.ToString();
            }

            return response;
        }

        /// <summary>
        /// Set all blades power limit to the given value
        /// </summary>
        /// <param name="powerLimitInWatts">Power limit to set</param>
        /// <returns>Array of blade responses indicating blade operation was success/failure</returns>
        public AllBladesResponse SetAllBladesPowerLimit(double powerLimitInWatts)
        {
            byte MaxbladeCount = (byte)ConfigLoaded.Population;

            AllBladesResponse responses = new AllBladesResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeResponseCollection = new List<BladeResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[MaxbladeCount];

            Tracer.WriteUserLog("Invoked SetAllBladesPowerLimit(powerLimitInWatts: {0})", powerLimitInWatts);
            try
            {
                for (int index = 0; index < ConfigLoaded.Population; index++)
                {
                    responses.bladeResponseCollection.Add(SetBladePowerLimit((int)(index + 1), powerLimitInWatts));

                    // Set the internal blade response to the blade completion code.
                    bladeInternalResponseCollection[index] = responses.bladeResponseCollection[index].completionCode;
                }
            }
            catch (Exception ex)
            {
                responses.completionCode = Contracts.CompletionCode.Failure;
                responses.statusDescription = responses.completionCode.ToString() + ": " + ex.Message;
                Tracer.WriteError("SetAllBladesPowerLimit Exception" + ex.Message);
                return responses;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Set power limit ON for specified blade
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <returns>Blade response indicating blade operation was success/failure</returns>
        public BladeResponse SetBladePowerLimitOn(int bladeId)
        {
            BladeResponse response = new BladeResponse();
            response.bladeNumber = bladeId;

            Tracer.WriteUserLog("Invoked SetBladePowerLimitOn(bladeId: {0})", bladeId);

            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("SetBladePowerLimitOn", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            if (!WcsBladeFacade.ActivatePowerLimit((byte)(bladeId), true))
            {
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
                return response;
            }
            Tracer.WriteInfo("ActivatePowerLimit({0}): Activated", bladeId);
            response.completionCode = Contracts.CompletionCode.Success;

            return response;
        }

        /// <summary>
        /// Set active power limit ON for all blades
        /// </summary>
        /// <returns>Array of blade responses indicating blade operation was success/failure</returns>
        public AllBladesResponse SetAllBladesPowerLimitOn()
        {
            byte MaxbladeCount = (byte)ConfigLoaded.Population;

            AllBladesResponse responses = new AllBladesResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeResponseCollection = new List<BladeResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[MaxbladeCount];

            Tracer.WriteUserLog("Invoked SetAllBladesPowerLimitOn()");

            try
            {
                for (int index = 0; index < ConfigLoaded.Population; index++)
                {
                    int bladeId = index + 1;
                    responses.bladeResponseCollection.Add(SetBladePowerLimitOn(bladeId));

                    // Set the internal blade response to the blade completion code.
                    bladeInternalResponseCollection[index] = responses.bladeResponseCollection[index].completionCode;
                }
            }
            catch (Exception ex)
            {
                responses.completionCode = Contracts.CompletionCode.Failure;
                responses.statusDescription = responses.completionCode.ToString() + ": " + ex.Message;
                Tracer.WriteError("SetAllBladesPowerLimitOn Exception");
                return responses;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        /// Set power limit OFF for specified blade
        /// </summary>
        /// <param name="bladeId">Blade ID(1-48)</param>
        /// <returns>Blade response indicating blade operation was success/failure</returns>
        public BladeResponse SetBladePowerLimitOff(int bladeId)
        {
            BladeResponse response = new BladeResponse();

            Tracer.WriteUserLog("Invoked SetBladePowerLimitOff(bladeId: {0})", bladeId);

            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("SetBladePowerLimitOff", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            if (!WcsBladeFacade.ActivatePowerLimit((byte)(bladeId), false))
            {
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString();
                return response;
            }
            Tracer.WriteInfo("ActivatePowerLimit({0}): Deactivated", bladeId);
            response.completionCode = Contracts.CompletionCode.Success;

            return response;
        }

        /// <summary>
        /// Set power limit OFF for all blades
        /// </summary>
        /// <returns>Array of blade responses indicating blade operation was success/failure</returns>
        public AllBladesResponse SetAllBladesPowerLimitOff()
        {
            byte MaxbladeCount = (byte)ConfigLoaded.Population;

            AllBladesResponse responses = new AllBladesResponse();
            responses.completionCode = Contracts.CompletionCode.Unknown;
            responses.statusDescription = string.Empty;
            responses.bladeResponseCollection = new List<BladeResponse>();
            Contracts.CompletionCode[] bladeInternalResponseCollection = new Contracts.CompletionCode[MaxbladeCount];

            Tracer.WriteUserLog("Invoked SetAllBladesPowerLimitOff()");

            try
            {
                for (int index = 0; index < ConfigLoaded.Population; index++)
                {
                    int bladeId = index + 1;
                    responses.bladeResponseCollection.Add(SetBladePowerLimitOff((int)bladeId));

                    // Set the internal blade response to the blade completion code.
                    bladeInternalResponseCollection[index] = responses.bladeResponseCollection[index].completionCode;
                }
            }
            catch (Exception ex)
            {
                responses.completionCode = Contracts.CompletionCode.Failure;
                responses.statusDescription = responses.completionCode.ToString() + ": " + ex.Message;
                Tracer.WriteError("SetAllBladesPowerLimitOff Exception");
                return responses;
            }

            Contracts.ChassisResponse varResponse = new Contracts.ChassisResponse();
            varResponse = ChassisManagerUtil.ValidateAllBladeResponse(bladeInternalResponseCollection);
            responses.completionCode = varResponse.completionCode;
            responses.statusDescription = varResponse.statusDescription;
            return responses;
        }

        /// <summary>
        ///  Get chassis network properties
        /// </summary>
        /// <returns>Response packet containing network properties</returns>
        public ChassisNetworkPropertiesResponse GetChassisNetworkProperties()
        {
            string[] ipAddresses = null;
            string[] subnets = null;
            string[] gateways = null;
            string dnsHostName = null;
            string dhcpServer = null;
            string dnsDomain = null;
            string macAddress = null;
            bool dhcpEnabled = true;
            ChassisNetworkPropertiesResponse response = new ChassisNetworkPropertiesResponse();
            response.chassisNetworkPropertyCollection = new List<ChassisNetworkProperty>();

            Tracer.WriteInfo("Received GetChassisNetworkProperties()");
            Tracer.WriteUserLog("Invoked GetChassisNetworkProperties()");

            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();

            // Set default completion code to unknown.
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

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
                        int loopCount = 0;
                        foreach (string ip in ipAddresses)
                        {
                            if (ChassisManagerUtil.CheckIpFormat(ip))
                            {
                                ChassisNetworkProperty cr = new ChassisNetworkProperty();
                                cr.ipAddress = ipAddresses.ToArray()[loopCount];
                                if (subnets != null)
                                {
                                    cr.subnetMask = subnets.ToArray()[loopCount];
                                }
                                if (gateways != null)
                                {
                                    cr.gatewayAddress = gateways.ToArray()[loopCount];
                                }
                                cr.dhcpServer = dhcpServer;
                                cr.dnsDomain = dnsDomain;
                                cr.dnsHostName = dnsHostName;
                                cr.macAddress = macAddress;
                                cr.dhcpEnabled = dhcpEnabled;
                                cr.completionCode = Contracts.CompletionCode.Success;
                                response.chassisNetworkPropertyCollection.Add(cr);
                            }
                            loopCount++;
                        }
                    }
                    else // all other interfaces (with ip not enables)
                    {
                        macAddress = (string)mo["MACAddress"];
                        // Populating interfaces only with valid mac addresses - ignoring loopback and other virtual interfaces
                        if (macAddress != null)
                        {
                            ChassisNetworkProperty cr = new ChassisNetworkProperty();
                            cr.macAddress = macAddress;
                            cr.completionCode = Contracts.CompletionCode.Success;
                            response.chassisNetworkPropertyCollection.Add(cr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.WriteError("GetChassisNetworkProperties failed with exception :" + ex.Message);
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = String.Format("GetChassisNetworkProperties failed with exception :" + ex.Message);
            }

            response.completionCode = Contracts.CompletionCode.Success;
            return response;
        }

        /// <summary>
        /// Method to add chassis controller user
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="passwordString">password</param>
        /// <returns>Response indicating if add user was success/failure</returns>
        public Contracts.ChassisResponse AddChassisControllerUser(string userName, string passwordString, Contracts.WCSSecurityRole role)
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Tracer.WriteUserLog("Invoked AddChassisControllerUser(UserName: {0}, role: {1})", userName, role.ToString());
            try
            {
                // Return BadRequest if any data is missing.
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(passwordString))
                {
                    Tracer.WriteError("AddChassisControllerUser: Invalid input parameters.");
                    response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                    response.statusDescription = "Username or Password is null or empty";
                    return response;
                }

                userName = userName.Trim();
                passwordString = passwordString.Trim();

                if (userName == null || passwordString == null)
                {
                    Tracer.WriteError("AddChassisControllerUser: Invalid input parameters.");

                    response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                    response.statusDescription = "Username or Password is null or empty";
                    return response;
                }

                DirectoryEntry AD = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
                DirectoryEntry NewUser = AD.Children.Add(userName, "user");

                NewUser.Invoke("SetPassword", new object[] { passwordString });
                NewUser.Invoke("Put", new object[] { "Description", "WcsCli chassis manager request" });
                NewUser.CommitChanges();
                DirectoryEntry grp;
                // Find group, if not exists, create
                grp = ChassisManagerUtil.FindGroupIfNotExistsCreate(role);

                if (grp != null)
                {
                    grp.Invoke("Add", new object[] { NewUser.Path.ToString() });

                    Tracer.WriteInfo("AddChassisControllerUser: User Account Created Successfully");
                    response.completionCode = Contracts.CompletionCode.Success;
                }
                else
                {
                    Tracer.WriteInfo("AddChassisControllerUser: Failed to create account, failed to add user to group");
                    response.completionCode = Contracts.CompletionCode.Failure;
                    response.statusDescription = String.Format("AddChassisControllerUser: Failed to create account, failed to add user to group");
                }

                return response;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Tracer.WriteError("AddChassisControllerUser: failed with exception: " + ex);

                // check if passowrd did not meet the requirements, display appropriate message to user.
                if (ex.ToString().Contains("0x800708C5"))
                {
                    response.completionCode = Contracts.CompletionCode.UserPasswordDoesNotMeetRequirement;
                    response.statusDescription = "User password does not meet requirement";
                }
                else if (ex.ToString().Contains("0x800708B0"))
                {
                    response.completionCode = Contracts.CompletionCode.UserAccountExists;
                    response.statusDescription = "User account already exists";
                }
                else
                {
                    response.completionCode = Contracts.CompletionCode.Failure;
                    response.statusDescription = String.Format("AddChassisControllerUser: failed with exception: " + ex);
                }
                return response;
            }
            catch (Exception ex)
            {
                Tracer.WriteError("AddChassisControllerUser failed with exception: " + ex);

                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = String.Format("AddChassisControllerUser failed with exception: " + ex);
                return response;
            }
        }

        /// <summary>
        /// Method to change chassis controller user role
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="role">WCS Security role</param>
        /// <returns>Chassis Response indicating if the update user settings was a success/failure</returns>
        public Contracts.ChassisResponse ChangeChassisControllerUserRole(string userName, WCSSecurityRole role)
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            DirectoryEntry grp;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Tracer.WriteUserLog(String.Format("Invoked ChangeChassisControllerUserRole(userName: {0}, role: {1})", userName, role.ToString()));
            try
            {
                userName = userName.Trim();

                if (userName == null)
                {
                    Tracer.WriteError("ChangeChassisControllerUserRole: Invalid input parameters.");
                    response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                    response.statusDescription = "User name provided is null";
                    return response;
                }

                DirectoryEntry AD = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
                DirectoryEntry myEntry = AD.Children.Find(userName, "user");

                // Remove user from other WCS security group , if it exists in them
                // This step is required as if the user permissions are decreased from 
                // admin to user, then he should no longer be in admin role.Similar with operator to user.

                if (role != WCSSecurityRole.WcsCmAdmin)
                {
                    ChassisManagerUtil.RemoveUserFromWCSSecurityGroups(userName, WCSSecurityRole.WcsCmAdmin);
                }

                if (role != WCSSecurityRole.WcsCmOperator)
                {
                    ChassisManagerUtil.RemoveUserFromWCSSecurityGroups(userName, WCSSecurityRole.WcsCmOperator);
                }

                if (role != WCSSecurityRole.WcsCmUser)
                {
                    ChassisManagerUtil.RemoveUserFromWCSSecurityGroups(userName, WCSSecurityRole.WcsCmUser);
                }

                // Add if user does not already exists in the given group
                if (!ChassisManagerUtil.CheckIfUserExistsInGroup(userName, role))
                {
                    // Find group if not exists create new
                    grp = ChassisManagerUtil.FindGroupIfNotExistsCreate(role);

                    if (grp != null)
                    {
                        // Add user to group
                        grp.Invoke("Add", new object[] { myEntry.Path.ToString() });
                        grp.CommitChanges();
                        grp.Close();
                    }
                    else
                    {
                        Tracer.WriteError("ChangeChassisControllerUserRole: Failed to change user role, failed to find/add group");
                        response.completionCode = Contracts.CompletionCode.Failure;
                        response.statusDescription = String.Format("ChangeChassisControllerUserRole: Failed to change user role, failed to find/add group");
                        return response;
                    }
                }

                Tracer.WriteInfo("ChangeChassisControllerUserRole: Role changed successfully");
                response.completionCode = Contracts.CompletionCode.Success;
                return response;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Tracer.WriteError("ChangeChassisControllerUserRole: failed with exception: " + ex);             

                // check if passowrd did not meet the requirements, display appropriate message to user.
                if (ex.ToString().Contains("0x800708C5"))
                {
                    response.completionCode = Contracts.CompletionCode.UserPasswordDoesNotMeetRequirement;
                    response.statusDescription = "User password does not meet system requirements";
                }
                // check the exception code for user not found
                else if (ex.ToString().Contains("0x800708AD"))
                {
                    response.completionCode = Contracts.CompletionCode.UserNotFound;
                    response.statusDescription = "User name provided cannot be found";
                }
                else
                {
                    response.completionCode = Contracts.CompletionCode.Failure;
                    response.statusDescription = String.Format("ChangeChassisControllerUserRole: failed with exception: " + ex);
                }
                return response;
            }
            catch (Exception ex)
            {
                Tracer.WriteError("ChangeChassisControllerUserRole failed with exception: " + ex);

                // user already belongs to the role, we don't need any action hence consider it success
                if (ex.ToString().Contains("The specified account name is already a member of the group"))
                {
                    response.completionCode = Contracts.CompletionCode.Success;
                }
                else
                {
                    response.completionCode = Contracts.CompletionCode.Failure;
                    response.statusDescription = String.Format("ChangeChassisControllerUserRole failed with exception: " + ex);
                }

                return response;
            }
        }

        /// <summary>
        /// Method to change chassis controller user password to given values
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="newPassword">New password</param>
        /// <returns>Chassis Response indicating if user password change was a success/failure</returns>
        public Contracts.ChassisResponse ChangeChassisControllerUserPassword(string userName, string newPassword)
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Tracer.WriteUserLog("Invoked ChangeChassisControllerUserPassword(userName: {0})", userName);
            try
            {
                userName = userName.Trim();
                newPassword = newPassword.Trim();

                if (userName == null || newPassword == null)
                {
                    Tracer.WriteError("ChangeChassisControllerUserPassword: Invalid input parameters.");
                    response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                    response.statusDescription = "User name or password is null";
                    return response;
                }

                DirectoryEntry AD = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
                DirectoryEntry myEntry = AD.Children.Find(userName, "user");

                if (myEntry != null)
                {
                    myEntry.Invoke("SetPassword", new object[] { newPassword });
                    myEntry.CommitChanges();
                    Tracer.WriteInfo("ChangeChassisControllerUserPassword: Password changed Successfully for user: {0}", userName);
                    response.completionCode = Contracts.CompletionCode.Success;
                }
                else
                {
                    Tracer.WriteError("ChangeChassisControllerUserPassword: Failed to change user password, User: {0} does not exists",
                        userName);
                }
                return response;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Tracer.WriteError("ChangeChassisControllerUserPassword: failed with exception: " + ex);
                response.completionCode = Contracts.CompletionCode.Failure;

                // check if passowrd did not meet the requirements, display appropriate message to user.
                if (ex.ToString().Contains("0x800708C5"))
                {
                    response.completionCode = Contracts.CompletionCode.UserPasswordDoesNotMeetRequirement;
                    response.statusDescription = "User password does not meet system requirements";
                }
                // check the exception code for user not found
                else if (ex.ToString().Contains("0x800708AD"))
                {
                    response.completionCode = Contracts.CompletionCode.UserNotFound;
                    response.statusDescription = "User not found";
                }
                else
                {
                    response.statusDescription = String.Format("ChangeChassisControllerUserPassword: failed with exception: " + ex);
                }
                return response;
            }
            catch (Exception ex)
            {
                Tracer.WriteError("ChangeChassisControllerUserPassword failed with exception: " + ex);

                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = String.Format("ChangeChassisControllerUserPassword failed with exception: " + ex);
                return response;
            }
        }

        /// <summary>
        /// Method to remove user. **TO-DO* Authenticate who can Add/delete user.
        /// </summary>
        /// <param name="userName">User Name</param>
        /// <returns>Chassis Response to indicate if reomve user operation was success/failure</returns>
        public Contracts.ChassisResponse RemoveChassisControllerUser(string userName)
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Tracer.WriteUserLog("Invoked RemoveChassisControllerUser(userName: {0})", userName);
            try
            {
                userName = userName.Trim();

                if (userName == null)
                {
                    Tracer.WriteError("RemoveChassisControllerUser: Invalid input parameters.");
                    response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                    response.statusDescription = "Username is null";
                    return response;
                }

                DirectoryEntry AD = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer");
                DirectoryEntry myEntry = AD.Children.Find(userName, "user");
                AD.Children.Remove(myEntry);
                Tracer.WriteInfo("RemoveChassisControllerUser: User Account deleted Successfully");
                response.completionCode = Contracts.CompletionCode.Success;

                return response;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Tracer.WriteError("RemoveChassisControllerUser: failed with exception: " + ex);

                // check the exception code for passowrd did not meet the requirements, display appropriate message to user.
                if (ex.ToString().Contains("0x800708C5"))
                {
                    response.completionCode = Contracts.CompletionCode.UserPasswordDoesNotMeetRequirement;
                    response.statusDescription = "User password does not meet system requirements";
                }
                // check the exception code for user not found
                else if (ex.ToString().Contains("0x800708AD"))
                {
                    response.completionCode = Contracts.CompletionCode.UserNotFound;
                    response.statusDescription = "User not found";
                }
                else
                {
                    response.completionCode = Contracts.CompletionCode.Failure;
                    response.statusDescription = String.Format("RemoveChassisControllerUser: failed with exception: " + ex);
                }
                return response;
            }
            catch (Exception ex)
            {
                Tracer.WriteError("RemoveChassisControllerUser failed with exception: " + ex);

                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = String.Format("RemoveChassisControllerUser failed with exception: " + ex);
                return response;
            }
        }

        /// <summary>
        /// ***TODO - M2*** Starts serial session on a blade
        /// </summary>
        /// <returns> Returns information about the new session created including the exit key sequence. If null then failure</returns>
        public StartSerialResponse StartBladeSerialSession(int bladeId, int sessionTimeoutInSecs)
        {
            StartSerialResponse response = new StartSerialResponse();
            response.serialSessionToken = null;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Tracer.WriteInfo("Received startbladeserialsession({0})", bladeId);
            Tracer.WriteUserLog("Invoked StartBladeSerialSession(bladeId: {0})", bladeId);

            if (ChassisManagerUtil.CheckBladeId((byte)bladeId) != (byte)CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                Tracer.WriteWarning("StartBladeSerialSession failed : Blade ID: {0} out of range ", bladeId);
                return response;
            }

            if (!FunctionValidityChecker.checkBladeStateValidity((byte)bladeId))
            {
                response.completionCode = Contracts.CompletionCode.DevicePoweredOff;
                return response;
            }

            if (!FunctionValidityChecker.checkBladeTypeValidity((byte)bladeId))
            {
                response.completionCode = Contracts.CompletionCode.CommandNotValidForBlade;
                response.statusDescription = response.completionCode.ToString();
                return response;
            }

            if (!ChassisManagerUtil.CheckBladeSerialSessionTimeout(sessionTimeoutInSecs))
            {
                response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                response.statusDescription = response.completionCode.ToString();
                return response;
            }

            response = BladeSerialSessionMetadata.StartBladeSerialSession(bladeId, sessionTimeoutInSecs);
            if (ChassisManagerUtil.CheckCompletionCode(response.completionCode))
            {
                Tracer.WriteInfo("StartBladeSerialSession succeeded for bladeId: " + bladeId);
            }
            else
            {
                Tracer.WriteError("StartBladeSerialSession: failed for bladeId: {0} with completion code: {1}", bladeId, response.completionCode.ToString());
            }

            response.statusDescription = response.completionCode.ToString();

            return response;
        }

        /// <summary>
        /// ***TODO - M2*** Stops serial session on a blade
        /// </summary>
        public Contracts.ChassisResponse StopBladeSerialSession(int bladeId, string sessionToken, bool forceKill)
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Tracer.WriteInfo("Received Stopbladeserialsession({0})", bladeId);
            Tracer.WriteUserLog("Invoked StopBladeSerialSession(bladeId: {0})", bladeId);

            if (!forceKill)
            {
                if (ChassisManagerUtil.CheckBladeId((byte) bladeId) != (byte) CompletionCode.Success)
                {
                    response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;

                    Tracer.WriteWarning("StopBladeSerialSession failed : Blade ID: {0} out of range: ", bladeId);
                    return response;
                }

                if (!FunctionValidityChecker.checkBladeStateValidity((byte) bladeId))
                {
                    response.completionCode = Contracts.CompletionCode.DevicePoweredOff;
                    return response;
                }

                if (!FunctionValidityChecker.checkBladeTypeValidity((byte) bladeId))
                {
                    response.completionCode = Contracts.CompletionCode.CommandNotValidForBlade;
                    return response;
                }
            }

            response = BladeSerialSessionMetadata.StopBladeSerialSession(bladeId, sessionToken, forceKill);

            if (ChassisManagerUtil.CheckCompletionCode(response.completionCode))
            {
                Tracer.WriteInfo("StopBladeSerialSession succeeded for bladeId: " + bladeId);
            }
            else
            {
                Tracer.WriteError("StopBladeSerialSession: failed for bladeId: {0} with completion code: {1}", bladeId, response.completionCode.ToString());
            }

            response.statusDescription = response.completionCode.ToString();
            return response;
        }

        /// <summary>
        /// ***TODO - M2*** Send data to a blade serial session
        /// </summary>
        public Contracts.ChassisResponse SendBladeSerialData(int bladeId, string sessionToken, byte[] data)
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Tracer.WriteInfo("Received SendBladeSerialData({0}) API", bladeId);
            Tracer.WriteUserLog("Invoked SendBladeSerialData(bladeId: {0}) API", bladeId);

            if (data == null || ChassisManagerUtil.CheckBladeId((byte)bladeId) != (byte)CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                Tracer.WriteWarning("SendBladeSerialData failed : Blade Id: {0} out of range: ", bladeId);
                return response;
            }

            if (!FunctionValidityChecker.checkBladeStateValidity((byte)bladeId))
            {
                response.completionCode = Contracts.CompletionCode.DevicePoweredOff;
                return response;
            }

            if (!FunctionValidityChecker.checkBladeTypeValidity((byte)bladeId))
            {
                response.completionCode = Contracts.CompletionCode.CommandNotValidForBlade;
                return response;
            }

            response = BladeSerialSessionMetadata.SendBladeSerialData(bladeId, sessionToken, data);

            if (ChassisManagerUtil.CheckCompletionCode(response.completionCode))
            {
                Tracer.WriteInfo("SendBladeSerialdata succeeded for bladeId: " + bladeId);
            }
            else
            {
                Tracer.WriteError("SendBladeSerialData: failed for bladeId: {0} with completion code: {1}", bladeId, response.completionCode.ToString());
            }

            response.statusDescription = response.completionCode.ToString();
            return response;
        }

        /// <summary>
        /// ***TODO - M2*** Receive data from a blade serial session
        /// </summary>
        public SerialDataResponse ReceiveBladeSerialData(int bladeId, string sessionToken)
        {
            SerialDataResponse response = new SerialDataResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Tracer.WriteInfo("Received ReceiveBladeSerialData({0}) API", bladeId);
            Tracer.WriteUserLog("Invoked ReceiveBladeSerialData(bladeId: {0}, sessionToken: {1}) API", bladeId, sessionToken);

            if (ChassisManagerUtil.CheckBladeId((byte)bladeId) != (byte)CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                Tracer.WriteWarning("ReceiveBladeSerialData failed : Blade ID: {0} out of range: ", bladeId);
                return response;
            }

            if (!FunctionValidityChecker.checkBladeStateValidity((byte)bladeId))
            {
                response.completionCode = Contracts.CompletionCode.DevicePoweredOff;
                return response;
            }

            if (!FunctionValidityChecker.checkBladeTypeValidity((byte)bladeId))
            {
                response.completionCode = Contracts.CompletionCode.CommandNotValidForBlade;
                return response;
            }

            response = BladeSerialSessionMetadata.ReceiveBladeSerialData(bladeId, sessionToken);

            if (ChassisManagerUtil.CheckCompletionCode(response.completionCode))
            {
                Tracer.WriteInfo("ReceiveBladeSerialdata succeeded for bladeId: " + bladeId);
            }
            else if (response.completionCode == Contracts.CompletionCode.Timeout)
            {
                Tracer.WriteInfo("ReceiveBladeSerialdata: No data to be received from bladeId: {0} (expected timeout).", bladeId);
            }
            else
            {
                Tracer.WriteError("ReceiveBladeSerialdata: failed for bladeId: {0} with completion code: {1} ", bladeId, response.completionCode.ToString());
            }

            response.statusDescription = response.completionCode.ToString();
            return response;
        }

        /// <summary>
        /// Start serial console port session on the specified serial port
        /// 
        /// </summary>
        /// <returns> Returns information about the new session created including the exit key sequence. If null then failure</returns>
        public StartSerialResponse StartSerialPortConsole(int portId, int sessionTimeoutInSecs, int deviceTimeoutInMsecs)
        {
            StartSerialResponse response = new StartSerialResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;
            response.serialSessionToken = null;

            Tracer.WriteUserLog("Invoked StartSerialPortConsole(portId: {0})", portId);
            Tracer.WriteInfo("Received StartSerialPortConsole(portId: {0})", portId);

            if (ChassisManagerUtil.CheckSerialConsolePortId((byte)portId) != (byte)CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                Tracer.WriteWarning("StartSerialPortConsole failed : Port ID out of range " + portId);
                response.statusDescription = String.Format("StartSerialPortConsole failed : Port ID out of range " + portId);
                return response;
            }

            int portIndex = ChassisManagerUtil.GetSerialConsolePortIndexFromId(portId);
            response = ChassisState.SerialConsolePortsMetadata[portIndex].StartSerialPortConsole(ChassisManagerUtil.GetSerialConsolePortIdFromIndex(portIndex), sessionTimeoutInSecs, deviceTimeoutInMsecs);

            if (ChassisManagerUtil.CheckCompletionCode(response.completionCode))
            {
                Tracer.WriteInfo("StartSerialPortConsole succeeded for portId: " + portId);
            }
            else
            {
                Tracer.WriteError("StartSerialPortConsole: failed for portId: {0} with completion code: {1}", portId, response.completionCode.ToString());
                response.statusDescription = String.Format("StartSerialPortConsole: failed for portId: {0} with completion code: {1}", portId, response.completionCode.ToString());
            }
            return response;
        }

        /// <summary>
        /// ***TODO - M2*** Stops serial session on a blade
        /// </summary>
        public Contracts.ChassisResponse StopSerialPortConsole(int portId, string sessionToken, bool forceKill)
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Tracer.WriteUserLog("Invoked StopSerialPortConsole(portId: {0}, sessionToken: {1})", portId, sessionToken);
            Tracer.WriteInfo("Received StopSerialPortConsole({0})", portId);

            if (ChassisManagerUtil.CheckSerialConsolePortId((byte)portId) != (byte)CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                Tracer.WriteWarning("StopSerialPortConsole failed : Port ID out of range " + portId);
                response.statusDescription = String.Format("StopSerialPortConsole failed : Port ID out of range " + portId); 
                return response;
            }

            int portIndex = ChassisManagerUtil.GetSerialConsolePortIndexFromId(portId);
            response = ChassisState.SerialConsolePortsMetadata[portIndex].StopSerialPortConsole(ChassisManagerUtil.GetSerialConsolePortIdFromIndex(portIndex), sessionToken, forceKill);

            if (ChassisManagerUtil.CheckCompletionCode(response.completionCode))
            {
                Tracer.WriteInfo("StopSerialPortConsole succeeded for portId: " + portId);
            }
            else
            {
                Tracer.WriteError("StopSerialPortConsole: failed for portId: {0} with completion code: {1}", portId, response.completionCode.ToString());
                response.statusDescription = String.Format("StopSerialPortConsole: failed for portId: {0} with completion code: {1}", portId, response.completionCode.ToString());
            }
            return response;
        }

        /// <summary>
        /// ***TODO - M2*** Send data to a blade serial session
        /// </summary>
        public Contracts.ChassisResponse SendSerialPortData(int portId, string sessionToken, byte[] data)
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Tracer.WriteUserLog("Invoked SendSerialPortData(portId: {0}, sessionToken: {1}, data: {2})", portId, sessionToken, data);
            Tracer.WriteInfo("Received SendSerialPortData({0})", portId);

            if (ChassisManagerUtil.CheckSerialConsolePortId((byte)portId) != (byte)CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                response.statusDescription = 
                    String.Format("SendSerialPortData failed : Port ID out of range: {0}", portId);
                Tracer.WriteWarning("SendSerialPortData failed : Port ID out of range: {0}", portId);
                return response;
            }

            int portIndex = ChassisManagerUtil.GetSerialConsolePortIndexFromId(portId);
            response = ChassisState.SerialConsolePortsMetadata[portIndex].SendSerialPortData(ChassisManagerUtil.GetSerialConsolePortIdFromIndex(portIndex), sessionToken, data);

            if (ChassisManagerUtil.CheckCompletionCode(response.completionCode))
            {
                Tracer.WriteInfo("SendSerialPortData succeeded for portId: " + portId);
            }
            else
            {
                Tracer.WriteError("SendSerialPortData: failed for portId : {0} with completion code: {1} ", portId, response.completionCode.ToString());
                response.statusDescription = String.Format("SendSerialPortData: failed for portId : {0} with completion code: {1} ", portId, response.completionCode.ToString());
            }
            return response;
        }

        /// <summary>
        /// ***TODO - M2*** Receive data from a blade serial session
        /// </summary>
        public SerialDataResponse ReceiveSerialPortData(int portId, string sessionToken)
        {
            SerialDataResponse response = new SerialDataResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Tracer.WriteUserLog("Invoked ReceiveSerialPortData(portId: {0}, sessionToken: {1})", portId, sessionToken);
            Tracer.WriteInfo("Received ReceiveSerialPortData({0})", portId);

            if (ChassisManagerUtil.CheckSerialConsolePortId((byte)portId) != (byte)CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                Tracer.WriteWarning("ReceiveSerialPortData failed : ParameterOutOfRange for Port ID " + portId);
                response.statusDescription = String.Format("ReceiveSerialPortData failed : ParameterOutOfRange for Port ID " + portId);
                return response;
            }

            int portIndex = ChassisManagerUtil.GetSerialConsolePortIndexFromId(portId);
            response = ChassisState.SerialConsolePortsMetadata[portIndex].ReceiveSerialPortData(ChassisManagerUtil.GetSerialConsolePortIdFromIndex(portIndex), sessionToken);

            // Set Http code status
            if (ChassisManagerUtil.CheckCompletionCode(response.completionCode))
            {
                Tracer.WriteInfo("ReceiveSerialPortData succeeded for portId: " + portId);
            }
            else if (response.completionCode == Contracts.CompletionCode.Timeout)
            {
                Tracer.WriteInfo("ReceiveSerialPortdata: No data to be received from bladeId: {0} (expected timeout).", portId);
                response.statusDescription = String.Format("ReceiveSerialPortdata: No data to be received from bladeId: {0} (expected timeout).", portId);
            }
            else
            {
                Tracer.WriteError("ReceiveSerialPortData: failed for portId: {0} with completion code: {1}", portId, response.completionCode.ToString());
                response.statusDescription = String.Format("ReceiveSerialPortData: failed for portId: {0} with completion code: {1}", portId, response.completionCode.ToString());
            }
            return response;
        }

        /// <summary>
        /// Get Chassis Attention LED Status
        /// </summary>
        /// <returns>LED status response</returns>
        public LedStatusResponse GetChassisAttentionLEDStatus()
        {
            LedStatusResponse response = new LedStatusResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;
            Tracer.WriteInfo("Received GetChassisAttentionLEDStatus API");
            Tracer.WriteUserLog("Invoked GetChassisAttentionLEDStatus API");

            // Get Chassis Status LED.
            response = ChassisState.AttentionLed.GetLedStatus();

            if (response.completionCode != (byte)Contracts.CompletionCode.Success)
            {
                Tracer.WriteError("Chassis Attention LED status failed with Completion Code: {0:X}", response.completionCode);              
            }
            else
            {
                if (response.ledState == LedState.ON)
                {
                    Tracer.WriteInfo("Chassis AttentionLED status: ON");
                }
                else if (response.ledState == LedState.OFF)
                {
                    Tracer.WriteInfo("Chassis AttentionLED status: OFF");
                }
                else
                {
                    Tracer.WriteInfo("Chassis AttentionLED status: NA");
                }
            }

                response.completionCode =
                    ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)response.completionCode);
                if (response.completionCode != Contracts.CompletionCode.Success)
                {
                    response.statusDescription = response.completionCode.ToString();
                }

            return response;
        }

        /// <summary>
        /// Returns Chassis Health including Fan Speed and Health, PSU Health and Blade Type and Health
        /// </summary>
        /// <returns></returns>
        public Contracts.ChassisHealthResponse GetChassisHealth(bool bladeHealth = false, bool psuHealth = false, bool fanHealth = false)
        {
            Tracer.WriteInfo("Received GetChassisHealth({0},{1},{2})", bladeHealth, psuHealth, fanHealth);

            Tracer.WriteUserLog("Invoked GetChassisHealth({0},{1},{2})", bladeHealth, psuHealth, fanHealth);

            // If all options are not given by user, then default to providing all information
            if (!bladeHealth && !psuHealth && !fanHealth)
            {
                bladeHealth = psuHealth = fanHealth = true;
            }
            
            ChassisHealthResponse response = new ChassisHealthResponse();
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            if (bladeHealth)
            {
                response.bladeShellCollection = new List<BladeShellResponse>();

                // Populate Blade Shell information (Type, Internal State)
                for (byte i = 1; i <= ConfigLoaded.Population; i++)
                {
                    BladeShellResponse br = new BladeShellResponse();
                    br.completionCode = Contracts.CompletionCode.Success;
                    br.bladeNumber = i;
                    br.bladeState = ChassisState.GetStateName(i);
                    br.bladeType = ChassisState.GetBladeTypeName(ChassisState.GetBladeType(i));
                    response.bladeShellCollection.Add(br);

                }

                response.completionCode = Contracts.CompletionCode.Success; // Always success if bladeinfo requested, since reading static variable
                Tracer.WriteInfo("Populated Blade Shell information, state and type for blades");
            }

            // Get Psu health information
            if (psuHealth)
            {
                response.psuInfoCollection = GetPsuInfo();

                // if the master object is not successful, check child objects
                if(response.completionCode != Contracts.CompletionCode.Success)
                {
                    // if it received any positive results, return success.
                    foreach (PsuInfo psu in response.psuInfoCollection)
                    {
                        // if any children are successful, set master to success.
                        if (psu.completionCode == Contracts.CompletionCode.Success)
                        { 
                            response.completionCode = Contracts.CompletionCode.Success;
                            break; // once a match has been found escape foreach
                        }
                    }
                }
            }

            // Get Fan Health Information
            if (fanHealth)
            {
                response.fanInfoCollection = GetFanInfo();

                // if the master object is not successful, check child objects
                if (response.completionCode != Contracts.CompletionCode.Success)
                {
                    // if it received any positive results, return success.
                    foreach (FanInfo fan in response.fanInfoCollection)
                    {
                        // if any children are successful, set master to success.
                        if (fan.completionCode == Contracts.CompletionCode.Success ||
                            fan.completionCode == Contracts.CompletionCode.FanlessChassis )
                        {
                            response.completionCode = Contracts.CompletionCode.Success;
                            break; // once a match has been found escape foreach
                        }
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Returns Blade Information including Blade Type, and State Information, Processor information, Memory information
        /// PCie information and Hard Disk information
        /// </summary>
        /// <returns></returns>
        public Contracts.BladeHealthResponse GetBladeHealth(int bladeId, bool cpuInfo = false, bool memInfo = false, bool diskInfo = false,
            bool pcieInfo = false, bool sensorInfo = false, bool tempInfo = false, bool fruInfo = false)
        {
            Tracer.WriteInfo("Received GetBladeHealth({0})", bladeId);
            Tracer.WriteUserLog("Invoked GetBladeHealth({0})", bladeId);
            BladeHealthResponse response = new BladeHealthResponse();
            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("GetBladeHealth", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            // If all options are false (default values), then return everything
            if (!cpuInfo && !memInfo && !diskInfo && !pcieInfo && !sensorInfo && !tempInfo && !fruInfo)
            {
                cpuInfo = memInfo = diskInfo = pcieInfo = sensorInfo = fruInfo = tempInfo = true;
            }

            try
            {
                // proc, mem, disk, me, temp, power, fru, pcie, misc
                HardwareStatus hardwareStatus = WcsBladeFacade.GetHardwareInfo((byte)bladeId, cpuInfo, memInfo,
                    diskInfo, false, tempInfo, false, fruInfo, pcieInfo, sensorInfo);

                Type hwType = hardwareStatus.GetType();

                response.completionCode =
                    ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)hardwareStatus.CompletionCode);

                if (hardwareStatus.CompletionCode != (byte)CompletionCode.Success)
                {
                    Tracer.WriteError("GetBladeHealth: Hardware status command failed with completion code: {0:X}", hardwareStatus.CompletionCode);
                }
                else
                {
                    BladeShellResponse shellResp = new BladeShellResponse();
                    if (hardwareStatus.CompletionCode != (byte)Contracts.CompletionCode.Success)
                    {
                        shellResp.completionCode = Contracts.CompletionCode.Failure;
                        shellResp.statusDescription = Contracts.CompletionCode.Failure + ": Internal Error";
                    }
                    else
                    {
                        shellResp.completionCode = Contracts.CompletionCode.Success;
                    }

                    if (hwType == typeof(ComputeStatus))
                    {
                        ComputeStatus hwResponse = (ComputeStatus)hardwareStatus;

                        shellResp.completionCode = Contracts.CompletionCode.Success;
                        shellResp.bladeNumber = bladeId;
                        shellResp.bladeState = ChassisState.GetStateName((byte)bladeId);
                        shellResp.bladeType = hwResponse.BladeType;

                        // generate processor info
                        response.processorInfo = new List<ProcessorInfo>();
                        // populate processor info if required
                        if (cpuInfo)
                        {
                            foreach (KeyValuePair<byte, Ipmi.ProcessorInfo> instance in hwResponse.ProcInfo)
                            {
                                if (instance.Value.CompletionCode == (byte)CompletionCode.Success)
                                    response.processorInfo.Add(new ProcessorInfo(Contracts.CompletionCode.Success, instance.Key, instance.Value.ProcessorType.ToString(),
                                        instance.Value.ProcessorState.ToString(), instance.Value.Frequency));
                                else
                                    response.processorInfo.Add(new ProcessorInfo(
                                        ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)instance.Value.CompletionCode)));

                            }
                        }

                        // generate memory info
                        response.memoryInfo = new List<MemoryInfo>();
                        // populate memory info if required
                        if (memInfo)
                        {
                            foreach (KeyValuePair<byte, Ipmi.MemoryInfo> instance in hwResponse.MemInfo)
                            {
                                if (instance.Value.CompletionCode == (byte)CompletionCode.Success)
                                {
                                    if (instance.Value.Status == Ipmi.MemoryStatus.NotPresent)
                                    {
                                        response.memoryInfo.Add(new MemoryInfo(Contracts.CompletionCode.Success, instance.Key,
                                        instance.Value.Status.ToString(), instance.Value.Speed, instance.Value.MemorySize,
                                        instance.Value.Status.ToString(), instance.Value.Status.ToString()));
                                    }
                                    else
                                    {
                                        response.memoryInfo.Add(new MemoryInfo(Contracts.CompletionCode.Success, instance.Key, 
                                            instance.Value.MemoryType.ToString(), instance.Value.Speed, instance.Value.MemorySize,
                                            instance.Value.Voltage.ToString(), instance.Value.Status.ToString()));
                                    }
                                }
                                else
                                    response.memoryInfo.Add(new MemoryInfo(
                                        ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)instance.Value.CompletionCode)));
                            }
                        }

                        // initialize PcieInfo
                        response.pcieInfo = new List<PCIeInfo>();
                        // populate PcieInfo if required.
                        if (pcieInfo)
                        {
                            foreach (KeyValuePair<byte, Ipmi.PCIeInfo> instance in hwResponse.PcieInfo)
                            {
                                if (instance.Value.CompletionCode == (byte)CompletionCode.Success)
                                    response.pcieInfo.Add(new PCIeInfo(Contracts.CompletionCode.Success, instance.Key, instance.Value.CardState.ToString(), instance.Value.VendorId, instance.Value.DeviceId,
                                        instance.Value.SystemId, instance.Value.SubsystemId));
                                else
                                    response.pcieInfo.Add(new PCIeInfo(
                                        ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)instance.Value.CompletionCode)));
                            }

                        }

                        // initialize DiskInfo
                        response.bladeDisk = new List<DiskInfo>();
                        // populate disk info if required
                        if (diskInfo)
                        {

                            foreach (KeyValuePair<byte, Ipmi.SensorReading> instance in hwResponse.DiskSensors)
                            {
                                if (instance.Value.CompletionCode == (byte)CompletionCode.Success)
                                    response.bladeDisk.Add(new DiskInfo(Contracts.CompletionCode.Success, instance.Key, string.Format(instance.Value.EventDescription, instance.Value.EventState)));
                                else
                                    response.bladeDisk.Add(new DiskInfo(
                                        ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)instance.Value.CompletionCode)));
                            }
                        }

                        // initialize disk info
                        response.sensors = new List<SensorInfo>();
                        // add hardware sensor info if required.
                        if (sensorInfo)
                        {
                            foreach (HardwareSensor sensor in hwResponse.HardwareSdr)
                            {
                                if (sensor.Sdr.CompletionCode == (byte)CompletionCode.Success)
                                    response.sensors.Add(new SensorInfo(Contracts.CompletionCode.Success, sensor.Sdr.SensorNumber, sensor.Sdr.SensorType.ToString(), sensor.Sdr.Entity.ToString(),
                                        sensor.Sdr.EntityInstance.ToString(), sensor.Reading.Reading.ToString(), string.Format(sensor.Reading.EventDescription, sensor.Reading.EventState), sensor.Sdr.Description));
                                else
                                    response.sensors.Add(new SensorInfo(
                                        ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)sensor.Sdr.CompletionCode)));
                            }

                        }
                        // add temp sensor info if required.
                        if (tempInfo)
                        {
                            foreach (KeyValuePair<byte, Ipmi.SensorReading> temp in hwResponse.TempSensors)
                            {
                                if (temp.Value.CompletionCode == (byte)CompletionCode.Success)
                                    response.sensors.Add(new SensorInfo(Contracts.CompletionCode.Success, temp.Key, Ipmi.SensorType.Temperature.ToString(),
                                        temp.Value.Reading.ToString(), string.Format(temp.Value.EventDescription, temp.Value.EventState), temp.Value.Description));
                                else
                                    response.sensors.Add(new SensorInfo(
                                        ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)temp.Value.CompletionCode)));

                            }
                        }

                        if (fruInfo)
                        {
                            // append fru info if required
                            AppendFruInfo(ref response, hwResponse, fruInfo);
                        }
                    }
                    else if (hwType == typeof(JbodStatus))
                    {
                        JbodStatus hwResponse = (JbodStatus)hardwareStatus;

                        shellResp.bladeNumber = bladeId;
                        shellResp.bladeState = ChassisState.GetStateName((byte)bladeId);
                        shellResp.bladeType = hwResponse.BladeType;

                        if (diskInfo)
                        {
                            if (hwResponse.CompletionCode == (byte)CompletionCode.Success)
                            {
                                response.jbodDiskInfo = new JbodDiskStatus(Contracts.CompletionCode.Success, hwResponse.DiskStatus.Channel,
                                    hwResponse.DiskStatus.DiskCount);
                                response.jbodDiskInfo.diskInfo = new List<DiskInfo>();
                                foreach (KeyValuePair<byte, Ipmi.DiskStatus> instance in hwResponse.DiskStatus.DiskState)
                                {
                                    response.jbodDiskInfo.diskInfo.Add(new DiskInfo(Contracts.CompletionCode.Success, instance.Key, instance.Value.ToString()));
                                }
                            }
                            else
                            {
                                response.jbodDiskInfo = new JbodDiskStatus(ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)hwResponse.CompletionCode));
                            }
                        }

                        if (tempInfo)
                        {
                            if (hwResponse.DiskInfo.CompletionCode == (byte)CompletionCode.Success)
                                response.jbodInfo = new JbodInfo(Contracts.CompletionCode.Success, hwResponse.DiskInfo.Unit.ToString(),
                                    hwResponse.DiskInfo.Reading);
                            else
                                response.jbodInfo = new JbodInfo(ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)hwResponse.DiskInfo.CompletionCode));
                        }

                        if (fruInfo)
                        {
                            // append fru info if required
                            AppendFruInfo(ref response, hwResponse, fruInfo);
                        }
                    }
                    else if (hwType == typeof(UnknownBlade))
                    {
                        UnknownBlade hwResponse = (UnknownBlade)hardwareStatus;
                        // return errored response.

                        shellResp.bladeNumber = bladeId;
                        shellResp.bladeState = ChassisState.GetStateName((byte)bladeId);
                        shellResp.bladeType = hwResponse.BladeType;

                        if (fruInfo)
                        {
                            // append fru info if required
                            AppendFruInfo(ref response, hwResponse, false);
                        }
                    }
                    response.bladeShell = shellResp;
                }
            }
            catch (Exception ex)
            {
                response.completionCode = Contracts.CompletionCode.Failure;
                response.statusDescription = response.completionCode.ToString() + ": " + ex.Message;
                Tracer.WriteError("Exception while processing Hardware Status of Blade {0}: {1}", bladeId, ex);
            }
            return response;
        }
        
        /// <summary>
        /// GetNextBoot - gets the next boot device
        /// </summary>
        /// <returns></returns>
        public Contracts.BootResponse GetNextBoot(int bladeId)
        {
            BootResponse response = new BootResponse();
            Tracer.WriteUserLog("Invoked GetNextBoot({0})", bladeId);
            Tracer.WriteInfo("Received GetNextBoot({0})", bladeId);

            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("GetNextBoot", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            // Issue IPMI command
            Ipmi.NextBoot nextBoot = WcsBladeFacade.GetNextBoot((byte)bladeId);

            if (nextBoot.CompletionCode != (byte)Contracts.CompletionCode.Success)
            {
                response.completionCode = Contracts.CompletionCode.Failure;
                Tracer.WriteError("GetNextBoot failed with completion code: {0:X}", nextBoot.CompletionCode);
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.Success;
                response.nextBoot = ChassisManagerUtil.GetContractsBootType(nextBoot.BootDevice);
            }

            response.completionCode =
                            ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)nextBoot.CompletionCode);
            response.statusDescription = response.completionCode.ToString();
            return response;
        }

        /// <summary>
        /// SetNextBoot - sets the next boot device
        /// </summary>
        /// <returns></returns>
        public Contracts.BootResponse SetNextBoot(int bladeId, Contracts.BladeBootType bootType, bool uefi, bool persistent, int bootInstance = 0)
        {
            BootResponse response = new BootResponse();
            Tracer.WriteUserLog("Invoked SetNextBoot({0})", bladeId);
            Tracer.WriteInfo("Received SetNextBoot({0})", bladeId);

            response.bladeNumber = bladeId;
            response.completionCode = Contracts.CompletionCode.Unknown;
            response.statusDescription = String.Empty;

            Contracts.ChassisResponse varResponse = ValidateRequest("SetNextBoot", bladeId);
            if (varResponse.completionCode != Contracts.CompletionCode.Success)
            {
                response.completionCode = varResponse.completionCode;
                response.statusDescription = varResponse.statusDescription;
                return response;
            }

            // SetNextBoot should not be set to unknown. The unknown value is used only by getnextboot API
            if (bootType == Contracts.BladeBootType.Unknown)
            {
                response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                response.statusDescription = Contracts.CompletionCode.ParameterOutOfRange.ToString() + " Cannot be set to Unknown.";
                return response;
            }

            // Issue IPMI Command after checking
            if (Enum.IsDefined(typeof(Ipmi.BootType), ChassisManagerUtil.GetIpmiBootType(bootType)))
            {
                Ipmi.NextBoot nextBoot = WcsBladeFacade.SetNextBoot((byte)bladeId, ChassisManagerUtil.GetIpmiBootType(bootType), uefi, persistent, (byte)bootInstance);

                if (nextBoot.CompletionCode == (byte)Contracts.CompletionCode.Success)
                {
                    response.completionCode = Contracts.CompletionCode.Success;
                    response.nextBoot = bootType;
                }
                else
                {
                    Tracer.WriteError("SetNextBoot failed with Completion Code {0:X}", nextBoot.CompletionCode);
                }

                response.completionCode =
                            ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)nextBoot.CompletionCode);
            }
            else
            {
                response.completionCode = Contracts.CompletionCode.Failure;
                Tracer.WriteError("Boot Type {0} not defined in Boot Order Type", bootType);
            }

            response.statusDescription = response.completionCode.ToString();
            return response;
        }

        #region Undocumented Commands

        /// <summary>
        /// returns the max PWM requirement.  Undocumented command used for
        /// data center AHU control integration
        /// </summary>
        public Contracts.MaxPwmResponse GetMaxPwmRequirement()
        {  
            Contracts.MaxPwmResponse response = new Contracts.MaxPwmResponse();

            if(HelperFunction.MaxPwmRequirement > 0 )
                response.completionCode = Contracts.CompletionCode.Success;
            else
                response.completionCode = Contracts.CompletionCode.Unknown;

            response.maxPwmRequirement = HelperFunction.MaxPwmRequirement;

            return response;
        }

        /// <summary>
        /// Turns a power supply off and on
        /// </summary>
        public Contracts.ChassisResponse ResetPsu(int psuId)
        {
            Contracts.ChassisResponse response = new Contracts.ChassisResponse();

            if (psuId < 0 || psuId > ConfigLoaded.NumPsus)
            {
                response.completionCode = Contracts.CompletionCode.ParameterOutOfRange;
                return response;
            }

            // Step 1: Turn Psu off
            byte completionCode = (byte)ChassisState.Psu[(psuId - 1)].SetPsuOnOff(true);
            if (completionCode != (byte)CompletionCode.Success)
            {
                Tracer.WriteWarning("Error on psu power off: PsuId {0} CompletionCode: 0x{1:X2}",
                    psuId, completionCode);

                response.completionCode = ChassisManagerUtil.GetContractsCompletionCodeMapping(completionCode);
            }
            else
            {
                // subsequent commands are permitted to change upon failure.
                response.completionCode = Contracts.CompletionCode.Success;
            }

            // Step 2: Turn Psu On
            completionCode = (byte)ChassisState.Psu[(psuId - 1)].SetPsuOnOff(false);
            if (completionCode != (byte)CompletionCode.Success)
            {
                Tracer.WriteWarning("Error on psu power on: PsuId {0} CompletionCode: 0x{1:X2}",
                    (psuId - 1), completionCode);

                response.completionCode = ChassisManagerUtil.GetContractsCompletionCodeMapping(completionCode);
            }
            

            return response;

        }

        #endregion

        #region Support Functions

        /// <summary>
        /// Standalone Command to gets Power Consumption Info,  
        /// used with Data Center Power Monitoirng integration
        /// </summary>
        private List<PsuInfo> GetPsuInfo()
        {
            // Create Response Collection
            List<PsuInfo> response = new List<PsuInfo>(ConfigLoaded.NumPsus);

            try
            {
                for (int psuId = 0; psuId < ConfigLoaded.NumPsus; psuId++)
                {
                    // Step 1: Create PsuInfo Response object
                    Contracts.PsuInfo psuInfo = new Contracts.PsuInfo();
                    psuInfo.id = (uint)(psuId + 1);

                    // Add object to list.
                    response.Add(psuInfo);

                    // subsequent commands are permitted to change upon failure.
                    response[psuId].completionCode = Contracts.CompletionCode.Success;

                    // Step 2:  Get Psu Power
                    PsuPowerPacket psuPower = ChassisState.Psu[psuId].GetPsuPower();
                    if (psuPower.CompletionCode != CompletionCode.Success)
                    {
                        Tracer.WriteWarning("Getting psu power failed for psu: " + psuInfo.id);
                        response[psuId].powerOut = 0;
                        response[psuId].completionCode = ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)psuPower.CompletionCode);
                        response[psuId].statusDescription = psuPower.CompletionCode.ToString();
                    }
                    else
                    {
                        response[psuId].powerOut = (uint)psuPower.PsuPower;
                    }

                    // Step 3: Get Psu Serial Number
                    PsuSerialNumberPacket serialNumberPacket = new PsuSerialNumberPacket();
                    serialNumberPacket = ChassisState.Psu[psuId].GetPsuSerialNumber();
                    if (serialNumberPacket.CompletionCode != CompletionCode.Success)
                    {
                        Tracer.WriteWarning("Getting psu serial number failed for psu: " + psuInfo.id);
                        response[psuId].serialNumber = string.Empty;
                        response[psuId].completionCode = ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)serialNumberPacket.CompletionCode);
                        response[psuId].statusDescription = serialNumberPacket.CompletionCode.ToString();
                    }
                    else
                    {
                        response[psuId].serialNumber = serialNumberPacket.SerialNumber;
                    }

                    // Step 4: Get Psu Status
                    PsuStatusPacket psuStatusPacket = new PsuStatusPacket();
                    psuStatusPacket = ChassisState.Psu[psuId].GetPsuStatus();
                    if (psuStatusPacket.CompletionCode != CompletionCode.Success)
                    {
                        Tracer.WriteWarning("Getting psu status failed for psu " + psuId);
                        response[psuId].state = PowerState.NA;
                        response[psuId].completionCode = ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)psuStatusPacket.CompletionCode);
                        response[psuId].statusDescription = psuStatusPacket.CompletionCode.ToString();
                    }
                    else
                    {
                        if (psuStatusPacket.PsuStatus == (byte)Contracts.PowerState.OFF)
                        {
                            response[psuId].state = PowerState.OFF;
                        }
                        else if (psuStatusPacket.PsuStatus == (byte)Contracts.PowerState.ON)
                        {
                            response[psuId].state = PowerState.ON;
                        }
                        else
                        {
                            response[psuId].state = PowerState.NA;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.WriteError(" PsuInfo failed with exception: {0}", ex);
            }

            return response;

        }

        /// <summary>
        /// Gets Fan Infomation
        /// </summary>
        private List<FanInfo> GetFanInfo()
        {
            List<FanInfo> response = new List<FanInfo>(ConfigLoaded.NumFans);

            try
            {
                // Populate Fan Status and Fan Reading
                for (int fanId = 0; fanId < ConfigLoaded.NumFans; fanId++)
                {
                    FanInfo fanInfo = new FanInfo();
                    fanInfo.fanId = (fanId + 1);

                    response.Add(fanInfo);

                    if (!ConfigLoaded.EnableFan)
                    {
                        // no need to enumerate all fans.  escape the for loop and return.
                        response[fanId].completionCode = Contracts.CompletionCode.FanlessChassis;
                        break;
                    }
                    else
                    {
                        FanSpeedResponse fanSpeed = ChassisState.Fans[fanId].GetFanSpeed();

                        if (fanSpeed.CompletionCode != (byte)CompletionCode.Success)
                        {
                            Tracer.WriteWarning("Error getting fan speed on: FanId {0} CompletionCode: 0x{1:X2}",
                            (fanId + 1), fanSpeed.CompletionCode);

                            response[fanId].fanSpeed = 0;
                            response[fanId].isFanHealthy = false;
                            response[fanId].completionCode = ChassisManagerUtil.GetContractsCompletionCodeMapping((byte)fanSpeed.CompletionCode);
                            response[fanId].statusDescription = fanSpeed.CompletionCode.ToString();
                        }
                        else
                        {
                            response[fanId].completionCode = Contracts.CompletionCode.Success;

                            response[fanId].fanSpeed = fanSpeed.Rpm;

                            if (fanSpeed.Rpm > 0)
                                response[fanId].isFanHealthy = true;
                            else
                                response[fanId].isFanHealthy = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tracer.WriteError(" GetFanInfo failed with exception: {0}", ex);
            }

            return response;
        }

        /// <summary>
        /// Appends Fru for GetBaldeHealth function.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="hwresp"></param>
        /// <param name="append"></param>
        private void AppendFruInfo(ref Contracts.BladeHealthResponse response, HardwareStatus hwresp, bool append)
        {
            // append fru info if required.
            if (append)
            {
                response.serialNumber = hwresp.SerialNumber;
                response.assetTag = hwresp.AssetTag;
                response.productType = hwresp.ProductType;
                response.hardwareVersion = hwresp.HardwareVersion;
            }
            else
            {
                response.serialNumber = string.Empty;
                response.assetTag = string.Empty;
                response.productType = string.Empty;
                response.hardwareVersion = string.Empty;
            }
        }

        /// <summary>
        /// Converts NicInfo into contracts object
        /// </summary>
        /// <param name="ipmiNicInfo"></param>
        /// <returns></returns>
        internal Contracts.NicInfo GetNicInfoObject(Ipmi.NicInfo ipmiNicInfo)
        {
            Contracts.NicInfo nicInfoObject = new Contracts.NicInfo();
            if (ipmiNicInfo.CompletionCode == (byte)CompletionCode.Success)
            {
                nicInfoObject.completionCode = Contracts.CompletionCode.Success;
            }
            // IpmiInvalidDataFieldInRequest is returned when a NIC that is not present in the system is requested.
            else if (ipmiNicInfo.CompletionCode == (byte)CompletionCode.IpmiInvalidDataFieldInRequest)
            {
                nicInfoObject.completionCode = Contracts.CompletionCode.Success;
                nicInfoObject.statusDescription = "Not Present";
            }
            // Else an unkown error occured.
            else
            {
                nicInfoObject.completionCode = Contracts.CompletionCode.Failure;
                nicInfoObject.statusDescription = Contracts.CompletionCode.Failure.ToString() + ": Internal error";
            }

            nicInfoObject.deviceId = ipmiNicInfo.DeviceId;
            nicInfoObject.macAddress = ipmiNicInfo.MacAddress;
            
            return nicInfoObject;
        }

        /// <summary>
        /// Gets the status string from Enum
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        internal string GetDiskInfoStatusString(byte status)
        {
            return Enum.GetName(typeof(DiskStatus), status);
        }

        #endregion

    }
}

