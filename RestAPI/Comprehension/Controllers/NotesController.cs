using Comprehension.Attributes;
using Comprehension.Data;
using Comprehension.DTOs;
using Comprehension.Models;
using Comprehension.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Comprehension.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorize] 
    public class NotesController : ControllerBase
    {
        private readonly ComprehensionContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IAuthService _authService;

        public NotesController(ComprehensionContext context, IPermissionService permissionService, IAuthService authService)
        {
            _context = context;
            _permissionService = permissionService;
            _authService = authService;
        }

        private Guid GetCurrentUserId()
        {
            return (Guid)HttpContext.Items["UserId"]!;
        }

        //  Solo devuelve notas del usuario autenticado o compartidas con él
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>>> GetNote()
        {
            var userId = GetCurrentUserId();

            // Obtener notas propias
            var ownNotes = await _context.Note
                .Where(n => n.UserId == userId)
                .ToListAsync();

            // Obtener IDs de notas compartidas
            var sharedNoteIds = await _context.ResourcePermissions
                .Where(rp => rp.ResourceType == ResourceType.Note && rp.SharedWithUserId == userId)
                .Select(rp => rp.ResourceId)
                .ToListAsync();

            // Obtener notas compartidas
            var sharedNotes = await _context.Note
                .Where(n => sharedNoteIds.Contains(n.Id))
                .ToListAsync();

            return Ok(ownNotes.Concat(sharedNotes).Distinct());
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> GetNote(Guid id)
        {
            var userId = GetCurrentUserId();
            var note = await _context.Note.FindAsync(id);

            if (note == null)
            {
                return NotFound();
            }

            // Verificar permiso de lectura
            if (!await _permissionService.CanRead(userId, id, ResourceType.Note))
            {
                return Forbid(); // 403 No tiene permiso
            }

            return note;
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutNote(Guid id, Note note)
        {
            var userId = GetCurrentUserId();

            if (note.Id == default)
            {
                note.Id = id;
            }

            if (id != note.Id)
            {
                return BadRequest();
            }

            var original = await _context.Note
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id);

            if (original is null)
            {
                return NotFound();
            }

            // Verificar permiso de escritura
            if (!await _permissionService.CanWrite(userId, id, ResourceType.Note))
            {
                return Forbid(); // 403
            }

            note.UserId = original.UserId; // Mantener el usuario original
            note.CreatedAt = original.CreatedAt;
            note.UpdatedAt = DateTime.UtcNow;
            _context.Entry(note).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                //Profe estuve volviendome loco porque no funcionaba y me faltaba un await y me desespero como no se lo imagina 
                if (!await NoteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Note>> PostNote([FromBody] NoteCreateRequest request)
        {
            var userId = GetCurrentUserId();

            var note = new Note
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = request.Title,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Note.Add(note);
            await _context.SaveChangesAsync();

            // Compartir con usuarios si se especificaron
            if (request.SharedWith != null && request.SharedWith.Count > 0)
            {
                foreach (var share in request.SharedWith)
                {
                    var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == share.Username);
                    if (targetUser != null)
                    {
                        var permissionLevel = Enum.Parse<PermissionLevel>(share.PermissionLevel);
                        await _permissionService.ShareResource(note.Id, ResourceType.Note, userId, targetUser.Id, permissionLevel);
                    }
                }
            }

            return CreatedAtAction("GetNote", new { id = note.Id }, note);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(Guid id)
        {
            var userId = GetCurrentUserId();
            var note = await _context.Note.FindAsync(id);

            if (note == null)
            {
                return NotFound();
            }

            // Verificar permiso para eliminar 
            if (!await _permissionService.CanDelete(userId, id, ResourceType.Note))
            {
                return Forbid(); // 403
            }

            _context.Note.Remove(note);

            // Eliminar permisos asociados
            var permissions = await _context.ResourcePermissions
                .Where(rp => rp.ResourceId == id && rp.ResourceType == ResourceType.Note)
                .ToListAsync();
            _context.ResourcePermissions.RemoveRange(permissions);

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPost("{id}/share")]
        public async Task<IActionResult> ShareNote(Guid id, [FromBody] ShareResourceRequest request)
        {
            var userId = GetCurrentUserId();

            if (!await NoteExists(id))
            {
                return NotFound();
            }

            // Verificar permiso para gestionar permisos (que ironico no jajajaja)
            if (!await _permissionService.CanManagePermissions(userId, id, ResourceType.Note))
            {
                return Forbid();
            }

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (targetUser == null)
            {
                return BadRequest(new { message = "Usuario no encontrado" });
            }

            var permissionLevel = Enum.Parse<PermissionLevel>(request.PermissionLevel);
            await _permissionService.ShareResource(id, ResourceType.Note, userId, targetUser.Id, permissionLevel);

            return Ok(new { message = "Nota compartida exitosamente" });
        }


        [HttpDelete("{id}/share/{username}")]
        public async Task<IActionResult> RevokeNoteAccess(Guid id, string username)
        {
            var userId = GetCurrentUserId();

            if (!await NoteExists(id))
            {
                return NotFound();
            }

            // Verificar permiso para gestionar permisos (ironico x2)
            if (!await _permissionService.CanManagePermissions(userId, id, ResourceType.Note))
            {
                return Forbid();
            }

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (targetUser == null)
            {
                return BadRequest(new { message = "Usuario no encontrado" });
            }

            // No se puede revocar permisos del dueño
            if (await _permissionService.IsOwner(targetUser.Id, id, ResourceType.Note))
            {
                return BadRequest(new { message = "No se puede revocar permisos del dueño" });
            }

            await _permissionService.RevokePermission(id, ResourceType.Note, userId, targetUser.Id);

            return Ok(new { message = "Acceso revocado exitosamente" });
        }


        [HttpGet("{id}/permissions")]
        public async Task<ActionResult<IEnumerable<object>>> GetNotePermissions(Guid id)
        {
            var userId = GetCurrentUserId();

            if (!await NoteExists(id))
            {
                return NotFound();
            }

            // Solo el user o admin pueden ver permisos
            if (!await _permissionService.CanManagePermissions(userId, id, ResourceType.Note))
            {
                return Forbid();
            }

            var permissions = await _permissionService.GetResourcePermissions(id, ResourceType.Note);

            var result = permissions.Select(p => new
            {
                Username = p.SharedWithUser?.Username,
                PermissionLevel = p.PermissionLevel.ToString(),
                SharedAt = p.SharedAt
            });

            return Ok(result);
        }

        private async Task<bool> NoteExists(Guid id)
        {
            return await _context.Note.AnyAsync(e => e.Id == id);
        }
    }

    // DTO para crear notas
    public class NoteCreateRequest
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        public List<ShareResourceRequest>? SharedWith { get; set; }
    }
}