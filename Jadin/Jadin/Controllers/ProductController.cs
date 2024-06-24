using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Jadin.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Jadin.Controllers
{
    public class ProductController : Controller
    {
        private readonly JadinContext _context;

        public ProductController(JadinContext context)
        {
            _context = context;
        }

        // GET product
        public async Task<IActionResult> Index(string searchString, string category)
        {
            
                var categories = await _context.Products
                                               .Select(p => p.ProductCategory)
                                               .Distinct()
                                               .ToListAsync();

               
                ViewBag.Categories = categories;

  
                IQueryable<Product> products = _context.Products;

                if (!String.IsNullOrEmpty(searchString))
                {
                    products = products.Where(s => s.ProductName.Contains(searchString));
                }

                if (!String.IsNullOrEmpty(category))
                {
                    products = products.Where(x => x.ProductCategory == category);
                }

               
                var productList = await products.ToListAsync();

                return View(productList);
            }
        



        // GET product details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // Only users with email containing "@admin.co.za" can access CRUD operations
        public IActionResult Create()
        {
            if (User.Identity.IsAuthenticated && User.Identity.Name.Contains("@admin.co.za"))
            {
                ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Email");
                return View();
            }
            else
            {
                return Forbid(); // Returns 403 Forbidden if not authorized
            }
        }

        // POST Product(Create)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,ProductDescription,ProductCategory,ProductAvailability,ProductPrice,ProductImage,Userid")] Product product)
        {
            if (User.Identity.IsAuthenticated && User.Identity.Name.Contains("@admin.co.za"))
            {
                if (ModelState.IsValid)
                {
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Email", product.Userid);
                return View(product);
            }
            else
            {
                return Forbid(); // Returns 403 Forbidden if not authorized
            }
        }

        // GET product(Edit)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (User.Identity.IsAuthenticated && User.Identity.Name.Contains("@admin.co.za"))
            {
                ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Email", product.Userid);
                return View(product);
            }
            else
            {
                return Forbid(); // Returns 403 Forbidden if not authorized
            }
        }

        // POST product(Edit)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,ProductDescription,ProductCategory,ProductAvailability,ProductPrice,ProductImage,Userid")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (User.Identity.IsAuthenticated && User.Identity.Name.Contains("@admin.co.za"))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(product);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ProductExists(product.ProductId))
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
                ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Email", product.Userid);
                return View(product);
            }
            else
            {
                return Forbid(); // Returns 403 Forbidden if not authorized
            }
        }

        // GET product(Delete)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            if (User.Identity.IsAuthenticated && User.Identity.Name.Contains("@admin.co.za"))
            {
                return View(product);
            }
            else
            {
                return Forbid(); // Returns 403 Forbidden if not authorized
            }
        }

        // POST Product(Delete)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (User.Identity.IsAuthenticated && User.Identity.Name.Contains("@admin.co.za"))
            {
                var product = await _context.Products.FindAsync(id);
                if (product != null)
                {
                    // Remove related cart items
                    var cartItems = _context.Carts.Where(c => c.ProductId == id).ToList();
                    if (cartItems.Any())
                    {
                        _context.Carts.RemoveRange(cartItems);
                    }

                    // Remove the product
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            else
            {
                return Forbid(); // Returns 403 Forbidden if not authorized
            }
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
