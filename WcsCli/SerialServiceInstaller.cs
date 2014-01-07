using System;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace Microsoft.GFS.WCS.WcsCli
{
    // Provide the ProjectInstaller class which allows 
    // the service to be installed by the Installutil.exe tool
    [RunInstaller(true)]
    public class WcscliSerialServiceInstaller : Installer
    {
        private ServiceProcessInstaller process;
        private ServiceInstaller service;

        /// <summary>
        /// Intaller method.. Service name should match with that provided in the service class
        /// </summary>
        public WcscliSerialServiceInstaller()
        {
            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;
            service = new ServiceInstaller();
            //service.ServiceName = "WcscliSerialService";
            service.ServiceName = GetConfigurationValue("ServiceName") + GetConfigurationValue("COMPortName");
            this.service.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            Installers.Add(process);
            Installers.Add(service);
        }

        /// <summary>
        /// This function gets the service name parameter from the app.config file
        /// The code takes care of finding the correct path (assemnbly path) of the app.config at installation time
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetConfigurationValue(string key)
        {
            Assembly service = Assembly.GetAssembly(typeof(WcscliSerialServiceInstaller));
            Configuration config = ConfigurationManager.OpenExeConfiguration(service.Location);
            if (config.AppSettings.Settings[key] != null)
            {
                return config.AppSettings.Settings[key].Value;
            }
            else
            {
                throw new IndexOutOfRangeException 
                    ("We do not have the queried key: " + key);
            }
        }
    }
}