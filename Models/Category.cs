using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ecommerce.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        [JsonIgnore]
        public virtual ICollection<Product> Products { get; }
    }
}
