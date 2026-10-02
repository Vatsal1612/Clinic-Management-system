// using MVC.Models;
// using Microsoft.AspNetCore.Mvc;
// using MVC.BAL;
// using MVC.Models;

// namespace MVC.Controllers;

// public class ProfileController : Controller
// {
//     private readonly ProfileHelper _profileBal;

//     public ProfileController(ProfileHelper profileBal)
//     {
//         _profileBal = profileBal;
//     }

//     [HttpGet]
//     public IActionResult Profile()
//     {
//         return View("profile");
//     }

//     [HttpGet]
//     public async Task<IActionResult> GetProfile()
//     {
//         var getUser = HttpContext.Session.GetString("Role") ?? "";
//         var patientId = Convert.ToInt32(HttpContext.Session.GetInt32("Id"));
//         if (getUser == "Patient")
//         {
//             if (patientId <= 0)
//             {
//                 return BadRequest("patientId must be greater than zero.");
//             }

//             var patient = await _profileBal.GetProfileAsync(patientId);
            
//             Console.WriteLine(patient.c_CityId);

//             if (patient is null)
//             {
//                 return NotFound("Profile not found.");
//             }

//             return Json(patient);
//         }
//         else
//         {
//             return RedirectToAction("Index", "Home");
//         }

//     }

//     [HttpPost]
//     public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
//     {
//         var getUser = HttpContext.Session.GetString("Role") ?? "";
//         if (getUser == "Patient")
//         {
//             if (request is null)
//             {
//                 return BadRequest("Request body is required.");
//             }

//             if (string.IsNullOrWhiteSpace(request.c_Name))
//             {
//                 ModelState.AddModelError(nameof(PatientModel.c_Name), "Name is required.");
//             }

//             if (string.IsNullOrWhiteSpace(request.c_Mobile))
//             {
//                 ModelState.AddModelError(nameof(PatientModel.c_Mobile), "Mobile is required.");
//             }

//             if (string.IsNullOrWhiteSpace(request.c_Gender))
//             {
//                 ModelState.AddModelError(nameof(PatientModel.c_Gender), "Gender is required.");
//             }

//             if (request.c_StateId <= 0)
//             {
//                 ModelState.AddModelError(nameof(PatientModel.c_StateId), "State is required.");
//             }

//             if (request.c_CityId <= 0)
//             {
//                 ModelState.AddModelError(nameof(PatientModel.c_CityId), "City is required.");
//             }

//             if (!ModelState.IsValid)
//             {
//                 return ValidationProblem(ModelState);
//             }

//             var patient = new PatientModel
//             {
//                 c_PatientId = request.c_PatientId,
//                 c_Name = request.c_Name,
//                 c_Gender = request.c_Gender,
//                 c_Mobile = request.c_Mobile,
//                 c_StateId = request.c_StateId,
//                 c_CityId = request.c_CityId,
//                 c_Image = request.c_Image
//             };

//             var savedPatient = await _profileBal.SaveProfileAsync(patient);
//             if (savedPatient is null)
//             {
//                 return StatusCode(500, "Failed to save profile.");
//             }

//             return Json(new { patient = savedPatient });
//         }
//         else
//         {
//             return RedirectToAction("Index", "Home");
//         }

//     }

//     [HttpGet]
//     public async Task<IActionResult> GetStates()
//     {
//         var getUser = HttpContext.Session.GetString("Role") ?? "";
//         if (getUser == "Patient")
//         {
//             var states = await _profileBal.GetAllStates();
//             return Json(states.Select(s => new { stateId = s.c_StateId, stateName = s.c_StateName }));
//         }
//         else
//         {
//             return RedirectToAction("Index", "Home");
//         }

//     }

//     [HttpGet]
//     public async Task<IActionResult> GetCitiesByStateId(int stateId)
//     {
//         var getUser = HttpContext.Session.GetString("Role") ?? "";
//         if (getUser == "Patient")
//         {
//             if (stateId <= 0)
//             {
//                 return Json(Array.Empty<object>());
//             }

//             var cities = await _profileBal.GetCitiesByState(stateId);
//             return Json(cities.Select(c => new { cityId = c.c_CityId, cityName = c.c_CityName }));
//         }
//         else
//         {
//             return RedirectToAction("Index", "Home");
//         }
//     }
// }


// // using MVC.Bal;
// // using MVC.Models;
// // using Microsoft.AspNetCore.Mvc;
// // using MVC.BAL;

// // namespace MVC.Controllers;

// // public class ProfileController : Controller
// // {
// //     private readonly ProfileHelper _profileBal;

// //     public ProfileController(ProfileHelper profileBal)
// //     {
// //         _profileBal = profileBal;
// //     }

// //     [HttpGet]
// //     public IActionResult Profile()
// //     {
// //         return View("profile");
// //     }

// //     [HttpGet]
// //     public async Task<IActionResult> GetProfile(int patientId)
// //     {
// //         if (patientId <= 0)
// //         {
// //             return BadRequest("patientId must be greater than zero.");
// //         }

// //         var patient = await _profileBal.GetProfileAsync(patientId);
// //         if (patient is null)
// //         {
// //             return NotFound("Profile not found.");
// //         }

// //         return Json(patient);
// //     }

// //     [HttpPost]
// //     public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
// //     {
// //         if (request is null)
// //         {
// //             return BadRequest("Request body is required.");
// //         }

// //         if (string.IsNullOrWhiteSpace(request.c_Name))
// //         {
// //             ModelState.AddModelError(nameof(PatientModel.c_Name), "Name is required.");
// //         }

// //         if (string.IsNullOrWhiteSpace(request.c_Mobile))
// //         {
// //             ModelState.AddModelError(nameof(PatientModel.c_Mobile), "Mobile is required.");
// //         }

// //         if (string.IsNullOrWhiteSpace(request.c_Gender))
// //         {
// //             ModelState.AddModelError(nameof(PatientModel.c_Gender), "Gender is required.");
// //         }

// //         if (!request.c_StateId.HasValue || request.c_StateId <= 0)
// //         {
// //             ModelState.AddModelError(nameof(PatientModel.c_StateId), "State is required.");
// //         }

// //         if (!request.c_CityId.HasValue || request.c_CityId <= 0)
// //         {
// //             ModelState.AddModelError(nameof(PatientModel.c_CityId), "City is required.");
// //         }

// //         if (!ModelState.IsValid)
// //         {
// //             return ValidationProblem(ModelState);
// //         }

// //         var patient = new PatientModel
// //         {
// //             c_PatientId = request.c_PatientId,
// //             c_Name = request.c_Name,
// //             c_Gender = request.c_Gender,
// //             c_Mobile = request.c_Mobile,
// //             c_StateId = request.c_StateId,
// //             c_CityId = request.c_CityId,
// //             c_Image = request.c_Image
// //         };

// //         var savedPatient = await _profileBal.SaveProfileAsync(patient);
// //         if (savedPatient is null)
// //         {
// //             return StatusCode(500, "Failed to save profile.");
// //         }

// //         return Json(new { patient = savedPatient });
// //     }

// //     [HttpGet]
// //     public async Task<IActionResult> GetStates()
// //     {
// //         var states = await _profileBal.GetStatesAsync();
// //         return Json(states.Select(s => new { stateId = s.StateId, stateName = s.StateName }));
// //     }

// //     [HttpGet]
// //     public async Task<IActionResult> GetCitiesByStateId(int stateId)
// //     {
// //         if (stateId <= 0)
// //         {
// //             return Json(Array.Empty<object>());
// //         }

// //         var cities = await _profileBal.GetCitiesByStateIdAsync(stateId);
// //         return Json(cities.Select(c => new { cityId = c.CityId, cityName = c.CityName }));
// //     }
// // }


using MVC.Models;
using Microsoft.AspNetCore.Mvc;
using MVC.BAL;
using Microsoft.AspNetCore.Hosting; 
using System.IO; 
using System; 

namespace MVC.Controllers;

public class ProfileController : Controller
{
    private readonly ProfileHelper _profileBal;
    private readonly IWebHostEnvironment _env; // 1. Added IWebHostEnvironment

    // 2. Injected IWebHostEnvironment into the constructor
    public ProfileController(ProfileHelper profileBal, IWebHostEnvironment env)
    {
        _profileBal = profileBal;
        _env = env;
    }

    [HttpGet]
    public IActionResult Profile()
    {
        return View("profile");
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var getUser = HttpContext.Session.GetString("Role") ?? "";
        var patientId = Convert.ToInt32(HttpContext.Session.GetInt32("Id"));
        if (getUser == "Patient")
        {
            if (patientId <= 0)
            {
                return BadRequest("patientId must be greater than zero.");
            }

            var patient = await _profileBal.GetProfileAsync(patientId);

            if (patient is null)
            {
                return NotFound("Profile not found.");
            }

            return Json(patient);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var getUser = HttpContext.Session.GetString("Role") ?? "";
        if (getUser == "Patient")
        {
            if (request is null)
            {
                return BadRequest("Request body is required.");
            }

            if (string.IsNullOrWhiteSpace(request.c_Name))
            {
                ModelState.AddModelError(nameof(PatientModel.c_Name), "Name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.c_Mobile))
            {
                ModelState.AddModelError(nameof(PatientModel.c_Mobile), "Mobile is required.");
            }

            if (string.IsNullOrWhiteSpace(request.c_Gender))
            {
                ModelState.AddModelError(nameof(PatientModel.c_Gender), "Gender is required.");
            }

            if (request.c_StateId <= 0)
            {
                ModelState.AddModelError(nameof(PatientModel.c_StateId), "State is required.");
            }

            if (request.c_CityId <= 0)
            {
                ModelState.AddModelError(nameof(PatientModel.c_CityId), "City is required.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            // 3. IMAGE PROCESSING LOGIC
            // Default to whatever was passed (handles cases where the user saves without changing their photo)
            string imageFileName = request.c_Image;

            // Only process if it's a new Base64 string from the frontend
            if (!string.IsNullOrEmpty(request.c_Image) && request.c_Image.StartsWith("data:image"))
            {
                try
                {
                    // Extract the raw base64 data
                    var base64Data = request.c_Image.Substring(request.c_Image.IndexOf(",") + 1);
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    // Create a unique filename
                    imageFileName = $"patient_{request.c_PatientId}_{DateTime.Now.Ticks}.jpg";
                    
                    // Define the wwwroot/photos folder path
                    string photosFolder = Path.Combine(_env.WebRootPath, "photos");
                    if (!Directory.Exists(photosFolder))
                    {
                        Directory.CreateDirectory(photosFolder);
                    }

                    // Save the file
                    string filePath = Path.Combine(photosFolder, imageFileName);
                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error saving image: " + ex.Message);
                    return StatusCode(500, "Error processing the image file.");
                }
            }

            var patient = new PatientModel
            {
                c_PatientId = request.c_PatientId,
                c_Name = request.c_Name,
                c_Gender = request.c_Gender,
                c_Mobile = request.c_Mobile,
                c_StateId = request.c_StateId,
                c_CityId = request.c_CityId,
                c_Image = imageFileName // 4. Map the short filename to the model, not the giant base64 string
            };

            var savedPatient = await _profileBal.SaveProfileAsync(patient);
            if (savedPatient is null)
            {
                return StatusCode(500, "Failed to save profile.");
            }

            return Json(new { patient = savedPatient });
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetStates()
    {
        var getUser = HttpContext.Session.GetString("Role") ?? "";
        if (getUser == "Patient")
        {
            var states = await _profileBal.GetAllStates();
            return Json(states.Select(s => new { stateId = s.c_StateId, stateName = s.c_StateName }));
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCitiesByStateId(int stateId)
    {
        var getUser = HttpContext.Session.GetString("Role") ?? "";
        if (getUser == "Patient")
        {
            if (stateId <= 0)
            {
                return Json(Array.Empty<object>());
            }

            var cities = await _profileBal.GetCitiesByState(stateId);
            return Json(cities.Select(c => new { cityId = c.c_CityId, cityName = c.c_CityName }));
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }
}