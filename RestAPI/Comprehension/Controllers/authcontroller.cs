using Comprehension.DTOs;
using Comprehension.Services;
using Microsoft.AspNetCore.Mvc;

namespace Comprehension.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registrar un nuevo usuario
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, error, user) = await _authService.RegisterUserAsync(request);

            if (!success)
            {
                return BadRequest(new { message = error });
            }

            // Crear sesion
            var token = await _authService.CreateSessionAsync(user!.Id);

            var response = new AuthResponse
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = user.Name,
                    Email = user.Email,
                    DateOfBirth = user.DateOfBirth,
                    CreatedAt = user.CreatedAt
                },
                Message = "Usuario registrado exitosamente"
            };

            return Ok(response);
        }

        /// <summary>
        /// Iniciar sesión con credenciales
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, error, user) = await _authService.ValidateUserAsync(request.Username, request.Password);

            if (!success)
            {
                return Unauthorized(new { message = error });
            }

            // log out
            var token = await _authService.CreateSessionAsync(user!.Id);

            var response = new AuthResponse
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = user.Name,
                    Email = user.Email,
                    DateOfBirth = user.DateOfBirth,
                    CreatedAt = user.CreatedAt
                },
                Message = "Inicio de sesión exitoso"
            };

            return Ok(response);
        }

        /// <summary>
        /// Cerrar sesión actual
        /// </summary>
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            var token = GetTokenFromHeader();

            if (token != null)
            {
                await _authService.InvalidateSessionAsync(token);
            }

            return Ok(new { message = "Sesión cerrada exitosamente" });
        }

        /// <summary>
        /// Validar sesión actual
        /// </summary>
        [HttpGet("validate")]
        public async Task<ActionResult<SessionValidationResponse>> ValidateSession()
        {
            var token = GetTokenFromHeader();

            if (token == null)
            {
                return Ok(new SessionValidationResponse
                {
                    IsValid = false,
                    Message = "No hay token proporcionado"
                });
            }

            var user = await _authService.GetUserByTokenAsync(token);

            if (user == null)
            {
                return Ok(new SessionValidationResponse
                {
                    IsValid = false,
                    Message = "Token inválido o expirado"
                });
            }

            return Ok(new SessionValidationResponse
            {
                IsValid = true,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = user.Name,
                    Email = user.Email,
                    DateOfBirth = user.DateOfBirth,
                    CreatedAt = user.CreatedAt
                },
                Message = "Token válido"
            });
        }

        /// <summary>
        /// Obtener información del usuario actual
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var token = GetTokenFromHeader();

            if (token == null)
            {
                return Unauthorized(new { message = "No autenticado" });
            }

            var user = await _authService.GetUserByTokenAsync(token);

            if (user == null)
            {
                return Unauthorized(new { message = "Token inválido o expirado" });
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt
            });
        }

        private string? GetTokenFromHeader()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }

            return null;
        }
    }
}