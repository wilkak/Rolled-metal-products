using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models;
using Rolled_metal_products.Models.ViewModels;
using Rolled_metal_products.Repository.IRepository;
using Rolled_metal_products.Utility;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity.UI.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rolled_metal_products.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _prodRepo;
        private readonly ICategoryRepository _catRepo;
        private readonly ApplicationDbContext _context;
        private readonly ICallbackRequestRepository _CallbackRequestRepo;

        private readonly IEmailSender _emailSender;
       


       
        public HomeController(ILogger<HomeController> logger, IProductRepository prodRepo,
            ICategoryRepository catRepo, IEmailSender emailSender, ICallbackRequestRepository CallbackRequestRepo)
        {
            _logger = logger;
            _prodRepo = prodRepo;
            _catRepo = catRepo;
            _emailSender = emailSender;
            _CallbackRequestRepo = CallbackRequestRepo;
        }



        public IActionResult Index()
        {
            var homeVM = new HomeVM
            {
               Categories = _catRepo.GetAll(x => x.ParentId == null, includeProperties: "SubCategories").ToList()
            };


            return View(homeVM);
        }

        [HttpPost]
        public async Task<IActionResult> QuickRequest([FromBody] QuickRequestModel model)
        {
            // �������� �� ������ ���� � ������ �� "�����������"
            var search = string.IsNullOrWhiteSpace(model.Search) ? "�����������" : model.Search;
            var contact = string.IsNullOrWhiteSpace(model.Contact) ? "�����������" : model.Contact;

            var date = DateTime.UtcNow;

            // ����������� �������
            _logger.LogInformation("Received quick request: {Search}, {Contact}, {Date}", search, contact, date);

            var subject = "������ � ���� ����";
            var message = $"������: {search}\n��������: {contact}\n����: {date}";

            try
            {
                // �������� ������������ ������
                await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, message);

                // �������� ������� CallbackRequest ��� ���������� � ���� ������
                CallbackRequest callbackRequest = new CallbackRequest()
                {
                    Name = "�����������",  // ���� Name �� ������������ � ������ ������, ������� ������ "�����������"
                    PhoneNumber = contact,  // ���� ��� ��������
                    Text = search,      // ���� ��� ���������
                    Date = date,    // ���� ��� ���� �������
                    Email = "�����������" // �� ������������ ����
                };

                // ���������� � ���� ������
                _CallbackRequestRepo.Add(callbackRequest);
                _CallbackRequestRepo.Save();

                TempData[WC.Success] = "������ ���������! ����� �� � ���� ��������.";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending quick request email.");
                return Json(new { success = false, message = "������ �������� �������. ���������� �����." });
            }
        }




    }
}
