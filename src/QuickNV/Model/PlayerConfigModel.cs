namespace QuickNV.Model
{
    public class PlayerConfigModel
    {
        public bool UseProxy { get; set; } = false;
        public int WaitFrameChangeSeconds { get; set; } = 60;
        public int FastRetryTimes { get; set; } = 3;
        public int FastRetryInterval { get; set; } = 1000;
        public int SlowRetryInterval { get; set; } = 5000;
        public bool Stretch { get; set; } = true;
        public bool SupportDblclickFullscreen { get; set; } = true;
        public bool ShowFullScreenBtn { get; set; } = true;
        public bool ShowStretchBtn { get; set; } = true;
        public bool ShowZoomBtn { get; set; } = true;
        public bool ShowScreenShotBtn { get; set; } = true;
        public bool ShowAudioBtn { get; set; } = true;
        public bool ShowRecordBtn { get; set; } = true;
        public bool ShowBandwidth { get; set; } = false;
        public bool ShowPerformance { get; set; } = false;
    }
}
