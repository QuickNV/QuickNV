namespace QuickNV.Protocol.Driver.QpModels;

/// <summary>
/// 视频文件信息
/// </summary>
public class VideoFileInfo
{
    /// <summary>
    /// 编号
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// 文件名
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 文件大小
    /// </summary>
    public long Size { get; set; }
    /// <summary>
    /// 文件起始时间
    /// </summary>
    public DateTime StartTime { get; set; }
    /// <summary>
    /// 文件结束时间
    /// </summary>
    public DateTime EndTime { get; set; }
}