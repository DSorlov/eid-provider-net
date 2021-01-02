![stability-stable](https://img.shields.io/badge/stability-stable-green.svg)]
![version](https://img.shields.io/badge/version-0.0.2-green.svg)
![maintained](https://img.shields.io/maintenance/yes/2021.svg)
[![maintainer](https://img.shields.io/badge/maintainer-daniel%20sörlöv-blue.svg)](https://github.com/DSorlov)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://img.shields.io/github/license/DSorlov/eid-provider-net)

# eid-provider-net
This code is developed to enable rapid deployment of identity based authentication for .NET by creating a common interface to most of the suppliers for official electronic identification and it allows you to mix and match your suppliers. This is a .NET port from code that I have contributed in [eid-provider](https://github.com/DSorlov/eid-provider) and that is used in multiple projects. This library is using dotnet5.0.

### eid-provider-net library
A .net library that is performing all the operations towards the modules as outlined in the table below and the working horse of this project.
See the [basic method documentation](docs/methods.md) or the [basic examples](docs/examples.md).

### eid-provider-net powershell module
A powershell cmdlet project that provides a module for use with PowerShell to make sure simple admin devops easily can be used to interact with the library in scripts and wherever else it is needed, makes output more powershell friendly and is allaround a bit nicer to work with for interactive or scripting purposes.
See [powershell examples](docs/powershell_examples.md).

### Supported integrations
There are basically right now two main types of integrations: one is working directly with the service apis and the other kind is working with a broker service. The broker services can be usefull if you have many integrations or other sources in your enterprise and you wish to use the same sources for these. Right now I am working on moving over and adapting the code for the providers for [eid-provider](https://github.com/DSorlov/eid-provider) and will be added as they are needed and updated, submit an issue if you need to get one of them prioritized as most of my code uses the services apis directly.

| ID-Type | Module | Vendor | Authentication | Signing | Geographies | Readiness |
| --- | --- | --- | --- | --- | --- | --- |
| BankID | [bankid](docs/bankid.md) | BankID | :heavy_check_mark: | :heavy_check_mark: | :sweden: | Production |
| Freja eID and Freja Org ID | [frejaeid](docs/frejaeid.md) | Freja eID | :heavy_check_mark: | :heavy_check_mark: | :sweden: | Production |


