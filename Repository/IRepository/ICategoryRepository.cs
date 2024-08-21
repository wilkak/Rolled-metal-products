using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rolled_metal_products.Models;
using Rolled_metal_products.Models.ViewModels;

namespace Rolled_metal_products.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Add(Category obj);
        void Update(Category obj);
        Category GetCategoryWithSubCategories(int id);

        CreateCategoryVM GetCategory(int id);

        IEnumerable<CategoryParameter> GetCategoryParameters(int categoryId);
    }
}
