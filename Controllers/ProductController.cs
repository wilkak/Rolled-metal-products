using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models;
using Rolled_metal_products.Models.ViewModels;
using Rolled_metal_products.Repository.IRepository;
using System.Security.AccessControl;
using X.PagedList.Extensions;


namespace Rolled_metal_products.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _prodRepo;
        private readonly ICategoryRepository _catRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IProductRepository prodRepo, ICategoryRepository catRepo, IWebHostEnvironment webHostEnvironment)
        {
            _prodRepo = prodRepo;
            _catRepo = catRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(string? searchString, string? sortOrder, int? page)
        {
            // Устанавливаем значения для ViewData
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;

            int pageSize = 10; // Количество элементов на странице
            int pageNumber = page ?? 1; // Номер страницы, если не задано, будет 1

            IEnumerable<Product> productList = _prodRepo.GetAll(includeProperties: "Category");
            if (!String.IsNullOrEmpty(searchString))
            {
                productList = productList.Where(c => c.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name":
                    productList = productList.OrderBy(c => c.Name);
                    break;
                case "name_desc":
                    productList = productList.OrderByDescending(c => c.Name);
                    break;
                case "date":
                    productList = productList;
                    break;
                case "date_desc":
                    productList = productList.Reverse();
                    break;
                case "category":
                    productList = productList.OrderBy(c => c.Category.Name);
                    break;
                default:
                    productList = productList.Reverse();
                    break;
            }

            productList = productList.ToPagedList(pageNumber, pageSize);
            return View(productList);
        }


        // GET - CREATE
        public IActionResult Create(int categoryId)
        {
            var category = _catRepo.FirstOrDefault(c => c.Id == categoryId, includeProperties: "CategoryParameters");
            if (category == null)
            {
                return NotFound();
            }

            Product product = new Product();
            product.CategoryId = categoryId;

            var createProductVM = new CreateProductVM
            {
                Product = product,
                CategoryParameters = category.CategoryParameters.Select(cp => new ProductParameterVM
                {
                    CategoryParameterId = cp.Id,
                    Name = cp.Name
                }).ToList()
            };
            return View(createProductVM);
        }

        
        // POST - CREATE
        [HttpPost]
        public IActionResult Create(CreateProductVM createProductVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;
                string uploadPath = Path.Combine(webRootPath, "images", "product");

                // Проверка существования директории и создание при необходимости
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(files[0].FileName);

                using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }

                createProductVM.Product.ImageName = fileName + extension;

                var product = createProductVM.Product;

                product.ProductParameters = createProductVM.CategoryParameters.Select(pp => new ProductParameter
                {
                    ProductId = createProductVM.Product.Id,
                    CategoryParameterId = pp.CategoryParameterId,
                    Value = pp.Value
                }).ToList();

                _prodRepo.Add(product);
                _prodRepo.Save();
                TempData[WC.Success] = "Товар успешно создан";
                return RedirectToAction("Details", "Category", new { id = product.CategoryId });
            }
            TempData[WC.Error] = "Ошибка при создании товара";
            return View(createProductVM);
        }

        // GET - EDIT 
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _prodRepo.FirstOrDefault(p => p.Id == id);

            // Получение всех параметров категории
            var categoryParameters = _catRepo.GetCategoryParameters(product.CategoryId);

            // Получение параметров продукта
            var productParameters = _prodRepo.GetParameter(id);

            if (product == null)
            {
                return NotFound();
            }

            // Создание списка параметров для отображения
            var categoryParameterVMs = categoryParameters.Select(cp => new ProductParameterVM
            {
                Name = cp.Name,
                Id = cp.Id,
                CategoryParameterId = cp.Id,
                Value = productParameters.FirstOrDefault(pp => pp.CategoryParameterId == cp.Id)?.Value ?? string.Empty
            }).ToList();

            var viewModel = new CreateProductVM
            {
                Product = product,
                CategoryParameters = categoryParameterVMs
            };
            return View(viewModel);
        }

        // POST - EDIT
        [HttpPost]
        public IActionResult Edit(CreateProductVM createProductVM)
        {
            if (ModelState.IsValid)
            {
                var productFromDb = _prodRepo.FirstOrDefault(u => u.Id == createProductVM.Product.Id, isTracking: false);

                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;

                if (files.Count > 0)
                {
                    string upload = webRootPath + WC.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    var oldFile = Path.Combine(upload, productFromDb.ImageName);

                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Delete(oldFile);
                    }

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    createProductVM.Product.ImageName = fileName + extension;
                }
                else
                {
                    createProductVM.Product.ImageName = productFromDb.ImageName;
                }

                _prodRepo.DeleteExistingParameters(createProductVM.Product.Id);

                var product = createProductVM.Product;

                product.ProductParameters = createProductVM.CategoryParameters.Select(pp => new ProductParameter
                { 
                    ProductId = createProductVM.Product.Id,
                    CategoryParameterId = pp.CategoryParameterId,
                    Value = pp.Value
                }).ToList();

                _prodRepo.Update(product);
                _prodRepo.Save();
                TempData[WC.Success] = "Изменение успешно завершено";
                return RedirectToAction("Details", "Category", new { id = product.CategoryId });
            }
            TempData[WC.Error] = "Ошибка при изменении";
            return View(createProductVM);
        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product product = _prodRepo.FirstOrDefault(u => u.Id == id, includeProperties: "Category");
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    
        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var product = _prodRepo.FirstOrDefault(x => x.Id == id, includeProperties: "ProductParameters");
            if (product == null)
            {
                return NotFound();
            }

            string upload = _webHostEnvironment.WebRootPath + WC.ImagePath;
            var oldFile = Path.Combine(upload, product.ImageName);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }

            if (ModelState.IsValid)
            {
                _prodRepo.Remove(product);
                _prodRepo.Save();
                TempData[WC.Success] = "Успешно удалено";
                return RedirectToAction("Details", "Category", new { id = product.CategoryId });
            }
            TempData[WC.Error] = "Ошибка при удалении";
            return View(product);
        }
    }
}
