using Microsoft.AspNetCore.Mvc.Rendering;
using Rolled_metal_products.Repository.IRepository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models;
using Microsoft.EntityFrameworkCore;
using Rolled_metal_products.Models.ViewModels;

namespace Rolled_metal_products.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
        }

        public Category GetCategory(int id)
        {
            var category = _db.Categories
                .Include(c => c.CategoryParameters)
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return null;
            }

            return category;
        }

        public void DeleteExistingParameters(int id)
        {
            var existingParameters = _db.ProductParameters.Where(pp => pp.ProductId == id).ToList();
            _db.ProductParameters.RemoveRange(existingParameters);
            _db.SaveChanges();
        }

        public IEnumerable<SelectListItem> GetAllDropdownList(string obj)
        {
            if (obj == WC.CategoryName)
            {
                return _db.Categories.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
            }
            /*if (obj == WC.ApplicationTypeName)
            {
                return _db.ApplicationType.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
            }*/
            return null;
        }

        public void Update(Product obj)
        {
            _db.Products.Update(obj);
        }
    }
}
