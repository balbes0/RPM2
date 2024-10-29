using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication11.Models;

namespace WebApplication11.Controllers
{
    public class HomeController : Controller
    {
        private Rpm2Context db;
        public HomeController (Rpm2Context pm2Context)
        {
            db = pm2Context;
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Catalog product)
        {
            if (ModelState.IsValid)
            {
                db.Catalogs.Update(product);
                await db.SaveChangesAsync();
                return RedirectToAction("CRUD");
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                Catalog catalog = await db.Catalogs.FirstOrDefaultAsync(c => c.IdProduct == id);
                if (catalog != null)
                {
                    db.Catalogs.Remove(catalog);
                    await db.SaveChangesAsync();
                    return RedirectToAction("CRUD");
                }
            }
            return NotFound();
        }

        public async Task<IActionResult> CRUD()
        {
            return View(await db.Catalogs.ToListAsync());
        }

        public async Task<IActionResult> Index()
        {
            return View(await db.Catalogs.ToListAsync());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult Registration()
        {
            return View();
        }

        public IActionResult Authorization()
        {
            return View();
        }
        public IActionResult Favorites()
        {
            return View();
        }

        public IActionResult Cart()
        {
            return View();
        }

        public IActionResult Order()
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
