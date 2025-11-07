using Comprehension.Data;
using Comprehension.DTOs;
using Comprehension.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Comprehension.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string? Error, User? User)> RegisterUserAsync(RegisterRequest request);
        Task<(bool Success, string? Error, User? User)> ValidateUserAsync(string username, string password);
        Task<string> CreateSessionAsync(Guid userId);
        Task<User?> GetUserBySessionIdAsync(string sessionId);
        Task<User?> GetUserByTokenAsync(string token);
        Task<bool> ValidateSessionAsync(string sessionId);
        Task<bool> UpdateSessionActivityAsync(string sessionId);
        Task<bool> InvalidateSessionAsync(string sessionId);
        Task CleanExpiredSessionsAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly ComprehensionContext _context;
        private const int SaltSize = 128 / 8; // 16 bytes
        private const int HashSize = 256 / 8; // 32 bytes
        private const int Iterations = 100000;
        private const int SessionIdSize = 128 / 8; // 16 bytes

        public AuthService(ComprehensionContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string? Error, User? User)> RegisterUserAsync(RegisterRequest request)
        {
            // Verificar si el usuario ya existe
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return (false, "El nombre de usuario ya existe", null);
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return (false, "El correo electrónico ya está registrado", null);
            }

            // Generar salt y hash de la password
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = HashPassword(request.Password, salt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Name = request.Name,
                Email = request.Email,
                DateOfBirth = request.DateOfBirth,
                PasswordHash = hash,
                PasswordSalt = Convert.ToBase64String(salt),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, null, user);
        }

        public async Task<(bool Success, string? Error, User? User)> ValidateUserAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return (false, "Usuario o contraseña incorrectos", null);
            }

            // Verificar la password
            var salt = Convert.FromBase64String(user.PasswordSalt);
            var hash = HashPassword(password, salt);

            if (hash != user.PasswordHash)
            {
                return (false, "Usuario o contraseña incorrectos", null);
            }

            return (true, null, user);
        }

        public async Task<string> CreateSessionAsync(Guid userId)
        {
            // Generar un ID de sesion aleatorio de 128 bits
            var sessionIdBytes = RandomNumberGenerator.GetBytes(SessionIdSize);
            var sessionId = Convert.ToBase64String(sessionIdBytes);

            var session = new Session
            {
                SessionId = sessionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                ExpiredAt = null
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return sessionId;
        }

        public async Task<User?> GetUserBySessionIdAsync(string sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session == null || !session.IsValid())
            {
                return null;
            }

            // Actualizar ultima actividad
            await UpdateSessionActivityAsync(sessionId);

            return session.User;
        }

        public async Task<User?> GetUserByTokenAsync(string token)
        {
            return await GetUserBySessionIdAsync(token);
        }

        public async Task<bool> ValidateSessionAsync(string sessionId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            return session != null && session.IsValid();
        }

        public async Task<bool> UpdateSessionActivityAsync(string sessionId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session != null)
            {
                session.LastActivityAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> InvalidateSessionAsync(string sessionId)
        {
            var session = await _context.Sessions.FindAsync(sessionId);
            if (session != null)
            {
                session.ExpiredAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task CleanExpiredSessionsAsync()
        {
            var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
            var expiredSessions = await _context.Sessions
                .Where(s => s.LastActivityAt < fiveMinutesAgo && s.ExpiredAt == null)
                .ToListAsync();

            foreach (var session in expiredSessions)
            {
                session.ExpiredAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private string HashPassword(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: HashSize));
        }
    }
}