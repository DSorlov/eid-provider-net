[![stability-stable](https://img.shields.io/badge/stability-beta-red.svg)](#)
[![version](https://img.shields.io/badge/version-0.0.1-red.svg)](#)
[![maintained](https://img.shields.io/maintenance/yes/2020.svg)](#)
[![maintainer](https://img.shields.io/badge/maintainer-daniel%20sörlöv-blue.svg)](https://github.com/DSorlov)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://img.shields.io/github/license/DSorlov/eid-provider)

# eid-provider-net
This module is developed to enable rapid deployment of identity based authentication for .NET by creating a common interface to most of the suppliers for official electronic identification and it allows you to mix and match your suppliers. This is a .NET port from code that I have contributed in [eid-provider](https://github.com/DSorlov/eid-provider) and that is used in multiple projects. Documentation will be updated closer to release.

| :warning:  This library is not relased yet for production and lacking documentation!   |
|----------------------------------------------------------|

| :warning:  This library requires .NET 5.0 to run!   |
|----------------------------------------------------------|

The code in this repo consists of two projects (binary releases will be available once I get a bit further into the project). The first is the C# library that is performing all the operations towards the modules as outlined below and the other is a powershell cmdlet project that provides a module for use with PowerShell to make sure simple admin devops easily can be used to interact with the library.

There are basically right now two main types of integrations: one is working directly with the service apis and the other kind is working with a broker service. The broker services can be usefull if you have many integrations or other sources in your enterprise and you wish to use the same sources for these. Right now I am working on moving over and adapting the code for the providers for [eid-provider](https://github.com/DSorlov/eid-provider) and these will all be availiable before first stable release.

| ID-Type | Module | Vendor | Authentication | Signing | Geographies | Readiness |
| --- | --- | --- | --- | --- | --- | --- |
| BankID | [bankid](docs/bankid.md) | BankID | :heavy_check_mark: | :heavy_check_mark: | :sweden: | Production |
| Freja eID | [frejaeid](docs/frejaeid.md) | BankID | :heavy_check_mark: | :heavy_check_mark: | :sweden: | Production |


