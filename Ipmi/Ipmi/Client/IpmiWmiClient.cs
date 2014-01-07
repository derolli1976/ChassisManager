/********************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.       *
*                                                       *
*   Auther:  Bryankel@Microsoft.com                     *
*                                                       *
********************************************************/

namespace Microsoft.GFS.WCS.ChassisManager.Ipmi
{
    using System;
    using System.Reflection;
    using System.Management;
    using System.Diagnostics;

    /// <summary>
    /// WMI Management class for IPMI RequestResponse invoke and transformation.
    /// </summary>
    internal sealed partial class IpmiWmiClient : IpmiClientExtended
    {
        /// <summary>
        /// ipmi method paramaters
        /// </summary>
        private ManagementBaseObject wmiPacket;

        /// <summary>
        /// microsoft_ipmi instances
        /// </summary>
        private ManagementObjectCollection _ipmi_Instance;

        /// <summary>
        /// wmi method name
        /// </summary>
        private string _ipmi_Method = "RequestResponse";

        public IpmiWmiClient(ManagementScope scope)
        {
            // wmi scope
            ManagementScope _wmiScope = scope;

            // Management Path
            ManagementPath path = new ManagementPath("microsoft_ipmi");

            // Management Class
            using (ManagementClass _ipmiClass = new ManagementClass(_wmiScope, path, null))
            {
                // ipmi instances
                _ipmi_Instance = _ipmiClass.GetInstances();

                // Get RequestResponse mothod paramaters
                wmiPacket = _ipmiClass.GetMethodParameters(_ipmi_Method);
            }
        }

        /// <summary>
        /// Generics method IpmiSendReceive for easier use
        /// </summary>
        internal override T IpmiSendReceive<T>(IpmiRequest ipmiRequest)
        {
            return (T)this.IpmiSendReceive(ipmiRequest, typeof(T));
        }

        /// <summary>
        /// Create instance of ManagementBaseObject derived from packet frame
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private ManagementBaseObject GetManagementObject(byte[] message)
        {
            // create management base object
            ManagementBaseObject response = wmiPacket;

            // create payload
            byte[] payload = new byte[(message.Length - 5)];

            // extract command payload
            Buffer.BlockCopy(message, 5, payload, 0, payload.Length);

            // complete management object header
            response["Command"] = message[0]; // IpmiCommand
            response["NetworkFunction"] = message[1]; // IpmiFunction
            response["Lun"] = message[2]; // 0x00
            response["RequestData"] = payload; // Ipmi payload
            response["RequestDataSize"] = message[3]; // dataLength
            response["ResponderAddress"] = message[4]; // 32

            return response;
        }

        /// <summary>
        /// Send Receive Ipmi messages
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseType"></param>
        /// <returns></returns>
        internal override IpmiResponse IpmiSendReceive(IpmiRequest ipmiRequest, Type responseType, bool allowRetry = true)
        {
            
            byte[] message = ipmiRequest.GetBytes(IpmiTransport.Wmi, 0x00);
            
            // Serialize the IPMI request into bytes.
            ManagementBaseObject ipmiRequestMessage = this.GetManagementObject(message);

            // invoke new method options
            InvokeMethodOptions methodOptions = new InvokeMethodOptions(null, System.TimeSpan.FromMilliseconds(base.Timeout));

            // management return object
            ManagementBaseObject ipmiResponseMessage = null;

            // get instance and invoke RequestResponse method
            foreach (ManagementObject mo in _ipmi_Instance)
            {
                ipmiResponseMessage = mo.InvokeMethod(_ipmi_Method, wmiPacket, methodOptions);
            }

            if (ipmiResponseMessage == null)
            {
                // throw ipmi/dcmi response exception with a custom string message and the ipmi completion code
                throw new IpmiResponseException();
            }

            // ipmi response completion code
            byte completionCode = (byte)ipmiResponseMessage["CompletionCode"];
           
            // Create the response based on the provided type and message bytes.
            ConstructorInfo constructorInfo = responseType.GetConstructor(Type.EmptyTypes);
            IpmiResponse ipmiResponse = (IpmiResponse)constructorInfo.Invoke(new Object[0]);

            if (completionCode == 0)
            {
                try
                {
                    // extract response data array
                    byte[] responseData = (byte[])ipmiResponseMessage["ResponseData"];

                    // extract response message lenght
                    int lenght = (int)ipmiResponseMessage["ResponseDataSize"];                  

                    // initialize the response to set the paramaters.
                    ipmiResponse.Initialize(IpmiTransport.Wmi, responseData, lenght, 0x00);
                    ipmiResponseMessage = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception Source: {0} Message{1}", ex.Source.ToString(), ex.Message.ToString());
                }
            }
            else
            {
                // throw ipmi/dcmi response exception with a custom string message and the ipmi completion code
                Debug.WriteLine("Completion Code: " + IpmiSharedFunc.ByteToHexString(ipmiResponse.CompletionCode));
                if (ipmiResponseMessage == null)
                Debug.WriteLine("Request Type: {0} Response Packet: {1} Completion Code {2}", ipmiRequest.GetType().ToString(),            
                    IpmiSharedFunc.ByteArrayToHexString((byte[])ipmiResponseMessage["ResponseData"]), IpmiSharedFunc.ByteToHexString(ipmiResponse.CompletionCode));
            }

            // Response to the IPMI request message.
            return ipmiResponse;
        }
    }
}
