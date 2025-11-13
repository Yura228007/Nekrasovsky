using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.Services;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserPermissionsController : ControllerBase
    {
        private readonly IUserPermissionsService _userPermissionsService;

        public UserPermissionsController(IUserPermissionsService userPermissionsService)
        {
            _userPermissionsService = userPermissionsService;
        }

        // GET: api/user-permissions/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetUserPermissions(int userId)
        {
            var permissions = await _userPermissionsService.GetUserPermissionsAsync(userId);
            return Ok(permissions);
        }

        // GET: api/user-permissions/permission/5
        [HttpGet("permission/{permissionId}")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersWithPermission(int permissionId)
        {
            var users = await _userPermissionsService.GetUsersWithPermissionAsync(permissionId);
            return Ok(users);
        }

        // POST: api/user-permissions/add
        [HttpPost("add")]
        public async Task<IActionResult> AddPermissionToUser([FromBody] UserPermissions userPermission)
        {
            try
            {
                await _userPermissionsService.AddPermissionToUserAsync(userPermission.UserId, userPermission.PermissionId);
                return Ok(new { message = "Permission added to user successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/user-permissions/remove
        [HttpPost("remove")]
        public async Task<IActionResult> RemovePermissionFromUser([FromBody] UserPermissions userPermission)
        {
            try
            {
                await _userPermissionsService.RemovePermissionFromUserAsync(userPermission.UserId, userPermission.PermissionId);
                return Ok(new { message = "Permission removed from user successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/user-permissions/check?userId=5&permissionCode=Admin
        [HttpGet("check")]
        public async Task<ActionResult<bool>> CheckPermission([FromQuery] int userId, [FromQuery] string permissionCode)
        {
            var hasPermission = await _userPermissionsService.HasPermissionAsync(userId, permissionCode);
            return Ok(new { hasPermission });
        }

        // POST: api/user-permissions/update/5
        [HttpPost("update/{userId}")]
        public async Task<IActionResult> UpdateUserPermissions(int userId, [FromBody] IEnumerable<int> permissionIds)
        {
            try
            {
                await _userPermissionsService.UpdateUserPermissionsAsync(userId, permissionIds);
                return Ok(new { message = "User permissions updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}

