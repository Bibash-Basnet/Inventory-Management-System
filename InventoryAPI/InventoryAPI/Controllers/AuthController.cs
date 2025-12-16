using InventoryAPI.Data;
using InventoryAPI.Models;
using InventoryAPI.Models.DTO.Auth;
using InventoryAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IJwtService _jwtService;

        public AuthController(ApplicationDbContext db,
                              IPasswordHasher<User> hasher,
                              IJwtService jwtService)
        {
            _db = db;
            _hasher = hasher;
            _jwtService = jwtService;
        }

        //  REGISTER 
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match." });

            if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest(new { message = "Username already exists." });

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Role = string.IsNullOrEmpty(dto.Role) ? "User" : dto.Role 
            };

            // Hash password
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            // Save user to database
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Generate JWT token
            var token = _jwtService.GenerateToken(user, out DateTime expiresAt);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                Username = user.Username,
                Role = user.Role 
            });
        }

        //  LOGIN 
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or password." });

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid username or password." });

            // Create JWT
            var token = _jwtService.GenerateToken(user, out DateTime expiresAt);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                Username = user.Username,
                Role = user.Role 
            });
        }
    }
}