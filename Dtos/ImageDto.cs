namespace ecommerce.Dtos
{
    public class ImageDto
    {
        public int ProductId { get; set; }

        public List<IFormFile> Images { get; set; } = [];
    }
}