using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models;
using Rolled_metal_products.Models.ViewModels;
using Rolled_metal_products.Repository.IRepository;
using Rolled_metal_products.Utility;
using System.Diagnostics;

namespace Rolled_metal_products.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _prodRepo;
        private readonly ICategoryRepository _catRepo;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, IProductRepository prodRepo,
            ICategoryRepository catRepo)
        {
            _logger = logger;
            _prodRepo = prodRepo;
            _catRepo = catRepo;
        }



        public IActionResult Index()
        {
            return View();
        }
      
    }
}
