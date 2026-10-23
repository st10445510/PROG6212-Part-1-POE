
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using RaceDay.API.Data;
using RaceDay.API.Models;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;


namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly RaceDayContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            RaceDayContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        public class RegisterRequest
        {
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Password { get; set; } = "";
            public string? PhoneNumber { get; set; }
            public string Role { get; set; } = "Participant";
        }

        public class LoginRequest
        {
            public string Email { get; set; } = "";
            public string Password { get; set; } = "";
        }

        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string storedPassword)
        {
            try
            {
                string[] parts = storedPassword.Split(':');
                if (parts.Length != 2) return false;

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedHash = Convert.FromBase64String(parts[1]);

                byte[] enteredHash = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 32);

                return CryptographicOperations.FixedTimeEquals(
                    enteredHash, storedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Complete all required fields." });
            }

            if (request.Role != "Organiser" &&
                request.Role != "Participant")
            {
                return BadRequest(new
                {
                    message = "Role must be Organiser or Participant."
                });
            }

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email);

            if (emailExists)
            {
                return Conflict(new { message = "Email is already registered." });
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber,
                Role = request.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration successful.",
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role
            });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null ||
                !VerifyPassword(request.Password, user.Password))
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Name, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"])),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return Ok(new
            {
                message = "Login successful.",
                token = tokenString,
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role
            });
        }

    }
}
