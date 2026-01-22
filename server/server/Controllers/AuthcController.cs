using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NekrasovskyAPP.Domain;
using NekrasovskyAPP.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AppDbContext context, 
            IPasswordService passwordService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new AuthResponse 
                    { 
                        User = null, 
                        Message = "Логин и пароль обязательны" 
                    });
                }

                // Находим пользователя по логину
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Login == request.Login);

                if (user == null)
                {
                    _logger.LogWarning($"Login attempt failed: user '{request.Login}' not found");
                    return Unauthorized(new AuthResponse 
                    { 
                        User = null, 
                        Message = "Неверный логин или пароль" 
                    });
                }

                // Проверяем пароль с возможностью обновления
                var verificationResult = _passwordService.VerifyPasswordWithUpgrade(
                    request.Password, 
                    user.Password);

                if (!verificationResult.IsValid)
                {
                    _logger.LogWarning($"Login attempt failed: invalid password for user '{request.Login}'");
                    return Unauthorized(new AuthResponse 
                    { 
                        User = null, 
                        Message = "Неверный логин или пароль" 
                    });
                }

                // Если пароль нужно обновить, обновляем его в базе данных
                if (verificationResult.NeedsUpgrade && !string.IsNullOrEmpty(verificationResult.NewHash))
                {
                    _logger.LogInformation($"Upgrading password hash for user '{user.Login}' from legacy format to BCrypt");
                    
                    user.Password = verificationResult.NewHash;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Password successfully upgraded for user '{user.Login}'");
                }

                _logger.LogInformation($"User '{user.Login}' logged in successfully");

                return Ok(new AuthResponse 
                { 
                    User = user, 
                    Message = "Успешный вход" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new AuthResponse 
                { 
                    User = null, 
                    Message = "Внутренняя ошибка сервера" 
                });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new AuthResponse 
                    { 
                        User = null, 
                        Message = "Логин и пароль обязательны" 
                    });
                }

                // Проверяем, существует ли пользователь
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Login == request.Login || u.Email == request.Email);

                if (existingUser != null)
                {
                    return BadRequest(new AuthResponse 
                    { 
                        User = null, 
                        Message = "Пользователь с таким логином или email уже существует" 
                    });
                }

                // Создаем нового пользователя с BCrypt хэшем
                var user = new User
                {
                    Login = request.Login,
                    Email = request.Email,
                    Password = _passwordService.HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Patronymic = request.Patronymic
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"New user registered: '{user.Login}'");

                return Ok(new AuthResponse 
                { 
                    User = user, 
                    Message = "Регистрация успешна" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new AuthResponse 
                { 
                    User = null, 
                    Message = "Внутренняя ошибка сервера" 
                });
            }
        }

        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(request.UserId);
                
                if (user == null)
                {
                    return NotFound(new { Message = "Пользователь не найден" });
                }

                // Проверяем старый пароль
                if (!_passwordService.VerifyPassword(request.OldPassword, user.Password))
                {
                    return BadRequest(new { Message = "Неверный текущий пароль" });
                }

                // Устанавливаем новый пароль (автоматически используется BCrypt)
                user.Password = _passwordService.HashPassword(request.NewPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password changed for user '{user.Login}'");

                return Ok(new { Message = "Пароль успешно изменен" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { Message = "Внутренняя ошибка сервера" });
            }
        }
    }

    // DTOs
    public class LoginRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Patronymic { get; set; }
    }

    public class ChangePasswordRequest
    {
        public int UserId { get; set; }
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public User? User { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}