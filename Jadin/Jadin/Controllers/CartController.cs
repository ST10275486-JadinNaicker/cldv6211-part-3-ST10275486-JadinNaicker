using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Jadin.Models;
using System.Text;

namespace Jadin.Controllers
{
    public class CartController : Controller
    {
        private readonly JadinContext _context;

        public CartController(JadinContext context)
        {
            _context = context;
        }

        // GET Cart
        public async Task<IActionResult> Index()
        {
            var jadinContext = _context.Carts.Include(c => c.Product).Include(c => c.User);
            return View(await jadinContext.ToListAsync());
        }

        // GET Cart(Details)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Product)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CartId == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // GET Cart(Create)
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName");
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Email");
            return View();
        }

        // POST Cart(Create)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CartId,ProductId,Userid")] Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", cart.ProductId);
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Email", cart.Userid);
            return View(cart);
        }

        // GET Cart(Edit)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", cart.ProductId);
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Email", cart.Userid);
            return View(cart);
        }

        // POST Cart(Edit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CartId,ProductId,Userid")] Cart cart)
        {
            if (id != cart.CartId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.CartId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", cart.ProductId);
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Email", cart.Userid);
            return View(cart);
        }

        // GET Cart(Delete)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Product)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CartId == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // POST: Cart/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.CartId == id);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var userId = GetUserId();
            var cartItem = await _context.Carts.FirstOrDefaultAsync(c => c.ProductId == productId && c.Userid == userId);

            if (cartItem == null)
            {
                cartItem = new Cart
                {
                    ProductId = productId,
                    Userid = userId
                };
                _context.Carts.Add(cartItem);
            }
            else
            {
                // Handle scenario where product already exists in cart
            }

            await _context.SaveChangesAsync();

            // Trigger Azure Function to start payment orchestration
            using (var httpClient = new HttpClient())
            {
                var requestPayload = new { orderId = cartItem.CartId }; // Assuming orderId is CartId for simplicity
                var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(requestPayload);
                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var functionUrl = "https://durablefunctions20240624160634.azurewebsites.net"; // Replace with your Azure Function URL
                var response = await httpClient.PostAsync(functionUrl, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    // Handle failure scenario
                    // Example: return a view indicating failure to start payment process
                    return View("PaymentError");
                }
            }

            return RedirectToAction("Index", "Product");
        }

        private int GetUserId()
        {

            return 2;
        }

    }


}