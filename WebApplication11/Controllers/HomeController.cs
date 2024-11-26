using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using WebApplication11.Models;

namespace WebApplication11.Controllers
{
    public class HomeController : Controller
    {
        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool IsPhoneNumberValid(string phoneNumber)
        {
            string pattern = @"^((8|\+7)[\- ]?)?(\(?\d{3}\)?[\- ]?)?[\d\- ]{7,10}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        private Rpm2Context db;
        public HomeController (Rpm2Context pm2Context)
        {
            db = pm2Context;
        }
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            Catalog catalog = new Catalog();
            catalog.ProductName = "Измените название";
            catalog.Description = "Измените описание";
            catalog.Price = 0;
            catalog.Weight = 0;
            catalog.Stock = 0;
            catalog.CategoryName = "Измените категорию";
            catalog.PathToImage = "Измените картинку";
            db.Catalogs.Add(catalog);
            await db.SaveChangesAsync();
            return RedirectToAction("CRUD");
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

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string phoneNumber, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "Пароли не совпадают.";
                return View("Registration");
            }

            if (!IsPhoneNumberValid(phoneNumber))
            {
                ViewBag.ErrorMessage = "Неверный формат номера.";
                return View("Registration");
            }

            // Проверка на существующего пользователя
            if (db.Users.Any(u => u.Email == email || u.Phone == phoneNumber))
            {
                ViewBag.ErrorMessage = "Пользователь с таким email или номером телефона уже существует.";
                return View("Registration");
            }

            // Хэширование пароля
            string hashedPassword = ComputeSha256Hash(password);

            // Сохранение пользователя
            var user = new User
            {
                Phone = phoneNumber,
                Email = email,
                Password = hashedPassword,
                RoleId = 2
            };

            db.Users.Add(user);
            db.SaveChanges();

            // Перенаправление на страницу входа или успешной регистрации
            return RedirectToAction("Authorization");
        }

        [HttpGet]
        public IActionResult Authorization()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Проверка, существует ли пользователь с таким email
            var user = db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Пользователь с таким email не найден.";
                return View("Authorization");
            }

            // Проверка пароля
            string hashedPassword = ComputeSha256Hash(password);  // Сравниваем введённый пароль с хэшированным в базе данных
            if (hashedPassword != user.Password)
            {
                ViewBag.ErrorMessage = "Неверный пароль.";
                return View("Authorization");
            }

            HttpContext.Session.SetInt32("UserID", user.IdUser);
            HttpContext.Session.SetString("IsAuthenticated", "true");

            return RedirectToAction("Profile");
        }

        public IActionResult Profile()
        {
            // Проверка на авторизацию
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                return RedirectToAction("Authorization");  // Если пользователь не авторизован, перенаправляем на страницу авторизации
            }

            int? isuser = HttpContext.Session.GetInt32("UserID");

            // Получаем информацию о пользователе из базы данных
            var user = db.Users.SingleOrDefault(u => u.IdUser == isuser);

            // Передаем объект пользователя в представление
            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();  // Очищаем сессию
            return RedirectToAction("Authorization");
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
