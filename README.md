## Table of Contents

- [Introduction] (#introduction)
- [Quick Start] (#quick-start)
- [Components] (#what-included)
- [Prerequisites] (#prerequisites)
- [BUILD/Install Instructions] (#build-install)
- [Test Instructions] (#test)
- [Bug and Feature Requests] (#bug-feature)
- [Contributing] (#contributing)

## Introduction

Microsoft Cloud Server Chasssis Manager is a management software for rack level devices like server, fan and PSU.  
It mainly consists of two software modules -- Chassis Manager Service and WcsCli. Chassis Manager Service provides implementation to manage various sub-services like fan service, PSU service, power control service, etc. The WcsCli provides a framework to carry out system management operations. This framework is exposed in two forms -- RESTful API for automated management; and a command-line interface for manual management.

## Quick Start

- Clone the repo: git clone https://github.com/MSOpenTech/MCS-ChassisManager.git

- Download the zip version of the repo (see the right-side pane)

## What's included: Components

(1) ChassisManager -- This folder contains all source/related files for the Chassis Manager Service. The Chassis Manager service includes 6 main services related to managing fan, PSU, power control, blade management, Top-of-rack (TOR), security and chassis manager control. 

(2) Contracts -- This folder contains all related files for Windows Chassis Manager service contract.

(3) IPMI -- This folder contains all source/related files for the implementation of native Windows intelligent platform management interface (IPMI) driver. This is required to provide the capability of in-band management of servers through the operating system. 

(4) WcsCli -- This folder contains all source/related files for the framework that the Chassis Manager (CM)leverages to manage the rack level devices. Through this module, a CM provides the front end through the application interface (RESTful web API) for automated management and the command-line interface for manual management. It implements various commands required to manage all devices within the rack and to establish communication directly with the blade management system through a serial multiplexor.

## Prerequisites

(1) .Net Framework 4.0 Full version

(2) .Net Framework 2.0 Software Development Kit (SDK)

(3) Visual Studio for building solution

(4) Windows machine: Windows Server operating system

## BUILD and Install Instructions

MCS-ChassisManager is developed in Microsoft Visual Studio environment and is completely written in C#. To build the serivce (ChassisManager) or command management interface (WcsCli), please follow the below steps:

(i) Import the project in Visual Studio by browsing and importing the specific project solution file. We have tested this on both Visual Studio 2012 Ultimate and Visual Studio Express versions.

(ii) Build the project in Visual Studio by going to menu->BUILD->Build Solution.

(iii) After successful build, the project executable created under a newly created sub-directory called 'bin' (under the parent project directory). 


To install Chassis Manager Service, use the following:

Start service: net start chassismanager

Stop service: net stop chassismanager

## Test Instructions

[TODO:bikash]

## Bug and Feature Requests

Have a bug or a feature request? Please read our [bug/feature filing guidelines] ().

## Contributing

Please read through our [contributing guidelines] (). Included are directions for opening issues, coding standards and notes on development.

