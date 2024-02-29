namespace ecommerce.Dtos
{
    public class UserWithPermissionsDTO
    {
        public UserDTO User { get; set; }

        public bool IsAdmin { get; set; }
        public ICollection<string> Permissions { get; set; } = new List<string>();
    }
}
