// using System.Diagnostics;
// using System.Text.Json;
// using Microsoft.AspNetCore.Mvc;
// using MVC.BAL;
// using MVC.Models;

// namespace MVC.Controllers;

// public class UserController : Controller
// {
//     private readonly UserHelper _userHelper;

//     public UserController(UserHelper userHelper)
//     {
//         _userHelper = userHelper;
//     }

//     public IActionResult BookAppointment()
//     {
//         var getUser = HttpContext.Session.GetString("Role") ?? "";
//         if (getUser == "Patient")
//         {
//             var departments = _userHelper.GetDepartments();
//             // Serialize here in controller — avoid using System.Text.Json in Razor view
//             ViewBag.DepartmentsJson = JsonSerializer.Serialize(departments);
//             return View();
//         }
//         else
//         {
//             return RedirectToAction("Index", "Home");
//         }
//     }

//     public IActionResult Dashboard()
//     {
//         var getUser = HttpContext.Session.GetString("Role") ?? "";
//         if (getUser == "Patient")
//         {
//             Console.WriteLine(HttpContext.Session.GetString("name"));
//             return View();
//         }
//         else
//         {
//             return RedirectToAction("Index", "Home");
//         }

//     }
//     public IActionResult MyAppointments()
//     {
//         var getUser = HttpContext.Session.GetString("Role") ?? "";
//         if (getUser == "Patient")
//         {
//             return View();
//         }
//         else
//         {
//             return RedirectToAction("Index", "Home");
//         }
//     }
//     [HttpPost]
//     public IActionResult Reschedule(int id, string date, string time)
//     {
//         var getUser = HttpContext.Session.GetString("Role") ?? "";
//         if (getUser == "Patient")
//         {
//             _userHelper.RescheduleAppointment(id, date, time);
//             return Ok();
//         }
//         else
//         {
//             return RedirectToAction("Index", "Home");
//         }

//     }

//     [HttpPost]
//     public IActionResult Cancel(int id)
//     {
//         var getUser = HttpContext.Session.GetString("Role") ?? "";
//         if (getUser == "Patient")
//         {
//             _userHelper.CancelAppointment(id);

//             return Json(new { success = true });
//         }
//         else
//         {
//             return RedirectToAction("Index", "Home");
//         }
//     }
//     public IActionResult GetRecentAppointments()
//     {
//         var getUser = HttpContext.Session.GetString("Role") ?? "";
//         if (getUser == "Patient")
//         {
//             int patientId = Convert.ToInt32(HttpContext.Session.GetInt32("Id"));
//             var data = _userHelper.GetRecentAppointments(patientId);
//             return Json(data);
//         }
//         else
//         {
//             return RedirectToAction("Index", "Home");
//         }

//     }
//     [HttpGet]
//     public IActionResult GetBookedSlots(int departmentId, string date)
//     {
//         var getUser = HttpContext.Session.GetString("Role") ?? "";
//         if (getUser == "Patient")
//         {
//             int patientId = Convert.ToInt32(HttpContext.Session.GetInt32("Id"));
//             var booked = _userHelper.GetBookedSlots(departmentId, patientId,date);
//             return Json(booked);
//         }
//         else
//         {
//             return RedirectToAction("Index", "Home");
//         }
//     }


//     [HttpPost]
//     public IActionResult BookAppointment([FromBody] AppointmentModel model)
//     {
//         var getUser = HttpContext.Session.GetString("Role") ?? "";
//         if (getUser == "Patient")
//         {
//             model.c_PatientId = Convert.ToInt32(HttpContext.Session.GetInt32("Id"));
//             var result = _userHelper.BookAppointment(model);
//             return Json(new
//             {
//                 success = result.success,
//                 message = result.message
//             });
//         }
//         else
//         {
//             return RedirectToAction("Index", "Home");
//         }
//     }



//     [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//     public IActionResult Error()
//     {
//         return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//     }
// }


using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MVC.BAL;
using MVC.Models;

namespace MVC.Controllers;

public class UserController : Controller
{
private readonly UserHelper _userHelper;
public UserController(UserHelper userHelper)
{
    _userHelper = userHelper;
}

public IActionResult BookAppointment()
{
    var role = HttpContext.Session.GetString("Role") ?? "";

    if (role != "Patient")
        return RedirectToAction("Index", "Home");

    var departments = _userHelper.GetDepartments();
    ViewBag.DepartmentsJson = JsonSerializer.Serialize(departments);

    return View();
}

public IActionResult Dashboard()
{
    var role = HttpContext.Session.GetString("Role") ?? "";

    if (role != "Patient")
        return RedirectToAction("Index", "Home");

    return View();
}

public IActionResult MyAppointments()
{
    var role = HttpContext.Session.GetString("Role") ?? "";

    if (role != "Patient")
        return RedirectToAction("Index", "Home");

    return View();
}

[HttpPost]
public IActionResult Reschedule(int id, string date, string time)
{
    var role = HttpContext.Session.GetString("Role") ?? "";

    if (role != "Patient")
        return RedirectToAction("Index", "Home");

    _userHelper.RescheduleAppointment(id, date, time);

    return Ok();
}

[HttpPost]
public IActionResult Cancel(int id)
{
    var role = HttpContext.Session.GetString("Role") ?? "";

    if (role != "Patient")
        return RedirectToAction("Index", "Home");

    _userHelper.CancelAppointment(id);

    return Json(new { success = true });
}

public IActionResult GetRecentAppointments()
{
    var role = HttpContext.Session.GetString("Role") ?? "";

    if (role != "Patient")
        return RedirectToAction("Index", "Home");

    int patientId = Convert.ToInt32(HttpContext.Session.GetInt32("Id"));

    var data = _userHelper.GetRecentAppointments(patientId);

    return Json(data);
}

[HttpGet]
public IActionResult GetBookedSlots(int departmentId, string date)
{
    var role = HttpContext.Session.GetString("Role") ?? "";

    if (role != "Patient")
        return RedirectToAction("Index", "Home");

    int patientId = Convert.ToInt32(HttpContext.Session.GetInt32("Id"));

    var booked = _userHelper.GetBookedSlots(departmentId, patientId, date);

    return Json(booked);
}

[HttpPost]
public IActionResult BookAppointment([FromBody] AppointmentModel model)
{
    var role = HttpContext.Session.GetString("Role") ?? "";

    if (role != "Patient")
        return RedirectToAction("Index", "Home");

    model.c_PatientId = Convert.ToInt32(HttpContext.Session.GetInt32("Id"));

    var result = _userHelper.BookAppointment(model);

    return Json(new
    {
        success = result.success,
        message = result.message
    });
}

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public IActionResult Error()
{
    return View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}
}
