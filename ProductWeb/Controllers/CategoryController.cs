using Microsoft.AspNetCore.Mvc;
using ProductWeb.Data;
using ProductWeb.Models;

namespace ProductWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ProductContext db;

        public CategoryController(ProductContext productContext)
        {
            db = productContext;
        }
        public IActionResult Index()
        {

            return View(db.Categories);
        }

        public IActionResult CreateAndUpdate(int? Id)
        {
            if (Id == 0 || Id == null) return View();

            var category = db.Categories.Find(Id);
            return View(category);
        }
        [HttpPost]
        public IActionResult CreateAndUpdate(Category data)
        {
            if(!ModelState.IsValid) return View(data);

            if(data.Id == 0 || data.Id == null)
            {
                //db.Categories.Add(data);
                TempData["Success"] = "Created SuccessFully";
                db.Add(data);
            }
            else
            {
                TempData["Success"] = "Updated SuccessFully";
                db.Update(data);
            }

            db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int Id)
        {
            var category = db.Categories.Find(Id);
            db.Categories.Remove(category);
            db.SaveChanges();

            TempData["Success"] = "Deleted SuccessFully";
            return RedirectToAction(nameof(Index));
        }
    }
}
