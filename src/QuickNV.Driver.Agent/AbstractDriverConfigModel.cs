namespace QuickNV.Driver.Agent;

public abstract class AbstractDriverConfigModel
{
    public string QuickNVDriverInterfaceUrl { get; set; } = "qp.pipe://./QuickNV.QpInterface";
    public string QuickNVDriverInterfacePassword { get; set; } = "123456";
}
