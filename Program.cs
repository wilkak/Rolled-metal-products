using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rolled_metal_products.Data;
using Rolled_metal_products.Initializer;
using Rolled_metal_products.Utility;
using Rolled_metal_products.Repository;
using Rolled_metal_products.Repository.IRepository;
using AspNet.Security.OAuth.Yandex;

namespace Rolled_metal_products
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            //IDbInitializer dbInitializer;
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
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            //builder.Services.AddScoped<IApplicationTypeRepository, ApplicationTypeRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IInquiryHeaderRepository, InquiryHeaderRepository>();
            builder.Services.AddScoped<IInquiryDetailRepository, InquiryDetailRepository>();

            builder.Services.AddScoped<IOrderHeaderRepository, OrderHeaderRepository>();
            builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();
            /* builder.Services.AddAuthentication().AddGoogle(googleOptions =>
             {
                 googleOptions.ClientId = configuration["477494116425-6gv9to29vdmqovf7vgi4hiuslv1nj11f.apps.googleusercontent.com"];
                 googleOptions.ClientSecret = configuration["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"];
             });*/
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddGoogle(options =>
                {
                    options.ClientId = configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                    options.CallbackPath = "/signin-google";
                })
                .AddYandex(options =>
                {
                    options.ClientId = configuration["Authentication:Yandex:ClientId"];
                    options.ClientSecret = configuration["Authentication:Yandex:ClientSecret"];
                    options.CallbackPath = "/signin-yandex";
                });
            /*    .AddYandex(options =>
            {
                options.ClientId = "9d56cb99abb54f6dbb07aafc6c3798e2";
                options.ClientSecret = "0936d01a00ac4158beea665486efeb2c";
                options.CallbackPath = "/signin-yandex";
             

            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = "477494116425-6gv9to29vdmqovf7vgi4hiuslv1nj11f.apps.googleusercontent.com";
                googleOptions.ClientSecret = "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";
            }); */
            /* .AddGoogle(options =>
             {
                 options.ClientId = "477494116425-6gv9to29vdmqovf7vgi4hiuslv1nj11f.apps.googleusercontent.com";
                 options.ClientSecret = "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";
                 options.CallbackPath = "/signin-google"; 
                 // You can set other options as needed.
             });*/


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
            //dbInitializer.Initialize();
            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                dbInitializer.Initialize();
            }
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
