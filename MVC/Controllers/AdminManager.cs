using Microsoft.AspNetCore.Mvc;
using MVC.BAL;

namespace MVC.Controllers;

public class AdminController : Controller
{
  private readonly AdminHelper _adminHelper;

  public AdminController(AdminHelper adminHelper)
  {
    _adminHelper = adminHelper;
  }

  public IActionResult Dashboard()
  {
     var getUser = HttpContext.Session.GetString("Role") ?? "";
    if (getUser == "Admin")
    {
         return View();
    }
    else
    {
      return RedirectToAction("Index", "Home");
    }
  }

  public IActionResult AllAppointments()
  {
      var getUser = HttpContext.Session.GetString("Role") ?? "";
    if (getUser == "Admin")
    {
         return View();
    }
    else
    {
      return RedirectToAction("Index", "Home");
    }
  }

  public IActionResult AllPatients()
  {
      var getUser = HttpContext.Session.GetString("Role") ?? "";
    if (getUser == "Admin")
    {
         return View();
    }
    else
    {
      return RedirectToAction("Index", "Home");
    }
  }

  [HttpGet]
  public IActionResult GetDashboardData()
  {
    var getUser = HttpContext.Session.GetString("Role") ?? "";
    if (getUser == "Admin")
    {
      try
      {
        var response = _adminHelper.GetDashboardData();
        return Json(response);
      }
      catch (Exception ex)
      {
        return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
      }
    }
    else
    {
      return RedirectToAction("Index", "Home");
    }
  }

  [HttpGet]
  public IActionResult GetAppointments(string? search, int? departmentId, string? status)
  {
    var getUser = HttpContext.Session.GetString("Role") ?? "";
    if (getUser == "Admin")
    {
      var items = _adminHelper.GetAppointments(search, departmentId, status);
      return Json(items);
    }
    else
    {
      return RedirectToAction("Index", "Home");
    }
  }

  [HttpGet]
  public IActionResult GetPatients(string? search)
  {
    var getUser = HttpContext.Session.GetString("Role") ?? "";
    if (getUser == "Admin")
    {
      var items = _adminHelper.GetPatients(search);
      return Json(items);
    }
    else
    {
      return RedirectToAction("Index", "Home");
    }
  }

  [HttpGet]
  public IActionResult GetDepartments()
  {

    var getUser = HttpContext.Session.GetString("Role") ?? "";
    if (getUser == "Admin")
    {
      var departments = _adminHelper.GetDepartments();
      return Json(departments);
    }
    else
    {
      return RedirectToAction("Index", "Home");
    }

  }

  [HttpPost]
  public IActionResult UpdateAppointmentStatus([FromBody] UpdateAppointmentStatusRequest request)
  {

    var getUser = HttpContext.Session.GetString("Role") ?? "";
    if (getUser == "Admin")
    {
 var requestedStatus = request.Status?.Trim();
    if (string.IsNullOrWhiteSpace(requestedStatus))
    {
      return BadRequest(new { message = "Status is required." });
    }

    // Only allow Confirmed and Cancelled
    if (requestedStatus != "Confirmed" && requestedStatus != "Cancelled")
    {
      return BadRequest(new { message = "Invalid status. Only Confirmed and Cancelled are allowed." });
    }

    try
    {
      var updated = _adminHelper.UpdateAppointmentStatus(request.AppointmentId, requestedStatus);
      if (!updated)
      {
        return NotFound(new { message = "Appointment not found." });
      }
    }
    catch (Exception ex)
    {
      return BadRequest(new { message = ex.Message });
    }

    return Ok(new { message = "Status updated successfully.", appointmentId = request.AppointmentId, status = requestedStatus });
    }
    else
    {
      return RedirectToAction("Index", "Home");
    }
  }

  /// <summary>
  /// Get latest record IDs for polling - returns the highest patient ID and appointment ID
  /// </summary>
  [HttpGet]
  public IActionResult GetLatestRecordIds()
  {
     var getUser = HttpContext.Session.GetString("Role") ?? "";
    if (getUser == "Admin")
    {
  try
    {
      var appointments = _adminHelper.GetAppointments(null, null, null);
      var patients = _adminHelper.GetPatients(null);

      // Convert to dynamic to get max
      var latestAppointmentId = 0;
      var latestPatientId = 0;

      foreach (var apt in appointments)
      {
        var dict = (System.Collections.IDictionary)apt;
        var id = Convert.ToInt32(dict["appointmentId"]);
        if (id > latestAppointmentId) latestAppointmentId = id;
      }

      foreach (var pat in patients)
      {
        var dict = (System.Collections.IDictionary)pat;
        var id = Convert.ToInt32(dict["patientId"]);
        if (id > latestPatientId) latestPatientId = id;
      }

      return Json(new
      {
        latestAppointmentId,
        latestPatientId
      });
    }
    catch (Exception ex)
    {
      return StatusCode(500, new { error = ex.Message });
    }
    }
    else
    {
      return RedirectToAction("Index", "Home");
    }
  
  }
}

public class UpdateAppointmentStatusRequest
{
  public int AppointmentId { get; set; }
  public string? Status { get; set; }
}

