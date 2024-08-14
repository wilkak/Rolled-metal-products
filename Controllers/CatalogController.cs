using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models.ViewModels;
using Rolled_metal_products.Models;
using Rolled_metal_products.Utility;
using Rolled_metal_products.Repository.IRepository;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace Rolled_metal_products.Controllers
{
    public class CatalogController : Controller
    {
        /*private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public CatalogController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }*/

        private readonly IProductRepository _prodRepo;
        private readonly ICategoryRepository _catRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public CatalogController(IProductRepository prodRepo, ICategoryRepository catRepo, IWebHostEnvironment webHostEnvironment)
        {
            _prodRepo = prodRepo;
            _catRepo = catRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(int? parentId)
        {
            var viewModel = new CatalogVM
            {
                ParentCategories = _catRepo.GetAll(x => x.ParentId == null, includeProperties: "SubCategories").ToList(),
                ChildCategories = new List<Category>(),
                BreadcrumbCategories = new List<Category>()
            };

            if (parentId.HasValue)
            {
                var parentCategory = _catRepo.Find(parentId.Value);
                if (parentCategory != null)
                {
                    viewModel.ParentCategoryName = parentCategory.Name;
                    viewModel.ChildCategories = _catRepo.GetAll(x => x.ParentId == parentId.Value, includeProperties: "SubCategories").ToList();
                    viewModel.BreadcrumbCategories = GetBreadcrumbCategories(parentCategory.Id);

                    if (!viewModel.ChildCategories.Any())
                    {
                        return RedirectToAction("CategoryProducts", new { categoryId = parentId.Value });
                    }
                }
            }
            else
            {
                viewModel.ParentCategoryName = "Категории";
                viewModel.ChildCategories = _catRepo.GetAll(x => x.ParentId == null, includeProperties: "SubCategories").ToList();
            }
            return View(viewModel);
        }

        public IActionResult CategoryProducts(int categoryId)
        {
            var category = _catRepo.FirstOrDefault(c => c.Id == categoryId, includeProperties: "CategoryParameters");

            if (category == null)
            {
                return NotFound();
            }

            var products = _prodRepo.GetAll(filter: p => p.CategoryId == categoryId, includeProperties: "ProductParameters");

            var categoryParameters = new Dictionary<string, List<string>>();

            foreach (var parameter in category.CategoryParameters)
            {
                var values = products
                    .SelectMany(p => p.ProductParameters)
                    .Where(pp => pp.Name == parameter.Name)
                    .Select(pp => pp.Value)
                    .Distinct()
                    .ToList();

                categoryParameters.Add(parameter.Name, values);
            }

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            // Создаем список ProductVM и проверяем наличие каждого товара в корзине
            var productVMList = products.Select(product => new ProductVM
            {
                Product = product,
                ExistsInCart = shoppingCartList.Any(cartItem => cartItem.ProductId == product.Id)
            }).ToList();

            var breadcrumbCategories = GetBreadcrumbCategories(categoryId);

            var viewModel = new CategoryProductsVM
            {
                Category = category,
                Products = productVMList,
                CategoryParameters = categoryParameters,
                BreadcrumbCategories = breadcrumbCategories
            };

            return View(viewModel);
        }

        private List<Category> GetBreadcrumbCategories(int categoryId)
        {
            var categories = new List<Category>();
            var currentCategory = _catRepo.Find(categoryId);

            while (currentCategory!= null)
            {
                if (currentCategory.ParentId != null)
                {
                    categories.Insert(0, currentCategory);
                    currentCategory = _catRepo.Find(currentCategory.ParentId.Value);
                }
                else {
                    categories.Insert(0, currentCategory);
                    currentCategory = null;
                }
            }

            return categories;
        }

        [HttpPost]
        public IActionResult FilterProducts([FromBody] Dictionary<string, List<string>> selectedParameters)
        {
            var productsQuery = _prodRepo.GetAll(includeProperties: "ProductParameters").AsQueryable();

            foreach (var parameter in selectedParameters)
            {
                productsQuery = productsQuery.Where(p => p.ProductParameters
                    .Any(pp => pp.Name == parameter.Key && parameter.Value.Contains(pp.Value)));
            }

            // Выполняем запрос к базе данных и загружаем продукты в память
            var products = productsQuery.ToList();

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            // Создаем список ProductVM и проверяем наличие каждого товара в корзине
            var productVMList = products.Select(product => new ProductVM
            {
                Product = product,
                ExistsInCart = shoppingCartList.Any(cartItem => cartItem.ProductId == product.Id)
            }).ToList();

            return PartialView("_IndividualProductCard", productVMList);
        }

        public IActionResult ProductDetails(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            var product = _prodRepo.FirstOrDefault(x => x.Id == id, includeProperties: "ProductParameters");

            DetailsVM detailsVM = new DetailsVM()
            {
                Product = product,
                ExistsInCart = false
            };

            foreach (var item in shoppingCartList)
            {
                if (item.ProductId == id)
                {
                    detailsVM.ExistsInCart = true;
                }
            }

            var breadcrumbCategories = GetBreadcrumbCategories(product.CategoryId);

            detailsVM.BreadcrumbCategories = breadcrumbCategories;

            return View(detailsVM);
        }

        public IActionResult ProductDetailsPost(int id, DetailsVM detailsVM)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            shoppingCartList.Add(new ShoppingCart { ProductId = id, SqFt = detailsVM.Product.TempSqFt });
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            TempData[WC.Success] = "Item add to cart successfully";
            return RedirectToAction("ProductDetails", new { id = id });
        }

        public IActionResult RemoveFromCart(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            var itemToRemove = shoppingCartList.FirstOrDefault(x => x.ProductId == id);

            if (itemToRemove != null)
            {
                shoppingCartList.Remove(itemToRemove);
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            TempData[WC.Success] = "Item removed from cart successfully";
            return RedirectToAction("ProductDetails", new { id = id });
        }

        [HttpPost]
        public IActionResult AddToCartAjax(int productId)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            shoppingCartList.Add(new ShoppingCart { ProductId = productId });
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            return Json(new { success = true, message = "Товар добавлен в корзину" });
        }

        [HttpPost]
        public IActionResult RemoveFromCartAjax(int productId)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();

            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            var itemToRemove = shoppingCartList.SingleOrDefault(r => r.ProductId == productId);
            if (itemToRemove != null)
            {
                shoppingCartList.Remove(itemToRemove);
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            return Json(new { success = true, message = "Товар удален из корзины" });
        }
    }
}
