namespace QuickNV.Driver.GB28181.Utils
{
    public static class MediaStreamUtils
    {
        public static string GetStreamId(string ssrc)
        {
            return string.Format("{0:X8}", uint.Parse(ssrc));
        }

        public static int GetMediaIdFromSSRC(string ssrc)
        {
            var ret = ssrc.PadLeft(10, '0');
            return int.Parse(ret.Substring(6, 4));
        }

        public static int GetMediaIdFromStreamId(string streamId)
        {
            var i = int.Parse(streamId, System.Globalization.NumberStyles.HexNumber);
            return GetMediaIdFromSSRC(i.ToString());
        }
    }
}
