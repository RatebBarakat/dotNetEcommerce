namespace ecommerce.Dtos
{
    public class UserDTO
    {
        public string Name { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string Email { get; set; }
    }

    public class UserWithPermissionsDTO
    {
        public UserDTO User { get; set; }
        public ICollection<string> Permissions { get; set; } = new List<string>();
    }
}
