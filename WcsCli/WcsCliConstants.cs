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
﻿
namespace Microsoft.GFS.WCS.WcsCli
{
    // Class for all constants 
    internal static class WcsCliConstants
    {
        public const int maxSledId = 24;
        public const int maxAcSocketPortId = 2;
        public const string powerStateOff = "OFF";
        public const string powerStateOn = "ON";
        public const string powerStateNotApplicable = "NA";
        public const string sledNamePrefix = "BLADE";
        public const string WcsCli = "wcscli";
        public const string getinfo = "getchassisinfo";
        public const string getscinfo = "getbladeinfo";
        public const string getChassisHealth = "getchassishealth";
        public const string getBladeHealth = "getbladehealth";
        public const string establishCmConnection = "establishCmConnection";
        public const string terminateCmConnection = "terminateCmConnection";
        public const string ncidoff = "setchassisattentionledoff";
        public const string ncidon = "setchassisattentionledon";
        public const string ncidstatus = "getchassisattentionledstatus";
        public const string scidoff = "setbladeattentionledoff";
        public const string scidon = "setbladeattentionledon";
        public const string setscponstate = "setbladedefaultpowerstate";
        public const string getscponstate = "getbladedefaultpowerstate";
        public const string poweron = "setpoweron";
        public const string poweroff = "setpoweroff";
        public const string bladeon = "setbladeon";
        public const string bladeoff = "setbladeoff";
        public const string powerinton = "setacsocketpowerstateon";
        public const string powerintoff = "setacsocketpowerstateoff";
        public const string powercycle = "setbladeactivepowercycle";
        public const string getscpowerstate = "getpowerstate";
        public const string getbladestate = "getbladestate";
        public const string getpowerintstate = "getacsocketpowerstate";
        public const string listsersessions = "listsersessions";
        public const string startBladeSerialSession = "startBladeSerialSession";
        public const string stopBladeSerialSession = "stopBladeSerialSession";
        public const string startPortSerialSession = "startPortSerialSession";
        public const string stopPortSerialSession = "stopPortSerialSession";
        public const string readsclog = "readbladelog";
        public const string clrsclog = "clearbladelog";
        public const string readnclog = "readchassislog";
        public const string clrnclog = "clearchassislog";
        public const string adduser = "adduser";
        public const string changeuserrole = "changeuserrole";
        public const string changeuserpassword = "changeuserpwd";
        public const string removeuser = "removeuser";
        public const string getnic = "getnic";
        public const string setnic = "setnic";
        public const string getpowerreading = "getbladepowerreading";
        public const string getpowerlimit = "getbladepowerlimit";
        public const string setpowerlimit = "setbladepowerlimit";
        public const string activatepowerlimit = "setbladepowerlimiton";
        public const string deactivatepowerlimit = "setbladepowerlimitoff";
        public const string help = "h";
        public const string getnextboot = "getnextboot";
        public const string setnextboot = "setnextboot";
        public const string getserviceversion = "getserviceversion";
        public const uint powercycleOfftime = 0;
        public const uint defaultChassisACSocketPortNo = 0;
        public const string consoleString = "WcsCli#";
        public const char argIndicatorVar = '-';
        public const string invalidCommandString = "Error: Invalid Command.";
        public const string argsMissingString = "Error! Required arguments missing";
        public const string NotApplicable = "Not Applicable";
        public const string getinfoComputeNodesHeader = "== Compute Nodes ==";
        public const string getinfoPowerSuppliesHeader = "== Power Supplies ==";
        public const string getinfoChassisControllerHeader = "== Chassis Controller ==";
        public const string getscinfoSledControllerHeader = "== blade Controller Info ==";
        public const string fanHealthHeader = "== Fan Health ==";
        public const string bladeHeathHeader = "== Blade Health ==";
        public const string cpuInfo = "== CPU Information ==";
        public const string memoryInfo = "== Memory Information ==";
        public const string diskInfo = "== Disk information ==";
        public const string pcieInfo = "== PCIE Information ==";
        public const string sensorInfo = "== Sensor Information ==";
        public const string tempSensorInfo = "== Temp Sensor information ==";
        public const string fruInfo = "== FRU Information ==";
        public const string psuHealthHeader = "== PSU Health ==";
        public const string macAddressesInfoHeader = "== MAC Address ==";
        public const string getscinfoComputeNodeHeader = "== Compute Node Info ==";
        public const string listsersessionsHeader = "== Active Interactive Mode Sessions ==";
        public const string readnclogHeader = "== Chassis Controller Log ==";
        public const string readsclogHeader = "== blade Controller Log ==";
        public const uint readsclogNumberEntries = 100;
        public const string dataFetchError = "Error in fetching data";
        public const string unknownError = "Unknown Error. Please retry";
        public const int invalidSledId = -1;
        public const string invalidSledName = "";
        public const string commandFailure = "Command Failed";
        public const string commandTimeout = "Response: Timeout, Blade/Switch is unreachable";
        public const string commandUserAccountExists = "Command failed: User account already exists";
        public const string commandUserNotFound = "Command failed: User not found";
        public const string commandUserPwdDoesNotMeetReq = "Command failed: User passowrd does not meet system requirements";
        public const string commandPartialFailure = "Partial failure: some data values could not be populated";
        public const string commandSucceeded = "OK";
        public const string bladeStateUnknown = "Unreachable, it is turned OFF or not present";
        public const string serviceResponseEmpty = "Response received from ChassisManager service is NULL/Empty";
        public const string BladeTypeCompute = "Server";
        public const string BladeTypeJBOD = "Jbod";
        public const string BladeTypeUnknown = "Unknown";
        public const string SensorTypeTemp = "Temperature";      
        public const string enablessl = "enablechassismanagerssl";
        public const string disablessl = "disablechassismanagerssl";
        public const string startchassismanager = "startchassismanager";
        public const string stopchassismanager = "stopchassismanager";
        public const string getchassismanagerstatus = "getchassismanagerstatus";

        public const string WcsCliHelp =
@"-------------------------------------------------------------
Chassis infrastructure command line interface
---------------------------------------------------------------

wcscli -getchassisinfo                 Get information about
                                       blades, power supplies and Chassis Manager.

wcscli -getchassishealth               Get health status for blades, power supplies and Fan

wcscli -getserviceversion              Get Chassis Manager service version.

wcscli -getbladeinfo                   Get information about blade.

wcscli -getbladehealth                 Get health information for blade, includes CPU info,
                                       Memory info, Disk info, PCIE info, Sensor info and Fru Info.
                                       This information can be requested seperately using 
                                       command options.

----------------------------------------------------------------
Blade management commands
----------------------------------------------------------------

wcscli -getpowerstate                 Return the powered
                                      on/off state of blade.

wcscli -setpoweron                    Power on (Active power state) 
                                      blade.

wcscli -setpoweroff                   Power off (Active power state)
                                      blade.

wcscli -getbladestate                 Returns the blade soft power state

wcscli -setbladeon                    Blade soft Power ON 

wcscli -setbladeoff                   Blade soft Power OFF 

wcscli -setbladedefaultpowerstate     Set the default power on
                                      state of a blade.

wcscli -getbladedefaultpowerstate     Get the
                                      default power on state of a blade.

wcscli -setbladeactivepowercycle      Power cycle 
                                      blade.

wcscli -setbladeattentionledon        Turn on the blue ID LED
                                      on each blade.

wcscli -setbladeattentionledoff       Turn off the blue ID LED
                                      on each blade.

wcscli -readbladelog                  Read Blade log

wcscli -clearbladelog                 Clear logs from a
                                      blade.

wcscli -getbladepowerreading          Get power reading for blade(s) in Watts

wcscli -getbladepowerlimit            Get power limit for blade(s) in Watts

wcscli -setbladepowerlimit            Set power limit for blade(s) in Watts

wcscli -setbladepowerlimiton              Activate power limit for blade(s)

wcscli -setbladepowerlimitoff            Deactivate power limit for blade(s) 

----------------------------------------------------------------
Chassis management commands
----------------------------------------------------------------


wcscli -getchassisattentionledstatus   Get the status of the LED (on/off)
                                       on the front of the 
                                       Chassis Manager.

wcscli -setchassisattentionledon       Turn on the blue ID light
                                       on the front of the 
                                       Chassis Manager.

wcscli -setchassisattentionledoff      Turns off the blue ID light
                                       on the front of the 
                                       Chassis Manager.

wcscli -readchassislog                 Read Persistent Log
                                       from the Chassis Controller.

wcscli -clearchassislog                Clear Persistent Log
                                       from the Chassis Controller.

wcscli -getacsocketpowerstate          Get the AC Socket current
                                       on/off state of the Chassis

wcscli -setacsocketpowerstateon        Turn on the AC socket (TOR Switches)
                                       of the Chassis

wcscli -setacsocketpowerstateoff       Turn off the AC socket (TOR Switches)
                                       of the Chassis

---------------------------------------------------------------------------
Local Commands (Only available in WCSCLI Serial Mode):
-----------------------------------------------------------------------------

wcscli -getnic                         Get chassis network configuration.

wcscli -setnic                         Set chassis manager network properties (available only over serial wcscli client).

-----------------------------------------------------------------------------
Chassis Manager Service Configuration Commands (Only available in WCSCLI Serial Mode):
-----------------------------------------------------------------------------

wcscli -startchassismanager             Start chassismanager service
        
wcscli -stopchassismanager              Stop CM service
        
wcscli -getchassismanagerstatus         Get CM service status

wcscli -enablechassismanagerssl         Enables SSL for chassis manager service                     

wcscli -disablechassismanagerssl        Disables SSL for chassis manager service                     

----------------------------------------------------------------
User Management Commands

Please note : These commands will be deprecated in future.
----------------------------------------------------------------

wcscli -adduser                        Add chassis controller user.

wcscli -changeuserrole                 Change chassis controller user role.

wcscli -changeuserpwd                  Change chassis controller user password.

wcscli -removeuser                     Remove chassis controller user.

----------------------------------------------------------------
Serial Session commands
----------------------------------------------------------------

wcscli -startbladeserialsession                Start serial session to a blade
wcscli -startportserialsession                 Start serial session to devices connected to COM ports
wcscli -stopbladeserialsession                 Force kill existing blade serial session for the given blade id.
wcscli -stopportserialsession                  Force kill existing serial session on given port.

wcscli -getnextboot                     Get the first boot order type of a blade

wcscli -setnextboot                     Set first boot order type for a blade

wcscli -establishCmConnection           Create a connection to the CM service

wcscli -terminateCmConnection           Terminate a connection to the CM service


";

        public const string terminateCmConnectionHelp = @"wcscli -terminateCmConnection -h help";

        public const string establishCmConnectionHelp = @"wcscli -establishCmConnection -m <host_name> -p <port> -s <SSL_option> [-u] <username> [-x] <password> [-b] <batchfileName>
-m host_name - Specify Host name for Chassis manager (for serial connection, localhost is assumed)
-p port - Specify a valid Port to connect to for Chassis Manager (default is 8000)
-s Select SSL Encryption enable/disable 
Enter 0 to disable SSl Encryption
Enter 1 to enable SSl Encryption.
-u & -x specify user credentials -- username and password -- to connect to CM service (Optional.. will use default credentials)
-b Optional batch file option (not supported in serial mode).
-v Get CLI version information
-h help
";

        public const string wcscliConsoleParameterHelp = @"wcscli.exe -h <host_name> -p <port> -s <SSL_option> [-u] <username> [-x] <password> [-b] <batchfileName>
-h host_name - Specify Host name for Chassis manager (for serial connection, localhost is assumed)
-p port - Specify a valid Port to connect to for Chassis Manager (default is 8000)
-s Select SSL Encryption enable/disable 
Enter 0 to disable SSl Encryption
Enter 1 to enable SSl Encryption.
-u & -x specify user credentials -- username and password -- to connect to CM service (Optional.. will use default credentials)
-b Optional batch file option.
-v Get CLI version information
-h help
";

        public const string getbladepowerreadingHelp = @"
Usage: wcscli -getbladepowerreading [-i <blade_index> | -a ] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - do for all blades
        -h - help; display the correct syntax
        ";
        public const string getbladebpowerlimitHelp = @"
Usage: wcscli -getbladepowerlimit [-i <blade_index> | -a ] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - do for all blades
        -h - help; display the correct syntax
        ";
        public const string setbladepowerlimitHelp = @"
Usage: wcscli -setbladepowerlimit [-i <blade_index> | -a ] -l <power_limit> [-h]
        blade_index - the target blade number. Typically 1-24
        -a - do for all blades
        -l - power limit per blade in Watts
        -h - help; display the correct syntax
        ";
        public const string setbladepowerlimitOnHelp = @"
Usage: wcscli -setbladepowerlimiton [-i <blade_index> | -a ] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - do for all blades
        -h - help; display the correct syntax
        ";
        public const string setbladepowerlimitoffHelp = @"
Usage: wcscli -setbladepowerlimitoff [-i <blade_index> | -a ] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - do for all blades
        -h - help; display the correct syntax
        ";

        public const string getchassisinfoHelp = @"
Usage: wcscli -getchassisinfo [-s] [-p] [-c] [-h]
        -s - show blade information
        -p - show power information
        -c - show chassis information
        -h - help; display the correct syntax
        ";

        public const string getChassisHealthHelp = @"
Usage: wcscli -getchassishealth [-b] [-p] [-f] [-h]
        -b - show blade health
        -p - show Psu health
        -f - show Fan health
        -h - help; display the correct syntax
        ";

        public const string getBladeHealthHelp = @"
Usage: wcscli -getbladehealth [-i <blade_index>] [-a] [-m] [-d] [-p] [-s] [-t] [-f] [-h]
        -a - Blade CPU Information
        -m - Blade Memory Information
        -d - Balde Disk Information
        -p - Blade PCIE Information
        -s - Blade Sensor Information
        -t - Temprature Sensor Information
        -f - Blade Fru Information
        -h - help; display the correct syntax
        ";

        public const string getbladeinfoHelp = @"
Usage: wcscli -getbladeinfo [-i <blade_index> | -a ] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - do for all blades
        -h - help; display the correct syntax
        ";
        public const string setbladeattentionledonHelp = @"
Usage: wcscli -setbladeattentionledon [-i <blade_index> -a ] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - Run this command on all blades.
        -h - help; display the correct syntax
        ";
        public const string setbladeattentionledoffHelp = @"
Usage: wcscli -setbladeattentionledoff [-i <blade_index> -a ] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - Run this command on all blades.
        -h - help; display the correct syntax
        ";
        public const string getchassisattentionledstatusHelp = @"
Usage: wcscli -getchassisattentionledstatus [-h]
        Displays the status of the chassis LED (On/Off).
        Usage: wcscli -getchassisattentionledstatus [-h]
        -h - help; display the correct syntax
        ";
        public const string setchassisattentionledonHelp = @"
Usage: wcscli -setchassisattentionledon [-h]
        -h - help; display the correct syntax
        ";
        public const string setchassisattentionledoffHelp = @"
Usage: wcscli -setchassisattentionledoff [-h]
        -h - help; display the correct syntax
        ";
        public const string setbladedefaultpowerstateHelp = @"
Usage: wcscli -setbladedefaultpowerstate [-i <blade_index>] -s <state>[-h]
        blade_index - the target blade number. Typically 1-24
        -a - all connected blades
        state - can be 0 (stay off) or 1 (power on)
        -h - help; display the correct syntax";

        public const string getbladedefaultpowerstateHelp = @"
Usage: wcscli -getbladedefaultpowerstate [-i <blade_index>] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - all connected blades
        -h - help; display the correct syntax";

        public const string setpoweronHelp = @"
        Usage: wcscli -setpoweron [-i <blade_index> | -a] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - all connected blades
        -h - help; display the correct syntax
        ";

        public const string setpoweroffHelp = @"        
        Usage: wcscli -setpoweroff [-i <blade_index> | -a] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - all; all connected blades.
        -h - help; display the correct syntax
        ";
        public const string setbladeonHelp = @"
        Usage: wcscli -setbladeon -i <blade_index> | -a] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - all connected blades
        -h - help; display the correct syntax
        ";
        public const string setbladeoffHelp = @"        
        Usage: wcscli -setbladeoff -i <blade_index> | -a] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - all; all connected blades.
        -h - help; display the correct syntax
        ";

        public const string setacsocketpowerstateonHelp = @"
Usage: wcscli -setacsocketpowerstateon [-p <port_number>] | [-h]
        port_number - port number user wants to turn off i.e. 1, 2 or 3
        -h - help; display the correct syntax
        ";

        public const string setacsocketpowerstateoffHelp = @"
Usage: wcscli -setacsocketpowerstateoff [-p <port_number>] | [-h]
        port_number - port number user wants to turn off i.e.1, 2 or 3
        -h - help; display the correct syntax
        ";

        public const string setbladeactivepowercycleHelp = @"
Usage: wcscli -setbladeactivepowercycle [[-t <off_time>] | -i <blade_index> [-t <off_time>] | [-a ] [-t <off_time>]][-h]
        blade_index - the target blade number. Typically 1-24
        off_time - the time interval in seconds for how long to wait before powering the blade back on;
        this is an optional parameter; if not specified, the default interval is 0 seconds
        -a - all connected blades
        -h - help; display the correct syntax
        ";

        public const string getpowerstateHelp = @"
Usage: wcscli -getpowerstate[-i <blade_index> | -a] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - all connected blades
        -h - help; display the correct syntax
        ";
        public const string getbladestateHelp = @"
Usage: wcscli -getbladestate[ -i <blade_index> | -a] [-h]
        blade_index - the target blade number. Typically 1-24
        -a - all connected blades
        -h - help; display the correct syntax
        ";

        public const string getacsocketpowerstateHelp = @"
Usage: wcscli -getacsocketpowerstate [-p <port_number>] | [-h]
        port_number - port number user wants to turn off i.e.1, 2 or 3
        -h - help; display the correct syntax
        ";

  
//           public const string listsersessionsHelp = @"
//    Usage: wcscli -listsersessions [-h]
//    -h - help; display the correct syntax
//    ";        
        public const string startBladeSerialSessionHelp = @"
Usage: wcscli -startBladeSerialSession [-s <session_timeout_in_secs>] [-i <blade_index>] [-h]
        blade_index - the target blade number. Typically 1-24
        -s Session timeout in secs,
        -h - help; display the correct syntax
        ";
        public const string stopBladeSerialSessionHelp = @"
Usage: wcscli -stopbladeserialsession -i<blade_id> [-h]
        -i - terminate all active sessions on given blade id.
        -h - help; display the correct syntax
        ";

        public const string startPortSerialSessionHelp = @"
Usage: wcscli -startPortSerialSession [-i <Port_index>][-s <session_timeout_in_secs>] [-d <device_timeout_in_millisecs>] [-h]
        Port_index - the number of the COM port the device is connected. Enter 1 for COM1, 2 for COM2 etc.,
        -s Session timeout in secs,
        -d Device timeout in millisecs
        -h - help; display the correct syntax
        ";

        public const string stopPortSerialSessionHelp = @"
Usage: wcscli -stopportserialsession -i<port_no> [-h]
        -i - terminate all active sessions on given port.
        -h - help; display the correct syntax
        ";
        public const string readbladelogHelp = @"
Usage: wcscli -readbladelog [-i <blade_index>] [-n <entries_count>] [-h]
        blade_index - the target blade number. Typically 1-24
        entries_count - how many of the most recent entries to report;
                this is an optional parameter; 
        -h - help; display the correct syntax
        ";

        public const string clearbladelogHelp = @"
Usage: wcscli -clearbladelog [-i <blade_index>] [-h]
        blade_index - the target blade number. Typically 1-24
                -h - help; display the correct syntax
        ";

        public const string readchassislogHelp = @"
Usage: wcscli -readchassislog [-h]
        -h - help; display the correct syntax
        ";
        
        public const string clearchassislogHelp = @"
Usage: wcscli -clearchassislog [-h]
        -h - help; display the correct syntax
        ";

        public const string adduserHelp = @"
Usage: wcscli –adduser –u <username> -p <password> -a|-o|-r  [-h] 
        username – the username for the new user 
        password – the password for the new user. 
        Select one of the WCS Security role for the user (Mandatory):
        -a Admin Role
        -o Operator Role
        -r User Role
        -h – help; display the correct syntax 
        ";

        public const string changeuserroleHelp = @"
Usage: wcscli –changeuserrole –u <username> [-a|-o|-r] [-h] 
        Select one of the following user roles:
            -a : Admin privilege
            -o : Operator privilege
            -r : User privilege
        -h – help; display the correct syntax 
        ";

        public const string changeUserPwdHelp = @"
Usage: wcscli –changeuserpwd –u <username> -p <new password> 
        -u Username
        -p <new password> New password
        -h – help; display the correct syntax 
        ";

        public const string removeuserHelp = @"
Usage: wcscli –removeuser –u <username>[-h] 
        username – the username for the new user 
        -h – help; display the correct syntax 
        ";

        public const string getnicHelp = @"
--- (Only available in WCSCLI Serial Mode) --- 

Usage: wcscli –getnic -h
        -h – help; display the correct syntax 
        ";
        public const string setnicHelp = @"
--- (Only available in WCSCLI Serial Mode) --- 

Usage: wcscli -setnic [-n] <hostname>  [-g] <gateway> [-s] <subnet> [-m] <netmask -Required!> [-i]
        <IP -Required!> [-p] <primary DNS -Required!> [-d] <secondary DNS -Required!>
        [-a] <IP addr source DHCP/STATIC -Required!> [-h]
        -n - hostname of the chassis controller
        -g - gateway of the chassis controller
        -s - subnet IP of the chassis controller
        -m - subnet mask of the chassis controller
        -i - ip address of the chassis controller
        -p - primary DNS server address for the chassis controller
        -d - secondary DNS server address for the chassis controller
        -a - IP addr source DHCP/STATIC
        -t - network interface number
        -h – help; display the correct syntax ";

        public const string getnextbootHelp = @"
Usage: wcscli -getnextboot [-i] <blade_index>  
        blade_index - the target blade number. Typically 1-24";

        public const string setnextbootHelp = @"
Usage: wcscli -setnextboot [-i] <blade_index>  [-t] <boot_type> [-m] <mode>  [-p] <is_persistent> [-n] <boot_instance> [-h]
        blade_index - the target blade number. Typically 1-24
        boot_type - 1. NoOverRide, 2. ForcePxe, 3. ForceDefaultHdd, 4. ForceIntoBiosSetup, 5. ForceFloppyOrRemovable
        mode - 0. legacy, 1. uefi
        is_persistent - is this a persistent setting (set value 1) or one-time setting (set value 0)
        boot_instance - instance number of the boot device. (Eg. 0 or 1 for NIC if there are two NICs)
        ";
        public const string getServiceVersionHelp = @"
Usage: wcscli -getserviceversion [-h]
        -h - help; display the correct syntax
        "; 
        
        public const string enablesslHelp = @"
--- (Only available in WCSCLI Serial Mode) --- 

Usage: wcscli –enablechassismanagerssl -h
        -h – help; display the correct syntax 
        ";

        public const string disablesslHelp = @"
--- (Only available in WCSCLI Serial Mode) --- 

Usage: wcscli –disablechassismanagerssl -h
        -h – help; display the correct syntax 
        ";

        public const string startchassismanagerHelp = @"
--- (Only available in WCSCLI Serial Mode) --- 

Usage: wcscli -startchassismanager [-h]
        -h - help; display the correct syntax
        ";

        public const string stopchassismanagerHelp = @"
--- (Only available in WCSCLI Serial Mode) --- 

Usage: wcscli -stopchassismanager [-h]
        -h - help; display the correct syntax
        ";
        public const string getchassismanagerstatusHelp = @"
--- (Only available in WCSCLI Serial Mode) --- 

Usage: wcscli -getchassismanagerstatus [-h]
        -h - help; display the correct syntax
";

        /*        public const string completeCommands = @"
        -----------------------------------------------
        Chassis infrastructure command line interface
        -----------------------------------------------

        wcscli -readnclog                       Read Persistent Log
                                               (PLOG) from the Node Controller.

        wcscli -clrnclog                        Clear Persistent Log
                                               (PLOG) from the Node Controller.

        wcscli -setnic                          Set the network configuration for
                                               the Chassis

        wcscli -getnic                          Get the network configuration for
                                               the Chassis

        wcscli -setncfwupdatecfg                Set the parameters for updating the
                                               Chassis firmware.

        wcscli -getncfwupdatecfg                Get the parameters for updating the
                                               Chassis firmware.

        wcscli -getncfwupdatestatus             Get the status or
                                               result of the update. For example the
                                               result of the download, the result of
                                               the update, etc.

        wcscli -startncfwupdate                 Start the Node Controller
                                               Firmware update.

        wcscli -getinfo                         Get information about
                                               blades, power supplies and Chassis.

        wcscli -getsctemp                       Get the temperature
                                               sensor readings from the blade.

        wcscli -listscserports                  List how many serial ports
                                               the blade has available.

        wcscli -setscserport                    Set the
                                               configuration of a serial port on the
                                               specified blade.

        wcscli -poweron                         Power on an
                                               individual computer board.

        wcscli -poweroff                        Power off an
                                               individual computer board.

        wcscli -powercycle                      Power cycle an
                                               individual computer board.

        wcscli -getscpowerstate                 Return the powered
                                               on/off state of an individual computer
                                               board.

        wcscli -getfanspeed                     Get the fan speed
                                               readings.

        wcscli -getscpwr                        Read the power consumption of the blade.

        wcscli -getschistoricpwr                Read the historical power
                                               consumption at the blade.

        wcscli -clearschistoricpwr              Clear historical powerRead the power
                                               consumption of the blade.

        wcscli -getfanpwr                       Read the power
                                               consumption of the fans.

        wcscli -getfanhistoricpwr               Read the historical power
                                               consumption of the fans.

        wcscli -clearfanhistoricpwr             Clear the power
                                               consumption of the fans.

        wcscli -getsyspower                     Read the power
                                               consumption as reported by the power
                                               supplies.

        wcscli -gethistdatasetting              Read the setting
                                               of enable/disable capturing of historic
                                               sensor readings.

        wcscli -histdata                        enable/disable
                                               capturing of historic sensor readings
                                               from blade Controllers or Fan Controllers
                                               by the Node Controller

        wcscli -getfanstate                     get the state of the fans.

        wcscli -setscserportbmode               Set buffer mode on a serial port
                                               on the specified blade.

        wcscli -getscserportbmodecontent        Get the content captured from
                                               motherboard's serial port when the
                                               blade is operating in Buffer
                                               Mode.

        wcscli -startsersession                 Start serial console session
                                               with the specified blade.

        wcscli -stopportserialsession                  Stop serial console session 
                                               for the provided port.
        
        wcscli -stopbladesersession             Stop blade serial session
                                               for the provided blade id.

        wcscli -setserexitseq                   Set a new session exit key
                                               sequence. The original key sequence is
                                               overwritten. The default key sequence
                                               is ~,.<Enter>

        wcscli -setscponstate                   Set the default power on
                                               state of a board installed on a blade

        wcscli -getscponstate                   Get the
                                               default power on state of a board
                                               installed on blade.

        wcscli -listsersessions                 List the serial
                                               sessions that are active.

        wcscli -getserexitseq                   Get the
                                               session exit key sequence.

        wcscli -readsclog                       Read Persistent Log
                                               (PLOG) or Runtime Log (RLOG) from a
                                               blade.

        wcscli -readfclog                       Read Persistent Log
                                               (PLOG) or Runtime Log (RLOG) from
                                               the fan controller (FC).

        wcscli -clrsclog                        Clear Persistent Log
                                               (PLOG) or Runtime Log (RLOG) from a
                                               blade.

        wcscli -clrfclog                        Clear Persistent Log
                                               (PLOG) or Runtime Log (RLOG) from
                                               the Fan Controller (FC).

        wcscli -getfcinfo                       Get information about FCM.

        wcscli -getscinfo                       Get information about blade including
                                               IPMI.

        wcscli -setscfwupdatecfg                Set the parameters for updating the Fan
                                               Controller firmware.

        wcscli -getscfwupdatecfg                Read blade firmware update
                                               configuration.

        wcscli -getscfwupdatestatus             Get the result of
                                               firmware update for one or more blades.

        wcscli -setfcfwupdatecfg                Set the parameters for updating the
                                               blade firmware.

        wcscli -getfcfwupdatecfg                Get the parameters for updating the Fan
                                               Controller firmware.

        wcscli -getfcfwupdatestatus             Get the result of
                                               firmware update for the Fan Controllers

        wcscli -getscserport                    Get the
                                               configuration of a serial port on the
                                               specified blade.

        wcscli -ncidon                          Get Status of blue ID light
                                               on the front of the Chassis
                                               chassis

        wcscli -ncidon                          Turn on the blue ID light
                                               on the front of the Chassis
                                               chassis

        wcscli -ncidoff                         Turns off the blue ID light
                                               on the front of the Chassis
                                               chassis.

        wcscli -startfcfwupdate                 Start the Fan Controller
                                               Firmware update.

        wcscli -startscfwupdate                 Start the blade
                                               Firmware update.

        wcscli -startscalert                    Start snmp alert for
                                               the blade.

        wcscli -stopscalert                     Stop snmp alert for
                                               blade.

        wcscli -stopfcalert                     Stop snmp alert for
                                               fan controller

        wcscli -startfcalert                    Start snmp alert
                                               for fan controller

        wcscli -getsnmpcfg                      read the SNMP configuration for
                                               the Chassis

        wcscli -addsnmpcfg                      add the SNMP configuration for
                                               the Chassis

        wcscli -delsnmpcfg                      delete the SNMP configuration for
                                               the Chassis

        wcscli -testsnmp                        test the SNMP configuration for
                                               the Chassis

        wcscli -setdroles                       Force the two Node
                                               Controllers to get their default roles.

        wcscli -setrroles                       Force the two Node
                                               Controllers to get their reversed roles.

        wcscli -getroles                        Get the
                                               MASTER/SLAVE assignments for each of
                                               the two Node Controllers.

        wcscli -getncstate                      Get the
                                               online/offline/not-present/etc. state
                                               of the node controllers.

        wcscli -setncconsole                    Set the configuration of
                                               a serial port on the specified blade.

        wcscli -getncconsole                    Get the configuration of
                                               a serial port on the specified blade.

        wcscli -scidon                          Turn on the blue ID LED
                                               on each blade.

        wcscli -scidoff                         Turn off the blue ID LED
                                               on each blade.

        wcscli -changeuser                      Change settings of
                                               existing users.

        wcscli -adduser                         Add a new users.

        wcscli -listusers                       List all the users.
                                               The command returns only usernames.

        wcscli -deluser                         Delete an existing
                                               users.

        wcscli -factdefaults                    Reset the system to
                                               factory defaults.

        wcscli -powerinton                      Turn on the AC socket
                                               of the Chassis

        wcscli -powerintoff                     Turn off the AC socket
                                               of the Chassis

        wcscli -setnccfg                        Set the configuration for
                                               the Chassis

        wcscli -getnccfg                        Get the configuration for
                                               the Chassis

        wcscli -getpowerintstate                Get the AC Socket current
                                               on/off state of the Chassis

        wcscli -reboot                          Reboot the system

        wcscli -resetsc                         Reset the specified Blade
                                               of the Chassis

        wcscli -sclinkstatus                    get the physical link
                                               of the specified Blade

        wcscli -getdebugdata                    Provide all debug information
                                               of node controller.

        wcscli -setcfgfile                      Set information about Chassis node
                                               in Config File.

        wcscli -getcfgfile                      Get information about Chassis node
                                               from Config File.

        wcscli -getbbuinfo                      Get the MAC Address
                                               if BBU is present.

        wcscli -psupoweron                      to power on an individual power supply
                                               or for all of them.

        wcscli -psupoweroff                     to power off an individual power supply
                                               or for all of them.

        wcscli -getpsupowerstate                to get the current power on/off state
                                               for an individual power supply or all of
                                               them.

        wcscli -getinverterinfo                 to get information about the inveter
                                               unit, if present

        wcscli -getbbuinfo                      to get information about the UPS/battery backup
                                               unit, if present

        wcscli -showgbports                     to show MAC address of GB ports

        wcscli -resetgbports                    to reset GB ports

        wcscli -setfccfg                        to set FC configuration

        wcscli -getfccfg                        to get FC configuration

        wcscli -setsshkeycfg                    to set SSH Key configuration

        wcscli -getsshkeycfg                    to get SSH Key configuration

        wcscli -getsshkey                       to download SSH Key file from configured server

        wcscli -clearsshkey                     to delete SSH Key file;
        
            */
    }
}


