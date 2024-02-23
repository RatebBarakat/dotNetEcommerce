namespace ecommerce.Dtos
{
    public class ProductCartDto
    {
        public int ProductId { get; set; }

        public int Quantity { get; set; }
    }

    public class UpdateProductCartDto
    {
        public int CartId { get; set; }
        public int Quantity { get; set; }
    }
}
