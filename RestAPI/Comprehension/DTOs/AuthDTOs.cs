using System.ComponentModel.DataAnnotations;

namespace Comprehension.DTOs
{
    // DTO para registro
    public class RegisterRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es necesario")]
        [StringLength(50, MinimumLength = 3)]
        public required string Username { get; set; }

        [Required(ErrorMessage = "El nombre es necesario")]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "El correo electronico es necesario")]
        [EmailAddress]
        public required string Email { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es necesario")]
        public required DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "La contrasena es necesario")]
        [StringLength(100, MinimumLength = 6)]
        public required string Password { get; set; }
    }

    // DTO para login
    public class LoginRequest
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Password { get; set; }
    }

    // DTO para respuesta de autenticación (Bearer Token)
    public class AuthResponse
    {
        public required string Token { get; set; } // Cambiado de SessionId a Token
        public required UserDto User { get; set; }
        public required string Message { get; set; }
    }

    // DTO para información de usuario
    public class UserDto
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO para validar sesión
    public class SessionValidationResponse
    {
        public bool IsValid { get; set; }
        public UserDto? User { get; set; }
        public string? Message { get; set; }
    }

    // DTO para compartir recursos
    public class ShareResourceRequest
    {
        [Required]
        public required string Username { get; set; } // Usuario con quien compartir

        public string PermissionLevel { get; set; } = "ReadOnly"; // ReadOnly, ReadWrite, Admin
    }

    // DTO para crear recursos con permisos compartidos
    public class CreateWithSharingRequest
    {
        public List<ShareResourceRequest>? SharedWith { get; set; }
    }
}