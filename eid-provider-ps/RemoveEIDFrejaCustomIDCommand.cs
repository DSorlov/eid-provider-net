using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;  // Windows PowerShell assembly.

namespace com.sorlov.eidprovider.ps
{
    // Declare the class as a cmdlet and specify the
    // appropriate verb and noun for the cmdlet name.
    [Cmdlet(VerbsCommon.Remove, "EIDFrejaCustomID")]
    [OutputType("com.sorlov.eidprovider.EIDResult")]
    public class RemoveEIDFrejaCustomIDCommand : Cmdlet
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
        public string CustomId
        {
            get => customId;
            set => customId = value;
        }
        private string customId;

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
            if (ShouldProcess(customId,"Remove custom id"))
            {
                EIDResult result = ((frejaeid.Client)client).DeleteCustomIdentifier(customId);

                List<PSNoteProperty> addProps = new List<PSNoteProperty>()
                {
                    new PSNoteProperty("CustomId", customId)
                };
                List<string> addDefaults = new List<string>()
                {
                    "CustomId"
                };

                WriteObject(PSObjectConverter.EIDResult(result, addProps, addDefaults));
            }
        }
    }
}