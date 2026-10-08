using Newtonsoft.Json;
using RectImageArchiveMillikitap.Models;
using RectImageArchiveMillikitap.Models.CustomAuthentication;
using RectImageArchiveMillikitap.Models.User;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using static RectImageArchiveMillikitap.Models.Helper;

namespace RectImageArchiveMillikitap.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account  
        public ActionResult Index(string ReturnUrl = "")
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "TimesMachine");
            }
            else
            {
                if (!string.IsNullOrEmpty(ReturnUrl))
                {
                    return RedirectToAction("Login", new { ReturnUrl = ReturnUrl });
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
        }

        [HttpGet]
        public ActionResult Privacy()
        {
            ViewBag.Title = "Политика конфиденциальности";
            return View();
        }

        public async Task<ActionResult> Login(string ReturnUrl = "", string info = "")
        {
            await getCustomStyleAndTranslate();            
            ViewBag.ReturnUrl = !string.IsNullOrEmpty (ReturnUrl)? ReturnUrl.Replace("*","&"):string.Empty;
            ViewBag.Info = info;
            ViewBag.Caption = System.Configuration.ConfigurationManager.AppSettings["LoginCaption"];
            return View();
        }

        /// <summary>
        /// Двухфакторная аутентификация
        /// Страница ввода кода верификации для проверки
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> userVerification(string info)
        {
            try
            {
                var user = (CustomMembershipUser)Membership.GetUser();
                var userFromTable = await new BLogicMillikitap().getUserByUserName(user.UserName);
                TempData["telephone"] = userFromTable.Telephone;
                TempData["info"] = info;
                ViewData["Translate"] = await new BLogicMillikitap().getLanguageTranslateList(PagesIdStatic.TwoFactorAuth);
                return View();
            }
            catch
            {
                return RedirectToAction("Index", "TimesMachine");
            }
        }

        /// <summary>
        /// Информация о прохождении двухфакторной аутентификации
        /// </summary>
        /// <returns></returns>
        private CustomSerializeModel getUserTwoFactorAuthInfo()
        {
            HttpCookie authCookie = Request.Cookies["Cookie1"];
            if (authCookie != null)
            {
                try
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    var serializeModel = JsonConvert.DeserializeObject<CustomSerializeModel>(authTicket.UserData);
                    return serializeModel;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Проверка СМС кода верификации пользователя
        /// </summary>
        /// <param name="telephoneNumber"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CheckCodeMobileTelephone(string telephoneNumber, string code)
        {
            var languagesPack = await new BLogicMillikitap().getLanguageTranslateList(PagesIdStatic.SignInSignOut);
            if (string.IsNullOrEmpty(telephoneNumber))
            {
                return RedirectToAction("LogOut");
            }
            if(string.IsNullOrEmpty(code))
            {
                return RedirectToAction("userVerification", new { @info = languagesPack["Необходимо_ввести_код_подтверждения"] });
            }
            //Сравниваем код отправки с текущим кодом
            var infoAuthUser = getUserTwoFactorAuthInfo();
            if(infoAuthUser.sms_Code == code)
            {
                var user = (CustomMembershipUser)Membership.GetUser();
                var userFromTable = await new BLogicMillikitap().getUserByUserName(user.UserName);
                if (user != null && userFromTable != null)
                {
                    CustomSerializeModel userModel = new CustomSerializeModel()
                    {
                        UserId = user.UserId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        RoleName = user.Roles.Select(r => r.RoleName).ToList(),
                        Telephone = userFromTable.Telephone,
                        sms_Authentication = true,
                        sms_Code = Helper.GeneratePassword(string.Format("{0}-{1}", user.UserId.ToString(), DateTime.Now.Ticks), 5, 6)
                    };

                    string userData = JsonConvert.SerializeObject(userModel);
                    FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket
                        (
                            1, userFromTable.Username, DateTime.Now, DateTime.Now.AddDays(10), false, userData
                        );

                    string enTicket = FormsAuthentication.Encrypt(authTicket);
                    HttpCookie faCookie = new HttpCookie("Cookie1", enTicket);
                    Response.Cookies.Add(faCookie);
                    return RedirectToAction("Index", "TimesMachine");
                }
            }
            return RedirectToAction("userVerification", new { @info = languagesPack["Неверный_код"] });
        }

        [HttpPost]
        public async Task<ActionResult> Login(User loginView, string ReturnUrl = "")
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(loginView.Username, loginView.Password))
                {
                    var user = (CustomMembershipUser)Membership.GetUser(loginView.Username, false);
                    var userFromTable = await new BLogicMillikitap().getUserByUserName(user.UserName);
                    if (user != null && userFromTable!=null)
                    {
                        CustomSerializeModel userModel = new CustomSerializeModel()
                        {
                            UserId = user.UserId,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            RoleName = user.Roles.Select(r => r.RoleName).ToList(),
                            Telephone = userFromTable.Telephone,
                            sms_Authentication = false,
                            sms_Code = Helper.GeneratePassword(string.Format("{0}-{1}",user.UserId.ToString(),DateTime.Now.Ticks),5,6)
                        };

                        string userData = JsonConvert.SerializeObject(userModel);
                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket
                            (
                                1, loginView.Username, DateTime.Now, DateTime.Now.AddDays(10), false, userData
                            );

                        string enTicket = FormsAuthentication.Encrypt(authTicket);
                        HttpCookie faCookie = new HttpCookie("Cookie1", enTicket);
                        Response.Cookies.Add(faCookie);
                    }

                    if(!string.IsNullOrEmpty(ReturnUrl))
                    {
                        if(ReturnUrl.Contains("_"))
                        {
                            ReturnUrl = ReturnUrl.Replace("_", "&");
                        }
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            ViewBag.ReturnUrl = ReturnUrl;
            var languagesPack = await new BLogicMillikitap().getLanguageTranslateList(PagesIdStatic.SignInSignOut);
            ModelState.AddModelError("", (string)languagesPack["Неверное_имя_пользователя_или_пароль"]);
            await getCustomStyleAndTranslate();
            return View(loginView);
        }
        
        [HttpPost]
        public async Task<ActionResult> Registration(RegistrationModelUser registrationView)
        {
            var param = System.Configuration.ConfigurationManager.AppSettings["EnableRegister"];
            var languagesPack = await new BLogicMillikitap().getLanguageTranslateList(PagesIdStatic.SignInSignOut);
            if (param == "0")
            {
                return RedirectToAction("Login", new { info = languagesPack["Регистрация_на_сайте_закрыта"] });
            }
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(registrationView.FirstName) || string.IsNullOrWhiteSpace(registrationView.LastName))
                {
                    return RedirectToAction("Login", new { info = "Укажите имя и фамилию" });
                }
                if (string.IsNullOrWhiteSpace(registrationView.PlaceOfStudyWork))
                {
                    return RedirectToAction("Login", new { info = "Укажите место учёбы или работы" });
                }
                if (!IsAllowedRegistrationEmail(registrationView.Username))
                {
                    return RedirectToAction("Login", new { info = "Допускаются только адреса на доменах .ru и .рф" });
                }
                if (!registrationView.PrivacyAccepted)
                {
                    return RedirectToAction("Login", new { info = "Нужно согласие с политикой конфиденциальности" });
                }
                if (string.IsNullOrEmpty(registrationView.Password) || string.IsNullOrEmpty(registrationView.ConfirmPassword))
                {
                    return RedirectToAction("Login", new { info = languagesPack["Некорректно_заполнено_поле_пароль"] });
                }
                if (registrationView.Password != registrationView.ConfirmPassword)
                {
                    return RedirectToAction("Login", new { info = languagesPack["Пароли_не_совпадают"] });
                }
                if (string.IsNullOrEmpty (registrationView.Telephone) || registrationView.Telephone.Length<3)
                {
                    return RedirectToAction("Login", new { info = languagesPack["Номер_телефона_указан_неверно"] });
                }
                // Email Verification  
                var userIsRegistered = Membership.GetUser(registrationView.Username);
                if (userIsRegistered != null)
                {
                    return RedirectToAction("Login", new { info = languagesPack["Указанный_Email_уже_зарегистрирован"] });
                }
                
                using (ModelAppBaseMillikitap dbContext = new ModelAppBaseMillikitap())
                {
                    var user = new User()
                    {
                        Username = registrationView.Username.Trim(),
                        FirstName = registrationView.FirstName.Trim(),
                        LastName = registrationView.LastName.Trim(),
                        Email = registrationView.Username,
                        Password = registrationView.Password,
                        IsActive = true,
                        ActivationCode = Guid.NewGuid(),
                        Telephone = registrationView.Telephone,
                        PlaceOfStudyWork = registrationView.PlaceOfStudyWork.Trim(),
                        PrivacyAcceptedAt = DateTime.Now
                    };
                    
                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();
                }
                //VerificationEmail(registrationView.Email, registrationView.ActivationCode.ToString());                
            }
            else
            {
                return RedirectToAction("Login", languagesPack["Что_то_пошло_не_так_Повторите_попытку"]);
            }
            return RedirectToAction("Login", new { info = languagesPack["Аккаунт_успешно_создан_Войдите_под_своей_учетной_записью"] });
        }

        [HttpGet]
        public async Task<ActionResult> ActivationAccount(string id)
        {
            bool statusAccount = false;
            var languagesPack = await new BLogicMillikitap().getLanguageTranslateList(PagesIdStatic.SignInSignOut);
            using (ModelAppBaseMillikitap dbContext = new ModelAppBaseMillikitap())
            {
                var userAccount = dbContext.Users.Where(u => u.ActivationCode.ToString().Equals(id)).FirstOrDefault();

                if (userAccount != null)
                {
                    userAccount.IsActive = true;
                    dbContext.SaveChanges();
                    statusAccount = true;
                }
                else
                {
                    ViewBag.Message = languagesPack["Что_то_пошло_не_так_Повторите_попытку"];
                }

            }
            ViewBag.Status = statusAccount;
            return View();
        }

        public ActionResult LogOut(string info = "")
        {
            HttpCookie cookie = new HttpCookie("Cookie1", "");
            cookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie);

            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account", new { @info = info });
        }

        [NonAction]
        public void VerificationEmail(string email, string activationCode)
        {
            var url = string.Format("/Account/ActivationAccount/{0}", activationCode);
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, url);

            var fromEmail = new MailAddress("mehdi.rami2012@gmail.com", "Activation Account - AKKA");
            var toEmail = new MailAddress(email);

            var fromEmailPassword = "******************";
            string subject = "Код активации";

            string body = "<br/> Пожалуйста, нажмите на следующую ссылку, чтобы активировать свою учетную запись" + "<br/><a href='" + link + "'> Активировать аккаунт </a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true

            })
                smtp.Send(message);
        }

        /// <summary>
        /// Установить модель языка (LangId)
        /// </summary>
        /// <returns></returns>
        private async Task SetLanguagePack()
        {
            //Получаем LangId языка
            ViewData["Translate"] = await new BLogicMillikitap().getLanguageTranslateList(PagesIdStatic.SignInSignOut);
        }

        private async Task getCustomStyleAndTranslate()
        {
            var styleClass = await new BLogicMillikitap().getCustomStyleApp();
            ViewData["CSS"] = styleClass;            
            await SetLanguagePack();
        }

        public async Task<ActionResult> Account(string info = "")
        {
            ViewBag.Info = info;
            if (User.Identity.IsAuthenticated)
            {
                await getCustomStyleAndTranslate();
                var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
                ViewBag.Caption = System.Configuration.ConfigurationManager.AppSettings["AccountCaption"];
                ViewData["Translate"] = await new BLogicMillikitap().getLanguageTranslateList(PagesIdStatic.Account);
                return View(user);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<ActionResult> SaveProfile(string firstName, string lastName)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
                using (ModelAppBaseMillikitap dbContext = new ModelAppBaseMillikitap())
                {
                    var userAccount = await dbContext.Users.Where(u => u.UserId == user.UserId).FirstOrDefaultAsync();
                    if (userAccount != null)
                    {
                        userAccount.FirstName = firstName;
                        userAccount.LastName = lastName;
                        await dbContext.SaveChangesAsync();
                    }
                }
                return RedirectToAction("Account");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }        
                
        [HttpPost]
        public async Task<ActionResult> ChangePassword(string current, string password, string confirm)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userMembership = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
                var languagesPack = await new BLogicMillikitap().getLanguageTranslateList(PagesIdStatic.SignInSignOut);
                if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(confirm))
                {
                    using (ModelAppBaseMillikitap dbContext = new ModelAppBaseMillikitap())
                    {
                        var userAccount = await dbContext.Users.Where(u => u.UserId == userMembership.UserId).FirstOrDefaultAsync();                        
                        if (userAccount != null)
                        {
                            if(userAccount.Password!=current)
                            {
                                return RedirectToAction("Account", new { @info = languagesPack["Текущий_пароль_неверный"] });
                            }
                            if (password != confirm)
                            {
                                return RedirectToAction("Account", new { @info = languagesPack["Пароли_не_совпадают"] });
                            }
                            userAccount.Password = password;
                            await dbContext.SaveChangesAsync();
                            return RedirectToAction("LogOut", new { @info = languagesPack["Пароль_успешно_изменен"] });
                        }
                    }                    
                }
                return RedirectToAction("Account", new { @message = languagesPack["Пароль_не_может_быть_пустым"] });

            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        static bool IsAllowedRegistrationEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            email = email.Trim();
            try
            {
                var address = new MailAddress(email);
                if (!string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                var idn = new IdnMapping();
                var unicodeHost = idn.GetUnicode(address.Host);
                var asciiHost = idn.GetAscii(address.Host);
                return EndsWithDomain(unicodeHost, ".ru")
                    || EndsWithDomain(unicodeHost, ".рф")
                    || EndsWithDomain(asciiHost, ".ru")
                    || EndsWithDomain(asciiHost, ".xn--p1ai");
            }
            catch
            {
                return false;
            }
        }

        static bool EndsWithDomain(string host, string suffix)
        {
            return host != null && host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }
    }
}