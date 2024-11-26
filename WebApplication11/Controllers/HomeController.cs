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
            catalog.ProductName = "�������� ��������";
            catalog.Description = "�������� ��������";
            catalog.Price = 0;
            catalog.Weight = 0;
            catalog.Stock = 0;
            catalog.CategoryName = "�������� ���������";
            catalog.PathToImage = "�������� ��������";
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
                ViewBag.ErrorMessage = "������ �� ���������.";
                return View("Registration");
            }

            if (!IsPhoneNumberValid(phoneNumber))
            {
                ViewBag.ErrorMessage = "�������� ������ ������.";
                return View("Registration");
            }

            // �������� �� ������������� ������������
            if (db.Users.Any(u => u.Email == email || u.Phone == phoneNumber))
            {
                ViewBag.ErrorMessage = "������������ � ����� email ��� ������� �������� ��� ����������.";
                return View("Registration");
            }

            // ����������� ������
            string hashedPassword = ComputeSha256Hash(password);

            // ���������� ������������
            var user = new User
            {
                Phone = phoneNumber,
                Email = email,
                Password = hashedPassword,
                RoleId = 2
            };

            db.Users.Add(user);
            db.SaveChanges();

            // ��������������� �� �������� ����� ��� �������� �����������
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
            // ��������, ���������� �� ������������ � ����� email
            var user = db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ViewBag.ErrorMessage = "������������ � ����� email �� ������.";
                return View("Authorization");
            }

            // �������� ������
            string hashedPassword = ComputeSha256Hash(password);  // ���������� �������� ������ � ������������ � ���� ������
            if (hashedPassword != user.Password)
            {
                ViewBag.ErrorMessage = "�������� ������.";
                return View("Authorization");
            }

            HttpContext.Session.SetInt32("UserID", user.IdUser);
            HttpContext.Session.SetString("IsAuthenticated", "true");

            return RedirectToAction("Profile");
        }

        public IActionResult Profile()
        {
            // �������� �� �����������
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                return RedirectToAction("Authorization");  // ���� ������������ �� �����������, �������������� �� �������� �����������
            }

            int? isuser = HttpContext.Session.GetInt32("UserID");

            // �������� ���������� � ������������ �� ���� ������
            var user = db.Users.SingleOrDefault(u => u.IdUser == isuser);

            // �������� ������ ������������ � �������������
            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();  // ������� ������
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
