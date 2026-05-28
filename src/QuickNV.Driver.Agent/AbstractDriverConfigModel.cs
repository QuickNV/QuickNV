using System;

namespace QuickNV.Driver.Agent;

public abstract class AbstractDriverConfigModel
{
    public string QuickNVDriverInterfaceUrl { get; set; } = "qp.pipe://./QuickNV.DriverInterface";
    public string QuickNVDriverInterfacePassword { get; set; } = "123456";
}
