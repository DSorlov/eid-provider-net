using System;
using System.CodeDom;
using System.Collections;
using System.Management.Automation;  // Windows PowerShell assembly.

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
    [OutputType("com.sorlov.eidprovider.ApiResponse")]
    public class RequestEIDOperationCommand : Cmdlet, IDynamicParameters
    {
        // Declare the parameters for the cmdlet.
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty()]
        public IInitializationData Configuration
        {
            get => config;
            set => config = value;
        }
        private IInitializationData config;

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
            if (type == EIDTypesEnum.Signing)
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
                module = (Modules)Enum.Parse(typeof(Modules), typeString);

                switch (module)
                {
                    case Modules.bankid:
                        client = new bankid.Client((bankid.InitializationData)config);
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
        private Modules module;
        private IClient client;

        protected override void ProcessRecord()
        {
            if (ShouldProcess(id))
            {
                if (wait)
                {
                    client.StatusUpdate += Client_StatusUpdate;
                    WriteObject(type == EIDTypesEnum.Authentication ? client.AuthRequest(id) : client.SignRequest(id, context.Text));
                }
                else
                {
                    WriteObject(type == EIDTypesEnum.Authentication ? client.InitAuthRequest(id) : client.InitSignRequest(id,context.Text));
                }
            }
        }

        private void Client_StatusUpdate(object sender, EventArgs e)
        {
            WriteObject(((ApiEventArgs)e).ApiResponse);
        }

    }
}