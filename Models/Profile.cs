using System.Text.Json.Serialization;

namespace ecommerce.Models
{
    public class Profile
    {
        public int Id { get; set; }

        public string Avatar { get; set; }

        public string UserId { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }
    }
}
