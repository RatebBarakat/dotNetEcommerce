using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace ecommerce.Models
{
    public class User : IdentityUser
    {
        [JsonIgnore]
        public virtual Profile Profile { get; set; }

        [JsonIgnore]
        public virtual ICollection<Cart>? Carts { get; set; } = [];
    }
}

