using Quick.Protocol.Utils;
using QuickNV.YS7;
using QuickNV.YS7.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiQiDong.Agent;

namespace QuickNV.Driver.Ys7
{
    public class Ys7Context
    {
        private CancellationTokenSource cts;
        private Dictionary<string, DeviceInfo> deviceDict;
        public Ys7Client Ys7Client { get; private set; }
        public EventHandler<DeviceInfo> DeviceOnline;
        public EventHandler<DeviceInfo> DeviceOffline;

        public Ys7Context(Ys7ClientOptions options)
        {
            Ys7Client = new Ys7Client(options);
        }

        private DeviceInfo[] getDevicesFromYs7()
        {
            var currentPage = 0;
            var pageSize = 50;

            List<DeviceInfo> list = new List<DeviceInfo>();
            while (true)
            {
                var ret = Ys7Client.GetDeviceListAsync(currentPage, pageSize).Result;
                if (ret.data == null || ret.data.Length <= 0)
                    break;
                list.AddRange(ret.data);
                currentPage++;
                if (currentPage * pageSize >= ret.page.total)
                    break;
            }
            return list.ToArray();
        }

        public void Start()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            deviceDict = getDevicesFromYs7().ToDictionary(t => t.deviceSerial, t => t);
            checkDeviceList();
            beginCheckDeviceList(cts.Token);
        }

        public DeviceInfo GetDevice(string deviceSerial)
        {
            if (deviceDict == null)
                return null;
            lock (deviceDict)
                if (deviceDict.TryGetValue(deviceSerial, out var device))
                    return device;
            return null;
        }

        public DeviceInfo[] GetDevices()
        {
            lock (deviceDict)
                return deviceDict.Values.ToArray();
        }

        private void noticeDeviceStateChanged(DeviceInfo device)
        {
            switch (device.status)
            {
                case DeviceStatus.Offline:
                    DeviceOffline?.Invoke(this, device);
                    break;
                case DeviceStatus.Online:
                    DeviceOnline?.Invoke(this, device);
                    break;
            }
        }

        private void checkDeviceList()
        {
            var currentDeviceDict = getDevicesFromYs7().ToDictionary(t => t.deviceSerial, t => t);
            lock (deviceDict)
            {
                //检查新增的设备
                foreach (var deviceSerial in currentDeviceDict.Keys)
                {
                    var currentDevice = currentDeviceDict[deviceSerial];
                    if (deviceDict.ContainsKey(deviceSerial))
                    {
                        var device = deviceDict[deviceSerial];
                        if (device.status != currentDevice.status)
                        {
                            device.status = currentDevice.status;
                            noticeDeviceStateChanged(device);
                        }
                    }
                    else
                    {
                        deviceDict[deviceSerial] = currentDevice;
                        noticeDeviceStateChanged(currentDevice);
                    }
                }
                //检查删除的设备
                foreach (var deviceSerial in deviceDict.Keys)
                {
                    var device = deviceDict[deviceSerial];
                    if (!currentDeviceDict.ContainsKey(deviceSerial))
                    {
                        if (device.status == DeviceStatus.Online)
                        {
                            device.status = DeviceStatus.Offline;
                            noticeDeviceStateChanged(device);
                        }
                        deviceDict.Remove(deviceSerial);
                    }
                }
            }
        }

        private void beginCheckDeviceList(CancellationToken token)
        {
            Task.Delay(TimeSpan.FromMinutes(1), token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;
                try
                {
                    checkDeviceList();
                }
                catch (Exception ex)
                {
                    AgentContext.LogError("获取设备列表时出错，原因：" + ExceptionUtils.GetExceptionMessage(ex));
                }
                beginCheckDeviceList(token);
            });
        }

        public void Stop()
        {
            cts?.Cancel();
            cts = null;
        }
    }
}
