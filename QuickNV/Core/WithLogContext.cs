using System.ComponentModel.DataAnnotations.Schema;

namespace QuickNV.Core
{
    public abstract class WithLogContext
    {
        private class DefaultWithLogContext : WithLogContext { }

        public static WithLogContext CreateLogContext() => new DefaultWithLogContext();

        private Queue<string> logQueue = new Queue<string>();
        [NotMapped]
        public virtual int MaxLogLines => 1000;
        public event EventHandler<string> NewLogPushed;

        public void PushLog(string message)
        {
            try
            {
                var line = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {message}";
                lock (logQueue)
                {
                    logQueue.Enqueue(line);
                    while (logQueue.Count > MaxLogLines)
                        logQueue.Dequeue();
                }
                NewLogPushed?.Invoke(this, line);
            }
            catch { }
        }

        public string[] GetLogLines()
        {
            lock (logQueue)
                return logQueue.ToArray();
        }
    }
}
