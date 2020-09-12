using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;  // Windows PowerShell assembly.

namespace com.sorlov.eidprovider.ps
{
    // Declare the class as a cmdlet and specify the
    // appropriate verb and noun for the cmdlet name.
    [Cmdlet(VerbsCommon.Remove, "EIDFrejaOrgID")]
    [OutputType("com.sorlov.eidprovider.EIDResult")]
    public class RemoveEIDFrejaOrgIDCommand : Cmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty()]
        public EIDClientInitializationData Configuration
        {
            get => config;
            set => config = value;
        }
        private EIDClientInitializationData config;

        [Parameter(Position = 2, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty()]
        public string OrgId
        {
            get => orgId;
            set => orgId = value;
        }
        private string orgId;

        private EIDClient client;
        private EIDModulesEnum module;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            string typeString = config.GetType().FullName.Replace("com.sorlov.eidprovider.", "").Replace(".InitializationData", "");
            try
            {
                module = (EIDModulesEnum)Enum.Parse(typeof(EIDModulesEnum), typeString);

                if (module == EIDModulesEnum.frejaeid)
                {
                    client = new frejaeid.Client((frejaeid.InitializationData)config);
                    return;
                }

                WriteError(new ErrorRecord(new ProviderNotFoundException("This command only supports frejaeid as config"), "101", ErrorCategory.InvalidArgument, Configuration));
                StopProcessing();

            }
            catch (ArgumentException)
            {
                WriteError(new ErrorRecord(new ProviderNotFoundException(typeString + " is not a valid EID Module"), "100", ErrorCategory.InvalidArgument, Configuration));
                StopProcessing();
                return;
            }


        }

        protected override void ProcessRecord()
        {
            if (ShouldProcess(orgId,"Remove org id"))
            {
                EIDResult result = ((frejaeid.Client)client).DeleteOrgId(orgId);

                List<PSNoteProperty> addProps = new List<PSNoteProperty>()
                {
                    new PSNoteProperty("OrgId", orgId)
                };
                List<string> addDefaults = new List<string>()
                {
                    "OrgId"
                };

                WriteObject(PSObjectConverter.EIDResult(result, addProps, addDefaults));
            }
        }
    }
}