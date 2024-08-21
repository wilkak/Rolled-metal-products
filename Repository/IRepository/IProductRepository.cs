using Microsoft.AspNetCore.Mvc.Rendering;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rolled_metal_products.Models;

namespace Rolled_metal_products.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product obj);

        void DeleteExistingParameters(int id);

        IEnumerable<ProductParameter> GetParameter(int productId);
    }
}
