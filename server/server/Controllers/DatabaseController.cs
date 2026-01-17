using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(
            AppDbContext context,
            IPasswordService passwordService,
            ILogger<DatabaseController> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _logger = logger;
        }

        /// <summary>
        /// Проверка подключения к базе данных и состояния пользователей
        /// GET: api/database/check
        /// </summary>
        [HttpGet("check")]
        public async Task<ActionResult<object>> CheckDatabase()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Не удалось подключиться к базе данных",
                        connectionString = "Проверьте строку подключения в appsettings.json"
                    });
                }

                var users = await _context.Users.ToListAsync();
                var usersInfo = users.Select(u => new
                {
                    id = u.Id,
                    login = u.Login,
                    email = u.Email,
                    name = $"{u.Name} {u.Surname}",
                    isPasswordEncrypted = IsBase64(u.EncryptedPassword),
                    passwordLength = u.EncryptedPassword?.Length ?? 0
                }).ToList();

                return Ok(new
                {
                    success = true,
                    message = "Подключение к базе данных успешно",
                    usersCount = users.Count,
                    users = usersInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке базы данных");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при проверке базы данных",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Зашифровать пароль пользователя (для пользователей, добавленных вручную)
        /// POST: api/database/encrypt-user-password
        /// Body: { "userId": 1, "password": "новый_пароль" }
        /// </summary>
        [HttpPost("encrypt-user-password")]
        public async Task<ActionResult<object>> EncryptUserPassword([FromBody] EncryptPasswordRequest request)
        {
            try
            {
                if (request.UserId <= 0)
                {
                    return BadRequest(new { message = "UserId должен быть больше 0" });
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { message = "Пароль не может быть пустым" });
                }

                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return NotFound(new { message = $"Пользователь с ID {request.UserId} не найден" });
                }

                var wasEncrypted = IsBase64(user.EncryptedPassword);
                // Используем HashPassword вместо устаревшего Encrypt
                var hashedPassword = _passwordService.HashPassword(request.Password);

                user.EncryptedPassword = hashedPassword;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Пароль пользователя {Login} (ID: {UserId}) хэширован", user.Login, user.Id);

                return Ok(new
                {
                    success = true,
                    message = "Пароль успешно хэширован",
                    userId = user.Id,
                    login = user.Login,
                    wasEncrypted = wasEncrypted
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при хэшировании пароля пользователя");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ошибка при зашифровке пароля",
                    error = ex.Message
                });
            }
        }

        private static bool IsBase64(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return false;

            try
            {
                Convert.FromBase64String(str);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class EncryptPasswordRequest
    {
        public int UserId { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}
