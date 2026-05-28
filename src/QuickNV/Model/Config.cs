using Quick.EntityFrameworkCore.Plus;

namespace QuickNV.Model
{
    /// <summary>
    /// 配置
    /// </summary>
    public class Config : BaseModel
    {
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        public Config() { }
        public Config(string id) { Id = id; }
    }
}
