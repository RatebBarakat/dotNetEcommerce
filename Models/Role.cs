using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace ecommerce.Models
{
    public class Role : IdentityRole
    {
        [JsonIgnore]
        public virtual ICollection<RolePermission>? RolePermissions { get; set; } = new List<RolePermission>(); 
    }
}
