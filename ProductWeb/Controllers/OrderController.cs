using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductWeb.Data;
using ProductWeb.Utility;
using ProductWeb.ViewModels;

namespace ProductWeb.Controllers
{
    public class OrderController : Controller
    {
        private readonly ProductContext productContext;
        private readonly IWebHostEnvironment webHostEnvironment;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(ProductContext productContext, IWebHostEnvironment webHostEnvironment)
        {
            this.productContext = productContext;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var data = productContext.OrderHeaders.ToList();

            return View(data);
        }

        public IActionResult Detail(int id)
        {
            //OrderVM = new OrderVM();
            OrderVM = new()
            {
                OrderHeader = productContext.OrderHeaders.Include(x=>x.User).FirstOrDefault(x=>x.Id == id),
                OrderDetail = productContext.OrderDetails.Include(x=>x.Product).Where(d => d.OrderId == id).ToList()
            };

            return View(OrderVM);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult UpdateOrderHeader()
        {
            var data = productContext.OrderHeaders.Find(OrderVM.OrderHeader.Id);

            var o = OrderVM.OrderHeader;
            data.Name = o.Name;
            data.StreetAddress = o.StreetAddress;
            data.City = o.City;
            data.State = o.State;
            data.PostalCode = o.PostalCode;

            productContext.SaveChanges();

            TempData["Success"] = "Update Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult StatusOrder(string status)
        {
            var data = productContext.OrderHeaders.Find(OrderVM.OrderHeader.Id);

            if(data.OrderStatus == SD.StatusPending)
            {
                data.OrderStatus = status;
                TempData["Success"] = "Update Successfully";
            }
            else
            {
                TempData["Success"] = "Not alloe to Update";
            }

            productContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            //var detail = productContext.OrderDetails.Where(p => p.ProductId == id).ToList();
            //productContext.OrderDetails.RemoveRange(detail);
            //productContext.SaveChanges();

            var data = productContext.OrderHeaders.Find(id);
            productContext.Remove(data);

            string wwwRootPath = webHostEnvironment.WebRootPath;
            var uploads = Path.Combine(wwwRootPath, @"images\payments");

            //if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
            //กรณีมีรูปภาพเดิมต้องลบทิ้งก่อน
            if (data.PaymentImage != null)
            {
                var oldImagePath = Path.Combine(wwwRootPath, data.PaymentImage.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            productContext.SaveChanges();

            TempData["Success"] = "Delete Successfully";
            return RedirectToAction(nameof(Index));
        }



    }
}
