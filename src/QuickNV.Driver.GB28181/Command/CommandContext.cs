using SIPSorcery.SIP;

namespace QuickNV.Driver.GB28181.Command
{
    public class CommandContext
    {
        public static string GenerateNewId() => Guid.NewGuid().ToString("N").ToLower();
        public string Id { get; private set; }
        private Exception commandException;
        private bool isTimeout = false;
        private SIPResponse response;
        public Task<SIPResponse> ResponseTask { get; private set; }

        public CommandContext(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));
            Id = id;
            ResponseTask = new Task<SIPResponse>(() =>
            {
                if (isTimeout)
                    throw new TimeoutException();
                if (commandException != null)
                    throw commandException;
                return response;
            });
        }

        public virtual void SetResponse(Exception ex)
        {
            if (isTimeout)
                return;
            this.commandException = ex;
            if (ResponseTask.Status == TaskStatus.Created)
                ResponseTask.Start();
        }

        public virtual void SetResponse(SIPResponse response)
        {
            if (isTimeout)
                return;

            this.response = response;

            if (ResponseTask.Status == TaskStatus.Created)
                ResponseTask.Start();
        }

        public virtual void Timeout()
        {
            isTimeout = true;
            if (ResponseTask.Status == TaskStatus.Created)
                ResponseTask.Start();
        }
    }
}
