using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rolled_metal_products.Models.ViewModels;
using Rolled_metal_products.Models;
using System.Security.Claims;
using System.Text;
using Rolled_metal_products.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rolled_metal_products.Data;
using Rolled_metal_products.Models;
using Rolled_metal_products.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rolled_metal_products.Models.ViewModels;
using System.Security.Claims;
using System.Text;
using Rolled_metal_products.Repository.IRepository;





namespace Rolled_metal_products.Controllers
{
    public class CallbackController : ControllerBase
    {
        private readonly ILogger<CallbackController> _logger;
        private readonly IEmailSender _emailSender;
       // private readonly IEmailSender _emailService;

        public CallbackController(ILogger<CallbackController> logger, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult RequestCallback()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestCallback(CallbackRequest request)
        {
            if (ModelState.IsValid)
            {
                var Date = DateTime.UtcNow;
                // Логирование запроса
                _logger.LogInformation("Received callback request from {Name}, {Email}, {PhoneNumber}, {Date}", request.Name, request.Email, request.PhoneNumber, Date);
                    
                // Отправка электронного письма
                var subject = "Request for Callback";
                
                var message = $"Name: {request.Name}\nEmail: {request.Email}\nPhone Number: {request.PhoneNumber}\n\nMessage:\n{request.Text}\n Date:\n{Date}";

                try
                {
                    await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, message);
                    // await _emailSender.SendEmailAsync("your-email@example.com", subject, message);
                   // ViewData["Message"] = "Callback request received and email sent.";
                    return View();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending email.");
                    ModelState.AddModelError("", "Internal server error");
                    return View(request);
                }
            }

            return View(request);
        }


        public IActionResult Summary()
        {


            ApplicationUser applicationUser = null;



            if (User.IsInRole(WC.AdminRole))
            {
                if (HttpContext.Session.Get<int>(WC.SessionInquiryId) != 0)
                {
                    //cart has been loaded using an inquiry
                    InquiryHeader inquiryHeader = _inqHRepo.FirstOrDefault(u => u.Id == HttpContext.Session.Get<int>(WC.SessionInquiryId));
                    applicationUser = new ApplicationUser()
                    {
                        Email = inquiryHeader.Email,
                        FullName = inquiryHeader.FullName,
                        PhoneNumber = inquiryHeader.PhoneNumber
                    };
                }
                else
                {
                    applicationUser = new ApplicationUser();
                }

                // var gateway = _brain.GetGateway();
                //   var clientToken = gateway.ClientToken.Generate();
                // ViewBag.ClientToken = clientToken;

            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                //var userId = User.FindFirstValue(ClaimTypes.Name);
                if (claim != null)
                {
                    applicationUser = _userRepo.FirstOrDefault(u => u.Id == claim.Value);
                }
                //applicationUser = _userRepo.FirstOrDefault(u => u.Id == claim.Value);
            }

            // Проверка, если пользователь не аутентифицирован, перенаправляем на страницу входа
            if (applicationUser == null)
            {
                // return RedirectToAction("Login", "Account");
                return Redirect("/Identity/Account/Login");
            }

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = applicationUser,
            };


            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = _prodRepo.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempSqFt = cartObj.SqFt;
                ProductUserVM.ProductList.Add(prodTemp);
            }


            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(IFormCollection collection, ProductUserVM ProductUserVM)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (User.IsInRole(WC.AdminRole))
            {
                //we need to create an order
                //var orderTotal = 0.0;
                //foreach(Product prod in ProductUserVM.ProductList)
                //{
                //    orderTotal += prod.Price * prod.TempSqFt;
                //}
                OrderHeader orderHeader = new OrderHeader()
                {
                    CreatedByUserId = claim.Value,
                    FinalOrderTotal = ProductUserVM.ProductList.Sum(x => x.TempSqFt * x.Price),
                    City = ProductUserVM.ApplicationUser.City,
                    StreetAddress = ProductUserVM.ApplicationUser.StreetAddress,
                    State = ProductUserVM.ApplicationUser.State,
                    PostalCode = ProductUserVM.ApplicationUser.PostalCode,
                    FullName = ProductUserVM.ApplicationUser.FullName,
                    Email = ProductUserVM.ApplicationUser.Email,
                    PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                    OrderDate = DateTime.UtcNow,
                    OrderStatus = WC.StatusPending,
                    TransactionId = Guid.NewGuid().ToString()
                };
                _orderHRepo.Add(orderHeader);
                _orderHRepo.Save();

                foreach (var prod in ProductUserVM.ProductList)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        OrderHeaderId = orderHeader.Id,
                        PricePerSqFt = prod.Price,
                        Sqft = prod.TempSqFt,
                        ProductId = prod.Id
                    };
                    _orderDRepo.Add(orderDetail);

                }
                _orderDRepo.Save();

                //  string nonceFromTheClient = collection["payment_method_nonce"];

                /*               var request = new TransactionRequest
                               {
                                   Amount = Convert.ToDecimal(orderHeader.FinalOrderTotal),
                                   PaymentMethodNonce = nonceFromTheClient,
                                   OrderId = orderHeader.Id.ToString(),
                                   Options = new TransactionOptionsRequest
                                   {
                                       SubmitForSettlement = true
                                   }
                               };

                               var gateway = _brain.GetGateway();
                               Result<Transaction> result = gateway.Transaction.Sale(request);

                               if (result.Target.ProcessorResponseText == "Approved")
                               {
                                   orderHeader.TransactionId = result.Target.Id;
                                   orderHeader.OrderStatus = WC.StatusApproved;
                               }
                               else
                               {
                                   orderHeader.OrderStatus = WC.StatusCancelled;
                               }*/
                _orderHRepo.Save();
                return RedirectToAction(nameof(InquiryConfirmation), new { id = orderHeader.Id });


            }
            else
            {
                //we need to create an inquiry
                var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
               + "templates" + Path.DirectorySeparatorChar.ToString() +
               "Inquiry.html";

                var subject = "New Inquiry";
                string HtmlBody = "";
                using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
                {
                    HtmlBody = sr.ReadToEnd();
                }
               

                StringBuilder productListSB = new StringBuilder();
                foreach (var prod in ProductUserVM.ProductList)
                {
                    productListSB.Append($" - Name: {prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})</span><br />");
                }

                string messageBody = string.Format(HtmlBody,
                    ProductUserVM.ApplicationUser.FullName,
                    ProductUserVM.ApplicationUser.Email,
                    ProductUserVM.ApplicationUser.PhoneNumber,
                    productListSB.ToString());


                await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);

                InquiryHeader inquiryHeader = new InquiryHeader()
                {
                    ApplicationUserId = claim.Value,
                    FullName = ProductUserVM.ApplicationUser.FullName,
                    Email = ProductUserVM.ApplicationUser.Email,
                    PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                    InquiryDate = DateTime.UtcNow

                };

                _inqHRepo.Add(inquiryHeader);
                _inqHRepo.Save();

                foreach (var prod in ProductUserVM.ProductList)
                {
                    InquiryDetail inquiryDetail = new InquiryDetail()
                    {
                        InquiryHeaderId = inquiryHeader.Id,
                        ProductId = prod.Id,

                    };
                    _inqDRepo.Add(inquiryDetail);

                }
                _inqDRepo.Save();
                TempData[WC.Success] = "Inquiry submitted successfully";
            }
            //  private readonly IEmailService _emailService;
            /*  private readonly ICrmService _crmService;
              private readonly IAnalyticsService _analyticsService;

              public CallbackController(
                  ILogger<CallbackController> logger,
                  IEmailService emailService,
                  ICrmService crmService,
                  IAnalyticsService analyticsService)
              {
                  _logger = logger;
                  _emailService = emailService;
                  _crmService = crmService;
                  _analyticsService = analyticsService;
              }

              [HttpPost]
              public async Task<IActionResult> OrderCallback([FromBody] CallbackOrder order)
              {
                  if (!ModelState.IsValid)
                  {
                      return BadRequest(ModelState);
                  }

                  try
                  {
                      // Генерируем уникальный идентификатор заказа
                      var orderId = Guid.NewGuid().ToString("N");

                      // Создаем заказ в CRM
                      var crmOrder = await _crmService.CreateOrder(order, orderId);

                      // Отправляем уведомление администратору
                      await _emailService.SendCallbackNotification(crmOrder);

                      // Отправляем подтверждение клиенту
                      await _emailService.SendConfirmationEmail(order, orderId);

                      // Регистрируем событие в аналитике
                      _analyticsService.TrackCallbackOrder(crmOrder);

                      return Ok($"Заказ звонка #{orderId} успешно отправлен. Мы вам перезвоним в ближайшее время.");
                  }
                  catch (Exception ex)
                  {
                      _logger.LogError(ex, "Ошибка при отправке заказа звонка");
                      return StatusCode(500, "Произошла ошибка. Пожалуйста, повторите попытку позднее.");
                  }
              }*/
        }
}
