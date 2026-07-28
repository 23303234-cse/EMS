using EMS.Documents;
using EMS.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using QuestPDF.Fluent;

namespace EMS.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly MongoDbService _mongoDbService;

        public InvoiceController(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        public IActionResult Download(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var order = _mongoDbService.Orders
                .Find(o => o.Id == id)
                .FirstOrDefault();

            if (order == null)
                return NotFound();

            var document = new InvoiceDocument(order);

            byte[] pdf = document.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Invoice-{order.Id}.pdf");
        }
    }
}