
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models;
using Rolled_metal_products.Models.ViewModels;
using Rolled_metal_products.Repository.IRepository;

namespace Rolled_metal_products.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
        }

        /*public void Add(CreateCategoryVM model)
        {
            var category = model.Category;
            category.CategoryParameters = model.Parameters.ToList();

            _db.Categories.Add(category);
        }*/


        public void Add(Category obj)
        {
            _db.Categories.Add(obj);
        }

        public void Update(Category obj)
        {
            var categoryFromDb = _db.Categories.Include(x => x.CategoryParameters).FirstOrDefault(c => c.Id == obj.Id);

            /*categoryFromDb.CategoryParameters.Clear();*/

            if (obj != null)
            {
                categoryFromDb.Name = obj.Name;
                categoryFromDb.DisplayOrder = obj.DisplayOrder;
                if(obj.CategoryParameters != null) {
                    categoryFromDb.CategoryParameters = obj.CategoryParameters.ToList();
                }
            }
        }

        /*public void Update(Category obj)
        {
            _db.Categories.Update(obj);
        }*/

        public Category GetCategoryWithSubCategories(int id)
        {
            return _db.Categories.Include(c => c.SubCategories)
                .Include(c => c.CategoryParameters)
                .FirstOrDefault(c => c.Id == id);
        }

        public CreateCategoryVM GetCategory(int id)
        {
            var category = _db.Categories
                .Include(c => c.CategoryParameters)
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return null;
            }

            var viewModel = new CreateCategoryVM
            {
                Category = category,
                Parameters = category.CategoryParameters.ToList()
            };

            return viewModel;
        }

        public IEnumerable<CategoryParameter> GetCategoryParameters(int categoryId)
        {
            return _db.CategoryParameters.Where(cp => cp.CategoryId == categoryId).ToList();
        }

    }
}
