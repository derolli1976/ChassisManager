## MCS Chassis Manager
Microsoft Cloud Server Chassis Manager is a management software for rack level devices like server, fan and PSU. 
It primarily consists of two modules -- Chassis Manager Service and WcsCli. Chassis Manager Service provides implementation to manage various sub-services like fan service, PSU service, power control service, etc. The WcsCli provides a framework to carry out system management operations. This framework is exposed in two forms -- RESTful APIs for automated management; and a command-line interface for manual management.

The intent of this community project is to collaborate with [Open Compute Project (OCP)] (http://www.opencompute.org/) to build a thriving ecosystem of OSS within OCP and contribute this project to OCP.

If your intent is to use the Chassis Manager software without contributing back to this project, then use the MASTER branch which holds the approved and stable public releases.

If your goal is to improve or extend the code and contribute back to this project, then you should make your changes in, and submit a pull request against, the DEVELOPMENT branch. Read through our wiki section on [how to contribute] (https://github.com/MSOpenTech/MCS-ChassisManager/wiki/how-to-contribute) for a walk-through of the contribution process.

All new work should be in the development branch. Master is now reserved to tag builds.


## Quick Start

- Clone the repo: git clone https://github.com/MSOpenTech/MCS-ChassisManager.git

- Download the zip version of the repo (see the right-side pane)

- Microsoft Visual Studio build environment. README contains further instructions on how to build the project and generate required executables. 


## Components Included

(i) ChassisManager -- This folder contains all source/related files for the Chassis Manager Service. The Chassis Manager service includes 6 main services related to managing fan, PSU, power control, blade management, Top-of-rack (TOR), security and chassis manager control. 

(ii) Contracts -- This folder contains all related files for Windows Chassis Manager service contract.

(iii) IPMI -- This folder contains all source/related files for the implementation of native Windows intelligent platform management interface (IPMI) driver. This is required to provide the capability of in-band management of servers through the operating system. 

(iv) WcsCli -- This folder contains all source/related files for the framework that the Chassis Manager (CM) leverages to manage the rack level devices. Through this module, a CM provides the front end through the application interface (RESTful web API) for automated management and the command-line interface for manual management. It implements various commands required to manage all devices within the rack and to establish communication directly with the blade management system through a serial multiplexor.

## Prerequisites

- .Net Framework 4.0 Full version

- .Net Framework 2.0 Software Development Kit (SDK)

- Visual Studio for building and testing solution

- Windows machine: Windows Server operating system

- Note that no other external dependencies (DLLs or EXEs) are required to be installed as all are self contained in respective project directory. 


## BUILD and Install Instructions

MCS-ChassisManager is developed in Microsoft Visual Studio environment and is completely written in C#. To build the serivce (ChassisManager) or command management interface (WcsCli), please follow the below steps:

- Import the project in Visual Studio by browsing and importing the specific project solution file. We have tested this on both Visual Studio 2012 Ultimate and Visual Studio Express versions.

- Build the project in Visual Studio by going to menu->BUILD->Build Solution or Ctrl+Shift+B.

- After successful build, the project executable is created under a newly created sub-directory called 'bin' (under the parent project directory). 


To install Chassis Manager Service, use the following commands:

Start service: net start chassismanager

Stop service: net stop chassismanager

## Test Instructions

We are working on providing a suite of packaged test cases soon.







