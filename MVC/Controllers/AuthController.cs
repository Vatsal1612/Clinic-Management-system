using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LoginReg.BAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVC.Models;

namespace LoginReg.Controllers
{

    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AuthHelper _helper;

        public AuthController(ILogger<AuthController> logger, AuthHelper helper)
        {
            _logger = logger;
            _helper = helper;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(PatientModel patient)
        {
            if (patient.c_ImageFile != null && patient.c_ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(patient.c_ImageFile.FileName);

                patient.c_Image = fileName;

                var folder = Path.Combine("../MVC/wwwroot/", "photos");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    patient.c_ImageFile.CopyTo(stream);
                }
                var res = await _helper.Register(patient);

                if (res == 1)
                {
                    TempData["message"] = "User registered successfully";
                    return RedirectToAction("Login", "Auth");

                }
                else if (res == 0)
                {
                    TempData["error"] = "User already exists";
                    return RedirectToAction("Register", "Auth");

                }
                else
                {
                    TempData["error"] = "There was some error while Registration";
                    return RedirectToAction("Register", "Auth");
                }
            }
            TempData["error"] = "There was some error while Registration";
            return RedirectToAction("Register", "Auth");
        }
        [HttpGet]
        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel login)
        {
            PatientModel patient = await _helper.Login(login);

            if (patient != null && patient.c_PatientId != 0)
            {

                HttpContext.Session.SetInt32("Id", patient.c_PatientId);
                HttpContext.Session.SetString("Name", patient.c_Name);
                HttpContext.Session.SetString("Role", patient.c_Role);
                HttpContext.Session.SetString("Image", patient.c_Image);

                if (patient.c_Role == "Admin")
                {
                    return RedirectToAction("DashBoard", "Admin");
                }
                else
                {
                    TempData["message"] = "Login successfully";
                    return RedirectToAction("Dashboard", "User");
                }
            }
            else
            {
                TempData["error"] = "Invalid Credentials";
                return RedirectToAction("Login", "Auth");
            }
        }

        // [HttpPost]
        // public async Task<IActionResult> Login(LoginModel login)
        // {
        //     PatientModel patient = await _helper.Login(login);

        //     if (patient != null && patient.c_PatientId != 0)
        //     {
        //         // Set Session Variables
        //         HttpContext.Session.SetInt32("Id", patient.c_PatientId);
        //         HttpContext.Session.SetString("Name", patient.c_Name ?? "");

        //         // Grab the role (default to "Patient" if null)
        //         string userRole = patient.c_Role ?? "Patient";
        //         HttpContext.Session.SetString("Role", userRole);

        //         TempData["message"] = "Login successfully";

        //         // ─── ROLE & EMAIL BASED REDIRECTION ───

        //         // Check if the user is an Admin by Role OR by a specific hardcoded admin email
        //         if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) || login.c_Email == "admin@cam.com")
        //         {
        //             // Redirects to the Dashboard action inside the AdminController
        //             return RedirectToAction("Dashboard", "Admin");
        //         }
        //         else
        //         {
        //             // Redirects standard patients/users to the Index action inside the HomeController
        //             return RedirectToAction("Dashboard", "User");
        //         }
        //     }
        //     else
        //     {
        //         TempData["error"] = "Invalid Credentials";
        //         return RedirectToAction("Login", "Auth");
        //     }
        // }

        [HttpGet]
        public async Task<IActionResult> getAllState()
        {
            var list = await _helper.GetAllState();
            return Json(list);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        [HttpGet]
        public async Task<IActionResult> GetCities(int StateId)
        {
            var cities = await _helper.GetCities(StateId);
            if (cities.Any())
            {
                return Json(cities);
            }
            else
            {
                return Json(new { success = false, message = "Data fetched error" });
            }
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }
    }
}