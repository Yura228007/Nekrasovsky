using Microsoft.EntityFrameworkCore;
using server.Models;
using server.Data;

namespace server.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly IPasswordService _passwordService;

        public UserService(AppDbContext context, ILogger<UserService> logger, IPasswordService passwordService)
        {
            _context = context;
            _logger = logger;
            _passwordService = passwordService;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            
            // Decrypt passwords for all users
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.EncryptedPassword))
                {
                    user.EncryptedPassword = _passwordService.Decrypt(user.EncryptedPassword);
                }
            }
            
            return users;
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string? name, string? surname)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(u => EF.Functions.ILike(u.Name, $"%{name}%"));

            if (!string.IsNullOrWhiteSpace(surname))
                query = query.Where(u => EF.Functions.ILike(u.Surname, $"%{surname}%"));

            var users = await query.ToListAsync();
            
            // Decrypt passwords for all users
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.EncryptedPassword))
                {
                    user.EncryptedPassword = _passwordService.Decrypt(user.EncryptedPassword);
                }
            }
            
            return users;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            
            // Decrypt password if user exists
            if (user != null && !string.IsNullOrEmpty(user.EncryptedPassword))
            {
                user.EncryptedPassword = _passwordService.Decrypt(user.EncryptedPassword);
            }
            
            return user;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            // Business logic: Check if login already exists
            if (await _context.Users.AnyAsync(u => u.Login == user.Login))
            {
                throw new InvalidOperationException($"User with login '{user.Login}' already exists");
            }

            // Business logic: Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                throw new InvalidOperationException($"User with email '{user.Email}' already exists");
            }

            // Encrypt password before saving
            if (!string.IsNullOrEmpty(user.EncryptedPassword))
            {
                user.EncryptedPassword = _passwordService.Encrypt(user.EncryptedPassword);
            }

            user.CreatedAt = DateTime.UtcNow;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User created with ID: {UserId}, Login: {Login}", user.Id, user.Login);
            return user;
        }

        public async Task<User> UpdateUserAsync(int id, User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            // Business logic: Check if login is being changed and if new login already exists
            if (user.Login != updatedUser.Login)
            {
                if (await _context.Users.AnyAsync(u => u.Login == updatedUser.Login && u.Id != id))
                {
                    throw new InvalidOperationException($"User with login '{updatedUser.Login}' already exists");
                }
            }

            // Business logic: Check if email is being changed and if new email already exists
            if (user.Email != updatedUser.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == updatedUser.Email && u.Id != id))
                {
                    throw new InvalidOperationException($"User with email '{updatedUser.Email}' already exists");
                }
            }

            // Update user properties
            user.Login = updatedUser.Login;
            
            // Encrypt password if it's being updated
            if (!string.IsNullOrEmpty(updatedUser.EncryptedPassword))
            {
                user.EncryptedPassword = _passwordService.Encrypt(updatedUser.EncryptedPassword);
            }
            // If password is not provided, keep the existing encrypted password
            
            user.Name = updatedUser.Name;
            user.Surname = updatedUser.Surname;
            user.Email = updatedUser.Email;
            user.Phone = updatedUser.Phone;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User updated with ID: {UserId}", user.Id);
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User deleted with ID: {UserId}", id);
            return true;
        }
    }
}

