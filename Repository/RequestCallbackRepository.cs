using Microsoft.AspNetCore.Mvc.Rendering;
using Rolled_metal_products.Repository.IRepository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models;
namespace Rolled_metal_products.Repository
{
    public class RequestCallbackRepository: Repository<CallbackRequest>, ICallbackRequestRepository
    { 
        
        private readonly ApplicationDbContext _db;
        public RequestCallbackRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(CallbackRequest obj)
        {
            _db.CallbackRequest.Update(obj);
        }
    }
}
