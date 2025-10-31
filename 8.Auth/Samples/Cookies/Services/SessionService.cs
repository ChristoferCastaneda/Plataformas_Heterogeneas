using Cookies.Data;
using Cookies.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Cookies.Services
{

    public interface ISessionService
    {
        Task<string> CreateSessionAsync(int userId);
        Task<User?> ValidateSessionAsync(string sessionId);
        Task InvalidateSessionAsync(string sessionId);
        Task CleanupExpiredSessionsAsync();
    }

    public class SessionService : ISessionService
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(5);

        public SessionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreateSessionAsync(int userId)
        {
            // Generate a cryptographically secure random 128-bit session ID
            byte[] randomBytes = RandomNumberGenerator.GetBytes(128 / 8);
            string sessionId = Convert.ToBase64String(randomBytes);

            var session = new Session
            {
                SessionId = sessionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return sessionId;
        }

        public async Task<User?> ValidateSessionAsync(string sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (session == null)
                return null;

            // Check if session has expired (5 minutes of inactivity)
            if (DateTime.UtcNow - session.LastAccessedAt > _sessionTimeout)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
                return null;
            }

            // Update last accessed time
            session.LastAccessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return session.User;
        }

        public async Task InvalidateSessionAsync(string sessionId)
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task CleanupExpiredSessionsAsync()
        {
            var expiredTime = DateTime.UtcNow.Subtract(_sessionTimeout);
            var expiredSessions = await _context.Sessions
                .Where(s => s.IsActive && s.LastAccessedAt < expiredTime)
                .ToListAsync();

            foreach (var session in expiredSessions)
            {
                session.IsActive = false;
            }

            await _context.SaveChangesAsync();
        }
    }
}
