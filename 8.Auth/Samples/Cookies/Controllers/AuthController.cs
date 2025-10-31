using Cookies.Data;
using Cookies.Models;
using Cookies.Models.ViewModels;
using Cookies.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cookies.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ISessionService _sessionService;
        private const string SessionCookieName = "AuthSessionId";

        public AuthController(
            ApplicationDbContext context,
            IPasswordHasher passwordHasher,
            ISessionService sessionService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _sessionService = sessionService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Checa si el nombre de usuario ya existe
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "El nombre de usuario ya existe");
                return View(model);
            }

            // si el correo electronico ya esta eistente
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "El correo electronico ya esta registrado");
                return View(model);
            }

            // Hash the password con salt
            var (hash, salt) = _passwordHasher.HashPassword(model.Password);

            var user = new User
            {
                Username = model.Username,
                PasswordHash = hash,
                Salt = salt,
                Name = model.Name,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registro exitoso. Por favor inicia sesion.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null)
            {
                ModelState.AddModelError("", "Usuario o contrasena incorrectos");
                return View(model);
            }

            if (!_passwordHasher.VerifyPassword(model.Password, user.PasswordHash, user.Salt))
            {
                ModelState.AddModelError("", "Usuario o contrasena incorrectos");
                return View(model);
            }

            string sessionId = await _sessionService.CreateSessionAsync(user.Id);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, 
                Secure = true,   
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(5)
            };

            Response.Cookies.Append(SessionCookieName, sessionId, cookieOptions);

            return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue(SessionCookieName, out var sessionId))
            {
                await _sessionService.InvalidateSessionAsync(sessionId);

                Response.Cookies.Delete(SessionCookieName);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
