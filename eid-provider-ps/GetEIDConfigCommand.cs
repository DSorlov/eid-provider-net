using System.Management.Automation;  // Windows PowerShell assembly.

namespace com.sorlov.eidprovider.ps
{
    // Declare the class as a cmdlet and specify the
    // appropriate verb and noun for the cmdlet name.
    [Cmdlet(VerbsCommon.Get, "EIDConfig")]
    [OutputType("com.sorlov.eidprovider.IInitializationData")]
    public class GetEIDConfig : Cmdlet
    {
        // Declare the parameters for the cmdlet.
        [Parameter(Position = 0, Mandatory = true)]
        public Modules Module
        {
            get => module;
            set => module = value;
        }
        private Modules module;

        [Parameter(Position = 1)]
        [ValidateNotNullOrEmpty()]
        public Enviroments Enviroment
        {
            get => enviroment;
            set => enviroment = value;
        }
        private Enviroments enviroment = Enviroments.testing;

        // Override the ProcessRecord method to process
        // the supplied user name and write out a
        // greeting to the user by calling the WriteObject
        // method.
        protected override void ProcessRecord()
        {
            switch (module)
            {
                case Modules.bankid:
                    WriteObject(new bankid.InitializationData(enviroment));
                    break;
                default:
                    WriteError(new ErrorRecord(new ProviderNotFoundException(Modules.bankid.ToString() + " is not supported in this version of eid-provider-ps, upgrade?"), "101", ErrorCategory.InvalidArgument, Module));
                    break;
            }
        }
    }
}