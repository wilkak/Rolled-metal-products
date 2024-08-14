using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models;
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

        public CategoryController(ICategoryRepository catRepo)
        {
            _catRepo = catRepo;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> objList = _catRepo.GetAll();
            return View(objList);
        }
        /*
        public IActionResult Index()
        {
            IEnumerable<Category> objList = _db.Categories;
            return View(objList);
        }*/


        /*public IActionResult Index()
        {
            IEnumerable<Category> categoryList = _context.Categories;
            return View(categoryList);
        }*/

        //GET - CREATE
        public IActionResult Create()
        {
            return View();
        }

        //POST - CREATE
        /* [HttpPost]
         [ValidateAntiForgeryToken]
         public IActionResult Create(Category category)
         {
             if (ModelState.IsValid)
             {
                 _db.Categories.Add(category);
                 _db.SaveChanges();
                 return RedirectToAction("Index");
             }
             return View(category);
         }

         //GET - EDIT
         public IActionResult Edit(int? id)
         {
             if(id == null || id == 0) 
             {
                 return NotFound();
             }
             var category = _db.Categories.Find(id);
             if (category == null) 
             {
                 return NotFound();
             }

             return View(category);
         }

         //POST - EDIT
         [HttpPost]
         [ValidateAntiForgeryToken]
         public IActionResult Edit(Category category)
         {
             if (ModelState.IsValid)
             {
                 _db.Categories.Update(category);
                 _db.SaveChanges();
                 return RedirectToAction("Index");
             }
             return View(category);
         }


         //GET - DELETE
         public IActionResult Delete(int? id)
         {
             if (id == null || id == 0)
             {
                 return NotFound();
             }

             var category = _db.Categories.Find(id);

             if (category == null)
             {
                 return NotFound();
             }

             return View(category);
         }

         //POST - DELETE
         [HttpPost]
         [ValidateAntiForgeryToken]
         public IActionResult DeletePost(int? id)
         {
             if (id == null || id == 0)
             {
                 return NotFound();
             }

             var category = _db.Categories.Find(id);

             if (category == null)
             {
                 return NotFound();
             }

             if (ModelState.IsValid)
             {
                 _db.Remove(category);
                 _db.SaveChanges();
                 return RedirectToAction("Index");
             }
             return View(category);
         }*/
        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _catRepo.Add(obj);
                _catRepo.Save();
                TempData[WC.Success] = "Category created successfully";
                return RedirectToAction("Index");
            }
            TempData[WC.Error] = "Error while creating category";
            return View(obj);

        }


        //GET - EDIT
        public IActionResult Edit(int? id)
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

        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _catRepo.Update(obj);
                _catRepo.Save();
                TempData[WC.Success] = "Action completed successfully";
                return RedirectToAction("Index");
            }
            return View(obj);

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
            var obj = _catRepo.Find(id.GetValueOrDefault());
            if (obj == null)
            {
                return NotFound();
            }
            TempData[WC.Success] = "Action completed successfully";
            _catRepo.Remove(obj);
            _catRepo.Save();
            return RedirectToAction("Index");
        }
    }
}
