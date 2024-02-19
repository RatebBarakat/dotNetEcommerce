using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace ecommerce.Models
{
    public class Role : IdentityRole
    {
        [JsonIgnore]
        public virtual ICollection<RolePermission>? RolePermissions { get; set; } = new List<RolePermission>(); 
    }
}
