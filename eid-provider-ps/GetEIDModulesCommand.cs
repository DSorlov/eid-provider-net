using System;
using System.Management.Automation;  // Windows PowerShell assembly.

namespace com.sorlov.eidprovider.ps
{
    // Declare the class as a cmdlet and specify the
    // appropriate verb and noun for the cmdlet name.
    [Cmdlet(VerbsCommon.Get, "EIDModules")]
    [OutputType("com.sorlov.eidprovider.Modules")]
    public class GetEIDModulesCommand : Cmdlet
    {
        // Override the ProcessRecord method to process
        // the supplied user name and write out a
        // greeting to the user by calling the WriteObject
        // method.
        protected override void ProcessRecord()
        {
            foreach (Modules module in (Modules[])Enum.GetValues(typeof(Modules)))
            {
                WriteObject(module);
            }
            
        }
    }
}