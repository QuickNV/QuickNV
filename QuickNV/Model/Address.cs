using System.ComponentModel.DataAnnotations;
using Quick.EntityFrameworkCore.Plus;

namespace QuickNV.Model
{
    public class Address
    {
        /// <summary>
        /// 编号
        /// </summary>
        [Key]
        [MaxLength(100)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        
        public override int GetHashCode()
        {
            return this.GetHashCode(
                t => t.Id);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj,
                t => t.Id);
        }

        public override string ToString()
        {
            return $"地点[{Name}]";
        }
    }
}
