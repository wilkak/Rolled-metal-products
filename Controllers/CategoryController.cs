using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models;
using Rolled_metal_products.Models.ViewModels;
using Rolled_metal_products.Repository.IRepository;


namespace Rolled_metal_products.Controllers
{
    [Authorize(Roles =  WC.AdminRole)]
    public class CategoryController : Controller
    {
        /* private readonly ApplicationDbContext _db;

         public CategoryController(ApplicationDbContext db)
         {
             _db = db;
         }*/

        private readonly ICategoryRepository _catRepo;
        private readonly IProductRepository _prodRepo;
        private readonly IWebHostEnvironment _environment;

        public CategoryController(ICategoryRepository catRepo, IProductRepository prodRepo, IWebHostEnvironment environment)
        {
            _catRepo = catRepo;
            _prodRepo = prodRepo;
            _environment = environment;

        }

        public IActionResult Index()
        {
            IEnumerable<Category> objList = _catRepo.GetAll(includeProperties: "SubCategories", filter: x => x.ParentId == null);
            return View(objList);
        }

        [HttpGet]
        public IActionResult Create(int? parentId)
        {
            var category = new Category();

            if (parentId != null && parentId != 0)
            {
                category.ParentId = parentId;
            }

            CreateCategoryVM categoryVM = new CreateCategoryVM()
            {
                Category = category
            };

            return View(categoryVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateCategoryVM model)
        {
            if (ModelState.IsValid)
            {
                _catRepo.Add(model);
                _catRepo.Save();
                TempData[WC.Success] = "Category created successfully";
                if (model.Category.ParentId == 0 || model.Category.ParentId == null)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction("Details", new { id = model.Category.ParentId });
                }

            }
            TempData[WC.Error] = "Error while creating category";
            return View(model);
        }

        //GET - EDIT
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _catRepo.GetCategory(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CreateCategoryVM viewModel)
        {
            if (ModelState.IsValid)
            {
                _catRepo.Update(viewModel);
                _catRepo.Save();
                TempData[WC.Success] = "Action completed successfully";
                return RedirectToAction("Index");
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
        public IActionResult DeletePost(int?id)
        {
            var obj = _catRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }

            DeleteCategoryAndSubCategories(obj.Id);
            _catRepo.Save();

            if (obj.ParentId == null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction("Details", new { id = obj.ParentId });
            }
        }

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
