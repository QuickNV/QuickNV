using Quick.Fields;
using System.Text.Json.Serialization.Metadata;
using YiQiDong.Agent;
using YiQiDong.Core.Functions;
using YiQiDong.Protocol.V1.Model;

namespace QuickNV.Driver.Agent.Functions
{
    public abstract class DriverModelJsonConfig<T> : ModelJsonConfig<T>
        where T : AbstractDriverConfigModel, new()
    {
        public override string Name => "配置";
        public DriverModelJsonConfig(JsonTypeInfo<T> jsonTypeInfo)
            : base(
                jsonTypeInfo,
                AgentContext.Container?.ContainerFolder ?? Environment.CurrentDirectory,
                () => AgentContext.Container.AutoStart,
                "config.json")
        { }

        protected FieldForGet getQuickNVDriverInterfaceGroup(FunctionRequest request, T requestModel, bool isReadOnly = false)
        {
            var model = requestModel ?? Model;
            return new FieldForGet()
            {
                Id = "QuickNVDriverInterface",
                Type = FieldType.ContainerGroup,
                Name = "QuickNV驱动接口",
                Children =
                [
                    new ()
                    {
                        Id = nameof(AbstractDriverConfigModel.QuickNVDriverInterfaceUrl),
                        Name = "地址",
                        Type = FieldType.InputText,
                        Value = model.QuickNVDriverInterfaceUrl,
                        Input_AllowBlank = false,
                        Input_ReadOnly = isReadOnly
                    },
                    new ()
                    {
                        Id = nameof(AbstractDriverConfigModel.QuickNVDriverInterfacePassword),
                        Name = "密码",
                        Type = FieldType.InputText,
                        Value = model.QuickNVDriverInterfacePassword,
                        Input_AllowBlank = false,
                        Input_ReadOnly = isReadOnly
                    }
                ]
            };
        }
    }
}
