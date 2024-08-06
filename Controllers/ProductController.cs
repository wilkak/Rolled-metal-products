using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models;
using Rolled_metal_products.Models.ViewModels;
using System.Security.AccessControl;


namespace Rolled_metal_products.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment environment) 
        {
            _context = context;
            _environment = environment;
        }


        public IActionResult Index()
        {
            IEnumerable<Product> productList = _context.Products.Include(x => x.Category);

            /*foreach (var product in productList) {
                product.Category = _context.Categories.FirstOrDefault(x => x.Id == product.CategoryId);
            }*/

            return View(productList);
        }

        //GET - CREATE
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _context.Categories.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                ApplicationTypeSelectList = _context.ApplicationType.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            if(id == null)
            {
                return View(productVM);
            }
            {
                productVM.Product = _context.Products.Find(id); 
                if(productVM.Product == null)
                {
                    return NotFound();
                }
            }
            return View(productVM);
        }

        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _environment.WebRootPath;

                if (productVM.Product.Id == 0)
                {
                    string upload = webRootPath + WC.ImagePath;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    productVM.Product.ImageName = fileName + extension;
                    _context.Products.Add(productVM.Product);
                }
                else 
                {
                    var productFromDb = _context.Products.AsNoTracking().FirstOrDefault(x => x.Id == productVM.Product.Id);


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

                        productVM.Product.ImageName = fileName + extension;
                    }
                    else 
                    {
                        productVM.Product.ImageName = productFromDb.ImageName;
                    }
                    _context.Products.Update(productVM.Product);
                }
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            productVM.CategorySelectList = _context.Categories.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            productVM.ApplicationTypeSelectList = _context.ApplicationType.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });

            return View(productVM);
        }

        
        //GET - DELETE
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product product = _context.Products.Include(u => u.Category).FirstOrDefault(u => u.Id == id);
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
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var product = _context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            string webRootPath = _environment.WebRootPath;
            string upload = webRootPath + WC.ImagePath;
            var oldFile = Path.Combine(upload, product.ImageName);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);
            }

            if (ModelState.IsValid)
            {
                _context.Remove(product);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }
    }
}
