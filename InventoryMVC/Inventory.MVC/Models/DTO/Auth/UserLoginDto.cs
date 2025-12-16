using System.ComponentModel.DataAnnotations;

namespace Inventory.Models.DTO.Auth
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Role is NOT sent to API, but used in the view for UI purposes
        // The actual role comes from the API response
        public string Role { get; set; } = "User";
    }
}