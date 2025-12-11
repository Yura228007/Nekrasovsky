using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all users");
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        // GET: api/users/paged?page=1&pageSize=10
        [HttpGet("paged")]
        public async Task<ActionResult<object>> GetAllPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Validate parameters
                if (page < 1)
                {
                    return BadRequest(new { message = "Page must be greater than 0" });
                }
                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(new { message = "PageSize must be between 1 and 100" });
                }

                var (Users, TotalCount) = await _userService.GetAllUsersPagedAsync(page, pageSize);
                
                return Ok(new
                {
                    users = Users,
                    totalCount = TotalCount,
                    page,
                    pageSize,
                    totalPages = TotalCount > 0 ? (int)Math.Ceiling(TotalCount / (double)pageSize) : 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged users");
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        // GET: api/users/search?name=Alex&surname=Smith
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<User>>> GetByNameOrSurname(
            [FromQuery] string? name,
            [FromQuery] string? surname)
        {
            try
            {
                var users = await _userService.SearchUsersAsync(name, surname);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching users");
                return StatusCode(500, new { message = "An error occurred while searching users" });
            }
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound(new { message = $"User with ID {id} not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user with ID {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the user" });
            }
        }

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var createdUser = await _userService.CreateUserAsync(user);
                _logger.LogInformation("User created successfully with ID: {UserId}", createdUser.Id);
                return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, 
                    new { message = "User created successfully", user = createdUser });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating user");
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating user");
                return StatusCode(500, new { message = "An error occurred while saving the user to the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating user");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the user" });
            }
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updated)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Id must be greater than 0" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid model state", errors = ModelState });
            }

            try
            {
                var user = await _userService.UpdateUserAsync(id, updated);
                _logger.LogInformation("User updated successfully with ID: {UserId}", id);
                return Ok(new { message = "User updated successfully", user });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User with ID {UserId} not found for update", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating user with ID {UserId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating user with ID {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the user in the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating user with ID {UserId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while updating the user" });
            }
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Id must be greater than 0" });
                }

                var deleted = await _userService.DeleteUserAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("User with ID {UserId} not found for deletion", id);
                    return NotFound(new { message = $"User with ID {id} not found" });
                }

                _logger.LogInformation("User deleted successfully with ID: {UserId}", id);
                return Ok(new { message = "User deleted successfully" });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting user with ID {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the user from the database" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting user with ID {UserId}", id);
                return StatusCode(500, new { message = "An unexpected error occurred while deleting the user" });
            }
        }
    }
}
