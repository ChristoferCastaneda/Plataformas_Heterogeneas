using Blog.Data;
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

namespace Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //esto me lo dio gemini para que no me diera error de DateTimeOffset
            string path = builder.Environment.ContentRootPath;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");
            builder.Services.AddDbContext<BlogArtDbContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services.AddScoped<IArticleRepository, EFArticleRepository>();

            builder.Services.AddControllersWithViews();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Articles}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
