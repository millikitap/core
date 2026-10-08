using Newtonsoft.Json;
using RectImageArchiveMillikitap.Models;
using RectImageArchiveMillikitap.Models.CustomAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using static RectImageArchiveMillikitap.Models.BLogicMillikitap;
using static RectImageArchiveMillikitap.Models.CustomAuthentication.AttributesMethod;
using static RectImageArchiveMillikitap.Models.Helper;

namespace RectImageArchiveMillikitap.Controllers
{
    [NotAuthoriseAttribute]
    public class TimesMachineController : Controller
    {

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
        
        //Главная страница (все книги)        
        public async Task<ActionResult> Index()
        {   
            await checkTwoFactorAuth(string.Empty);
            ViewData["folderBookUrl"] = new Uri(Request.Url, Url.Content("~/Books/")).LocalPath;
            ViewBag.Caption = System.Configuration.ConfigurationManager.AppSettings["IndexCaption"];
            var bl = new BLogicMillikitap();            
            ViewData["states"] = await bl.getAllState();
            ViewData["themes"] = await bl.getAllThemes();
            ViewData["types"] = await bl.getAllTypes();
            ViewData["access"] = checkAccessEditor();            
            ViewData["Translate"] = await new BLogicMillikitap().getLanguageTranslateList(PagesIdStatic.Index);
            await getCustomStyle(bl);
            return View();
        }

        /// <summary>
        /// Инструкция для администратора/редактора/эксперта
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> HowTo()
        {
            if (checkAccessEditor())
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account", new { info = messages.NotPermission });
            }
        }

        /// <summary>
        /// Проверка на двухфакторную аутентификацию
        /// </summary>
        public async Task checkTwoFactorAuth(string info)
        {
            var useSMSAuth = System.Configuration.ConfigurationManager.AppSettings["UseTwoFactorSMS"];
            int IdDemoUser = int.Parse(System.Configuration.ConfigurationManager.AppSettings["IdDemoUser"]);
            if (useSMSAuth == "1")
            {   
                var twoFactorAuth = getUserTwoFactorAuthInfo();
                if (twoFactorAuth.UserId != IdDemoUser)
                {
                    if (!twoFactorAuth.sms_Authentication)
                    {
                        string messageCode = string.Format($"Код для входа на сайт: {twoFactorAuth.sms_Code}");
                        await new BLogicMillikitap().sendSMS(twoFactorAuth.Telephone, messageCode);
                        RedirectToAction("userVerification", "Account", new { @info = info }).ExecuteResult(this.ControllerContext); ;
                    }
                }
            }
        }        

        private async Task getCustomStyle(BLogicMillikitap bl)
        {
            var styleClass = await bl.getCustomStyleApp();
            ViewData["CSS"] = styleClass;
        }
        
        /// <summary>
        /// Панель управления точка входа
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ControlPanel()
        {
            var access = checkAccessEditor();
            if (!access)
            {
                return RedirectToAction("Login", "Account", new { info = messages.NotPermission });
            }
            var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
            var bl = new BLogicMillikitap();
            var res = await bl.getBooksForAdminPanel(user);
            ViewData["InfoBooks"] = res;
            ViewData["IsSuperAdmin"] = isSuperAdmin();
            ViewData["Roles"] = await bl.getRoles();
            ViewData["Users"] = await bl.getUsers();
            return View();
        }

        /// <summary>
        /// Новая книга/Добавление
        /// </summary>
        /// <param name="BookId"></param>
        /// <returns></returns>
        public async Task<ActionResult> EditorBook(int? BookId)
        {
            //Проверка на доступ
            bool access = checkAccessEditor();
            if (!access)
            {
                return RedirectToAction("Login", "Account", new { info = messages.NotPermission });
            }
            //Проверка на администратора или редактора
            bool grant_accessInBook = isSuperAdmin();
            var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
            Books book = null;
            var bl = new BLogicMillikitap();
            if (BookId.HasValue)
            {
                book = await bl.getBookById(BookId.Value, false,false);
            }
            //Проверка на эксперта
            if (!grant_accessInBook)
            {
                //Проверка на Books[UserId]
                if(book!=null && book.UserExpert.UserId != user.UserId)
                {
                    return RedirectToAction("Login", "Account", new { info = messages.NotPermission });
                }
            }
            if (BookId.HasValue)
            {                    
                ViewData["folderBookUrl"] = new Uri(Request.Url, Url.Content("~/Books/" + book.HashFolder + "/")).LocalPath;
                ViewData["BookInfo"] = book;                
            }
            else
            {
                ViewData["BookInfo"] = null;
            }
            if (grant_accessInBook)
            {
                //Если грант права
                //Получаем список всех пользователей
                ViewData["Users"] = await bl.getUsersForControlBook();
            }
            ViewData["UserId"] = user.UserId;
            ViewData["GrantUser"] = grant_accessInBook;
            //Справочные материалы для редактора
            ViewData["StatesList"] = await bl.getStatesList();
            //Языки
            ViewData["LangList"] = await bl.getLanguagesList();
            //Тематики
            ViewData["ThemesList"] = await bl.getThemeLanguagesList();
            //Типы
            ViewData["TypesList"] = await bl.getTypesLanguagesList();
            //Регионы
            ViewData["RegionsList"] = await bl.getRegionsList();
            //Коллекции
            ViewData["CollectionsList"] = await bl.getAllCollections();
            //Для сборника сочинений
            ViewData["InventoryCollectedList"] = await bl.getAllInventoryCollections();
            //Названия сборников
            ViewData["NamesCollectedList"] = await bl.getAllNamesCollections();
            return View();
        }

        
        [HttpPost]
        public async Task<ActionResult> SaveBook(SaveModelBook model)
        {
            var access = checkAccessEditor();
            if (!access)
            {
                return RedirectToAction("Login", "Account", new { info = messages.NotPermission });
            }
            var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
            bool isVisible = BLogicMillikitap.getCheckedStateByString(model.visible);

            bool isEditor = isSuperAdmin();
            var res = await new BLogicMillikitap().SaveBook(user, isVisible, model, isEditor);
            if(string.IsNullOrEmpty(res.OperationInfo))
            {
                return RedirectToAction("EditorBook", new { BookId = res.BookId });
            }
            else
            {
                return Json(new { result = $"Книга не сохранена: {res.OperationInfo}" });
            }
        }

        //Копирование книги
        [HttpPost]
        public async Task<JsonResult> СopyBook(int bookId, bool copyDir)
        {
            var access = checkAccessEditor();
            if (!access)
            {
                return Json(401, JsonRequestBehavior.DenyGet);
            }
            var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
            var res = await new BLogicMillikitap().CopyBook(bookId, copyDir, user);
            return Json(res, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public async Task<ActionResult> uploadPage (HttpPostedFileBase file, int i)
        {
            var access = checkAccessEditor();
            if (!access)
            {
                return RedirectToAction("Login", "Account", new { info = messages.NotPermission });
            }
            List<HttpPostedFileBase> attachment = new List<HttpPostedFileBase>();
            attachment.Add(file);
            var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
            var res = await new BLogicMillikitap().uploadFiles(user,attachment, i);
            if (res.Result)
            {
                return RedirectToAction("EditBook", new { BookId = i });
            }
            return Json(new { status = res.Result, exception = res.exception });
        }

        [HttpPost]
        public async Task<ActionResult> uploadZIP(HttpPostedFileBase file, int i)
        {
            var access = checkAccessEditor();
            if (!access)
            {
                return RedirectToAction("Login", "Account", new { info = messages.NotPermission });
            }
            List<HttpPostedFileBase> attachment = new List<HttpPostedFileBase>();
            attachment.Add(file);
            var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
            var res = await new BLogicMillikitap().uploadZIPFile(user, attachment, i);            
            return Json(res);
        }

        [HttpPost]
        public async Task<ActionResult> uploadPages(IEnumerable<HttpPostedFileBase> upload_imgs, int i)
        {
            var access = checkAccessEditor();
            if (!access)
            {
                return RedirectToAction("Login", "Account", new { info = messages.NotPermission });
            }
            var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
            var res = await new BLogicMillikitap().uploadFiles(user,upload_imgs, i);
            if (res.Result)
            {
                return RedirectToAction("EditBook", new { BookId = i });
            }
            return Json(new { status = res.Result, exception = res.exception });            
        }
        

        //Просмотр книги
        [HttpGet]
        public async Task<ActionResult> View (int? BookId, int? PageId)
        {
            await checkTwoFactorAuth(string.Empty);
            if (!BookId.HasValue)
            {
                return RedirectToAction("Index");
            }
            var bl = new BLogicMillikitap();
            var res = await bl.getBookById(BookId.Value, false,true);
            if (res != null)
            {
                await getCustomStyle(bl);
                ViewData["folderBookUrl"] = new Uri(Request.Url, Url.Content("~/Books/" + res.HashFolder + "/")).LocalPath;
            }
            //Текущий пользователь
            if (User.Identity.IsAuthenticated)
            {
                ViewData["currentUser"] = await bl.getUserByUserName(Membership.GetUser(User.Identity.Name).UserName);                
            }
            else
            {
                ViewData["currentUser"] = null;
            }
            //Проверяем на сборник сочинений
            ViewData["сollectionDataBook"] = await bl.getCollectionData(res);
            ViewBag.Caption = System.Configuration.ConfigurationManager.AppSettings["ViewCaption"];
            ViewData["access"] = checkAccessEditor();

            //Тематики
            ViewData["ThemesList"] = await bl.getThemeLanguagesList();
            //Типы
            ViewData["TypesList"] = await bl.getTypesLanguagesList();
            //Коллекции
            ViewData["CollectionsList"] = await bl.getCollectionsList();

            ViewData["Translate"] = await new BLogicMillikitap().getLanguageTranslateList(PagesIdStatic.View);
            return View(res);
        }

        /// <summary>
        /// Получить лайк
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> isLike(int viewerid)
        {
            if(User.Identity.IsAuthenticated)
            {
                var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
                var data = await new BLogicMillikitap().isLike(viewerid,user);
                return Json(data, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.DenyGet);
            }
            
        }
                
        [HttpPost]
        public async Task<JsonResult> getBookInfo(int bookId)
        {
            var res = await new BLogicMillikitap().getBookInfo(bookId);
            return Json(res, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        public async Task<JsonResult> getContent(int viewerid, int pagesid)
        {
            var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
            var res = await new BLogicMillikitap().getDescription(viewerid, pagesid, user);
            return Json(res, JsonRequestBehavior.DenyGet);
        }

        public async Task<ActionResult> Favorite()
        {
            var bl = new BLogicMillikitap();
            var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
            var data = await bl.getFavorite(user);
            await getCustomStyle(bl);
            ViewBag.Caption = System.Configuration.ConfigurationManager.AppSettings["BookmarksCaption"];
            ViewData["Translate"] = await new BLogicMillikitap().getLanguageTranslateList(PagesIdStatic.Favorite);
            return View(data);
        }

        private bool checkAccessEditor()
        {
            var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
            if (user != null)
            {
                var res = from a in user.Roles
                          where a.RoleName.Contains(EnumTypes.Roles.Admin) || a.RoleName.Contains(EnumTypes.Roles.Editor) || a.RoleName.Contains(EnumTypes.Roles.Expert)
                          select a;
                if (!res.Any())
                {
                    return false;
                }
                return true;
            }
            return false;
        }
        

        //Редактор страницы  
        [Authorize]        
        public async Task<ActionResult> Edit(int bookId, int pageId)
        {
            var access = checkAccessEditor();
            if (!access)
            {
                return RedirectToAction("Login", "Account", new { info = messages.NotPermission });
            }
            var book = await new BLogicMillikitap().getBookById(bookId, true,true);
            var page = book?.Pages?.Where(p => p.Id == pageId)?.FirstOrDefault();
            ViewData["EditorSVGOriginal"] = page?.RectAreasEditor?.FirstOrDefault()?.RectEditorSVGOriginal;            
            ViewData["pageModel"] = page;
            string urlImage = Url.Content($"//Books//{book.HashFolder}//{page?.FileName}");
            ViewData["imageUrl"] = urlImage;
            return View();
        }

        //Сохранение страницы
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> saveChanges(int bookId, int pageId, string RectEditorSVGOriginal, string RectForViewerSVG, string BlobImage)
        {
            var access = checkAccessEditor();
            if (!access)
            {
                return RedirectToAction("Login", "Account", new { info = messages.NotPermission });
            }
            var res = await new BLogicMillikitap().saveChanges(bookId, pageId, RectEditorSVGOriginal, RectForViewerSVG, BlobImage);
            if(res.Result)
            {
                return RedirectToAction("Edit", new { bookId = bookId, pageId = pageId });
            }
            else
            {                
                return Json(new { status = res.Result, exception = res.exception });
            }            
        }

        /// <summary>
        /// Вариант поиска для главной страницы (PartialView)
        /// </summary>
        /// <param name="searchForm"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>       
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [OutputCache(Duration = 10, Location = OutputCacheLocation.Server, VaryByParam = "DtStart;DtEnd;Name;Tag;Text;Person;Event;Place;Organization;ArticleTitle;Attributes.Inventory;Attributes.Width;Attributes.Height;Attributes.StatesId;Attributes.NameOfBookArabic;Attributes.AuthorOfArabic;Attributes.ThemeOfArabic;Attributes.TypeOfArabicId;  Attributes.NameOfBookCyrillic;Attributes.AuthorOfCyrillic;Attributes.ThemeOfRussian;Attributes.TypeOfRussianId;Attributes.NameOfBookEnglish;Attributes.AuthorOfEnglish;Attributes.ThemeOfEnglish;Attributes.TypeOfEnglishId;Attributes.ThemesLanguagesListId;Attributes.TypesLanguagesListId;")]
        public async Task<ActionResult> SearchPartialMain(SearchForm searchForm)
        {
            searchForm.isAccessEditor = checkAccessEditor();
            var bl = new BLogicMillikitap();
            //данные
            var data = await bl.searchBooks(searchForm);
            //справочники
            ViewData["states"] = await bl.getAllState();
            ViewData["themes"] = await bl.getAllThemes();
            ViewData["types"] = await bl.getAllTypes();
            ViewData["Translate"] = await new BLogicMillikitap().getLanguageTranslateList(PagesIdStatic.SearchPartialMain);
            return PartialView("SearchPartialMain", data);
        }

        [Authorize]
        [OutputCache(Duration = 10, Location = OutputCacheLocation.Server, VaryByParam = "DtStart;DtEnd;Name;Tag;Text;Person;Event;Place;Organization;ArticleTitle;Attributes.Inventory;Attributes.Width;Attributes.Height;Attributes.StatesId;Attributes.NameOfBookArabic;Attributes.AuthorOfArabic;Attributes.ThemeOfArabic;Attributes.TypeOfArabicId;  Attributes.NameOfBookCyrillic;Attributes.AuthorOfCyrillic;Attributes.ThemeOfRussian;Attributes.TypeOfRussianId;Attributes.NameOfBookEnglish;Attributes.AuthorOfEnglish;Attributes.ThemeOfEnglish;Attributes.TypeOfEnglishId;Attributes.ThemesLanguagesListId;Attributes.TypesLanguagesListId;")]
        public async Task<JsonResult> Search(SearchForm searchForm)
        {
            searchForm.isAccessEditor = checkAccessEditor();
            var data = await new BLogicMillikitap().searchBooks(searchForm);            
            return Json(data, JsonRequestBehavior.DenyGet);
        }

        [OutputCache(Duration = 10, Location = OutputCacheLocation.Server, VaryByParam = "Person;Event;Name;Place;Organization")]
        [Authorize]        
        public async Task<ActionResult> SearchByTag(string Person, string Event, string Place, string Organization)
        {
            SearchForm model = new SearchForm();
            model.Person = Person;
            model.Event = Event;
            model.Place = Place;
            model.Organization = Organization;
            var data = await new BLogicMillikitap().searchBooks(model);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize]
        [OutputCache(Duration = 30, Location = OutputCacheLocation.Server, VaryByParam = "s")]
        public async Task<JsonResult> Autocomplete(string s, EnumTypes.AutoEnum type)
        {
            if(s.Length<1)
            {
                return Json(null, JsonRequestBehavior.DenyGet);
            }
            var data = await new BLogicMillikitap().Autocomplete(s, type);
            return Json(data, JsonRequestBehavior.DenyGet);
        }

        [HttpPost]
        [Authorize]
        [OutputCache(Duration = 30, Location = OutputCacheLocation.Server)]
        public async Task<JsonResult> getAutocompleteData()
        {
            var data = await new BLogicMillikitap().getAutocompleteData();
            return Json(data);
        }

        [Authorize]
        [OutputCache(Duration = 60, Location = OutputCacheLocation.Server)]
        public async Task<ActionResult> tagsCloud()
        {
            var bl = new BLogicMillikitap();
            await getCustomStyle(bl);
            var tagsJson = await bl.getTagsCloud();
            ViewData["tagsJson"] = tagsJson;
            ViewData["Translate"] = await new BLogicMillikitap().getLanguageTranslateList(PagesIdStatic.TagsCloud);
            ViewBag.Caption = System.Configuration.ConfigurationManager.AppSettings["AlphabetCaption"];
            return View();
        }

        /// <summary>
        /// Применение обложки для книги
        /// </summary>
        /// <param name="bookId"></param>
        /// <param name="currCheckState"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<JsonResult> SetCoverBook(int bookId, bool currCheckState, string src)
        {
            if (checkAccessEditor())
            {
                var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
                var res = await new BLogicMillikitap().SetCoverBook(bookId, currCheckState, src);
                return Json(new { result = res }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(new { result = 403 }, JsonRequestBehavior.DenyGet);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> RemoveBook(int id)
        {
            if (checkAccessEditor())
            {
                var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
                var res = await new BLogicMillikitap().RemoveBook(id, user);
                return Json(new { result = res }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(new { result = 403 }, JsonRequestBehavior.DenyGet);
            }
        }
        
        /// <summary>
        /// Удаление страницы
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<JsonResult> RemovePage(int id)
        {   
            if (checkAccessEditor())
            {
                var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
                var res = await new BLogicMillikitap().RemovePage(id, user);
                return Json(new { result = res }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(new { result = 403 }, JsonRequestBehavior.DenyGet);
            }
        }


        /// <summary>
        /// Удаление страниц массивом
        /// </summary>
        /// <param name="pagesId"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<JsonResult> RemovePageArray(int bookId, int[] pagesId)
        {
            if (checkAccessEditor())
            {
                var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
                if(pagesId!=null && pagesId.Length>0)
                {
                    var res = await new BLogicMillikitap().RemovePageArray(bookId, pagesId, user);
                    return Json(new { result = res }, JsonRequestBehavior.DenyGet);
                }                
                return Json(new { result = 100 }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(new { result = 403 }, JsonRequestBehavior.DenyGet);
            }
        }

        //**********************************************************************************
        //*************************** Чат **************************************************
        //**********************************************************************************

        /// <summary>
        /// Последние 50 сообщений в чате
        /// </summary>
        /// <param name="BookId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> getLastMessageChat(int BookId, int skip, int take)
        {
            var select = await new BLogicMillikitap().getOldMessages(BookId, skip, take);
            if(select!=null && select.Any())
            {
                var res = select.Select(c => new { c.Id, c.IdStringSocket, c.MessageText, dt = c.dt.ToString("dd.MM.yyyy HH:mm") , c.User.Username, c.isAnswer, c.MessageAnswerId, c?.PageId })?.ToList();               
                return new JsonResult
                {
                    MaxJsonLength = int.MaxValue,
                    Data = res,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }        

        //**********************************************************************************
        //*************************** Права для пользователя *******************************
        //**********************************************************************************

        /// <summary>
        /// Есть ли доступ ко всему
        /// </summary>
        /// <returns></returns>
        public bool isSuperAdmin()
        {
            bool grant_accessInBook = false;
            var user = (CustomMembershipUser)Membership.GetUser(User.Identity.Name);
            foreach (var r in user.Roles)
            {
                if (r.RoleName.Contains(EnumTypes.Roles.Admin) || r.RoleName.Contains(EnumTypes.Roles.Editor))
                {
                    grant_accessInBook = true;
                    break;
                }
            }
            return grant_accessInBook;
        }

        public async Task<JsonResult> SetupRoleUser(int UserId,string[] roles)
        {                   
            if(isSuperAdmin())
            {
                //check role
                var res = await new BLogicMillikitap().setupRoleUser(UserId, roles);
                return Json(res, JsonRequestBehavior.DenyGet);
            }
            return Json(401, JsonRequestBehavior.DenyGet);
        }

        //**********************************************************************************
        //*************************** Справочники ******************************************
        //**********************************************************************************

        //Язык
        [Authorize]
        public async Task<JsonResult> LangAction(EnumTypes.MethodTypeList type, int? Id, string name)
        {
            Langs classLang = new Langs(this);
            if (classLang.haveAccess)
            {
                switch (type)
                {
                    case EnumTypes.MethodTypeList.Add:
                        {
                            var res = await classLang.add(name);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.Delete:
                        {
                            var res = await classLang.delete(Id.Value);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.Update:
                        {
                            var res = await classLang.update(Id.Value, name);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.GetAll:
                        {
                            var res = await classLang.getAll();
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                }
                return Json(null, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(401, JsonRequestBehavior.DenyGet);
            }
        }

        //Состояние
        [Authorize]
        public async Task<JsonResult> StateAction (EnumTypes.MethodTypeList type, int? Id, string name)
        {
            States classState = new States(this);
            if(classState.haveAccess)
            {
                switch(type)
                {
                    case EnumTypes.MethodTypeList.Add:
                        {
                            var res = await classState.add(name);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }                        
                    case EnumTypes.MethodTypeList.Delete:
                        {
                            var res = await classState.delete(Id.Value);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }                        
                    case EnumTypes.MethodTypeList.Update:
                        {
                            var res = await classState.update(Id.Value,name);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }                        
                    case EnumTypes.MethodTypeList.GetAll:
                        {
                            var res = await classState.getAll();
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }                        
                }
                return Json(null, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(401, JsonRequestBehavior.DenyGet);
            }            
        }

        //Тематика
        [Authorize]
        public async Task<JsonResult> ThemeAction(EnumTypes.MethodTypeList type, int? Id, string name, string ru, string en, string ar)
        {
            Themes classThemes = new Themes(this);
            if (classThemes.haveAccess)
            {
                switch (type)
                {
                    case EnumTypes.MethodTypeList.Add:
                        {
                            var res = await classThemes.add(ru,en,ar);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.Delete:
                        {
                            var res = await classThemes.delete(Id.Value);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.Update:
                        {
                            var res = await classThemes.update(Id.Value, ru, en, ar);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.GetAll:
                        {
                            var res = await classThemes.getAll();
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                }
                return Json(null, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(401, JsonRequestBehavior.DenyGet);
            }
        }

        //Тип
        [Authorize]
        public async Task<JsonResult> TypeAction(EnumTypes.MethodTypeList type, int? Id, string name, string ru, string en, string ar)
        {
            Types classTypes = new Types(this);
            if (classTypes.haveAccess)
            {
                switch (type)
                {
                    case EnumTypes.MethodTypeList.Add:
                        {
                            var res = await classTypes.add(ru, en, ar);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.Delete:
                        {
                            var res = await classTypes.delete(Id.Value);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.Update:
                        {
                            var res = await classTypes.update(Id.Value, ru, en, ar);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.GetAll:
                        {
                            var res = await classTypes.getAll();
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                }
                return Json(null, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(401, JsonRequestBehavior.DenyGet);
            }
        }

        //Регион
        [Authorize]
        public async Task<JsonResult> RegionAction(EnumTypes.MethodTypeList type, int? Id, string name)
        {
            Regions classRegion = new Regions(this);
            if (classRegion.haveAccess)
            {
                switch (type)
                {
                    case EnumTypes.MethodTypeList.Add:
                        {
                            var res = await classRegion.add(name);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.Delete:
                        {
                            var res = await classRegion.delete(Id.Value);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.Update:
                        {
                            var res = await classRegion.update(Id.Value, name);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.GetAll:
                        {
                            var res = await classRegion.getAll();
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                }
                return Json(null, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(401, JsonRequestBehavior.DenyGet);
            }
        }

        //Коллекция
        [Authorize]
        public async Task<JsonResult> CollectionAction(EnumTypes.MethodTypeList type, int? Id, string name)
        {
            Collections classCollection = new Collections(this);
            if (classCollection.haveAccess)
            {
                switch (type)
                {
                    case EnumTypes.MethodTypeList.Add:
                        {
                            var res = await classCollection.add(name);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.Delete:
                        {
                            var res = await classCollection.delete(Id.Value);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.Update:
                        {
                            var res = await classCollection.update(Id.Value, name);
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                    case EnumTypes.MethodTypeList.GetAll:
                        {
                            var res = await classCollection.getAll();
                            return Json(res, JsonRequestBehavior.DenyGet);
                        }
                }
                return Json(null, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(401, JsonRequestBehavior.DenyGet);
            }
        }


        //Справочники
        //Языки
        public class Langs
        {
            private TimesMachineController controller = null;
            public bool haveAccess = false;
            public Langs(TimesMachineController controller)
            {
                this.controller = controller;
                haveAccess = controller.checkAccessEditor();
            }
            public async Task<Boolean?> delete(int LangId)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().deleteLang(LangId);
                    return res;
                }
                return false;
            }
            public async Task<universalListClass> add(string Name)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().addLang(Name);
                    return res;
                }
                return null;
            }
            public async Task<Boolean?> update(int LangId, string Name)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().updateLang(LangId, Name);
                    return res;
                }
                return false;
            }
            public async Task<List<Models.LanguageTableList>> getAll()
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().getAllLang();
                    return res;
                }
                return null;
            }
        }
        //Состояние
        public class States
        {
            private TimesMachineController controller = null;
            public bool haveAccess = false;
            public States(TimesMachineController controller)
            {
                this.controller = controller;
                haveAccess = controller.checkAccessEditor();
            }
            public async Task<Boolean?> delete(int StateId)
            {
                if(haveAccess)
                {
                    var res = await new BLogicMillikitap().deleteState(StateId);
                    return res;
                }
                return false;
            }
            public async Task<universalListClass> add(string Name)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().addState(Name);
                    return res;
                }
                return null;
            }
            public async Task<Boolean?> update(int StateId, string Name)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().updateState(StateId,Name);
                    return res;
                }
                return false;
            }
            public async Task<List<Models.States>> getAll()
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().getAllState();
                    return res;
                }
                return null;
            }
        }
        //Тематика
        public class Themes
        {
            private TimesMachineController controller = null;
            public bool haveAccess = false;
            public Themes(TimesMachineController controller)
            {
                this.controller = controller;
                haveAccess = controller.checkAccessEditor();
            }
            public async Task<Boolean?> delete(int ThemeId)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().deleteTheme(ThemeId);
                    return res;
                }
                return false;
            }
            public async Task<universalListClass> add(string ru, string en, string ar)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().addTheme(ru,en,ar);
                    return res;
                }
                return null;
            }
            public async Task<Boolean?> update(int ThemeId, string ru,string en,string ar)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().updateTheme(ThemeId, ru, en, ar);
                    return res;
                }
                return false;
            }
            public async Task<List<Models.ThemesLanguagesList>> getAll()
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().getAllThemes();
                    if (res != null && res.Any())
                    {
                        res = res.Select(c => new ThemesLanguagesList()
                        {
                            ThemesLanguagesListId = c.ThemesLanguagesListId,
                            DescriptionThemeRussian = c.DescriptionThemeRussian != null ? c.DescriptionThemeRussian : "-",
                            DescriptionThemeEnglish = c.DescriptionThemeEnglish != null ? c.DescriptionThemeEnglish : "-",
                            DescriptionThemeArabic = c.DescriptionThemeArabic != null ? c.DescriptionThemeArabic : "-",
                        })?.ToList();
                        return res;
                    }
                }
                return null;
            }
        }
        //Тип
        public class Types
        {
            private TimesMachineController controller = null;
            public bool haveAccess = false;
            public Types(TimesMachineController controller)
            {
                this.controller = controller;
                haveAccess = controller.checkAccessEditor();
            }
            public async Task<Boolean?> delete(int TypeId)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().deleteType(TypeId);
                    return res;
                }
                return false;
            }
            public async Task<universalListClass> add(string ru, string en, string ar)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().addType(ru,en,ar);
                    return res;
                }
                return null;
            }
            public async Task<Boolean?> update(int TypeId, string ru, string en, string ar)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().updateType(TypeId, ru, en, ar);
                    return res;
                }
                return false;
            }
            public async Task<List<Models.TypesLanguagesList>> getAll()
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().getAllTypes();
                    if (res!= null && res.Any())
                    {
                        res = res.Select(c => new TypesLanguagesList
                        {
                            TypesLanguagesListId = c.TypesLanguagesListId,
                            DescriptionTypeRussian = c.DescriptionTypeRussian != null ? c.DescriptionTypeRussian : "-",
                            DescriptionTypeEnglish = c.DescriptionTypeEnglish != null ? c.DescriptionTypeEnglish : "-",
                            DescriptionTypeArabic = c.DescriptionTypeArabic != null ? c.DescriptionTypeArabic : "-",
                        })?.ToList();
                    }
                    return res;
                }
                return null;
            }            
        }

        //Регион
        public class Regions
        {
            private TimesMachineController controller = null;
            public bool haveAccess = false;
            public Regions(TimesMachineController controller)
            {
                this.controller = controller;
                haveAccess = controller.checkAccessEditor();
            }
            public async Task<Boolean?> delete(int RegionId)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().deleteRegion(RegionId);
                    return res;
                }
                return false;
            }
            public async Task<universalListClass> add(string Name)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().addRegion(Name);
                    return res;
                }
                return null;
            }
            public async Task<Boolean?> update(int RegionId, string Name)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().updateRegion(RegionId, Name);
                    return res;
                }
                return false;
            }
            public async Task<List<Models.Regions>> getAll()
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().getAllRegions();
                    return res;
                }
                return null;
            }
        }

        //Коллекция
        public class Collections
        {
            private TimesMachineController controller = null;
            public bool haveAccess = false;
            public Collections(TimesMachineController controller)
            {
                this.controller = controller;
                haveAccess = controller.checkAccessEditor();
            }
            public async Task<Boolean?> delete(int CollectionId)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().deleteCollection(CollectionId);
                    return res;
                }
                return false;
            }
            public async Task<universalListClass> add(string Name)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().addCollection(Name);
                    return res;
                }
                return null;
            }
            public async Task<Boolean?> update(int CollectionId, string Name)
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().updateCollection(CollectionId, Name);
                    return res;
                }
                return false;
            }
            public async Task<List<Models.Collection>> getAll()
            {
                if (haveAccess)
                {
                    var res = await new BLogicMillikitap().getAllCollections();
                    return res;
                }
                return null;
            }
        }
    }
}