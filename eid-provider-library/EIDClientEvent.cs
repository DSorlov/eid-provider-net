using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.sorlov.eidprovider
{
    public class EIDClientEvent : EventArgs
    {
        public EIDResult Result
        {
            get => result;
        }
        private EIDResult result;

        internal EIDClientEvent(EIDResult result)
        {
            this.result = result;
        }
    }
}
