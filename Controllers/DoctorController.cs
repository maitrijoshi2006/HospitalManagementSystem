using DoctorAppointment.Extensions;
using DoctorAppointment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace DoctorAppointment.Controllers
{
    public class DoctorController : Controller
    {
        private readonly IMemoryCache _cache;

        public DoctorController(IMemoryCache cache)
        {
            _cache = cache;
        }

        private List<Doctor> GetDoctors()
        {
            return new List<Doctor>
            {
                new Doctor
                {
                    DoctorId = 1,
                    DoctorName = "Dr. Raj Patel",
                    Specialization = "Cardiologist",
                    Experience = 12,
                    ConsultationFee = 800
                },

                new Doctor
                {
                    DoctorId = 2,
                    DoctorName = "Dr. Priya Shah",
                    Specialization = "Dermatologist",
                    Experience = 8,
                    ConsultationFee = 600
                },

                new Doctor
                {
                    DoctorId = 3,
                    DoctorName = "Dr. Amit Mehta",
                    Specialization = "Neurologist",
                    Experience = 15,
                    ConsultationFee = 1000
                },

                new Doctor
                {
                    DoctorId = 4,
                    DoctorName = "Dr. Neha Joshi",
                    Specialization = "Pediatrician",
                    Experience = 10,
                    ConsultationFee = 700
                },

                new Doctor
                {
                    DoctorId = 5,
                    DoctorName = "Dr. Karan Patel",
                    Specialization = "Orthopedic",
                    Experience = 14,
                    ConsultationFee = 900
                }
            };
        }

        // IMemoryCache - 3 minutes
        public IActionResult Index()
        {
            const string cacheKey = "doctorList";

            if (!_cache.TryGetValue(cacheKey, out List<Doctor>? doctors))
            {
                doctors = GetDoctors();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(3));

                _cache.Set(cacheKey, doctors, cacheOptions);

                ViewBag.CacheMessage = "Doctor list loaded and stored in cache.";
            }
            else
            {
                ViewBag.CacheMessage = "Doctor list loaded from cache.";
            }

            return View(doctors);
        }

        // Query String: DoctorId
        // Response Caching
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public IActionResult Details(int id)
        {
            var doctors = GetDoctors();

            var doctor = doctors.FirstOrDefault(d => d.DoctorId == id);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // Cookie + Session
        [HttpGet]
        public IActionResult Appointment(int doctorId)
        {
            var doctors = GetDoctors();

            var doctor = doctors.FirstOrDefault(d => d.DoctorId == doctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            // Cookie
            string? patientName = Request.Cookies["PatientName"];

            ViewBag.PatientName = patientName;

            // Session
            ViewBag.SelectedSpecialization =
                HttpContext.Session.GetString("Specialization");

            return View(doctor);
        }

        [HttpPost]
        public IActionResult Appointment(
            Doctor doctor,
            string PatientName,
            string Specialization)
        {
            // Store patient name in Cookie
            Response.Cookies.Append(
                "PatientName",
                PatientName,
                new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(1)
                });

            // Store specialization in Session
            HttpContext.Session.SetString(
                "Specialization",
                Specialization);

            ViewBag.Message =
                "Appointment request submitted successfully!";

            ViewBag.TotalCharge =
                doctor.CalculateTotalCharge();

            return View(doctor);
        }

        // Asynchronous Web Request
        public async Task<IActionResult> Posts()
        {
            using HttpClient client = new HttpClient();

            string url =
                "https://jsonplaceholder.typicode.com/posts";

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            var posts =
                JsonSerializer.Deserialize<List<Post>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            return View(posts);
        }
    }

    public class Post
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public string Body { get; set; } = "";
    }
}