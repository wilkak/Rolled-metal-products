using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rolled_metal_products.Data;
using Rolled_metal_products.Utility;
namespace Rolled_metal_products
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var configuration = builder.Configuration;

            // Добавление сервисов в контейнер.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddDefaultUI()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession(Options => 
            {
                Options.IdleTimeout = TimeSpan.FromMinutes(10);
                Options.Cookie.HttpOnly = true;
                Options.Cookie.IsEssential = true;
            });
           
           

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
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            
            /*app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            */
            app.Run();
        }
    }
}
