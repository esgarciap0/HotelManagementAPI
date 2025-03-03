using HotelReservationAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HotelReservationAPI.Models.DTOs;


namespace HotelReservationAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Agency> _userManager;
        private readonly SignInManager<Agency> _signInManager;

        public AuthController(UserManager<Agency> userManager, SignInManager<Agency> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        // Registrar Agencia
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
        {
            var user = new Agency { UserName = model.Email, Email = model.Email, Name = model.Name };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = "User registered successfully" });
        }
        //INICIO DE SESIÓN (Login)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized("Invalid credentials");

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded) return Unauthorized("Invalid credentials");

            return Ok(new { message = "Login successful" });
        }
        //Logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "Cierre de sesión exitoso" });
        }

    }

    public class LoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
    public class RegisterUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}


