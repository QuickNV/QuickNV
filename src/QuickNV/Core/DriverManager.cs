using Quick.Protocol;
using System.IO;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Core
{
    public class DriverManager
    {
        public static DriverManager Instance { get; private set; } = new DriverManager();
        private Dictionary<string, DriverContext> driverContextDict;

        public DriverContext[] DriverContexts { get; private set; } = new DriverContext[0];
        public event EventHandler DriverChanged;

        private void RaiseDriverChanged()
        {
            DriverChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RegisterDriver(QpChannel channel, DriverInfo driverInfo)
        {
            lock (driverContextDict)
            {
                var driverContext = new DriverContext(channel, driverInfo);
                driverContextDict[driverInfo.Id] = driverContext;
                DriverContexts = driverContextDict.Values.ToArray();
            }
            RaiseDriverChanged();
        }

        public void UnregisterDriver(QpChannel channel, DriverInfo driverInfo)
        {
            lock (driverContextDict)
            {
                if (driverContextDict.TryGetValue(driverInfo.Id, out var driverContext))
                {
                    driverContextDict.Remove(driverInfo.Id);
                    DriverContexts = driverContextDict.Values.ToArray();
                    driverContext.Unregister();
                }
            }
            RaiseDriverChanged();
        }

        public DriverContext GetDriverContext(string driverId)
        {
            if (string.IsNullOrEmpty(driverId))
                return null;
            lock (driverContextDict)
                if (driverContextDict.ContainsKey(driverId))
                    return driverContextDict[driverId];
            return null;
        }

        public void Start()
        {
            driverContextDict = new Dictionary<string, DriverContext>();
        }

        public void Stop()
        {
            lock (driverContextDict)
            {
                driverContextDict.Clear();
            }
            RaiseDriverChanged();
        }
    }
}
