using System;
using System.Collections;
using System.Threading.Tasks;

namespace com.sorlov.eidprovider
{
    public interface IClient
    {

        event EventHandler StatusUpdate;

        ApiResponse InitAuthRequest(string id);
        ApiResponse PollAuthRequest(string requestId);
        ApiResponse CancelAuthRequest(string requestId);
        ApiResponse AuthRequest(string id);
        ApiResponse InitSignRequest(string id, string agreementText);
        ApiResponse PollSignRequest(string requestId);
        ApiResponse CancelSignRequest(string requestId);
        ApiResponse SignRequest(string id, string agreementText);
    }
}
