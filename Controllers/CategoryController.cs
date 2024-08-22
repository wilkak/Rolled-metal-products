using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models;
using Rolled_metal_products.Models.ViewModels;
using Rolled_metal_products.Repository.IRepository;
using Syncfusion.EJ2.Layouts;


namespace Rolled_metal_products.Controllers
{
    [Authorize(Roles =  WC.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _catRepo;
        private readonly IProductRepository _prodRepo;
        private readonly IWebHostEnvironment _environment;

        public CategoryController(ICategoryRepository catRepo, IProductRepository prodRepo, IWebHostEnvironment environment)
        {
            _catRepo = catRepo;
            _prodRepo = prodRepo;
            _environment = environment;

        }

        // GET - INDEX
        public IActionResult Index(string? searchString, string? sortOrder)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;

            IEnumerable<Category> categoryList = _catRepo.GetAll(includeProperties: "SubCategories,Products", filter: x => x.ParentId == null);

            if (!String.IsNullOrEmpty(searchString))
            {
                categoryList = categoryList.Where(c => c.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    categoryList = categoryList.OrderByDescending(c => c.Name);
                    break;
                case "displayOrder":
                    categoryList = categoryList.OrderBy(c => c.DisplayOrder);
                    break;
                case "displayOrder_desc":
                    categoryList = categoryList.OrderByDescending(c => c.DisplayOrder);
                    break;
                case "date":
                    categoryList = categoryList;
                    break;
                case "date_desc":
                    categoryList = categoryList.Reverse();
                    break;
                default:
                    categoryList = categoryList.OrderBy(c => c.Name);
                    break;
            }

            return View(categoryList);
        }

        // GET - CREATE
        [HttpGet]
        public IActionResult Create(int? parentId)
        {
            var category = new Category();
            if (parentId != null)
            {
                category.ParentId = parentId;
            }
            CreateCategoryVM categoryVM = new CreateCategoryVM()
            {
                Category = category
            };
            return View(categoryVM);
        }

        // POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateCategoryVM createCategoryVM)
        {
            if (ModelState.IsValid)
            {
                # region Add Image

                var files = HttpContext.Request.Form.Files;
                string webRootPath = _environment.WebRootPath;
                string uploadPath = Path.Combine(webRootPath, "images", "category");

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

                createCategoryVM.Category.ImageName = fileName + extension;

                # endregion

                var category = createCategoryVM.Category;

                category.CategoryParameters = createCategoryVM.Parameters;
                _catRepo.Add(category);

                _catRepo.Save();
                TempData[WC.Success] = "Категория успешно создана";

                if (createCategoryVM.Category.ParentId == null)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction("Details", new { id = createCategoryVM.Category.ParentId });
                }

            }
            TempData[WC.Error] = "Ошибка при создании категории";
            return View(createCategoryVM);
        }

        //GET - EDIT
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var createCategoryVM = _catRepo.GetCategory(id.GetValueOrDefault());
            if (createCategoryVM == null)
            {
                return NotFound();
            }
            return View(createCategoryVM);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CreateCategoryVM createCategoryVM)
        {
            if (ModelState.IsValid)
            {
                var categoryFromDb = _catRepo.FirstOrDefault(u => u.Id == createCategoryVM.Category.Id, isTracking: false);
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _environment.WebRootPath;

                if (files.Count > 0)
                {
                    string upload = webRootPath + WC.ImagePathCategory;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    var oldFile = Path.Combine(upload, categoryFromDb.ImageName);

                    if (System.IO.File.Exists(oldFile))
                    {
                        System.IO.File.Delete(oldFile);
                    }

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    createCategoryVM.Category.ImageName = fileName + extension;
                }
                else
                {
                    createCategoryVM.Category.ImageName = categoryFromDb.ImageName;
                }

                var category = createCategoryVM.Category;

                category.CategoryParameters = createCategoryVM.Parameters;
                _catRepo.Update(category);

                _catRepo.Save();
                TempData[WC.Success] = "Изменение успешно завершено";

                if (createCategoryVM.Category.ParentId == 0 || createCategoryVM.Category.ParentId == null)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction("Details", new { id = createCategoryVM.Category.ParentId });
                }
            }
            return View(createCategoryVM);
        }

        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _catRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }


        //POST - DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var category = _catRepo.Find(id.GetValueOrDefault());
            if (category == null)
            {
                return NotFound();
            }
            DeleteCategoryAndSubCategories(category.Id);
            _catRepo.Save();
            if (category.ParentId == null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction("Details", new { id = category.ParentId });
            }
        }

        // DELETE SUBCATEGORIES AND PRODUCTS
        private void DeleteCategoryAndSubCategories(int categoryId)
        {
            var category = _catRepo.GetCategoryWithSubCategories(categoryId);
            if (category == null)
            {
                return;
            }
            string webRootPath = _environment.WebRootPath;
            string upload = webRootPath + WC.ImagePath;
            // Удаляем все товары, связанные с категорией
            var products = _prodRepo.GetAll(x => x.CategoryId == categoryId, includeProperties: "ProductParameters");
            foreach (var product in products)
            {
                var oldFile = Path.Combine(upload, product.ImageName);
                if (System.IO.File.Exists(oldFile))
                {
                    System.IO.File.Delete(oldFile);
                }
                _prodRepo.Remove(product);
            }
            if (category.SubCategories != null && category.SubCategories.Any())
            {
                foreach (var subCategory in category.SubCategories.ToList())
                {
                    DeleteCategoryAndSubCategories(subCategory.Id);
                }
            }
            // Удаляем изображения для категории
            string uploadCategory = webRootPath + WC.ImagePathCategory;
            var oldFileCategory = Path.Combine(uploadCategory, category.ImageName);
            if (System.IO.File.Exists(oldFileCategory))
            {
                System.IO.File.Delete(oldFileCategory);
            }
            _catRepo.Remove(category);
        }


        //GET - DETAILS
        public IActionResult Details(int id)
        {
            var category = _catRepo.GetCategoryWithSubCategories(id);
            if (category == null)
            {
                return NotFound();
            }
            var products = _prodRepo.GetAll(filter: c => c.CategoryId == id).ToList();
            var model = new CategoryDetailsVM()
            {
                Category = category,
                Products = products
            };
            return View(model);
        }
    }
}
