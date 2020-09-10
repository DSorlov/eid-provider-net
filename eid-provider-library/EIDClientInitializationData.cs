using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.sorlov.eidprovider
{
    public abstract class EIDClientInitializationData : Dictionary<string, string>
    {
        protected EIDClientInitializationData(EIDEnvironment environment)
        {
        }

    }
}
