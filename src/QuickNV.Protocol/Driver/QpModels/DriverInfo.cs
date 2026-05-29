namespace QuickNV.Driver.Protocol.QpModels
{
    public class DriverInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public bool HasChannelConfig { get; set; }
        public bool CanImportChannel { get; set; }
    }
}
