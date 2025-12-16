namespace InventoryAPI.Models.DTO.Auth
{
    public class UserRegisterDto
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;

        public string? Role { get; set; }
    }
}