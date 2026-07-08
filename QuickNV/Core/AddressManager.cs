using Quick.EntityFrameworkCore.Plus;

namespace QuickNV.Core
{
    public class AddressManager
    {
        public static AddressManager Instance { get; private set; } = new AddressManager();

        public List<KeyValuePair<string, string>> GetAddressTree(string parentAddressId = null, int level = 0)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            Model.Address[] items = null;
            if (string.IsNullOrEmpty(parentAddressId))
                items = ConfigDbContext.CacheContext.Query<Model.Address>(t => t.ParentId == null || t.ParentId == string.Empty);
            else
                items = ConfigDbContext.CacheContext.Query<Model.Address>(t => t.ParentId == parentAddressId);
            foreach (var item in items)
            {
                var id = item.Id;
                var name = string.Empty.PadLeft(level * 2, '-') + item.Name;
                list.Add(new KeyValuePair<string, string>(id, name));
                list.AddRange(GetAddressTree(id, level + 1));
            }
            return list;
        }
    }
}
