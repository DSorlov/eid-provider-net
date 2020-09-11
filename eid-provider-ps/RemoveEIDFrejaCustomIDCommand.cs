using System;
using System.IO;
using System.Management.Automation;  // Windows PowerShell assembly.

namespace com.sorlov.eidprovider.ps
{
    // Declare the class as a cmdlet and specify the
    // appropriate verb and noun for the cmdlet name.
    [Cmdlet(VerbsCommon.Remove, "EIDFrejaCustomID")]
    [OutputType("com.sorlov.eidprovider.PSCustomObject")]
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
        public string CustomID
        {
            get => customID;
            set => customID = value;
        }
        private string customID;

        private EIDClient client;
        private EIDModulesEnum module;
        private PSPropertySet defaltDisplayProperties;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            string typeString = config.GetType().FullName.Replace("com.sorlov.eidprovider.", "").Replace(".InitializationData", "");
            try
            {
                defaltDisplayProperties = new PSPropertySet("DefaultDisplayPropertySet", new[] { "CustomID", "Result" });
                module = (EIDModulesEnum)Enum.Parse(typeof(EIDModulesEnum), typeString);

                if (module == EIDModulesEnum.frejaeid)
                {
                    client = new frejaeid.Client((frejaeid.InitializationData)config);
                    return;
                }

                WriteError(new ErrorRecord(new ProviderNotFoundException("This command only supports frejaeid or frejaorgid as config"), "101", ErrorCategory.InvalidArgument, Configuration));
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
            if (ShouldProcess(customID))
            {
                PSObject resultObject = new PSObject();
                resultObject.Members.Add(new PSMemberSet("PSStandardMembers", new[] { defaltDisplayProperties }));
                resultObject.Members.Add(new PSNoteProperty("CustomID", customID));

                try
                {
                    if (module == EIDModulesEnum.frejaeid)
                        ((frejaeid.Client)client).DeleteCustomIdentifier(customID);
                    else
                        ((frejaeid.Client)client).DeleteCustomIdentifier(customID);

                    resultObject.Members.Add(new PSNoteProperty("Success", true));
                    resultObject.Members.Add(new PSNoteProperty("Details", "Custom ID removed successfully."));
                }
                catch (Exception e)
                {
                    resultObject.Members.Add(new PSNoteProperty("Success", false));
                    resultObject.Members.Add(new PSNoteProperty("Details", e.Message));
                    WriteError(new ErrorRecord(new InvalidDataException(e.Message), "100", ErrorCategory.InvalidArgument, CustomID));
                }

                WriteObject(resultObject);
            }
        }
    }
}