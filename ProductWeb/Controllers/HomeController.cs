using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductWeb.Data;
using ProductWeb.Models;
using ProductWeb.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace ProductWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductContext productContext;
        private readonly ShoppingCartService shoppingCartService;

        public HomeController(ProductContext productContext,ShoppingCartService shoppingCartService)
        {
            this.productContext = productContext;
            this.shoppingCartService = shoppingCartService;
        }

        public IActionResult Index()
        {
            var data = productContext.Products;
            return View(data);
        }

        public IActionResult Details(int productId)
        {
            var data = new ShoppingCart
            {
                ProductId = productId,
                Count = 1,
                Product = productContext.Products.Include(x => x.Category)
                .FirstOrDefault(p => p.Id.Equals(productId))

            };
            return View(data);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.UserId = claim.Value; //ใครทำรายการ

            var oldCart = productContext.ShoppingCarts.FirstOrDefault(x => x.UserId.Equals(shoppingCart.UserId)
                && x.ProductId.Equals(shoppingCart.ProductId));

            if(oldCart == null) //New
            {
                shoppingCartService.Add(shoppingCart);
            }
            else //Update
            {
                shoppingCartService.IncrementCount(oldCart,shoppingCart.Count);
            }

            shoppingCartService.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
