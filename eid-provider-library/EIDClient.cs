using System;
using System.Threading;
using System.Threading.Tasks;

namespace com.sorlov.eidprovider
{
    public abstract class EIDClient
    {
        protected EIDClientInitializationData configuration;

        protected EIDClient(EIDClientInitializationData configuration)
        {
            this.configuration = configuration;
        }

        //Provide a classic event handler type of support
        public event EventHandler<EIDClientEvent> RequestEvent;
        protected virtual void OnRequestEvent(EIDClientEvent e)
        {
            RequestEvent?.Invoke(this, e);
        }

        // All sync operations, most are handled by downstream modules
        public abstract EIDResult InitAuthRequest(string id);
        public abstract EIDResult InitSignRequest(string id, string text);
        public abstract EIDResult PollAuthRequest(string id);
        public abstract EIDResult PollSignRequest(string id);
        public abstract EIDResult CancelAuthRequest(string id);
        public abstract EIDResult CancelSignRequest(string id);
        public EIDResult AuthRequest(string id) => request(id, null).Result;
        public EIDResult SignRequest(string id, string text) => request(id, text).Result;

        // Provide a Async interface layer by wrapping above methods
        public async Task<EIDResult> InitAuthRequestAsync(string id) => await Task.Run(() => { return InitAuthRequest(id); });
        public async Task<EIDResult> InitSignRequestAsync(string id, string text) => await Task.Run(() => { return InitSignRequest(id, text); });
        public async Task<EIDResult> PollAuthRequestAsync(string id) => await Task.Run(() => { return PollAuthRequest(id); });
        public async Task<EIDResult> PollSignRequestAsync(string id) => await Task.Run(() => { return PollSignRequest(id); });
        public async Task<EIDResult> CancelAuthRequestAsync(string id) => await Task.Run(() => { return CancelAuthRequest(id); });
        public async Task<EIDResult> CancelSignRequestAsync(string id) => await Task.Run(() => { return CancelSignRequest(id); });
        public async Task<EIDResult> AuthRequestAsync(string id) => await request(id, null);
        public async Task<EIDResult> SignRequestAsync(string id, string text) => await request(id, text);
        public async Task<EIDResult> AuthRequestAsync(string id, IProgress<EIDResult> progress) => await request(id, null, progress);
        public async Task<EIDResult> SignRequestAsync(string id, string text, IProgress<EIDResult> progress) => await request(id, text, progress);
        public async Task<EIDResult> AuthRequestAsync(string id, CancellationToken ct) => await request(id, null, null, ct);
        public async Task<EIDResult> SignRequestAsync(string id, string text, CancellationToken ct) => await request(id, text, null, ct);
        public async Task<EIDResult> AuthRequestAsync(string id, IProgress<EIDResult> progress, CancellationToken ct) => await request(id, null, progress, ct);
        public async Task<EIDResult> SignRequestAsync(string id, string text, IProgress<EIDResult> progress, CancellationToken ct) => await request(id, text, progress, ct);


        // The master request handler for everything that is a long running p
        private async Task<EIDResult> request(string id, string text, IProgress<EIDResult> progress = null, CancellationToken ct = default)
        {
            return await Task.Run(() => {
                EIDResult initRequest = String.IsNullOrEmpty(text) ? InitAuthRequest(id) : InitSignRequest(id, text);
                if (initRequest.Status != EIDResult.ResultStatus.initialized) return initRequest;

                progress?.Report(initRequest);
                OnRequestEvent(new EIDClientEvent(initRequest));

                while (true)
                {
                    Thread.Sleep(2000);
                    EIDResult pollRequest = String.IsNullOrEmpty(text) ? PollAuthRequest((string)initRequest["id"]) : PollSignRequest((string)initRequest["id"]);

                    if (pollRequest.Status == EIDResult.ResultStatus.error || pollRequest.Status == EIDResult.ResultStatus.completed || pollRequest.Status == EIDResult.ResultStatus.cancelled)
                        return pollRequest;

                    progress?.Report(pollRequest);
                    OnRequestEvent(new EIDClientEvent(pollRequest));

                    if (ct.IsCancellationRequested)
                    {
                        EIDResult cancelRequest = String.IsNullOrEmpty(text) ? CancelAuthRequest((string)initRequest["id"]) : CancelSignRequest((string)initRequest["id"]);
                        progress?.Report(cancelRequest);
                        OnRequestEvent(new EIDClientEvent(cancelRequest));
                        ct.ThrowIfCancellationRequested();
                    }
                }
            });
        }
    }
}
