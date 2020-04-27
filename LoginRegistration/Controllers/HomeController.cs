using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LoginRegistration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace LoginRegistration.Controllers
{
    public class HomeController : Controller
    {

        private MyContext dbContext;
        
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User fromForm)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == fromForm.Email))
                {
                    ModelState.AddModelError("Email", "Email is already in database");
                    return View("Index");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    fromForm.Password = Hasher.HashPassword(fromForm, fromForm.Password);
                    dbContext.Add(fromForm);
                    dbContext.SaveChanges();
                    int ID = fromForm.UserId;
                    HttpContext.Session.SetInt32("CurrentUser", ID);
                    return RedirectToAction("Success");
                }
            }
            else
            {
                return View("Index", fromForm);
            }
        }

        [HttpGet("login")]
        public ViewResult Login()
        {
            return View("Login");
        }
        
        [HttpPost]
        public IActionResult LoginProcess(LoginUser fromForm)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == fromForm.Email);
                if(userInDb == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(fromForm, userInDb.Password, fromForm.Password);
                if(result == 0)
                {
                    return View("Login");
                }
                else
                {
                    int ID = userInDb.UserId;
                    HttpContext.Session.SetInt32("CurrentUser", ID);
                    return RedirectToAction("Success", new{ID = ID});
                }
            }
            return View("Login");
        }
        
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            // HttpContext.Session.Clear();
            HttpContext.Session.Remove("CurrentUser");
            return RedirectToAction("Index");
        }

        [HttpGet("success")]
        public ViewResult Success()
        {
            return View();
        }
    }
}
