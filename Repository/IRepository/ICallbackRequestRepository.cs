using Microsoft.AspNetCore.Mvc.Rendering;
using Rolled_metal_products.Models;

namespace Rolled_metal_products.Repository.IRepository
{
    public interface ICallbackRequestRepository : IRepository<CallbackRequest>

    {
        void Update(CallbackRequest obj);
    }

}
