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

        /*
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;

        }*/

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                Products = _prodRepo.GetAll(includeProperties: "Category"),
                Categories = _catRepo.GetAll()
            };
            return View(homeVM);
        }
        /*
        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                Products = _context.Products.Include(x => x.Category),
                Categories = _context.Categories
            };
            return View(homeVM);
        }*/
        public IActionResult Details(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            DetailsVM DetailsVM = new DetailsVM()
            {
                Product = _prodRepo.FirstOrDefault(u => u.Id == id, includeProperties: "Category"),
                ExistsInCart = false
            };


            foreach (var item in shoppingCartList)
            {
                if (item.ProductId == id)
                {
                    DetailsVM.ExistsInCart = true;
                }
            }

            return View(DetailsVM);
        }
        //[HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id, DetailsVM detailsVM)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            shoppingCartList.Add(new ShoppingCart { ProductId = id, SqFt = detailsVM.Product.TempSqFt });
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            TempData[WC.Success] = "Item add to cart successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveFromCart(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            var itemToRemove = shoppingCartList.SingleOrDefault(r => r.ProductId == id);
            if (itemToRemove != null)
            {
                shoppingCartList.Remove(itemToRemove);
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            TempData[WC.Success] = "Item removed from cart successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        /* public IActionResult Details(int id)
         {
             List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
             if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
             {
                 shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
             }

             DetailsVM detailsVM = new DetailsVM()
             {
                 Product = _context.Products.Include(_x => _x.Category).Where(x => x.Id == id).FirstOrDefault(),
                 ExistsInCart = false

             };

             foreach (var item in shoppingCartList) { 
                 if(item.ProductId == id)
                 {
                     detailsVM.ExistsInCart = true;
                 }
             }
             return View(detailsVM);
         }

         public IActionResult DetailsPost(int id)
         {
             List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
             if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0) 
             {
                 shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
             }

             shoppingCartList.Add(new ShoppingCart { ProductId = id }); 
             HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
             return RedirectToAction(nameof(Index));
         }

         public IActionResult RemoveFromCart(int id)
         {
             List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
             if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
             {
                 shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
             }

             var itemToRemove = shoppingCartList.FirstOrDefault(x => x.ProductId == id);

             if(itemToRemove != null)
             {
                 shoppingCartList.Remove(itemToRemove);
             }

             HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
             return RedirectToAction(nameof(Index));
         }

         public IActionResult Privacy()
         {
             return View();
         }

         [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
         public IActionResult Error()
         {
             return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
         }*/
    }
}
