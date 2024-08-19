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

        public IActionResult Index(string? searchString, string? sortOrder)
        {
            // Устанавливаем значения для ViewData
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;

            IEnumerable<Product> objList = _prodRepo.GetAll(includeProperties: "Category");

            if (!String.IsNullOrEmpty(searchString))
            {
                objList = objList.Where(c => c.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    objList = objList.OrderByDescending(c => c.Name);
                    break;
                case "date":
                    objList = objList;
                    break;
                case "date_desc":
                    objList = objList.Reverse();
                    break;
                case "category":
                    objList = objList.OrderBy(c => c.Category.Name);
                    break;
                default:
                    objList = objList.OrderBy(c => c.Name);
                    break;
            }


            return View(objList);
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
            var model = new CreateProductVM
            {
                Product = product,
                CategoryParameters = category.CategoryParameters.Select(cp => new ProductParameterVM
                {
                    CategoryParameterId = cp.Id,
                    Name = cp.Name
                }).ToList()
            };

            return View(model);
        }

        // POST - CREATE
        /* [HttpPost]
         public IActionResult Create(CreateProductVM viewModel)
         {
             if (ModelState.IsValid)
             {
                 var files = HttpContext.Request.Form.Files;
                 string webRootPath = _webHostEnvironment.WebRootPath;

                 string upload = webRootPath + WC.ImagePath;
                 string fileName = Guid.NewGuid().ToString();
                 string extension = Path.GetExtension(files[0].FileName);

                 using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                 {
                     files[0].CopyTo(fileStream);
                 }

                 viewModel.Product.ImageName = fileName + extension;

                 var product = viewModel.Product;

                 product.ProductParameters = viewModel.CategoryParameters.Select(pp => new ProductParameter
                 {
                     ProductId = viewModel.Product.Id,
                     Name = pp.Name,
                     Value = pp.Value
                 }).ToList();

                 _prodRepo.Add(product);
                 _prodRepo.Save();

                 return RedirectToAction("Details", "Category", new { id = product.CategoryId });
                 //return RedirectToAction("","Index");
             }

             return View(viewModel);
         }*/

        // POST - CREATE
        [HttpPost]
        public IActionResult Create(CreateProductVM viewModel)
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

                viewModel.Product.ImageName = fileName + extension;

                var product = viewModel.Product;

                product.ProductParameters = viewModel.CategoryParameters.Select(pp => new ProductParameter
                {
                    ProductId = viewModel.Product.Id,
                    Name = pp.Name,
                    Value = pp.Value
                }).ToList();

                _prodRepo.Add(product);
                _prodRepo.Save();

                return RedirectToAction("Details", "Category", new { id = product.CategoryId });
            }

            return View(viewModel);
        }

        // GET - EDIT 
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _prodRepo.FirstOrDefault(p => p.Id == id, includeProperties: "ProductParameters");

            if (product == null)
            {
                return NotFound();
            }

            var categoryParameters = product.ProductParameters
                .Select(pp => new ProductParameterVM
                {
                    Name = pp.Name,
                    Value = pp.Value
                }).ToList();

            var viewModel = new CreateProductVM
            {
                Product = product,
                CategoryParameters = categoryParameters
            };

            return View(viewModel);
        }

        // POST - EDIT
        [HttpPost]
        public IActionResult Edit(CreateProductVM viewModel)
        {
            if (ModelState.IsValid)
            {
                var productFromDb = _prodRepo.FirstOrDefault(u => u.Id == viewModel.Product.Id, isTracking: false);

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

                    viewModel.Product.ImageName = fileName + extension;
                }
                else
                {
                    viewModel.Product.ImageName = productFromDb.ImageName;
                }

                _prodRepo.DeleteExistingParameters(viewModel.Product.Id);

                var product = viewModel.Product;

                product.ProductParameters = viewModel.CategoryParameters.Select(pp => new ProductParameter
                {
                    ProductId = viewModel.Product.Id,
                    Name = pp.Name,
                    Value = pp.Value
                }).ToList();

                _prodRepo.Update(product);
                _prodRepo.Save();

                return RedirectToAction("Details", "Category", new { id = product.CategoryId });
            }
            return View(viewModel);
        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product product = _prodRepo.FirstOrDefault(u => u.Id == id, includeProperties: "Category");
            //product.Category = _db.Category.Find(product.CategoryId);
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
            var obj = _prodRepo.FirstOrDefault(x => x.Id == id, includeProperties: "ProductParameters");
            if (obj == null)
            {
                return NotFound();
            }

            string upload = _webHostEnvironment.WebRootPath + WC.ImagePath;
            var oldFile = Path.Combine(upload, obj.ImageName);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }

            if (ModelState.IsValid)
            {
                _prodRepo.Remove(obj);
                _prodRepo.Save();
                TempData[WC.Success] = "Успешно удалено";
                return RedirectToAction("Details", "Category", new { id = obj.CategoryId });
            }
            return View(obj);
        }
    }
}
