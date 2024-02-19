using Azure;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ecommerce.Models
{
    public class Permission
    {
        [Key]
        public string Id { get; set; } 

        public string Name { get; set; }
        [JsonIgnore]
        public virtual ICollection<RolePermission>? RolePermissions { get; set; } = new List<RolePermission>();
    }
}
