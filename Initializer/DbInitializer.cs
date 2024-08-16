using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rolled_metal_products;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models;

namespace Rolled_metal_products.Initializer 
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public DbInitializer(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                Console.WriteLine($"Ошибка применения миграций: {ex.Message}");
            }

            // Создаем роли, если они еще не существуют
            if (!_roleManager.RoleExistsAsync(WC.AdminRole).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(WC.AdminRole)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(WC.CustomerRole)).GetAwaiter().GetResult();
              //  _roleManager.CreateAsync(new IdentityRole("Anonymous")).GetAwaiter().GetResult(); // Роль для анонимного пользователя
            }

            // Проверяем и создаем администратора
            if (!_db.ApplicationUsers.Any(u => u.Email == "admin@gmail.com"))
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                    FullName = "Admin Tester",
                    PhoneNumber = "111111111111"
                };

                var result = _userManager.CreateAsync(adminUser, "Admin123*").GetAwaiter().GetResult();

                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(adminUser, WC.AdminRole).GetAwaiter().GetResult();
                }
                else
                {
                    Console.WriteLine("Не удалось создать администратора: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // Проверяем и создаем анонимного пользователя
            if (!_db.ApplicationUsers.Any(u => u.Email == "anonymous@domain.com"))
            {
                var anonymousUser = new ApplicationUser
                {
                    Id = "anonymous",
                    UserName = "anonymous",
                    Email = "anonymous@domain.com",
                    EmailConfirmed = true,
                    FullName = "Anonymous User"
                };

                var anonResult = _userManager.CreateAsync(anonymousUser, "Anonymous123*").GetAwaiter().GetResult();

                if (anonResult.Succeeded)
                {
                    _userManager.AddToRoleAsync(anonymousUser, WC.CustomerRole).GetAwaiter().GetResult();
                }
                else
                {
                    Console.WriteLine("Не удалось создать анонимного пользователя: " + string.Join(", ", anonResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
