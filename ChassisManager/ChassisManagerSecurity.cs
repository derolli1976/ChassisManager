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
using System.ServiceModel;
using System.Security.Principal;
using System.ServiceModel.Channels;
using System.Xml;
using System.ServiceModel.Web;
using System.Net;

namespace Microsoft.GFS.WCS.ChassisManager
{
    // This class inherits from SeriveAuthorizationManager and its overridden checkaccess method will be called before any API execution
    public class MyServiceAuthorizationManager : ServiceAuthorizationManager
    {
        public override bool CheckAccess(OperationContext operationContext, ref Message message)
        {
            // Open the request message using an xml reader
            XmlReader xr = OperationContext.Current.IncomingMessageHeaders.GetReaderAtHeader(0);

                // Split the URL at the API name--Parameters junction indicated by the '?' character - taking the first string will ignore all parameters
                string[] urlSplit = xr.ReadElementContentAsString().Split('/');
                // Extract just the API name and rest of the URL, which will be the last item in the split using '/'
                string[] apiSplit = urlSplit[3].Split('?');
                // Logging the username and API name
                Tracer.WriteUserLog(apiSplit[0] + " request from user: " + operationContext.ServiceSecurityContext.WindowsIdentity.Name);

            // If the most-privileged-role that this user belongs has access to this api, then allow access, otherwise deny access
            // Returning true will allow the user to execute the actual API function; Returning false will deny access to the user
            if (ChassisManagerSecurity.GetCurrentUserMostPrivilegedRole() <= ChassisManagerSecurity.GetCurrentApiLeastPrivilegedRole(apiSplit[0]))
            {
                Tracer.WriteUserLog("CheckAccess: Authorized");
                return true;
            }
            else
            {
                Tracer.WriteUserLog("CheckAccess: NOT Authorized");
                return false;
            }
        }
    }

    static internal class ChassisManagerSecurity
    {
        // Dictionary that holds the mapping between the APIs and the least privileged role with access to that API
        // That is, if an API is mapped to WcsCmOperator, it automatically authorize WcsCmAdmin as well but not WcsCmUser 
        // OrdinalIgnoreCase is used to make the api names comparison case insensitive
        static Dictionary<string, authorizationRole> apiNameLeastPrivilegeRoleMap = new Dictionary<string, authorizationRole>(StringComparer.OrdinalIgnoreCase);

        // 4 WCS CM User Roles
        public enum authorizationRole : int
        {
            WcsCmAdmin = 0,
            WcsCmOperator = 1,
            WcsCmUser = 2,
            WcsCmUnAuthorized = 3,
        }

        static ChassisManagerSecurity()
        {
            //Initializing the API-UserRole authorization mapping
            apiNameLeastPrivilegeRoleMap.Add("GetChassisInfo", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("GetBladeInfo", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("GetAllBladesInfo", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("SetChassisAttentionLEDOn", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetChassisAttentionLEDOff", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("GetChassisAttentionLEDStatus", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("SetBladeAttentionLEDOn", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetAllBladesAttentionLEDOn", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetBladeAttentionLEDOff", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetAllBladesAttentionLEDOff", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetBladeDefaultPowerStateOn", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetAllBladesDefaultPowerStateOn", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetBladeDefaultPowerStateOff", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetAllBladesDefaultPowerStateOff", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("GetBladeDefaultPowerState", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("GetAllBladesDefaultPowerState", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("SetPowerOn", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetAllPowerOn", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetPowerOff", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetAllPowerOff", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetBladeOn", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetAllBladesOn", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetBladeOff", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetAllBladesOff", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetBladeActivePowerCycle", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetAllBladesActivePowerCycle", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("GetPowerState", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("GetAllPowerState", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("GetBladeState", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("GetAllBladesState", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("SetACSocketPowerStateOn", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetACSocketPowerStateOff", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("GetACSocketPowerState", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("StartBladeSerialSession", authorizationRole.WcsCmAdmin);
            apiNameLeastPrivilegeRoleMap.Add("StopBladeSerialSession", authorizationRole.WcsCmAdmin);
            apiNameLeastPrivilegeRoleMap.Add("SendBladeSerialData", authorizationRole.WcsCmAdmin);
            apiNameLeastPrivilegeRoleMap.Add("ReceiveBladeSerialData", authorizationRole.WcsCmAdmin);
            apiNameLeastPrivilegeRoleMap.Add("StartSerialPortConsole", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("StopSerialPortConsole", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SendSerialPortData", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("ReceiveSerialPortData", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("ReadChassisLogWithTimestamp", authorizationRole.WcsCmAdmin);
            apiNameLeastPrivilegeRoleMap.Add("ReadChassisLog", authorizationRole.WcsCmAdmin);
            apiNameLeastPrivilegeRoleMap.Add("ClearChassisLog", authorizationRole.WcsCmAdmin);
            apiNameLeastPrivilegeRoleMap.Add("ReadBladeLogWithTimestamp", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("ReadBladeLog", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("ClearBladeLog", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("GetBladePowerReading", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("GetAllBladesPowerReading", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("GetBladePowerLimit", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("GetAllBladesPowerLimit", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("SetBladePowerLimit", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetAllBladesPowerLimit", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetBladePowerLimitOn", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetAllBladesPowerLimitOn", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetBladePowerLimitOff", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("SetAllBladesPowerLimitOff", authorizationRole.WcsCmOperator);
            apiNameLeastPrivilegeRoleMap.Add("GetChassisNetworkProperties", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("SetChassisNetworkProperties", authorizationRole.WcsCmAdmin);
            apiNameLeastPrivilegeRoleMap.Add("AddChassisControllerUser", authorizationRole.WcsCmAdmin);
            apiNameLeastPrivilegeRoleMap.Add("RemoveChassisControllerUser", authorizationRole.WcsCmAdmin);
            apiNameLeastPrivilegeRoleMap.Add("ChangeChassisControllerUserPassword", authorizationRole.WcsCmAdmin);
            apiNameLeastPrivilegeRoleMap.Add("ChangeChassisControllerUserRole", authorizationRole.WcsCmAdmin);
            apiNameLeastPrivilegeRoleMap.Add("GetChassisHealth", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("GetBladeHealth", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("GetNextBoot", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("SetNextBoot", authorizationRole.WcsCmUser);
            apiNameLeastPrivilegeRoleMap.Add("GetMaxPwmRequirement", authorizationRole.WcsCmAdmin);
            apiNameLeastPrivilegeRoleMap.Add("ResetPsu", authorizationRole.WcsCmAdmin);
        }

        // Returns the least privileged role with access to this API 
        static internal authorizationRole GetCurrentApiLeastPrivilegedRole(string apiName)
        {
            try
            {
                authorizationRole val = authorizationRole.WcsCmUnAuthorized;
                // If api name do not map to any of the mapped roles, then by default make it accessible only by WcsCmAdmin role 
                if (!apiNameLeastPrivilegeRoleMap.TryGetValue(apiName, out val))
                {
                    Tracer.WriteWarning("GetCurrentApiLeastPrivilegedRole: There is no role mapping for the api " + apiName);
                    return authorizationRole.WcsCmAdmin;
                }
                Tracer.WriteInfo("Requested API's minimum privilege requirement: " + val); 
                return val;
            }
            catch (Exception ex)
            {
                Tracer.WriteError("Api-to-Role Mapping Exception was thrown: " + ex);
                // Return as UnAuthorized if there is an exception, then by default make this API accessible only by WcsCmAdmin role 
                Tracer.WriteInfo("Requested API's minimum privilege requirement (after exception):  " + authorizationRole.WcsCmAdmin);
                return authorizationRole.WcsCmAdmin;
            }
        }

        // Returns the most privileged role this user belongs to
        static internal authorizationRole GetCurrentUserMostPrivilegedRole()
        {
            try
            {
                ServiceSecurityContext context = OperationContext.Current.ServiceSecurityContext;
                WindowsIdentity windowsIdentity = context.WindowsIdentity;
                var principal = new WindowsPrincipal(windowsIdentity);
                
                // Extract domain + role names to check for access privilege
                // The first item before '\' is the domain name - extract it
                string[] usernameSplit = windowsIdentity.Name.Split('\\');
                // Apend role names after the domain name
                string wcscmadminRole = usernameSplit[0] + "\\" + "WcsCmAdmin";
                string wcscmoperatorRole = usernameSplit[0] + "\\" + "WcsCmOperator";
                string wcscmuserRole = usernameSplit[0] + "\\" + "WcsCmUser";

                if (principal.IsInRole("Administrators"))
                {
                    Tracer.WriteUserLog("User({0}) belongs to Administrators group and hence belongs to WcsCmAdmin privilege role", windowsIdentity.Name);
                    return authorizationRole.WcsCmAdmin;
                }

                // Is user in local WcsCmAdmin group or domain's WcsCmAdmin group?
                if (principal.IsInRole("WcsCmAdmin") || principal.IsInRole(wcscmadminRole))
                {
                    Tracer.WriteUserLog("User({0}) belongs to WcsCmAdmin privilege role", windowsIdentity.Name);
                    return authorizationRole.WcsCmAdmin;
                }

                // Is user in local WcsCmOperator group or domain's WcsCmOperator group?
                if (principal.IsInRole("WcsCmOperator") || principal.IsInRole(wcscmoperatorRole))
                {
                    Tracer.WriteUserLog("User({0}) belongs to WcsCmOperator privilege role", windowsIdentity.Name);
                    return authorizationRole.WcsCmOperator;
                }

                // Is user in local WcsCmUser group or domain's WcsCmUser group?
                if (principal.IsInRole("WcsCmUser") || principal.IsInRole(wcscmuserRole))
                {
                    Tracer.WriteUserLog("User({0}) belongs to WcsCmUser privilege role", windowsIdentity.Name);
                    return authorizationRole.WcsCmUser;
                }
                // User not mapped to any standard roles
                Tracer.WriteWarning("GetCurrentUserMostPrivilegedRole: Current user({0}) not mapped to the standard WCS roles", windowsIdentity.Name);
                Tracer.WriteUserLog("GetCurrentUserMostPrivilegedRole: Current user({0}) not mapped to the standard WCS roles", windowsIdentity.Name);
            }
            catch (Exception ex)
            {
                Tracer.WriteError("User Authorization check exception  was thrown: " + ex);
            }
            
            // Return as unauthorized if the user do not belong to any of the category or if there is an exception
            return authorizationRole.WcsCmUnAuthorized;
        }
    }
}
