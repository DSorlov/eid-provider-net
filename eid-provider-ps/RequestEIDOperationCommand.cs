using System;
using System.CodeDom;
using System.Collections;
using System.Management.Automation;  // Windows PowerShell assembly.
using System.Threading;

namespace com.sorlov.eidprovider.ps
{
    public class RequestEIDOperationCommandSigningDynamicParameters
    {
        [Parameter(Position = 3, Mandatory =true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty()]
        public string Text
        {
            get => text;
            set => text = value;
        }
        private string text = string.Empty;
    }

    // Declare the class as a cmdlet and specify the
    // appropriate verb and noun for the cmdlet name.
    [Cmdlet(VerbsLifecycle.Request, "EIDOperation", SupportsShouldProcess=true)]
    [OutputType("System.Management.Automation.PSObject#com.sorlov.eidprovider.EIDResult")]
    public class RequestEIDOperationCommand : Cmdlet, IDynamicParameters
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

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
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

        [Parameter(Position = 4)]
        public SwitchParameter Wait
        {
            get => wait;
            set => wait = value;
        }
        private bool wait;

        public object GetDynamicParameters()
        {
            if (type == EIDTypesEnum.sign)
            {
                context = new RequestEIDOperationCommandSigningDynamicParameters();
                return context;
            }

            return null;
        }
        private RequestEIDOperationCommandSigningDynamicParameters context;

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
                        break;
                    case EIDModulesEnum.frejaeid:
                        client = new frejaeid.Client((frejaeid.InitializationData)config);
                        break;
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
            if (ShouldProcess(id))
            {
                if (wait)
                {

                    EIDResult initRequest = (type == EIDTypesEnum.auth) ? client.InitAuthRequest(id) : client.InitSignRequest(id, context.Text);
                    WriteObject(PSObjectConverter.EIDResult(initRequest));

                    if (initRequest.Status != EIDResult.ResultStatus.initialized) return;

                    while (true)
                    {
                        Thread.Sleep(2000);
                        EIDResult pollRequest = (type == EIDTypesEnum.auth) ? client.PollAuthRequest((string)initRequest["id"]) : client.PollSignRequest((string)initRequest["id"]);
                        WriteObject(PSObjectConverter.EIDResult(pollRequest));

                        if (pollRequest.Status == EIDResult.ResultStatus.error || pollRequest.Status == EIDResult.ResultStatus.completed || pollRequest.Status == EIDResult.ResultStatus.cancelled)
                            return;

                    }
                }
                else
                {
                    WriteObject(PSObjectConverter.EIDResult(type == EIDTypesEnum.auth ? client.InitAuthRequest(id) : client.InitSignRequest(id,context.Text)));
                }
            }
        }
    }
}