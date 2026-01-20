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
            var users = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking() // Отключаем отслеживание изменений
                .ToListAsync();
            
            // НЕ расшифровываем пароли - они должны оставаться зашифрованными/хэшированными
            // Пароли используются только для проверки через VerifyPassword, не для отображения
            
            return users;
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetAllUsersPagedAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Limit max page size

            var totalCount = await _context.Users.CountAsync();
            
            var users = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking() // Отключаем отслеживание изменений
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // НЕ расшифровываем пароли - они должны оставаться зашифрованными/хэшированными
            // Пароли используются только для проверки через VerifyPassword, не для отображения
            
            return (Users: users, TotalCount: totalCount);
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string? name, string? surname)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(u => EF.Functions.ILike(u.Name, $"%{name}%"));

            if (!string.IsNullOrWhiteSpace(surname))
                query = query.Where(u => EF.Functions.ILike(u.Surname, $"%{surname}%"));

            var users = await query
                .Include(u => u.Role)
                .AsNoTracking() // Отключаем отслеживание изменений
                .ToListAsync();
            
            // НЕ расшифровываем пароли - они должны оставаться зашифрованными/хэшированными
            // Пароли используются только для проверки через VerifyPassword, не для отображения
            
            return users;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .AsNoTracking() // Отключаем отслеживание изменений
                .FirstOrDefaultAsync(u => u.Id == id);
            
            // НЕ расшифровываем пароль - он должен оставаться зашифрованным/хэшированным
            // Пароль используется только для проверки через VerifyPassword, не для отображения
            
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

            // Hash password before saving (use new BCrypt hashing)
            // Проверяем, не является ли пароль уже хэшированным (BCrypt хэши начинаются с $2)
            if (!string.IsNullOrEmpty(user.EncryptedPassword))
            {
                // Если пароль уже хэширован (начинается с $2), не хэшируем повторно
                if (!user.EncryptedPassword.StartsWith("$2"))
                {
                    user.EncryptedPassword = _passwordService.HashPassword(user.EncryptedPassword);
                }
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
            
            // Hash password if it's being updated (use new BCrypt hashing)
            // Проверяем, не является ли пароль уже хэшированным (BCrypt хэши начинаются с $2)
            if (!string.IsNullOrEmpty(updatedUser.EncryptedPassword))
            {
                // Если пароль уже хэширован (начинается с $2), не хэшируем повторно
                if (!updatedUser.EncryptedPassword.StartsWith("$2"))
                {
                    user.EncryptedPassword = _passwordService.HashPassword(updatedUser.EncryptedPassword);
                }
                else
                {
                    // Если пароль уже хэширован, используем его как есть
                    user.EncryptedPassword = updatedUser.EncryptedPassword;
                }
            }
            // If password is not provided, keep the existing hashed password
            
            user.Name = updatedUser.Name;
            user.Surname = updatedUser.Surname;
            user.Email = updatedUser.Email;
            user.Phone = updatedUser.Phone;
            user.RoleId = updatedUser.RoleId; // Обновляем роль пользователя

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

