using Comprehension.Data;
using Comprehension.Models;
using Microsoft.EntityFrameworkCore;

namespace Comprehension.Services
{
    public interface IPermissionService
    {
        Task<bool> CanRead(Guid userId, Guid resourceId, ResourceType resourceType);
        Task<bool> CanWrite(Guid userId, Guid resourceId, ResourceType resourceType);
        Task<bool> CanDelete(Guid userId, Guid resourceId, ResourceType resourceType);
        Task<bool> CanManagePermissions(Guid userId, Guid resourceId, ResourceType resourceType);
        Task<bool> IsOwner(Guid userId, Guid resourceId, ResourceType resourceType);
        Task ShareResource(Guid resourceId, ResourceType resourceType, Guid ownerId, Guid sharedWithUserId, PermissionLevel permissionLevel);
        Task RevokePermission(Guid resourceId, ResourceType resourceType, Guid ownerId, Guid sharedWithUserId);
        Task<List<ResourcePermission>> GetResourcePermissions(Guid resourceId, ResourceType resourceType);
    }

    public class PermissionService : IPermissionService
    {
        private readonly ComprehensionContext _context;

        public PermissionService(ComprehensionContext context)
        {
            _context = context;
        }

        public async Task<bool> IsOwner(Guid userId, Guid resourceId, ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.Note => await _context.Note.AnyAsync(n => n.Id == resourceId && n.UserId == userId),
                ResourceType.Reminder => await _context.Reminder.AnyAsync(r => r.Id == resourceId && r.UserId == userId),
                ResourceType.Event => await _context.Event.AnyAsync(e => e.Id == resourceId && e.UserId == userId),
                _ => false
            };
        }

        public async Task<bool> CanRead(Guid userId, Guid resourceId, ResourceType resourceType)
        {
            // El user siempre puede leer
            if (await IsOwner(userId, resourceId, resourceType))
                return true;

            // Verificar si tiene permiso compartido (cualquier nivel permite lectura)
            return await _context.ResourcePermissions.AnyAsync(rp =>
                rp.ResourceId == resourceId &&
                rp.ResourceType == resourceType &&
                rp.SharedWithUserId == userId);
        }

        public async Task<bool> CanWrite(Guid userId, Guid resourceId, ResourceType resourceType)
        {
            // El user siempre puede escribir
            if (await IsOwner(userId, resourceId, resourceType))
                return true;

            // Verificar si tiene permiso ReadWrite o Admin
            return await _context.ResourcePermissions.AnyAsync(rp =>
                rp.ResourceId == resourceId &&
                rp.ResourceType == resourceType &&
                rp.SharedWithUserId == userId &&
                (rp.PermissionLevel == PermissionLevel.ReadWrite || rp.PermissionLevel == PermissionLevel.Admin));
        }

        public async Task<bool> CanDelete(Guid userId, Guid resourceId, ResourceType resourceType)
        {
            // El user siempre puede eliminar
            if (await IsOwner(userId, resourceId, resourceType))
                return true;

            // Solo Admin puede eliminar
            return await _context.ResourcePermissions.AnyAsync(rp =>
                rp.ResourceId == resourceId &&
                rp.ResourceType == resourceType &&
                rp.SharedWithUserId == userId &&
                rp.PermissionLevel == PermissionLevel.Admin);
        }

        public async Task<bool> CanManagePermissions(Guid userId, Guid resourceId, ResourceType resourceType)
        {
            // El user siempre puede gestionar permisos
            if (await IsOwner(userId, resourceId, resourceType))
                return true;

            // Solo Admin puede gestionar permisos (pero no puede revocar al user)
            return await _context.ResourcePermissions.AnyAsync(rp =>
                rp.ResourceId == resourceId &&
                rp.ResourceType == resourceType &&
                rp.SharedWithUserId == userId &&
                rp.PermissionLevel == PermissionLevel.Admin);
        }

        public async Task ShareResource(Guid resourceId, ResourceType resourceType, Guid ownerId, Guid sharedWithUserId, PermissionLevel permissionLevel)
        {
            // Verificar si ya existe el permiso
            var existing = await _context.ResourcePermissions.FirstOrDefaultAsync(rp =>
                rp.ResourceId == resourceId &&
                rp.ResourceType == resourceType &&
                rp.SharedWithUserId == sharedWithUserId);

            if (existing != null)
            {
                // Actualizar nivel de permiso
                existing.PermissionLevel = permissionLevel;
            }
            else
            {
                // Crear nuevo permiso
                var permission = new ResourcePermission
                {
                    Id = Guid.NewGuid(),
                    ResourceId = resourceId,
                    ResourceType = resourceType,
                    SharedWithUserId = sharedWithUserId,
                    OwnerId = ownerId,
                    PermissionLevel = permissionLevel,
                    SharedAt = DateTime.UtcNow
                };

                _context.ResourcePermissions.Add(permission);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RevokePermission(Guid resourceId, ResourceType resourceType, Guid ownerId, Guid sharedWithUserId)
        {
            var permission = await _context.ResourcePermissions.FirstOrDefaultAsync(rp =>
                rp.ResourceId == resourceId &&
                rp.ResourceType == resourceType &&
                rp.SharedWithUserId == sharedWithUserId);

            if (permission != null)
            {
                _context.ResourcePermissions.Remove(permission);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ResourcePermission>> GetResourcePermissions(Guid resourceId, ResourceType resourceType)
        {
            return await _context.ResourcePermissions
                .Include(rp => rp.SharedWithUser)
                .Where(rp => rp.ResourceId == resourceId && rp.ResourceType == resourceType)
                .ToListAsync();
        }
    }
}