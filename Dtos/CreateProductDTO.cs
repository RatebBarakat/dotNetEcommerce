namespace ecommerce.Dtos
{
    using Microsoft.AspNetCore.Http;

    public class CreateProductDTO
    {
        public string Name { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string SmallDescription { get; set; }

        public string Description { get; set; }

        public List<IFormFile> Images { get; set; } = [];

        public int CategoryId { get; set; }
    }
}
