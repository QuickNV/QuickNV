using QuickNV.HikvisionISUPSDK.Api.Rtp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.Driver.HikvisionISUP
{
    public class StreamPushContext
    {
        public int SessionId { get; internal set; }
        public DeviceContext Device { get; private set; }
        public Driver.Agent.DriverChannel<ChannelConfig> Channel { get; private set; }
        public RtpSender RtpSender { get; private set; }
        public QuickNV.HikvisionISUPSDK.Api.SmsStreamFormat StreamFormat { get; internal set; }
        public QuickNV.HikvisionISUPSDK.Api.SmsStreamType StreamType { get; internal set; }

        public StreamPushContext(DeviceContext device, Driver.Agent.DriverChannel<ChannelConfig> channel, RtpSender rtpSender)
        {
            Device = device;
            Channel = channel;
            RtpSender = rtpSender;
        }
    }
}
