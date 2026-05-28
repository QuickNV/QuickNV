using YiQiDong.Agent;

namespace QuickNV.Core
{
    public class AgentContextLogger : ILogger
    {
        private string prefix;
        private bool enableTrace, enableDebug, enableInfo, enableError;
        public AgentContextLogger(
            string prefix,
            bool enableTrace = true,
            bool enableDebug = true,
            bool enableInfo = true,
            bool enableError = true)
        {
            this.prefix = prefix;
            this.enableTrace = enableTrace;
            this.enableDebug = enableDebug;
            this.enableInfo = enableInfo;
            this.enableError = enableError;
        }


        public void LogTrace(string message)
        {
            if (!enableTrace)
                return;
            if (string.IsNullOrEmpty(prefix))
                AgentContext.LogTrace(message);
            else
                AgentContext.LogTrace(prefix + message);
        }

        public void LogDebug(string message)
        {
            if (!enableDebug)
                return;
            if (string.IsNullOrEmpty(prefix))
                AgentContext.LogDebug(message);
            else
                AgentContext.LogDebug(prefix + message);
        }

        public void LogInfo(string message)
        {
            if (!enableInfo)
                return;
            if (string.IsNullOrEmpty(prefix))
                AgentContext.LogInfo(message);
            else
                AgentContext.LogInfo(prefix + message);
        }

        public void LogError(string message)
        {
            if (!enableError)
                return;
            if (string.IsNullOrEmpty(prefix))
                AgentContext.LogError(message);
            else
                AgentContext.LogError(prefix + message);
        }
    }
}
