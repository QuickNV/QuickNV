namespace QuickNV.Driver.GB28181
{
    public class DeviceOnlineStateChangedEventArgs
    {
        public DeviceContext DeviceContext { get; set; }
        public bool IsOnline { get; set; }
        public string Reason { get; set; }
    }
}
