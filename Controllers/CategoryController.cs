using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models;
using Rolled_metal_products.Models.ViewModels;
using Rolled_metal_products.Repository.IRepository;
using Syncfusion.EJ2.Layouts;
using X.PagedList.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using MongoDB.Driver;
using System;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using System.Diagnostics.Metrics;


namespace Rolled_metal_products.Controllers
{
    [Authorize(Roles =  WC.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _catRepo;
        private readonly IProductRepository _prodRepo;
        private readonly IWebHostEnvironment _environment;
        private readonly IMongoCollection<ImageCategory> _imagesCategoryCollection;
        private readonly IMongoCollection<ImageProduct> _imagesProductCollection;

        public CategoryController(
            ICategoryRepository catRepo,
            IProductRepository prodRepo,
            IWebHostEnvironment environment,
            IMongoDatabase mongoDatabase)
        {
            _catRepo = catRepo;
            _prodRepo = prodRepo;
            _environment = environment;
            _imagesCategoryCollection = mongoDatabase.GetCollection<ImageCategory>("ImagesCategory");
            _imagesProductCollection = mongoDatabase.GetCollection<ImageProduct>("ImagesProduct");
        }

        // GET - INDEX
        public IActionResult Index(string? searchString, string? sortOrder, int? page)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;

            int pageSize = 10; // Количество элементов на странице
            int pageNumber = page ?? 1; // Номер страницы, если не задано, будет 1

            var categoryList = _catRepo.GetAll(includeProperties: "SubCategories,Products", filter: x => x.ParentId == null);

            if (!String.IsNullOrEmpty(searchString))
            {
                categoryList = categoryList.Where(c => c.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name":
                    categoryList = categoryList.OrderBy(c => c.Name);
                    break;
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
                    categoryList = categoryList.Reverse();
                    break;
            }

            categoryList = categoryList.ToPagedList(pageNumber, pageSize);

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
                var category = createCategoryVM.Category;

                category.CategoryParameters = createCategoryVM.Parameters;
                _catRepo.Add(category);
                _catRepo.Save();

                #region Add Image

                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    var file = files[0];
                    var image = new ImageCategory
                    {
                        Id = ObjectId.GenerateNewId(),
                        CategoryId = category.Id,
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        Data = GetFileBytes(file)
                    };

                    _imagesCategoryCollection.InsertOne(image);
                }

                #endregion

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

        private byte[] GetFileBytes(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
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

        // POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CreateCategoryVM createCategoryVM)
        {
            if (ModelState.IsValid)
            {
                var categoryFromDb = _catRepo.FirstOrDefault(u => u.Id == createCategoryVM.Category.Id, isTracking: false);

                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    var file = files[0];
                    var image = new ImageCategory
                    {
                        Id = ObjectId.GenerateNewId(),
                        FileName = file.FileName,
                        CategoryId = createCategoryVM.Category.Id,
                        ContentType = file.ContentType,
                        Data = GetFileBytes(file)
                    };

                    // Удаляем старое изображение из MongoDB
                    _imagesCategoryCollection.DeleteOne(i => i.CategoryId == categoryFromDb.Id);

                    // Добавляем новое изображение в MongoDB
                    _imagesCategoryCollection.InsertOne(image);
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
            TempData[WC.Error] = "Ошибка при изменении";
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

            TempData[WC.Success] = "Категория успешно удалена!";
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
                // Удаляем изображения Товаров из MongoDB
                _imagesProductCollection.DeleteOne(i => i.ProductId == product.Id);
            }

            // Удаляем подкатегории
            if (category.SubCategories != null && category.SubCategories.Any())
            {
                foreach (var subCategory in category.SubCategories.ToList())
                {
                    DeleteCategoryAndSubCategories(subCategory.Id);
                }
            }

            // Удаляем изображение категории из MongoDB
            _imagesCategoryCollection.DeleteOne(i => i.CategoryId == category.Id);

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
