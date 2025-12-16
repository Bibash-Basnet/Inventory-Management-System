using InventoryAPI.Models;

namespace InventoryAPI.Services
{
    public interface IJwtService
    {
        public string GenerateToken(User user, out DateTime expiresAt);

    }
}
