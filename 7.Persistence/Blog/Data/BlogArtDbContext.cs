//Profe no sabia que ocupaba la libreria de entity framework y eso me costo 10 min :( pero ya se pudo


using Blog.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data
{
    //No sabia que esta cosa se debia heredar (Ahhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh me esta matando esta tarea, pero lo peor es que me esta gustando)
    public class BlogArtDbContext: DbContext
    {
        public BlogArtDbContext(DbContextOptions<BlogArtDbContext> options) : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }

        // --- VERIFICA QUE ESTE MÉTODO ESTÉ AQUÍ Y SEA EXACTAMENTE ASÍ ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                    {
                        property.SetValueConverter(
                            new Microsoft.EntityFrameworkCore.Storage.ValueConversion.DateTimeOffsetToBinaryConverter()
                        );
                    }
                }
            }
        }
    }
}
