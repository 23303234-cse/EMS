using EMS.Models;
using EMS.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EMS.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public EmployeeController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public IActionResult Index()
        {
            var employees = _mongoDbService.Employees.Find(_ => true).ToList();
            return View(employees);
        }
    }
}