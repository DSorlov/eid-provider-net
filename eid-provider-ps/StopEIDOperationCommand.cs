﻿using System;
using System.CodeDom;
using System.Collections;
using System.Management.Automation;  // Windows PowerShell assembly.

namespace com.sorlov.eidprovider.ps
{
    // Declare the class as a cmdlet and specify the
    // appropriate verb and noun for the cmdlet name.
    [Cmdlet(VerbsLifecycle.Stop, "EIDOperation", SupportsShouldProcess = true)]
    [OutputType("System.Management.Automation.PSObject#com.sorlov.eidprovider.EIDResult")]
    public class StopEIDOperation : Cmdlet
    {
        

        // Declare the parameters for the cmdlet.
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty()]
        public EIDClientInitializationData Configuration
        {
            get => config;
            set => config = value;
        }
        private EIDClientInitializationData config;

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName=true)]
        public EIDTypesEnum Type
        {
            get => type;
            set => type = value;
        }
        private EIDTypesEnum type;

        [Parameter(Position = 2, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty()]
        public string Id
        {
            get => id;
            set => id = value;
        }
        private string id;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            string typeString = config.GetType().FullName.Replace("com.sorlov.eidprovider.", "").Replace(".InitializationData", "");
            try
            {
                module = (EIDModulesEnum)Enum.Parse(typeof(EIDModulesEnum), typeString);
                switch (module)
                {
                    case EIDModulesEnum.bankid:
                        client = new bankid.Client((bankid.InitializationData)config);
                        return;
                    case EIDModulesEnum.frejaeid:
                        client = new frejaeid.Client((frejaeid.InitializationData)config);
                        return;
                    default:
                        WriteError(new ErrorRecord(new ProviderNotFoundException(module.ToString() + " is not supported in this version of eid-provider-ps, upgrade?"), "101", ErrorCategory.InvalidArgument, Configuration));
                        StopProcessing();
                        return;
                }

            }
            catch (ArgumentException)
            {
                WriteError(new ErrorRecord(new ProviderNotFoundException(typeString + " is not a valid EID Module"), "100", ErrorCategory.InvalidArgument, Configuration));
                StopProcessing();
                return;
            }
        }
        private EIDModulesEnum module;
        private EIDClient client;

        protected override void ProcessRecord()
        {
                if (ShouldProcess(id, "Cancel"))
                {
                    WriteObject(PSObjectConverter.EIDResult(type == EIDTypesEnum.auth ? client.CancelAuthRequest(id) : client.CancelSignRequest(id)));
                }
        }

    }
}