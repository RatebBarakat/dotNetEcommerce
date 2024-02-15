using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ecommerce.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string SmallDescription { get; set; }
        public string Description { get; set; }
        [ForeignKey("Categories")]
        public int CategoryId { get; set; }

        public string Image {  get; set; }
        [JsonIgnore]
        public virtual Category? Category { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

    
    }
}
