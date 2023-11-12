using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductWeb.Data;
using ProductWeb.Models;
using ProductWeb.ViewModels;
using System.Linq;

namespace ProductWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductContext productContext;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductController(ProductContext productContext,IWebHostEnvironment webHostEnvironment)
        {
            this.productContext = productContext;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var data = productContext.Products.Include(x=>x.Category).ToList();
            return View(data);
        }
        
        public IActionResult UpSert(int? id=0)
        {
            //var productVM = new ProductVM();
            //ProductVM productVM = new ProductVM();
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = productContext.Categories.Select(x => new SelectListItem 
                { 
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };

            if (id != 0) //Update            
            {
                var data = productContext.Products.Find(id);
                productVM.Product = data;
            }

            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpSert(ProductVM data,IFormFile file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(file.FileName);
                    var uploads = Path.Combine(wwwRootPath, @"images\products");

                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);


                    //กรณีมีรูปภาพเดิมต้องลบทิ้งก่อน
                    if (data.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, data.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    //บันทึกรุปภาพใหม่
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    data.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }
            }

            if(data.Product.Id == 0) productContext.Products.Add(data.Product);

            else productContext.Products.Update(data.Product);


            productContext.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        public IActionResult Delete(int id)
        {
            var data = productContext.Products.Find(id);

            string wwwRootPath = webHostEnvironment.WebRootPath;
            var uploads = Path.Combine(wwwRootPath, @"images\products");

            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            //กรณีมีรูปภาพเดิมต้องลบทิ้งก่อน
            if (data.ImageUrl != null)
            {
                var oldImagePath = Path.Combine(wwwRootPath,data.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            productContext.Remove(data);
            productContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
