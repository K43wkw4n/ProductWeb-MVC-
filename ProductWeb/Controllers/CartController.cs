using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductWeb.Data;
using ProductWeb.Models;
using ProductWeb.Services;
using ProductWeb.Utility;
using ProductWeb.ViewModels;
using System.Security.Claims;

namespace ProductWeb.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ProductContext productContext;
        private readonly ShoppingCartService shoppingCartService;
        private readonly IWebHostEnvironment webHostEnvironment;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(ProductContext productContext,ShoppingCartService shoppingCartService,
            IWebHostEnvironment webHostEnvironment)
        {
            this.productContext = productContext;
            this.shoppingCartService = shoppingCartService;
            this.webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity; //ค้นหา User
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userId = claim.Value;

            ShoppingCartVM shoppingCartVM = new()
            {
                OrderHeader = new(),
                ListCart = productContext.ShoppingCarts.Include(x=>x.Product)
                .Where(x => x.UserId == userId).ToList(),
            };

            foreach(var cart in shoppingCartVM.ListCart)
            {
                shoppingCartVM.OrderHeader.OrderTotal += cart.Count * cart.Product.Price;
            }

            return View(shoppingCartVM);
        }
        public IActionResult Plus(int id)
        {
            var data = productContext.ShoppingCarts.Find(id);

            shoppingCartService.IncrementCount(data,1);
            shoppingCartService.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int id)
        {
            var data = productContext.ShoppingCarts.Find(id);

            shoppingCartService.DecrementCount(data, 1);
            shoppingCartService.Save();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int id)
        {
            var data = productContext.ShoppingCarts.Find(id);
            productContext.Remove(data);
            productContext.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userId = claim.Value;

            ShoppingCartVM = new()
            {
                OrderHeader = new(),
                ListCart = productContext.ShoppingCarts.Include(x=>x.Product)
                .Where(x=>x.UserId == userId)
            };

            var u = ShoppingCartVM.OrderHeader.User = productContext.Users.Find(userId);

            ShoppingCartVM.OrderHeader.StreetAddress = u.StreetAddress;
            ShoppingCartVM.OrderHeader.City = u.City;
            ShoppingCartVM.OrderHeader.State = u.State;
            ShoppingCartVM.OrderHeader.PostalCode = u.PostalCode;
            ShoppingCartVM.OrderHeader.Name = u.FullName;

            foreach (var item in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += item.Count * item.Product.Price;
            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Summary(IFormFile file)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var userId = claim.Value;

            ShoppingCartVM.ListCart = productContext.ShoppingCarts.Include(x=>x.Product).Where(x=>x.UserId == userId).ToList();

            #region Image Management
            string wwwRootPath = webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var extension = Path.GetExtension(file.FileName);
                var uploads = Path.Combine(wwwRootPath, @"images\payments");

                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                //บันทึกรุปภาพใหม่
                using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                {
                    file.CopyTo(fileStreams);
                }
                ShoppingCartVM.OrderHeader.PaymentImage = @"\images\payments\" + fileName + extension;
            }
            #endregion


            foreach (var item in ShoppingCartVM.ListCart)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += item.Count * item.Product.Price;
            }

            ShoppingCartVM.OrderHeader.UserId = userId;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;

            productContext.OrderHeaders.Add(ShoppingCartVM.OrderHeader);
            productContext.SaveChanges();

            foreach (var item in ShoppingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    ProductId = item.ProductId,
                    Count = item.Count,
                };
                productContext.OrderDetails.Add(orderDetail);
            }

            productContext.ShoppingCarts.RemoveRange(ShoppingCartVM.ListCart);
            productContext.SaveChanges();

            return RedirectToAction("Index","Home");
        }



    }
}
