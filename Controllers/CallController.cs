using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Rolled_metal_products.Controllers
{
    public class CallController : Controller
    {
        // GET: CallsController
        public ActionResult Index()
        {
            return View();
        }

        // GET: CallsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CallsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CallsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CallsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CallsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CallsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CallsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
