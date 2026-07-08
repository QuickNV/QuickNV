namespace QuickNV.Core
{
    public interface ILogger
    {
        public void LogTrace(string message);
        public void LogDebug(string message);
        public void LogInfo(string message);
        public void LogError(string message);
    }
}
