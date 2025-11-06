using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordManagerV1.Models;
using server.Data;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // ? GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // ? GET: api/users/search?name=Alex&surname=Smith
        // ???? ?? ?????????? ????? ???? ??????
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<User>>> GetByNameOrSurname(
            [FromQuery] string? name,
            [FromQuery] string? surname)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(u => EF.Functions.ILike(u.Name, $"%{name}%")); // PostgreSQL-friendly

            if (!string.IsNullOrWhiteSpace(surname))
                query = query.Where(u => EF.Functions.ILike(u.Surname, $"%{surname}%"));

            var users = await query.ToListAsync();
            return Ok(users);
        }

        // ? POST: api/users/add
        [HttpPost("add")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "???????????? ????????", user.Id });
        }

        // ? POST: api/users/edit/5
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditUser(int id, [FromBody] User updated)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("???????????? ?? ??????");

            user.Login = updated.Login;
            user.EncryptedPassword = updated.EncryptedPassword;
            user.Name = updated.Name;
            user.Surname = updated.Surname;
            user.Email = updated.Email;
            user.Phone = updated.Phone;

            await _context.SaveChangesAsync();
            return Ok(new { message = "???????????? ????????", user });
        }

        // ? POST: api/users/delete/5
        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("???????????? ?? ??????");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "???????????? ??????" });
        }
    }
}
