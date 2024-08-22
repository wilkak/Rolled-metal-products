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
using X.PagedList.Extensions;




namespace Rolled_metal_products.Controllers
{
    public class CallbackController : Controller
    {
        private readonly ILogger<CallbackController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ICallbackRequestRepository _CallbackRequestRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IApplicationUserRepository _userRepo;
        private readonly IProductRepository _prodRepo;
        private readonly IInquiryHeaderRepository _inqHRepo;
        private readonly IInquiryDetailRepository _inqDRepo;
        private readonly IOrderHeaderRepository _orderHRepo;
        private readonly IOrderDetailRepository _orderDRepo;


        // private readonly IEmailSender _emailService;

        public CallbackController(ILogger<CallbackController> logger, IEmailSender emailSender, IWebHostEnvironment webHostEnvironment,
            IApplicationUserRepository userRepo, IProductRepository prodRepo,
            IInquiryHeaderRepository inqHRepo, IInquiryDetailRepository inqDRepo,
            IOrderHeaderRepository orderHRepo, IOrderDetailRepository orderDRep, ICallbackRequestRepository CallbackRequestRepo)
        {
            _logger = logger;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _userRepo = userRepo;
            _prodRepo = prodRepo;
            _inqHRepo = inqHRepo;
            _inqDRepo = inqDRepo;
            _orderHRepo = orderHRepo;
            _CallbackRequestRepo = CallbackRequestRepo;


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
                _logger.LogInformation("Received callback request from {Name}, {Email}, {PhoneNumber}, {Text}, {Date}",
                    request.Name, request.Email, request.PhoneNumber, request.Text, Date);

                // Отправка электронного письма
                var subject = "Заявка на звонок";

                var message =
                    $"Имя: {request.Name}\nПочта: {request.Email}\nТелефон: {request.PhoneNumber}\n\nСообщение:\n{request.Text}\n Дата:\n{Date}";

                try
                {
                    await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, message);
                    // await _emailSender.SendEmailAsync("your-email@example.com", subject, message);
                    // ViewData["Message"] = "Callback request received and email sent.";
                    CallbackRequest Callback = new CallbackRequest()
                    {
                        Name = request.Name,
                        Email = request.Email,
                        PhoneNumber = request.PhoneNumber,
                        Text = request.Text,
                        Date = Date

                    };
                    _CallbackRequestRepo.Add(Callback);
                    _CallbackRequestRepo.Save();

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

       /* [HttpGet]
        [Authorize(Roles = WC.AdminRole)]
        public IActionResult AdminListCallbacks()
        {
            var callbacks = _CallbackRequestRepo.GetAll();
            return View(callbacks);
        }*/

        [HttpGet]
        [Authorize(Roles = WC.AdminRole)]
        public IActionResult AdminListCallbacks(int? page)
        {
            int pageSize = 10; // Количество элементов на странице
            int pageNumber = page ?? 1; // Номер страницы, если не задано, будет 1

            var callbacks = _CallbackRequestRepo.GetAll().OrderByDescending(c => c.Date).ToPagedList(pageNumber, pageSize);

            return View(callbacks);
        }



        [HttpPost]
        [Authorize(Roles = WC.AdminRole)]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var callback = _CallbackRequestRepo.FirstOrDefault(c => c.Id == id);
            if (callback != null)
            {
                _CallbackRequestRepo.Remove(callback);
                _CallbackRequestRepo.Save();
                _logger.LogInformation("Callback request with ID {Id} deleted successfully.", id);
                return RedirectToAction(nameof(AdminListCallbacks));
            }
            else
            {
                _logger.LogWarning("Callback request with ID {Id} not found.", id);
                return NotFound();
            }
        }

        [HttpGet]
        [Authorize(Roles = WC.AdminRole)]
        public IActionResult Details(int id)
        {
            var callback = _CallbackRequestRepo.FirstOrDefault(c => c.Id == id);
            if (callback == null)
            {
                _logger.LogWarning("Callback request with ID {Id} not found.", id);
                return NotFound();
            }

            return View(callback);
        }


        public IActionResult Summary(ProductUserVM productUserVM)
        {

            ApplicationUser applicationUser = null;


            if (User.IsInRole(WC.AdminRole))
            {
                if (HttpContext.Session.Get<int>(WC.SessionInquiryId) != 0)
                {
                    //cart has been loaded using an inquiry
                    InquiryHeader inquiryHeader =
                        _inqHRepo.FirstOrDefault(u => u.Id == HttpContext.Session.Get<int>(WC.SessionInquiryId));
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

            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                if (claim != null)
                {
                    applicationUser = _userRepo.FirstOrDefault(u => u.Id == claim.Value);
                }

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

            ProductUserVM test = new ProductUserVM();
            /* {
                 ApplicationUser = applicationUser,
             };*/

            // Проверка, если пользователь не аутентифицирован, перенаправляем на страницу входа
            if (applicationUser == null)
            {
                // return RedirectToAction("Login", "Account");
                //return Redirect("/Identity/Account/Login");

                //  test.ApplicationUser.FullName =  
                /* ProductUserVM = new ProductUserVM()
                 {
                     ApplicationUser = applicationUser,
                 };*/


                foreach (var cartObj in shoppingCartList)
                {
                    Product prodTemp = _prodRepo.FirstOrDefault(u => u.Id == cartObj.ProductId);
                    prodTemp.TempSqFt = cartObj.SqFt;
                    test.ProductList.Add(prodTemp);
                }


                return View(test);
            }

            test.ApplicationUser = applicationUser;
            /*ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = applicationUser,
            };*/


            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = _prodRepo.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempSqFt = cartObj.SqFt;
                test.ProductList.Add(prodTemp);
            }


            return View(test);
        }
        /*
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SumPost(IFormCollection collection, ProductUserVM ProductUserVM)
        {
            //ApplicationUser applicationUser = null;


            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
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
                    _orderHRepo.Save();
                    return RedirectToAction(nameof(InquiryConfirmation), new { id = orderHeader.Id });


                }
                else
                {
                    //we need to create an inquiry
                    var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                                                                         + "templates" + Path.DirectorySeparatorChar
                                                                             .ToString() +
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
                        productListSB.Append(
                            $" - Name: {prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})</span><br />");
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
            }

            if (claim == null)
            {
                //we need to create an inquiry
                var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                                                                     + "templates" + Path.DirectorySeparatorChar
                                                                         .ToString() +
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
                    productListSB.Append(
                        $" - Name: {prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})</span><br />");
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

            return RedirectToAction(nameof(InquiryConfirmation));
        }*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(IFormCollection collection, ProductUserVM ProductUserVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                // Обработка анонимного пользователя
                var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                                                                         + "templates" + Path.DirectorySeparatorChar
                                                                             .ToString() +
                                                                         "Inquiry.html";
                var subject = "Новый заказ";
                string HtmlBody = "";
                using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
                {
                    HtmlBody = sr.ReadToEnd();
                }

                StringBuilder productListSB = new StringBuilder();
                foreach (var prod in ProductUserVM.ProductList)
                {
                    productListSB.Append(
                        $" - Название: {prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})</span><br />");
                }

                string messageBody = string.Format(HtmlBody,
                    ProductUserVM.ApplicationUser.FullName,
                    ProductUserVM.ApplicationUser.Email,
                    ProductUserVM.ApplicationUser.PhoneNumber,
                    productListSB.ToString());

                await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);

                InquiryHeader inquiryHeader = new InquiryHeader()
                {
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
                        ProductId = prod.Id
                    };
                    _inqDRepo.Add(inquiryDetail);
                }

                _inqDRepo.Save();
                TempData[WC.Success] = "Заказ успешно отправлен!";

                return RedirectToAction(nameof(InquiryConfirmation));
            }

            // Обработка авторизованных пользователей (без изменений)
            if (User.IsInRole(WC.AdminRole))
            {
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
                _orderHRepo.Save();
                return RedirectToAction(nameof(InquiryConfirmation), new { id = orderHeader.Id });
            }
            else
            {
                //we need to create an inquiry
                var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                                                                     + "templates" + Path.DirectorySeparatorChar
                                                                         .ToString() +
                                                                     "Inquiry.html";
                var subject = "Новый заказ";
                string HtmlBody = "";
                using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
                {
                    HtmlBody = sr.ReadToEnd();
                }


                StringBuilder productListSB = new StringBuilder();
                foreach (var prod in ProductUserVM.ProductList)
                {
                    productListSB.Append(
                        $" - Название: {prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})</span><br />");
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
                TempData[WC.Success] = "Заказ успешно отправлен!";
            }
            return RedirectToAction(nameof(InquiryConfirmation));


        }

        public IActionResult Index()
        {

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodListTemp = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));
            IList<Product> prodList = new List<Product>();

            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = prodListTemp.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempSqFt = cartObj.SqFt;
                prodList.Add(prodTemp);
            }

            return View(prodList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost(IEnumerable<Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, SqFt = prod.TempSqFt });
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Summary));
        }
        public IActionResult InquiryConfirmation(int id = 0)
        {
            OrderHeader orderHeader = _orderHRepo.FirstOrDefault(u => u.Id == id);
            HttpContext.Session.Clear();
            return View(orderHeader);
        }

        public IActionResult Remove(int id)
        {

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                //session exsits
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable<Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, SqFt = prod.TempSqFt });
            }

            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Clear()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
