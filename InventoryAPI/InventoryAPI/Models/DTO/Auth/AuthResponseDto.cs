namespace InventoryAPI.Models.DTO.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!; 
    }
}