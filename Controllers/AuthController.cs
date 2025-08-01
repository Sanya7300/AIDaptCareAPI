using AIDaptCareAPI.Models;
using AIDaptCareAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIDaptCareAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;
        public AuthController(UserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }
        [HttpPost("register")]
        public IActionResult Register([FromBody] User newUser)
        {
            var existingUser = _userService.GetByUsername(newUser.Username);
            if (existingUser != null)
            {
                return BadRequest("User already exists");
            }
            // In real app, hash the password here before storing
            _userService.Create(newUser);
            return Ok("User registered successfully");
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginUser)
        {
            var valid = _userService.ValidateUser(loginUser.Username, loginUser.Password);
            if (!valid)
            {
                return Unauthorized("Invalid username or password");
            }
            // Generate JWT token here and return (omitted for brevity)
            var token = _authService.GenerateToken(loginUser);
            return Ok(new { Token = token });
        }
    }
}