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
    public class RemindersController : ControllerBase
    {
        private readonly ComprehensionContext _context;
        private readonly IPermissionService _permissionService;

        public RemindersController(ComprehensionContext context, IPermissionService permissionService)
        {
            _context = context;
            _permissionService = permissionService;
        }

        private Guid GetCurrentUserId()
        {
            return (Guid)HttpContext.Items["UserId"]!;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reminder>>> GetReminder()
        {
            var userId = GetCurrentUserId();

            var ownReminders = await _context.Reminder
                .Where(r => r.UserId == userId)
                .ToListAsync();

            var sharedReminderIds = await _context.ResourcePermissions
                .Where(rp => rp.ResourceType == ResourceType.Reminder && rp.SharedWithUserId == userId)
                .Select(rp => rp.ResourceId)
                .ToListAsync();

            var sharedReminders = await _context.Reminder
                .Where(r => sharedReminderIds.Contains(r.Id))
                .ToListAsync();

            return Ok(ownReminders.Concat(sharedReminders).Distinct());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reminder>> GetReminder(Guid id)
        {
            var userId = GetCurrentUserId();
            var reminder = await _context.Reminder.FindAsync(id);

            if (reminder == null)
            {
                return NotFound();
            }

            if (!await _permissionService.CanRead(userId, id, ResourceType.Reminder))
            {
                return Forbid();
            }

            return reminder;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutReminder(Guid id, Reminder reminder)
        {
            var userId = GetCurrentUserId();

            if (reminder.Id == default)
            {
                reminder.Id = id;
            }

            if (id != reminder.Id)
            {
                return BadRequest();
            }

            var original = await _context.Reminder.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);

            if (original is null)
            {
                return NotFound();
            }

            if (!await _permissionService.CanWrite(userId, id, ResourceType.Reminder))
            {
                return Forbid();
            }

            reminder.UserId = original.UserId;
            _context.Entry(reminder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReminderExists(id))
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
        public async Task<ActionResult<Reminder>> PostReminder([FromBody] ReminderCreateRequest request)
        {
            var userId = GetCurrentUserId();

            var reminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Message = request.Message,
                ReminderTime = request.ReminderTime,
                IsCompleted = false
            };

            _context.Reminder.Add(reminder);
            await _context.SaveChangesAsync();

            if (request.SharedWith != null && request.SharedWith.Count > 0)
            {
                foreach (var share in request.SharedWith)
                {
                    var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == share.Username);
                    if (targetUser != null)
                    {
                        var permissionLevel = Enum.Parse<PermissionLevel>(share.PermissionLevel);
                        await _permissionService.ShareResource(reminder.Id, ResourceType.Reminder, userId, targetUser.Id, permissionLevel);
                    }
                }
            }

            return CreatedAtAction("GetReminder", new { id = reminder.Id }, reminder);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReminder(Guid id)
        {
            var userId = GetCurrentUserId();
            var reminder = await _context.Reminder.FindAsync(id);

            if (reminder == null)
            {
                return NotFound();
            }

            if (!await _permissionService.CanDelete(userId, id, ResourceType.Reminder))
            {
                return Forbid();
            }

            _context.Reminder.Remove(reminder);

            var permissions = await _context.ResourcePermissions
                .Where(rp => rp.ResourceId == id && rp.ResourceType == ResourceType.Reminder)
                .ToListAsync();
            _context.ResourcePermissions.RemoveRange(permissions);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/share")]
        public async Task<IActionResult> ShareReminder(Guid id, [FromBody] ShareResourceRequest request)
        {
            var userId = GetCurrentUserId();

            if (!ReminderExists(id))
            {
                return NotFound();
            }

            if (!await _permissionService.CanManagePermissions(userId, id, ResourceType.Reminder))
            {
                return Forbid();
            }

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (targetUser == null)
            {
                return BadRequest(new { message = "Usuario no encontrado" });
            }

            var permissionLevel = Enum.Parse<PermissionLevel>(request.PermissionLevel);
            await _permissionService.ShareResource(id, ResourceType.Reminder, userId, targetUser.Id, permissionLevel);

            return Ok(new { message = "Recordatorio compartido exitosamente" });
        }

        [HttpDelete("{id}/share/{username}")]
        public async Task<IActionResult> RevokeReminderAccess(Guid id, string username)
        {
            var userId = GetCurrentUserId();

            if (!ReminderExists(id))
            {
                return NotFound();
            }

            if (!await _permissionService.CanManagePermissions(userId, id, ResourceType.Reminder))
            {
                return Forbid();
            }

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (targetUser == null)
            {
                return BadRequest(new { message = "Usuario no encontrado" });
            }

            if (await _permissionService.IsOwner(targetUser.Id, id, ResourceType.Reminder))
            {
                return BadRequest(new { message = "No se puede revocar permisos del dueño" });
            }

            await _permissionService.RevokePermission(id, ResourceType.Reminder, userId, targetUser.Id);

            return Ok(new { message = "Acceso revocado exitosamente" });
        }

        [HttpGet("{id}/permissions")]
        public async Task<ActionResult<IEnumerable<object>>> GetReminderPermissions(Guid id)
        {
            var userId = GetCurrentUserId();

            if (!ReminderExists(id))
            {
                return NotFound();
            }

            if (!await _permissionService.CanManagePermissions(userId, id, ResourceType.Reminder))
            {
                return Forbid();
            }

            var permissions = await _permissionService.GetResourcePermissions(id, ResourceType.Reminder);

            var result = permissions.Select(p => new
            {
                Username = p.SharedWithUser?.Username,
                PermissionLevel = p.PermissionLevel.ToString(),
                SharedAt = p.SharedAt
            });

            return Ok(result);
        }

        private bool ReminderExists(Guid id)
        {
            return _context.Reminder.Any(e => e.Id == id);
        }
    }

    public class ReminderCreateRequest
    {
        public required string Message { get; set; }
        public required DateTime ReminderTime { get; set; }
        public List<ShareResourceRequest>? SharedWith { get; set; }
    }
}