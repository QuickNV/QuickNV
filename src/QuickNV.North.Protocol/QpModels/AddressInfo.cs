namespace QuickNV.North.Protocol.QpModels;

public class AddressInfo
{
    /// <summary>
    /// 地点编号
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// 地点名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 地点父节点编号
    /// </summary>
    public string ParentId { get; set; }

    public override string ToString()
    {
        return $"地点[{Name}]";
    }
}
