namespace QuickNV.Driver.GB28181.Utils;

public class DateUtils
{
    private static DateTime unixTimestampBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long ToUnixTimestamp(DateTime time)
    {
        if (time.Kind != DateTimeKind.Utc)
            time = time.ToUniversalTime();
        return Convert.ToInt64((time - unixTimestampBaseTime).TotalMilliseconds);
    }
}
