using System.Text.Json.Serialization;

namespace ecommerce.Models
{
    public class RolePermission
    {
        public string RoleId { get; set; }
        [JsonIgnore]
        public virtual Role Role { get; set; }
        public string PermissionId { get; set; }
        [JsonIgnore]
        public virtual Permission Permission { get; set; }
    }
}
