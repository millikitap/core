using HtmlAgilityPack;
using ImageResizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RectImageArchiveMillikitap.Internals;
using RectImageArchiveMillikitap.Models.CustomAuthentication;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using static RectImageArchiveMillikitap.Models.CustomAuthentication.DataAccess;
using static RectImageArchiveMillikitap.Models.EnumTypes;
using static RectImageArchiveMillikitap.Models.Helper;
using static RectImageArchiveMillikitap.ModelsMillikitap.Map3d.Map3dModel;

namespace RectImageArchiveMillikitap.Models
{
    public class BLogicMillikitap
    {
        public ModelAppBaseMillikitap _context;
        public ModelAppBaseMillikitap Db
        {
            get
            {
                if (_context == null)
                    _context = new ModelAppBaseMillikitap();
                return _context;
            }
        }

        public BLogicMillikitap()
        {
            _context = Db;          
        }

        public class jsonSaveResult
        {
            public bool Result { get; set; }
            public string exception { get; set; }
        }

        /// <summary>
        /// Получить языковой идентификатор
        /// По-умолчанию LanguagesIdAStatic.Ru;
        /// </summary>
        /// <returns></returns>
        public static int ReturnLangValue()
        {
            try
            {
                if (HttpContext.Current.Request.Cookies["lng"] != null)
                {
                    return int.Parse(HttpContext.Current.Request.Cookies["lng"].Value);
                }
                return LanguagesIdAStatic.Ru;
            }
            catch
            {
                return LanguagesIdAStatic.Ru;
            }            
        }

        /// <summary>
        /// Получить перевод для отдельной страницы
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public async Task<Hashtable> getLanguageTranslateList(int pageId)
        {
            int langId = ReturnLangValue();
            var translates = await Db?.TranslateLanguage?.Where(c => c.OnPageId == pageId)?.ToListAsync();
            Hashtable Dictionary = new Hashtable();            
            List<TranslateModel> translateLangs = new List<TranslateModel>();
            switch (langId)
            {
                case 1:
                    {
                        translateLangs = translates?.Select(c => new TranslateModel { NameOnPage = c.NameOnPage, Translate = string.IsNullOrEmpty(c.Rus) ? string.Format("langId:{0}-{1}", langId, c.NameOnPage) : c.Rus })?.ToList();                        
                    }break;                    
                case 2:
                    {
                        translateLangs = translates?.Select(c => new TranslateModel { NameOnPage = c.NameOnPage, Translate = string.IsNullOrEmpty(c.Eng) ? string.Format("langId:{0}-{1}", langId, c.NameOnPage) : c.Eng })?.ToList();                        
                    }break;                    
                case 3:
                    {
                        translateLangs = translates?.Select(c => new TranslateModel { NameOnPage = c.NameOnPage, Translate = string.IsNullOrEmpty(c.Ar)?string.Format("langId:{0}-{1}", langId,c.NameOnPage) :c.Ar })?.ToList();                       
                    }break;                    
                case 4:
                    {
                        translateLangs = translates?.Select(c => new TranslateModel { NameOnPage = c.NameOnPage, Translate = string.IsNullOrEmpty(c.Tat) ? string.Format("langId:{0}-{1}", langId, c.NameOnPage) : c.Tat })?.ToList();                        
                    }break;                
            }
            foreach(var i in translateLangs)
            {
                Dictionary.Add(i.NameOnPage, i.Translate);
            }
            return Dictionary;
        }

        /// <summary>
        /// Отправка СМС
        /// </summary>
        /// <param name="number"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task sendSMS(string number, string message)
        {            
            string SMSAPIUrl = System.Configuration.ConfigurationManager.AppSettings["SMSAPIUrl"];
            string loginSMS = System.Configuration.ConfigurationManager.AppSettings["loginSMS"];
            string passwordSMS = System.Configuration.ConfigurationManager.AppSettings["passwordSMS"];

            var stringRequest = string.Format("{0}?login={1}&psw={2}&phones={3}&mes={4}",SMSAPIUrl, loginSMS, passwordSMS, number, message);
            var client = new RestClient();
            var request = new RestRequest(stringRequest);
            var cancellationTokenSource = new CancellationTokenSource();
            var restResponse = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);
            Console.WriteLine(restResponse.Content);
        }
        
        public async Task<CustomStyleAppClass> getCustomStyleApp()
        {
            CustomStyleAppClass style = new CustomStyleAppClass();
            var t = await Db?.CustomAttributesApp?.Where(c => c.isActive)?.FirstOrDefaultAsync();
            if(t!=null)
            {
                style.IndexCSS = t?.StyleIndexCSS;
                style.OtherPagesCSS = t?.StyleOtherPagesCSS;
                style.TextTitle = t?.TitleText;
            }
            return style;
        }
                
        //Все издания
        public async Task <List<Books>> getBooks()
        {
            try
            {                
                var b = await _context.Books?.Where(c=>c.Visible)?.ToListAsync();
                if(b!=null && b.Any())
                {
                    List<Books> bookList = new List<Books>();
                    foreach (var c in b)
                    {
                        List<Pages> _pagesList = new List<Pages>();
                        _pagesList = (from h in _context?.Pages?.Where(f => f.Book.BookId == c.BookId && f.Visible)?.ToList()
                                      select new Pages
                                      {
                                          Id = h.Id,
                                          FileName = h.FileName,
                                          FileNamePreview = h.FileNamePreview,
                                          DateTimeCreated = h.DateTimeCreated,
                                          Title = h.Title,
                                          Description = h.Description,
                                          RectAreasViewer = h.RectAreasViewer                                          
                                      })?.ToList();
                        
                        bookList.Add(new Books
                        {
                            BookId = c.BookId,
                            BookName = c.BookName,
                            Tags = c.Tags,
                            DateTimeCreated = c.DateTimeCreated,                            
                            Description = c.Description,
                            Pages = _pagesList
                        });
                    }
                    return bookList;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<BookInfo> getBookInfo (int bookId)
        {
            BookInfo info = new BookInfo();
            var res = await getBookById(bookId, false,true);
            if(res!=null)
            {

            }
            return info;
        }

        /// <summary>
        /// Получить пользователя по userName
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<User.User> getUserByUserName(string userName)
        {
            try
            {
                var u = await _context?.Users?.Where(c => c.Username == userName)?.FirstOrDefaultAsync();
                return u;
            }
            catch
            {
                return null;
            }
        }

        public async Task<DescriptionContent> getDescription(int viewerid, int pagesid, CustomAuthentication.CustomMembershipUser userMember)
        {
            try
            {
                var c = await _context?.RectAreasViewer?.Where(p => p.RectAreasViewerId == viewerid && p.PagesId == pagesid)?.FirstOrDefaultAsync();
                if(c!=null)
                {
                    DescriptionContent res = new DescriptionContent();
                    res.bookTag = c.Pages.Book.Tags;
                    res.descriptionBook = string.Empty; //c.Pages.Book.Description;
                    res.descriptionHtml = c?.Description;//?.Replace("***","\"");
                    res.dtCreated = c.Pages.Book.DateTimeCreated.Value;
                    
                    res.otherAttributes = new CustomAttributes();
                    res.otherAttributes.Person = c.Person;
                    res.otherAttributes.Event = c.Event;
                    res.otherAttributes.Place = c.Place;
                    res.otherAttributes.Organization = c.Organization;
                    res.otherAttributes.ArticleTitle = c.ArticleTitle;
                    //get Likes
                    var like = userMember?.Likes?.Where(l => l.RectAreasViewerId == viewerid)?.FirstOrDefault();
                    res.isLike = like != null ? true : false;
                    return res;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<LikeInfo> isLike(int viewerid, CustomAuthentication.CustomMembershipUser userMember)
        {
            LikeInfo like = new LikeInfo();
            var data = userMember?.Likes?.Where(c => c.RectAreasViewerId == viewerid)?.FirstOrDefault();
            if(data!=null)
            {
                var haveLike = await Db.Likes?.Where(c => c.UserId == userMember.UserId && c.RectAreasViewerId == viewerid)?.FirstOrDefaultAsync();
                if(haveLike!=null)
                {
                    Db.Likes.Remove(haveLike);
                }
                await Db.SaveChangesAsync();
                like.isLike = false;
            }
            else
            {
                Db.Likes.Add(new Likes
                {
                    UserId = userMember.UserId,
                    RectAreasViewerId = viewerid
                });                
                await Db.SaveChangesAsync();
                like.isLike = true;
            }
            return like;
        }

        private string getShortText(string DescriptionWithoutHTML)
        {
            if(!string.IsNullOrEmpty(DescriptionWithoutHTML))
            {
                int lenghtCut = 240;
                var lenght = DescriptionWithoutHTML.Length;
                if(lenght>= lenghtCut)
                {
                    DescriptionWithoutHTML = DescriptionWithoutHTML.Substring(0, lenghtCut)+"...";
                }                
            }
            return DescriptionWithoutHTML;
        }

        public async Task<List<Favorite>> getFavorite(CustomAuthentication.CustomMembershipUser userMember)
        {
            List<Favorite> favorite = new List<Favorite>();
            if(userMember.Likes!=null && userMember.Likes.Any())
            {
                var likes = await Db?.Likes?.Where(c=>c.UserId == userMember.UserId).ToListAsync();
                favorite = (from a in likes
                            from l in userMember.Likes
                            where a.LikeId == l.LikeId
                            select a)?.Select(c => new Favorite
                            {
                                AreaViewerId = c.RectAreasViewer.RectAreasViewerId,
                                ArticleTitle = c.RectAreasViewer?.ArticleTitle,
                                BookName = c.RectAreasViewer.Pages.Book.BookName,
                                BookId = c.RectAreasViewer.Pages.Book.BookId,
                                Description = getShortText(c.RectAreasViewer.DescriptionWithoutHTML),
                                PagesId = c.RectAreasViewer.PagesId,
                                PreviewImage = c.RectAreasViewer?.Pages?.FileNamePreview,
                                HashFolder = c?.RectAreasViewer?.Pages?.Book?.HashFolder,
                                DateTimeCreated = c?.RectAreasViewer?.Pages?.Book?.DateTimeCreated.Value.ToString("dd.MM.yyyy")                               
                            })?.ToList();
            }
            return favorite;
            
        }
        
        public async Task<Books> getBookById(int bookId, bool isEditor, bool isVisibleOnlyPages)
        {
            try
            {
                var c = await _context.Books?.Where(p=>p.BookId == bookId)?.FirstOrDefaultAsync();
                if(c.IsRemoved)
                {
                    return null;
                }
                List<Pages> _pagesList = new List<Pages>();
                string paramOther = isVisibleOnlyPages ? " and Visible = 1" : string.Empty;
                var request = await _context?.Pages.SqlQuery($"select * from [Pages] where [Book_BookId] = {c.BookId} {paramOther}")?.ToListAsync();
                _pagesList = (from h in request
                              select new Pages
                              {   
                                  Id = h.Id,
                                  Book = isEditor ? c :null,
                                  BlobImage = isEditor ? h.BlobImage : null,
                                  FileName = h.FileName,
                                  DateTimeCreated = h.DateTimeCreated,
                                  Title = h.Title,
                                  Description = h.Description,
                                  RectAreasViewer = isEditor ? null : h?.RectAreasViewer,
                                  RectAreasEditor = isEditor ? h?.RectAreasEditor : null,
                                  Visible = h.Visible,
                                  HashFolder = h.HashFolder
                              })?.ToList();                    
                            
                if(c != null)
                {
                    var res = new Books()
                    {
                        BookId = c.BookId,
                        BookName = c.BookName,                        
                        Tags = c.Tags,
                        DateTimeCreated = c.DateTimeCreated,
                        Description = c.Description,
                        Pages = _pagesList,
                        Visible = c.Visible,
                        HashFolder = c.HashFolder,
                        PdfFile = c.PdfFile,
                        Comment = c.Comment,
                        IsRemoved = c.IsRemoved,
                        ProgressCompleteValue = c.ProgressCompleteValue,
                        UserExpert = c.UserExpert!= null ? new User.User { Username = c?.UserExpert.Username, UserId = c.UserExpert.UserId }:null,
                        UserIdCreator = c?.UserIdCreator!=null ? new User.User { Username = c?.UserIdCreator.Username, UserId = c.UserIdCreator.UserId } :null,
                        Attributes = c?.Attributes,
                        AttributesId = c?.AttributesId,
                        BookLanguages = c?.BookLanguages,
                        isRtl = c.isRtl.HasValue ? c.isRtl.Value:false,
                        BookCoverFileName = c?.BookCoverFileName
                        
                    };
                    return res;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Является ли книга сборником сочинений
        /// </summary>
        /// <returns></returns>
        public async Task<List<BooksJson>> getCollectionData(Books Book)
        {
            List<BooksJson> bookJson = new List<BooksJson>();
            if (Book!=null && Book.Attributes!=null)
            {
                if(Book.Attributes.isCollectedBooks.HasValue)
                {
                    if(Book.Attributes.isCollectedBooks.Value && !string.IsNullOrEmpty(Book.Attributes.InventoryCollectedBook))
                    {
                        //Выполняем поиск книг с инв. номером сборника
                        string inventoryCollectedBookNumber = Book.Attributes.InventoryCollectedBook;
                        var resFinderSort = (from b in await Db?.Books?.AsNoTracking()?.ToListAsync()
                                   where b.Attributes != null
                                   &&
                                   !b.IsRemoved                                   
                                   &&
                                   (b.Attributes.isCollectedBooks.HasValue && !string.IsNullOrEmpty(b.Attributes.InventoryCollectedBook) && b.Attributes.InventoryCollectedBook == inventoryCollectedBookNumber)
                                   select b)?.OrderBy(c=>c.BookId)?.ToList();

                        if(resFinderSort!=null && resFinderSort.Any())
                        {
                            bookJson = await getFilteredResult(new SearchForm(), resFinderSort);
                        }
                    }
                }
                return bookJson;
            }
            return null;
        }

        public async Task<List<UsersForControlBook>> getUsersForControlBook()
        {
            var activeUsers = await Db.Users?.Where(c => c.IsActive && c.Roles.Any())?.ToListAsync();
            List<UsersForControlBook> res = new List<UsersForControlBook>();
            if (activeUsers!=null && activeUsers.Any())
            {
                res = activeUsers?.Select(c => new UsersForControlBook
                {
                    UserId = c.UserId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    UserName = c.Username,
                    Roles = string.Join(",", c.Roles.Select(c => c.RoleName))
                })?.ToList();
            }
            return res;
        }

        //Состояния
        public async Task<List<States>> getStatesList()
        {
            var res = await _context?.States?.ToListAsync();
            return res;
        }

        //Языки
        public async Task<List<LanguageTableList>> getLanguagesList()
        {
            var res = await _context?.LanguageTableList?.ToListAsync();
            return res;
        }

        //Тематики
        public async Task<List<ThemesLanguagesList>> getThemeLanguagesList()
        {
            var res = await _context?.ThemesLanguagesList?.ToListAsync();
            return res;
        }

        //Типы
        public async Task<List<TypesLanguagesList>> getTypesLanguagesList()
        {
            var res = await _context?.TypesLanguagesList?.ToListAsync();
            return res;
        }

        //Коллекции
        public async Task<List<Collection>> getCollectionsList()
        {
            var res = await _context?.Collections?.ToListAsync();
            return res;
        }

        //Регионы
        public async Task<List<Regions>> getRegionsList()
        {
            var res = await _context?.Regions?.ToListAsync();
            return res;
        }

        private async Task uploadFileMethod(Books book, List<HttpPostedFileBase> files)
        {
            using (var Db = new ModelAppBaseMillikitap())
            {
                if (!Directory.Exists(book.HashFolder))
                {
                    Directory.CreateDirectory(book.HashFolder);
                }
                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        if (isImageExtension(file.FileName))
                        {
                            string previewFileName = null;
                            try
                            {
                                previewFileName = getPreviewImage(file, book.HashFolder, file.FileName);
                            }
                            catch
                            {

                            }
                            try
                            {
                                file.SaveAs($"{book.HashFolder}{file.FileName}");
                                Db.Pages.Add(new Pages
                                {
                                    Book = book,
                                    FileName = file.FileName,
                                    FileNamePreview = previewFileName,
                                    DateTimeCreated = DateTime.Now,
                                    Visible = true
                                });
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
                await Db.SaveChangesAsync();
            }
        }
        
        private void uploadBookFilePDF(string path, Books bookSave, HttpPostedFileBase file)
        {
            //проверка на прикрепленный документ
            if (file != null)
            {                
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                try
                {                    
                    file.SaveAs($"{path}{file.FileName}");
                    bookSave.PdfFile = file.FileName;
                }
                catch
                {

                }
            }
        }
        
        private List<float> getLocation (string geolocation)
        {
            List<float> loc = new List<float>();
            loc.Add(0); //Lat
            loc.Add(0); //Lon
            try
            {
                if(!string.IsNullOrEmpty(geolocation))
                {
                    var loc1 = geolocation?.Trim()?.Split(',');
                    loc[0] = float.Parse(loc1[0].Trim()?.Replace(".",","));
                    loc[1] = float.Parse(loc1[1].Trim()?.Replace(".", ","));
                    return loc;
                }
                return loc;
            }
            catch
            {                
                return loc;
            }
        }

        //get bool flag by string //on//off
        public static bool getCheckedStateByString(string stateString)
        {
            bool isTrue = false;
            if (!string.IsNullOrEmpty(stateString))
            {
                if (stateString.ToLower() == "on" || stateString.ToLower() == "true")
                {
                    isTrue = true;
                }
            }
            return isTrue;
        }

        public async Task<Books> SaveBook (CustomAuthentication.CustomMembershipUser user, bool visible, SaveModelBook model, bool isEditor)
        {
            try
            {                
                if (model.BookId.HasValue)
                {                        
                    using (var Db = new ModelAppBaseMillikitap())
                    {
                        var book = await Db.Books?.Where(b => b.BookId == model.BookId.Value)?.FirstOrDefaultAsync();
                        book.BookName = model.bookName;                        
                        book.Description = model.description;
                        book.Tags = model.tags;
                        book.Visible = visible;
                        book.ProgressCompleteValue = model.progress;
                        book.Comment = model.comment;
                        //isRtl Boolean
                        book.isRtl = getCheckedStateByString(model.isRtl);
                        if (model.userId.HasValue)
                        {
                            if(book.UserExpert.UserId!= model.userId.Value)
                            {
                                //заменяем
                                var userExpertNew = await Db?.Users.SqlQuery($"select * from [Users] where [UserId] = {model.userId.Value}")?.FirstOrDefaultAsync();
                                book.UserExpert = userExpertNew;
                            }                            
                        }

                        if (model.fileBook!=null)
                        {
                            string path = HttpContext.Current.Server.MapPath($"~/Books/{book.HashFolder}/");
                            uploadBookFilePDF(path, book, model.fileBook);
                            await addLog(user.UserId, LogType.UploadFilePDF, book, null);
                        }

                        //атрибуты
                        if(book.Attributes==null)
                        {
                            book.Attributes = new Attributes();
                        }

                        //Перезаписываем
                        book.Attributes.Inventory = model.Inventory;
                        book.Attributes.Width = model?.Width;
                        book.Attributes.Height = model?.Height;
                        book.Attributes.PageCount = model?.PageCount;

                        book.Attributes.StartDateScan = model?.StartDateScan;
                        book.Attributes.EndDateScan = model?.EndDateScan;
                        book.Attributes.FIOScanner = model?.FIOScanner;
                        book.Attributes.PercentWorkScanner = model?.PercentWorkScanner;

                        book.Attributes.StartDateScanProcessing = model?.StartDateScanProcessing;
                        book.Attributes.EndDateScanProcessing = model?.EndDateScanProcessing;
                        book.Attributes.FIOProcessing = model.FIOProcessing;
                        book.Attributes.PercentWorkProcessing = model?.PercentWorkProcessing;


                        book.Attributes.StartDateDescriptionProcessing = model?.StartDateDescriptionProcessing;
                        book.Attributes.EndDateDescriptionProcessing = model?.EndDateDescriptionProcessing;
                        book.Attributes.FIODescriptionProcessing = model.FIODescriptionProcessing;
                        book.Attributes.PercentWorkDescr = model?.PercentWorkDescr;

                        book.Attributes.StatesId = model?.StatesId;

                        book.Attributes.ParametersPaper = model.ParametersPaper;
                        book.Attributes.NameOfBookArabic = model.NameOfBookArabic;
                        book.Attributes.NameOfBookCyrillic = model.NameOfBookCyrillic;
                        book.Attributes.NameOfBookEnglish = model.NameOfBookEnglish;
                        book.Attributes.AuthorOfArabic = model.AuthorOfArabic;
                        book.Attributes.AuthorOfCyrillic = model.AuthorOfCyrillic;
                        book.Attributes.AuthorOfEnglish = model.AuthorOfEnglish;

                        book.Attributes.ThemesLanguagesListId = model.ThemesLanguagesListId;
                        book.Attributes.TypesLanguagesListId = model.TypesLanguagesListId;                        

                        book.Attributes.DateOfCorrespondenceGregorianCalendar = model.DateOfCorrespondenceGregorianCalendar;
                        book.Attributes.DateOfCorrespondenceMusulmanCalendar = model.DateOfCorrespondenceMusulmanCalendar;
                        book.Attributes.CorrespondenceArabic = model.CorrespondenceArabic;
                        book.Attributes.CorrespondenceCyrillic = model.CorrespondenceCyrillic;
                        book.Attributes.CorrespondenceEnglish = model.CorrespondenceEnglish;
                        book.Attributes.PlaceCorrespondenceArabic = model.PlaceCorrespondenceArabic;
                        book.Attributes.PlaceCorrespondenceTatar = model.PlaceCorrespondenceTatar;
                        book.Attributes.PlaceCorrespondenceRussian = model.PlaceCorrespondenceRussian;
                        book.Attributes.PlaceCorrespondenceEnglish = model.PlaceCorrespondenceEnglish;

                        book.Attributes.PlaceCorrespondenceLat = getLocation(model.PlaceCorrespondenceLatLonString)[0];
                        book.Attributes.PlaceCorrespondenceLon = getLocation(model.PlaceCorrespondenceLatLonString)[1];
                                                
                        book.Attributes.LocalityOffDeliveryTatar = model.LocalityOffDeliveryTatar;
                        book.Attributes.LocalityOffDeliveryRussian = model.LocalityOffDeliveryRussian;
                        book.Attributes.LocalityOffDeliveryEnglish = model.LocalityOffDeliveryEnglish;

                        book.Attributes.LocalityOffDeliveryLat = getLocation(model.LocalityOffDeliveryLatLonString)[0];
                        book.Attributes.LocalityOffDeliveryLon = getLocation(model.LocalityOffDeliveryLatLonString)[1];
                          
                        //Если эксперт
                        if(book.UserExpert!=null)
                        {
                            if(book.UserExpert.UserId == user.UserId)
                            {
                                book.Attributes.CommentExpert = model.CommentExpert;
                            }
                        }

                        //Если редактор
                        if (isEditor)
                        {
                            book.Attributes.CommentEditor = model.CommentEditor;
                        }
                        
                        book.Attributes.ThemesLanguagesListId = model.ThemesLanguagesListId;
                        book.Attributes.TypesLanguagesListId = model.TypesLanguagesListId;

                        book.Attributes.RegionsId = model.RegionsId;

                        book.Attributes.ArabicDateText = model.ArabicDateText;

                        //Параметр сборника сочинений
                        var isCollectedBooks = getCheckedStateByString(model.isCollectedBooks);
                        if(isCollectedBooks)
                        {
                            book.Attributes.isCollectedBooks = true;
                            book.Attributes.InventoryCollectedBook = model?.InventoryCollectedBook;
                            book.Attributes.CollectedNameText = model?.CollectedNameText;
                        }
                        else
                        {
                            book.Attributes.isCollectedBooks = null;
                            book.Attributes.InventoryCollectedBook = null;
                            book.Attributes.CollectedNameText = null;
                        }

                        book.Attributes.CollectionId = model?.CollectionId;

                        //Содержит экстримисткое содержание
                        var isExrimistStatements = getCheckedStateByString(model.isExrimistStatements);
                        if (isExrimistStatements)
                        {
                            await MoveBookToExtremistPathDirectory(book);
                            book.Attributes.isExrimistStatements = true;
                            book.Attributes.exrimistStatementsText = model?.exrimistStatementsText;                            
                        }
                        else
                        {
                            book.Attributes.isExrimistStatements = null;
                            book.Attributes.exrimistStatementsText = string.Empty;
                            await ReturnBookFromExtremistPathDirectory(book);
                        }

                        //Языки
                        if (model.LanguageId!=null && model.LanguageId.Any())
                        {
                            if (book.BookLanguages != null && book.BookLanguages.Any())
                            {
                                var currLang = await Db?.BookLanguages?.Where(c => c.Book.BookId == book.BookId)?.ToListAsync();
                                if(currLang!=null && currLang.Any())
                                {
                                    Db.BookLanguages.RemoveRange(currLang);
                                    await Db.SaveChangesAsync();
                                }                                
                            }
                            else
                            {
                                book.BookLanguages = new List<BookLanguages>();
                            }
                            foreach (var l in model.LanguageId)
                            {
                                book.BookLanguages.Add(new BookLanguages
                                {
                                    BookId = book.BookId,
                                    LanguageTableListsId = l.Value
                                });
                            }
                        }
                        else
                        {
                            //clear langList
                            if (book.BookLanguages != null && book.BookLanguages.Any())
                            {
                                var currLang = await Db?.BookLanguages?.Where(c => c.Book.BookId == book.BookId)?.ToListAsync();
                                if (currLang != null && currLang.Any())
                                {
                                    Db.BookLanguages.RemoveRange(currLang);
                                    await Db.SaveChangesAsync();
                                }
                            }
                        }
                        
                        await Db.SaveChangesAsync();
                        await addLog(user.UserId, LogType.ChangeBook, book, null);
                        return book;
                    }
                }
               else
                {
                    if (!model.userId.HasValue)
                    {
                        model.userId = user.UserId;
                    }
                    var userDefaultExpert = await Db?.Users.SqlQuery($"select * from [Users] where [UserId] = {model.userId.Value}")?.FirstOrDefaultAsync();
                    var userCreatorExpert = await Db?.Users.SqlQuery($"select * from [Users] where [UserId] = {user.UserId}")?.FirstOrDefaultAsync();
                    var bookSave = Db.Books.Add(new Books
                    {
                        BookName = model.bookName,
                        DateTimeCreated = DateTime.Now,                        
                        Description = model.description,
                        Tags = model.tags,
                        Visible = visible,
                        ProgressCompleteValue = model.progress,
                        Comment = model.comment,
                        isRtl = getCheckedStateByString(model.isRtl),
                        UserExpert = userDefaultExpert,
                        UserIdCreator = userCreatorExpert,
                        IsRemoved = false,
                        Attributes = new Attributes()
                        {
                            Inventory = model?.Inventory,
                            Width = model?.Width,
                            Height = model?.Height,
                            PageCount = model?.PageCount,

                            StartDateScan = model?.StartDateScan,
                            EndDateScan = model?.EndDateScan,
                            FIOScanner = model?.FIOScanner,
                            PercentWorkScanner = model?.PercentWorkScanner,

                            StartDateScanProcessing = model?.StartDateScanProcessing,
                            EndDateScanProcessing = model?.EndDateScanProcessing,
                            FIOProcessing = model?.FIOProcessing,
                            PercentWorkProcessing = model?.PercentWorkProcessing,


                            StartDateDescriptionProcessing = model?.StartDateDescriptionProcessing,
                            EndDateDescriptionProcessing = model?.EndDateDescriptionProcessing,
                            FIODescriptionProcessing = model?.FIODescriptionProcessing,
                            PercentWorkDescr = model?.PercentWorkDescr,

                            StatesId = model?.StatesId,

                            ParametersPaper = model?.ParametersPaper,
                            NameOfBookArabic = model?.NameOfBookArabic,
                            NameOfBookCyrillic = model?.NameOfBookCyrillic,
                            NameOfBookEnglish = model?.NameOfBookEnglish,
                            AuthorOfArabic = model?.AuthorOfArabic,
                            AuthorOfCyrillic = model?.AuthorOfCyrillic,
                            AuthorOfEnglish = model?.AuthorOfEnglish,
                            ThemesLanguagesListId = model?.ThemesLanguagesListId,
                            TypesLanguagesListId = model?.TypesLanguagesListId,
                            DateOfCorrespondenceGregorianCalendar = model?.DateOfCorrespondenceGregorianCalendar,
                            DateOfCorrespondenceMusulmanCalendar = model?.DateOfCorrespondenceMusulmanCalendar,
                            CorrespondenceArabic = model?.CorrespondenceArabic,
                            CorrespondenceCyrillic = model?.CorrespondenceCyrillic,
                            CorrespondenceEnglish = model?.CorrespondenceEnglish,
                            PlaceCorrespondenceArabic = model?.PlaceCorrespondenceArabic,
                            PlaceCorrespondenceTatar = model?.PlaceCorrespondenceTatar,
                            PlaceCorrespondenceRussian = model?.PlaceCorrespondenceRussian,
                            PlaceCorrespondenceEnglish = model?.PlaceCorrespondenceEnglish,

                            PlaceCorrespondenceLat = getLocation(model?.PlaceCorrespondenceLatLonString)[0],
                            PlaceCorrespondenceLon = getLocation(model?.PlaceCorrespondenceLatLonString)[1],

                            LocalityOffDeliveryTatar = model?.LocalityOffDeliveryTatar,
                            LocalityOffDeliveryRussian = model?.LocalityOffDeliveryRussian,
                            LocalityOffDeliveryEnglish = model?.LocalityOffDeliveryEnglish,

                            LocalityOffDeliveryLat = getLocation(model?.LocalityOffDeliveryLatLonString)[0],
                            LocalityOffDeliveryLon = getLocation(model?.LocalityOffDeliveryLatLonString)[1],

                            CommentExpert = model?.CommentExpert,
                            CommentEditor = model?.CommentEditor,

                            RegionsId = model?.RegionsId,
                            ArabicDateText = model?.ArabicDateText,

                            CollectionId = model?.CollectionId
                        }
                    });

                    //Параметр сборника сочинений
                    var isCollectedBooks = getCheckedStateByString(model.isCollectedBooks);
                    if (isCollectedBooks)
                    {
                        bookSave.Attributes.isCollectedBooks = true;
                        bookSave.Attributes.InventoryCollectedBook = model?.InventoryCollectedBook;
                        bookSave.Attributes.CollectedNameText = model?.CollectedNameText;
                    }

                    //Содержит экстримисткое содержание
                    var isExrimistStatements = getCheckedStateByString(model.isExrimistStatements);
                    if (isExrimistStatements)
                    {
                        await MoveBookToExtremistPathDirectory(bookSave);
                        bookSave.Attributes.isExrimistStatements = true;
                        bookSave.Attributes.exrimistStatementsText = model?.exrimistStatementsText;                        
                    }
                    else
                    {
                        bookSave.Attributes.isExrimistStatements = null;
                        bookSave.Attributes.exrimistStatementsText = string.Empty;
                        await ReturnBookFromExtremistPathDirectory(bookSave);
                    }

                    //save book
                    await Db.SaveChangesAsync();
                    //save path
                    string namePath = $"{new Random().Next(0,6000)}_{bookSave.BookId}";
                    string hashFolder = CreateMD5(namePath);
                    bookSave.HashFolder = hashFolder;

                    string path = HttpContext.Current.Server.MapPath($"~/Books/{bookSave.HashFolder}/");
                    uploadBookFilePDF(path, bookSave, model.fileBook);

                    await Db.SaveChangesAsync();
                    await addLog(user.UserId, LogType.AddBook, bookSave,null);
                    return bookSave;
                }
            }
            catch (Exception ex)
            {
                Books operationInfo = new Books()
                {
                    OperationInfo = ex.Message
                };
                return operationInfo;
            }
        }


        /// <summary>
        /// Переименование книги >> в закрытую директорию
        /// </summary>
        /// <param name="bookModel"></param>
        /// <returns></returns>
        public async Task<bool> MoveBookToExtremistPathDirectory(Books bookModel)
        {
            if (bookModel.AttributesId.HasValue)
            {
                //Получаем текущую директорию
                string currentDirectory = bookModel.HashFolder;
                string currentDirectoryserverFolder = HttpContext.Current.Server.MapPath($"~/Books/{currentDirectory}/");
                //Проверяем на текущую директорию хранения книги
                if (Directory.Exists(currentDirectoryserverFolder))
                {
                    if (!bookModel.Attributes.isExrimistStatements.HasValue || !bookModel.HashFolder.Contains("_"))
                    {
                        //Генерация нового адреса директории
                        //К старому адресу добавляем новый адрес >> @{0}_{1},старый,новый
                        string namePath = CreateMD5(bookModel.BookId.ToString() + (DateTime.Now.Ticks.ToString() + new Random().Next(0, 100000)));
                        string newPathDirectory = $"{currentDirectory}_{CreateMD5(namePath)}";

                        string sourceDirectory = currentDirectoryserverFolder;
                        string destionationDirectory = HttpContext.Current.Server.MapPath($"~/Books/{newPathDirectory}");

                        Directory.Move(sourceDirectory, destionationDirectory);
                        bookModel.HashFolder = newPathDirectory;
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Переименование книги >> возвращение книги в публичный доступ
        /// </summary>
        /// <param name="bookModel"></param>
        /// <returns></returns>
        public async Task<bool> ReturnBookFromExtremistPathDirectory(Books bookModel)
        {
            if (bookModel.AttributesId.HasValue)
            {
                //Получаем текущую директорию
                string currentDirectory = bookModel.HashFolder;
                string currentDirectoryserverFolder = HttpContext.Current.Server.MapPath($"~/Books/{currentDirectory}/");
                //Проверяем на текущую директорию хранения книги
                if (Directory.Exists(currentDirectoryserverFolder))
                {
                    if (bookModel.HashFolder.Contains("_"))
                    {
                        //Конкатенация до _ старого адреса добавляем новый адрес >> @{0}_{1},старый оставляем, >> новый убираем                
                        string oldPathDirectory = bookModel.HashFolder.Split('_')[0];
                        string destionationDirectory = HttpContext.Current.Server.MapPath($"~/Books/{oldPathDirectory}");

                        Directory.Move(currentDirectoryserverFolder, destionationDirectory);
                        bookModel.HashFolder = oldPathDirectory;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Дубликат книги
        /// </summary>
        /// <param name="bookId"></param>
        /// <returns>BookId</returns>
        public async Task<int?> CopyBook(int bookId, bool copyDir, CustomAuthentication.CustomMembershipUser user)
        {
            try
            {
                Books bookEntity = null;
                if(copyDir)
                {
                    bookEntity = await _context.Books
                    .AsNoTracking()
                    .Include(x => x.Attributes)
                    .Include(x => x.BookLanguages)
                    .Include(x => x.Pages)
                    .FirstOrDefaultAsync(x => x.BookId == bookId);
                }
                else
                {
                    bookEntity = await _context.Books
                    .AsNoTracking()
                    .Include(x => x.Attributes)
                    .Include(x => x.BookLanguages)                    
                    .FirstOrDefaultAsync(x => x.BookId == bookId);
                }

                if(bookEntity != null)
                {
                    bookEntity.BookName = string.Format("{0} ({1})", bookEntity?.BookName, "копия");
                    bookEntity.DateTimeCreated = DateTime.Now;
                    bookEntity.Visible = false;                
                                        
                    if (copyDir)
                    {
                        var currHashDir = bookEntity?.HashFolder;
                        if (!string.IsNullOrEmpty(currHashDir))
                        {
                            string path = HttpContext.Current.Server.MapPath($"~/Books/{currHashDir}/");
                            if (Directory.Exists(path))
                            {
                                //создаем новую директорию
                                string namePath = $"{bookEntity.DateTimeCreated.Value.Ticks}_{new Random().Next(0, 6000)}_{bookEntity.BookId}";
                                string hashFolder = CreateMD5(namePath);
                                string serverNewFolder = HttpContext.Current.Server.MapPath($"~/Books/{hashFolder}/");
                                if (!Directory.Exists(serverNewFolder))
                                {
                                    Directory.CreateDirectory(serverNewFolder);
                                }

                                //Копируем файлы из старой директории -> в новую
                                foreach (var file in Directory.GetFiles(HttpContext.Current.Server.MapPath($"~/Books/{currHashDir}/")))
                                {
                                    FileInfo fi = new FileInfo(file);
                                    File.Copy(file, serverNewFolder + "//" + fi.Name,true);
                                }
                                //присваиваем новый сетевой путь книге
                                bookEntity.HashFolder = hashFolder;
                            }
                            else
                            {
                                bookEntity.HashFolder = null;
                                bookEntity.BookCoverFileName = null;
                            }
                        }
                    }
                    else
                    {
                        //создаем/резервируем новую директорию для книги
                        string namePath = $"{bookEntity.DateTimeCreated.Value.Ticks}_{new Random().Next(0, 6000)}_{bookEntity.BookId}";
                        string hashFolder = CreateMD5(namePath);
                        string serverNewFolder = HttpContext.Current.Server.MapPath($"~/Books/{hashFolder}/");
                        if (!Directory.Exists(serverNewFolder))
                        {
                            Directory.CreateDirectory(serverNewFolder);
                        }
                        bookEntity.HashFolder = hashFolder;
                        bookEntity.BookCoverFileName = null;
                    }

                    var copyBook = _context.Books.Add(bookEntity);
                    await _context.SaveChangesAsync();

                    //Изменяем Id создателя
                    await Db.Database.ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction, $"update Books set UserIdCreator_UserId = {user.UserId} where BookId = {copyBook.BookId}");
                    //Изменяем Id эксперта
                    await Db.Database.ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction, $"update Books set UserExpert_UserId = {user.UserId} where BookId = {copyBook.BookId}");
                    return copyBook.BookId;
                }
                return 404;
            }
            catch
            {
                return 500;
            }
        }

        private static bool isImageExtension(string filename)
        {
            return MimeMapping.GetMimeMapping(filename).StartsWith("image/");
        }

        private string getBase64Image (byte[]bytes)
        {            
            string fileAsString = Convert.ToBase64String(bytes);
            return fileAsString;
        }

        private string getPreviewImage(HttpPostedFileBase file, string path, string fileName)
        {
            try
            {
                string name = null;
                var versions = new Dictionary<string, string>();               
                versions.Add("small_", "maxwidth=200&maxheight=300&format=jpg");
                foreach (var suffix in versions.Keys)
                {
                    file.InputStream.Seek(0, SeekOrigin.Begin);
                    name = suffix+fileName;
                    string previewFileName = path + name;
                    ImageBuilder.Current.Build(
                        new ImageJob(
                            file.InputStream,
                            previewFileName,
                            new Instructions(versions[suffix]),
                            false,
                            false));
                }
                return name;
            }
            catch
            {
                return null;
            }
        }

        
        /// <summary>
        /// Загрузка изображений
        /// </summary>
        /// <param name="upload_imgs"></param>
        /// <param name="i">BookId</param>
        /// <returns></returns>
        public async Task<jsonSaveResult> uploadFiles(CustomAuthentication.CustomMembershipUser user,IEnumerable<HttpPostedFileBase> upload_imgs, int i)
        {
            jsonSaveResult result = new jsonSaveResult();
            try
            {   
                using (var Db = new ModelAppBaseMillikitap())
                {
                    var book = await Db?.Books?.Where(c => c.BookId == i)?.FirstOrDefaultAsync();
                    if (book == null)
                    {
                        result.Result = false;
                        result.exception = "Книга не найдена. Сохраните книгу и повторите загрузку.";
                        return result;
                    }
                    if (string.IsNullOrWhiteSpace(book.HashFolder))
                    {
                        result.Result = false;
                        result.exception = "У книги нет папки для файлов. Сохраните книгу и повторите загрузку.";
                        return result;
                    }
                    string path = HttpContext.Current.Server.MapPath($"~/Books/{book.HashFolder}/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    List<string> pagesSeparators = new List<string>();
                    List<string> errors = new List<string>();
                    int saved = 0;
                    foreach (var file in upload_imgs ?? Enumerable.Empty<HttpPostedFileBase>())
                    {
                        if (file == null || file.ContentLength <= 0)
                        {
                            errors.Add("Пустой файл");
                            continue;
                        }
                        if (!isImageExtension(file.FileName))
                        {
                            errors.Add($"«{file.FileName}» не является изображением");
                            continue;
                        }
                        string previewFileName = null;
                        string fileName = CreateMD5(Path.GetFileName(file.FileName));
                        string extension = Path.GetExtension(file.FileName);
                        string cryptFilename = string.Format("{0}{1}", fileName, extension);

                        try
                        {
                            previewFileName = getPreviewImage(file, path, cryptFilename);
                        }
                        catch
                        {
                        }
                        try
                        {
                            if (file.InputStream != null && file.InputStream.CanSeek)
                            {
                                file.InputStream.Seek(0, SeekOrigin.Begin);
                            }
                            file.SaveAs($"{path}{cryptFilename}");
                            pagesSeparators.Add($"{path}{cryptFilename}");
                            Db.Pages.Add(new Pages
                            {
                                Book = book,
                                FileName = cryptFilename,
                                FileNamePreview = previewFileName,
                                DateTimeCreated = DateTime.Now,
                                Visible = true
                            });
                            saved++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"«{file.FileName}»: {ex.Message}");
                        }
                    }
                    if (saved == 0)
                    {
                        result.Result = false;
                        result.exception = errors.Any()
                            ? string.Join("; ", errors)
                            : "Ни один файл не сохранён";
                        return result;
                    }
                    await Db.SaveChangesAsync();
                    try
                    {
                        await addLog(user.UserId, LogType.AddPage, book, string.Join(",", pagesSeparators));
                    }
                    catch
                    {
                    }
                    result.Result = true;
                    if (errors.Any())
                    {
                        result.exception = "Часть файлов не сохранена: " + string.Join("; ", errors);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.exception = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Загрузка/Распаковка ZIP архива
        /// </summary>
        /// <param name="user"></param>
        /// <param name="upload_imgs"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public async Task<jsonSaveResult> uploadZIPFile(CustomAuthentication.CustomMembershipUser user, ICollection<HttpPostedFileBase> upload_zip, int i)
        {
            jsonSaveResult result = new jsonSaveResult();
            try
            {
                using (var Db = new ModelAppBaseMillikitap())
                {
                    var book = await Db?.Books?.Where(c => c.BookId == i)?.FirstOrDefaultAsync();
                    if (book == null || string.IsNullOrWhiteSpace(book.HashFolder))
                    {
                        result.Result = false;
                        result.exception = "Книга не найдена или у неё нет папки для файлов. Сохраните книгу и повторите загрузку.";
                        return result;
                    }
                    string path = HttpContext.Current.Server.MapPath($"~/Books/{book.HashFolder}/");
                    //Iamge directory
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    //Zip temp directory
                    string pathZIPtemp = path + "ZipTemp";
                    if (!Directory.Exists(pathZIPtemp))
                    {
                        Directory.CreateDirectory(pathZIPtemp);
                    }

                    List<string> pagesSeparators = new List<string>();
                    int saved = 0;
                    List<string> errors = new List<string>();

                    foreach (var fileZip in upload_zip)
                    {
                        var zipExtension = Path.GetExtension(fileZip?.FileName ?? string.Empty);
                        if (!string.Equals(zipExtension, ".zip", StringComparison.OrdinalIgnoreCase))
                        {
                            errors.Add("Поддерживается только архив ZIP, не 7z");
                            continue;
                        }
                        string nameZip = pathZIPtemp + "/" + Path.GetFileName(fileZip.FileName);
                        fileZip.SaveAs(nameZip);
                                                
                        using (ZipArchive archive = ZipFile.Open(nameZip, ZipArchiveMode.Read, Encoding.GetEncoding("cp866")))
                        {

                            var orderedEntries = from entry in archive.Entries 
                                                 select entry;

                                                           

                            //var name = orderedEntries?.Select(c => c.Name)?.ToList();
                            //var sorted = name.OrderBy(c => c)?.ToList();
                            //var test =orderedEntries?.Where(c => c.Name == "07.jpg")?.FirstOrDefault();
                            //Array.Sort(name?.ToArray());
                            //var t = name;
                            foreach (ZipArchiveEntry entry in orderedEntries)
                            {
                                if (!isImageExtension(entry.FullName))
                                {
                                    continue;
                                }
                                string previewFileName = null;
                                string fileName = CreateMD5(Path.GetFileName(entry.FullName));
                                string extension = Path.GetExtension(entry.FullName);
                                string cryptFilename = string.Format("{0}{1}", fileName, extension);
                                try
                                {
                                    entry.ExtractToFile($"{path}{cryptFilename}", true);
                                    byte[] bytes = File.ReadAllBytes($"{path}{cryptFilename}");
                                    previewFileName = getPreviewImage(new Helper.MemoryPostedFile(bytes), path, cryptFilename);

                                    pagesSeparators.Add($"{path}{cryptFilename}");                                    
                                    Db.Pages.Add(new Pages
                                    {
                                        Book = book,
                                        FileName = cryptFilename,
                                        FileNamePreview = previewFileName,
                                        DateTimeCreated = DateTime.Now,
                                        //BlobImage = $"data:image/jpeg;base64,{getBase64Image(bytes)}",
                                        Visible = true
                                    });
                                    saved++;
                                }
                                catch (Exception ex)
                                {
                                    errors.Add($"«{entry.FullName}»: {ex.Message}");
                                }
                            }
                        }
                    }                    
                    
                    if (saved == 0)
                    {
                        result.Result = false;
                        result.exception = errors.Any()
                            ? string.Join("; ", errors)
                            : "В архиве нет изображений";
                        return result;
                    }
                    await Db.SaveChangesAsync();
                    try
                    {
                        await addLog(user.UserId, LogType.AddPage, book, string.Join(",", pagesSeparators));
                    }
                    catch
                    {
                    }
                    if (errors.Any())
                    {
                        result.exception = "Часть файлов не сохранена: " + string.Join("; ", errors);
                    }

                    try
                    {
                        //clear temp Zip folder
                        if (Directory.Exists(pathZIPtemp))
                        {
                            //clear temp file
                            DirectoryInfo di = new DirectoryInfo(pathZIPtemp);
                            foreach (FileInfo file in di.GetFiles())
                            {
                                file.Delete();
                            }                            
                        }
                    }
                    catch
                    {

                    }
                }
                result.Result = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.exception = ex.Message;
                return result;
            }
        }

            public string getHtmlFromArea(string areaRectHTML)
            {            
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(areaRectHTML);
            HtmlNode node = doc.DocumentNode.SelectNodes("//area")?.FirstOrDefault();
            if (node != null)
            {
               return node?.Attributes["html"]?.Value;
            }
            return string.Empty;
        }

        public List<string> getAreasFromString(string html)
        {
            List<string> resAresNodes = new List<string>();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//area");
            if (nodes != null && nodes.Any())
            {
                foreach (HtmlNode node in nodes)
                {   
                    resAresNodes.Add(node.OuterHtml);
                }
            }
            return resAresNodes;
        }

        /// <summary>
        /// Удаление всех html тегов
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripHTML(string input)
        {
            if(string.IsNullOrEmpty(input))
            {
                return null;
            }
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(input.Replace("<p>"," ").Replace("</p>"," "));
            return htmlDoc.DocumentNode.InnerText?.Replace("&laquo;"," ").Replace("&nbsp;"," ").Replace("&mdash;", "—").Replace("&raquo;"," ");             
        }

        private List<CustomAttributes> getCustomAttributesByRect(string RectEditorSVGOriginal)
        {
            List<CustomAttributes> objValuesClass = new List<CustomAttributes>();
            JObject attrText = JObject.Parse(RectEditorSVGOriginal);
            var areas = attrText["areas"];
            if(areas!=null && areas.Any())
            {
                int i = 0;
                foreach(var a in areas)
                {
                    string attrItems = (string)a["attributes"]?.ToString();
                    JObject attrValues = JObject.Parse(attrItems);
                    objValuesClass.Add(new CustomAttributes
                    {
                        Order = i,
                        Person = (string)attrValues["person"],
                        Event = (string)attrValues["event"],
                        Place = (string)attrValues["place"],
                        Organization = (string)attrValues["organization"],
                        ArticleTitle = (string)attrValues["articleTitle"]
                    });
                    i++;
                }
            }            
            return objValuesClass;
        }

        public async Task<jsonSaveResult> saveChanges(int bookId, int pageId, string RectEditorSVGOriginal, string RectForViewerSVG, string BlobImage)
        {
            jsonSaveResult result = new jsonSaveResult();
            try
            {
                using (var Db = new ModelAppBaseMillikitap())
                {
                    Db.Configuration.LazyLoadingEnabled = false;

                    var book = await Db.Books?.Where(b => b.BookId == bookId)?.FirstOrDefaultAsync();
                    var page = await Db.Pages.SqlQuery($"select * from Pages where Id = {pageId} and Book_BookId = {bookId}")?.FirstOrDefaultAsync();

                    //if (!string.IsNullOrEmpty(BlobImage))
                    //{
                    //    page.BlobImage = BlobImage;
                    //}

                    var rectAreasViewer = await Db?.RectAreasViewer?.Where(p => p.PagesId == page.Id)?.ToListAsync();
                    var rectAreasEditor = await Db?.RectAreasEditor?.Where(p => p.PagesId == page.Id)?.ToListAsync();

                    //Удаляем все Rect Viewer
                    Db.RectAreasViewer.RemoveRange(rectAreasViewer);
                    //Удаляем все Rect Editor
                    Db.RectAreasEditor.RemoveRange(rectAreasEditor);

                    //var book = await Db.Books?.Where(b => b.BookId == bookId && b.Visible)?.Include(p => p.Pages)?.FirstOrDefaultAsync();
                    //var page = book.Pages?.Where(p => p.Id == pageId)?.FirstOrDefault();
                    //if (!string.IsNullOrEmpty(BlobImage))
                    //{
                    //    page.BlobImage = BlobImage;
                    //}
                    ////Удаляем все Rect Viewer
                    //Db.RectAreasViewer.RemoveRange(page.RectAreasViewer);
                    ////Удаляем все Rect Editor
                    //Db.RectAreasEditor.RemoveRange(page.RectAreasEditor);
                    //Получаем дополнительные атрибуты
                    List<CustomAttributes> attributes = getCustomAttributesByRect(RectEditorSVGOriginal)?.OrderByDescending(a=>a.Order)?.ToList();
                    //Добавляем Viewer
                    List<string> RectsViewer = new BLogicMillikitap().getAreasFromString(RectForViewerSVG);
                    if(RectsViewer!=null && RectsViewer.Any())
                    {
                        int i = 0;
                        foreach (var rect in RectsViewer)
                        {
                            string htmlCode = getHtmlFromArea(rect)?.Replace("*","\"");                            
                            Db.RectAreasViewer.Add(new RectAreasViewer
                            {
                                PagesId = pageId,
                                RectForViewerSVG = rect,
                                Description = htmlCode,
                                DescriptionWithoutHTML = StripHTML(htmlCode),
                                //Custom attributes
                                Person = attributes[i].Person,
                                Event = attributes[i].Event,
                                Place = attributes[i].Place,
                                Organization = attributes[i].Organization,
                                ArticleTitle = attributes[i].ArticleTitle
                            });
                            await saveAttributesToCatalog(attributes[i]);
                            i++;
                        }
                    }
                    if (!string.IsNullOrEmpty(RectEditorSVGOriginal))
                    {
                        //Добавляем Editor
                        Db.RectAreasEditor.Add(new RectAreasEditor
                        {
                            PagesId = pageId,
                            RectEditorSVGOriginal = RectEditorSVGOriginal
                        });
                    }

                    //Сохраняем blob в директорию и получаем имя файла
                    if (!string.IsNullOrEmpty(BlobImage))
                    {
                        string fileName = $"{page.Id}.jpg";
                        string path = HttpContext.Current.Server.MapPath("~/Books/" + book.HashFolder + "/");
                        byte[] contents = Convert.FromBase64String(BlobImage.Replace("data:image/jpeg;base64,", string.Empty));
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        File.WriteAllBytes(path + fileName, contents);
                        page.FileName = fileName;
                        
                        try
                        {
                            HttpPostedFileBase file = (HttpPostedFileBase)new MemoryPostedFile(contents);
                            string previewFileName = getPreviewImage(file, path, fileName);
                            page.FileNamePreview = previewFileName;
                        }
                        catch
                        {

                        }
                    }
                    await Db.SaveChangesAsync();
                    result.Result = true;
                    return result;
                };                
            }
            catch(Exception ex)
            {
                result.Result = false;
                result.exception = ex.Message;
                return result;
            }            
        }

        private async Task saveAttributesToCatalog(CustomAttributes attributes)
        {
            try
            {
                if(!string.IsNullOrEmpty(attributes.Person))
                {
                    var split = attributes.Person.Split(',');
                    foreach(var i in split)
                    {
                        var data = await Autocomplete(i.Trim(), EnumTypes.AutoEnum.Person);
                        if (!data.Any())
                        {
                            Db.PersonCatalog.Add(new PersonCatalog
                            {
                                Person = i
                            });
                        }
                    }                    
                }
                if (!string.IsNullOrEmpty(attributes.Event))
                {
                    var split = attributes.Event.Split(',');
                    foreach (var i in split)
                    {
                        var data = await Autocomplete(i, EnumTypes.AutoEnum.Event);
                        if (!data.Any())
                        {
                            Db.EventCatalog.Add(new EventCatalog
                            {
                                Event = i
                            });
                        }
                    }
                }
                if (!string.IsNullOrEmpty(attributes.Place))
                {
                    var split = attributes.Place.Split(',');
                    foreach (var i in split)
                    {
                        var data = await Autocomplete(i, EnumTypes.AutoEnum.Place);
                        if (!data.Any())
                        {
                            Db.PlaceCatalog.Add(new PlaceCatalog
                            {
                                Place = i
                            });
                        }
                    }
                }
                if (!string.IsNullOrEmpty(attributes.Organization))
                {
                    var split = attributes.Organization.Split(',');
                    foreach (var i in split)
                    {
                        var data = await Autocomplete(i, EnumTypes.AutoEnum.Organization);
                        if (!data.Any())
                        {
                            Db.OrganizationCatalog.Add(new OrganizationCatalog
                            {
                                Organization = i
                            });
                        }
                    }
                }                
                await Db.SaveChangesAsync();
            }
            catch
            {

            }
        }
        
        private SearchForm removeQuotes(SearchForm model)
        {
            //if(!string.IsNullOrEmpty(model.Text))
            //{
            //    model.Text.Replace("'", "\"");
            //}
            if (!string.IsNullOrEmpty(model.Tag))
            {
                model.Tag = model.Tag.Replace("'", @"""");
            }
            if (!string.IsNullOrEmpty(model.Place))
            {
                model.Place = model.Place.Replace("'", @"""");
            }
            if (!string.IsNullOrEmpty(model.Person))
            {
                model.Person = model.Person.Replace("'", @"""");
            }
            if (!string.IsNullOrEmpty(model.Organization))
            {
                model.Organization = model.Organization.Replace("'", @"""");
            }
            if (!string.IsNullOrEmpty(model.Name))
            {
                model.Name = model.Name.Replace("'", @"""");
            }
            return model;
        }


        public class TestClass
        {
            public int? bookId { get; set; }
            public string bookName { get; set; }
            public DateTime dtCreatedBook { get; set; }
            public DateTime dtPublishDate { get; set; }
            public string descrBook { get; set; }
            public string Tags { get; set; }
            public Boolean visibleBook { get; set; }
            public string HashFolder { get; set; }
            public string PdfFile { get; set; }
            public int? pageId { get; set; }
            public string pagesTitle { get; set; }
            public string FileName { get; set; }
            public string FileNamePreview { get; set; }
            public int? rectId { get; set; }
            public string rectDescr { get; set; }
            public string rectSVG { get; set; }
            public string rectHTMLWithoutHtml { get; set; }            
            public string Person { get; set; }
            public string Event { get; set; }
            public string Place { get; set; }
            public string Organization { get; set; }
            public string rectArticle { get; set; }
        }

        private bool isCustomSearch(SearchForm model)
        {
            if (!string.IsNullOrEmpty(model.Text) || !string.IsNullOrEmpty(model.Person) || !string.IsNullOrEmpty(model.Event) || !string.IsNullOrEmpty(model.Place) || !string.IsNullOrEmpty(model.Organization) || !string.IsNullOrEmpty(model.ArticleTitle))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Соответствует ли условию для начала поиска
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool isCustomSearchAttr(SearchForm model)
        {
            if(model.Attributes == null)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(model.Attributes.Inventory) 
                || model.Attributes.Width.HasValue 
                || model.Attributes.Height.HasValue 
                || model.Attributes.PageCount.HasValue 
                || model.Attributes.StartDateScan.HasValue 
                || model.Attributes.EndDateScan.HasValue 
                || model.Attributes.StatesId.HasValue 
                || !string.IsNullOrEmpty (model.Attributes.ParametersPaper) 
                || !string.IsNullOrEmpty(model.Attributes.NameOfBookArabic) 
                || !string.IsNullOrEmpty(model.Attributes.NameOfBookCyrillic) 
                || !string.IsNullOrEmpty(model.Attributes.NameOfBookEnglish) 
                || !string.IsNullOrEmpty(model.Attributes.AuthorOfArabic) 
                || !string.IsNullOrEmpty(model.Attributes.AuthorOfCyrillic) 
                || !string.IsNullOrEmpty(model.Attributes.AuthorOfEnglish) 
                || model.Attributes.ThemesLanguagesListId.HasValue 
                || model.Attributes.TypesLanguagesListId.HasValue)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// фильтр результата
        /// </summary>
        /// <returns></returns>
        private async Task<List<BooksJson>> getFilteredResult(SearchForm model, List<Books>BooksByDateFiltered)
        {
            List<Books> booksFilteredList = BooksByDateFiltered;//Номера по дате
            List<BooksJson> rectAreasViewersList = new List<BooksJson>();//Найденные фрагменты
            List<BooksJson> bookJson = null;//Результат обработки
            int booksFilteredCount = 0;
            removeQuotes(model);

            //Поиск расширенный без атрибутов
            bool isCustomSearchNoAttrFlag = isCustomSearch(model);
            //Поиск расширенный с атрибутами
            bool isCustomSearchAttrFlag = isCustomSearchAttr(model);

            if (booksFilteredList != null && booksFilteredList.Any())
            {
                //Название книги
                if (!string.IsNullOrEmpty(model.Name))
                {
                    booksFilteredList = booksFilteredList?.Where(c => utils.insensitiveContains(c.BookName, model.Name, StringComparison.OrdinalIgnoreCase))?.ToList();
                };
                //Тег
                if (!string.IsNullOrEmpty(model.Tag))
                {
                    booksFilteredList = booksFilteredList?.Where(c => utils.insensitiveContains(c.Tags, model.Tag, StringComparison.OrdinalIgnoreCase))?.ToList();
                };

                //Фильтр по атрибутам
                if (isCustomSearchAttrFlag)
                {   
                    if(!string.IsNullOrEmpty (model.Attributes.Inventory))
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes!=null && c.Attributes.Inventory != null && utils.insensitiveContains(c.Attributes.Inventory, model.Attributes.Inventory, StringComparison.OrdinalIgnoreCase))?.ToList();
                    }
                    if (model.Attributes.StatesId.HasValue)
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes != null && c.Attributes.StatesId.HasValue && c.Attributes.StatesId.Value == model.Attributes.StatesId.Value)?.ToList();
                    }
                    if (model.Attributes.Width.HasValue)
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes != null && c.Attributes.Width.HasValue && c.Attributes.Width.Value == model.Attributes.Width.Value)?.ToList();
                    }
                    if (model.Attributes.Height.HasValue)
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes != null && c.Attributes.Height.HasValue && c.Attributes.Height.Value == model.Attributes.Height.Value)?.ToList();
                    }
                    if (model.Attributes.TypesLanguagesListId.HasValue)
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes != null && c.Attributes.TypesLanguagesListId.HasValue && c.Attributes.TypesLanguagesListId.Value == model.Attributes.TypesLanguagesListId.Value)?.ToList();
                    }
                    if (model.Attributes.ThemesLanguagesListId.HasValue)
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes != null && c.Attributes.ThemesLanguagesListId.HasValue && c.Attributes.ThemesLanguagesListId.Value == model.Attributes.ThemesLanguagesListId.Value)?.ToList();
                    }

                    //Русский
                    if (!string.IsNullOrEmpty(model.Attributes.NameOfBookCyrillic))
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes != null && c.Attributes.NameOfBookCyrillic!=null && utils.insensitiveContains(c.Attributes.NameOfBookCyrillic, model.Attributes.NameOfBookCyrillic, StringComparison.OrdinalIgnoreCase))?.ToList();
                    }
                    if (!string.IsNullOrEmpty(model.Attributes.AuthorOfCyrillic))
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes != null && c.Attributes.AuthorOfCyrillic != null && utils.insensitiveContains(c.Attributes.AuthorOfCyrillic, model.Attributes.AuthorOfCyrillic, StringComparison.OrdinalIgnoreCase))?.ToList();
                    }                    
                    //Английский
                    if (!string.IsNullOrEmpty(model.Attributes.NameOfBookEnglish))
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes != null && c.Attributes.NameOfBookEnglish != null && utils.insensitiveContains(c.Attributes.NameOfBookEnglish, model.Attributes.NameOfBookEnglish, StringComparison.OrdinalIgnoreCase))?.ToList();
                    }
                    if (!string.IsNullOrEmpty(model.Attributes.AuthorOfEnglish))
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes != null && c.Attributes.AuthorOfEnglish != null && utils.insensitiveContains(c.Attributes.AuthorOfEnglish, model.Attributes.AuthorOfEnglish, StringComparison.OrdinalIgnoreCase))?.ToList();
                    }                    
                    //Арабский
                    if (!string.IsNullOrEmpty(model.Attributes.NameOfBookArabic))
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes != null && c.Attributes.NameOfBookArabic != null && utils.insensitiveContains(c.Attributes.NameOfBookArabic, model.Attributes.NameOfBookArabic, StringComparison.OrdinalIgnoreCase))?.ToList();
                    }
                    if (!string.IsNullOrEmpty(model.Attributes.AuthorOfArabic))
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes != null && c.Attributes.AuthorOfArabic != null && utils.insensitiveContains(c.Attributes.AuthorOfArabic, model.Attributes.AuthorOfArabic, StringComparison.OrdinalIgnoreCase))?.ToList();
                    }

                    if (model.Attributes.ThemesLanguagesListId.HasValue)
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes != null && c.Attributes.ThemesLanguagesListId != null && c.Attributes.ThemesLanguagesListId.Value == model.Attributes.ThemesLanguagesListId.Value)?.ToList();
                    }
                    if (model.Attributes.TypesLanguagesListId.HasValue)
                    {
                        booksFilteredList = booksFilteredList?.Where(c => c.Attributes != null && c.Attributes.TypesLanguagesListId != null && c.Attributes.TypesLanguagesListId.Value == model.Attributes.TypesLanguagesListId.Value)?.ToList();
                    }
                }

                booksFilteredCount = booksFilteredList.Count();

                List<string> exprList = new List<string>();                
                if (isCustomSearchNoAttrFlag)
                {
                    if (!string.IsNullOrEmpty(model.ArticleTitle))
                    //like N if Arabic $"ArticleTitle like N'%{model.ArticleTitle}%'"
                    {
                        exprList.Add($"[ArticleTitle] like N'%{model.ArticleTitle}%'");
                    }
                    if (!string.IsNullOrEmpty(model.Text))
                    {
                        exprList.Add($"[DescriptionWithoutHTML] like N'%{model.Text}%'");
                    }
                    if (!string.IsNullOrEmpty(model.Person))
                    {
                        exprList.Add($"[Person] like N'%{model.Person}%'");
                    }
                    if (!string.IsNullOrEmpty(model.Organization))
                    {
                        exprList.Add($"[Organization] like N'%{model.Organization}%'");
                    }
                    if (!string.IsNullOrEmpty(model.Event))
                    {
                        exprList.Add($"[Event] like N'%{model.Event}%'");
                    }
                    if (!string.IsNullOrEmpty(model.Place))
                    {
                        exprList.Add($"[Place] like N'%{model.Place}%'");
                    }

                    string formatSQL = string.Join(" or ", exprList);
                    string SQLExression = $"SELECT RectAreasViewerId,PagesId,RectForViewerSVG,r.Title,r.Description,r.DescriptionWithoutHTML,r.Tag,BlobFragment,Person,Event,Place Organization,ArticleTitle,p.Id,p.DateTimeCreated,BlobImage,FileName,b.Visible,Book_BookId,FileNamePreview, Inventory,Width,Height,StatesId,BookCoverFileName FROM Books as b Left JOIN Attributes as a ON a.AttributesId = b.AttributesId Left JOIN Pages as p ON p.Book_BookId = b.BookId Left JOIN RectAreasViewers as r ON r.PagesId = p.Id where b.Visible = 1 and p.Visible = 1 and {formatSQL}";                    
                    rectAreasViewersList = (from a in await _context.Database.SqlQuery<SearchResult>(SQLExression)?.ToListAsync()
                              from b in booksFilteredList
                              where b.BookId == a.Book_BookId
                              select new BooksJson
                              {
                                  BookId = b.BookId,                                  
                                  PagesList = b?.Pages.ToList(),
                                  BookName = b.BookName,
                                  Visible = b?.Visible,
                                  DateTimeCreatedDt = b.DateTimeCreated.Value,                                  
                                  Tags = b.Tags,
                                  HashFolder = b.HashFolder,                                  
                                  searchAttrRectArea = new List<searchAttrRectArea>(){
                                                    new searchAttrRectArea
                                                    {
                                                        BookId = b.BookId,
                                                        Name = a?.FileName,
                                                        HashFolder = b.HashFolder,
                                                        Preview = a?.FileNamePreview,
                                                        PagesId = a.PagesId,
                                                        FinderText = !string.IsNullOrEmpty(a.ArticleTitle)?a.ArticleTitle : !string.IsNullOrEmpty(model.Person) ? a.Person : !string.IsNullOrEmpty(model.Event) ? a.Event : !string.IsNullOrEmpty(model.Place) ? a.Place : !string.IsNullOrEmpty(model.Organization) ? a.Organization:string.Empty,
                                                        Data_Viewerid = a.RectAreasViewerId
                                                    }
                                           }
                              })?.ToList();
                    
                    if (rectAreasViewersList != null && rectAreasViewersList.Any())
                    {
                        var rectSearchResult = (from a in booksFilteredList
                                                from r in rectAreasViewersList
                                                where r.BookId == a.BookId
                                                select r.searchAttrRectArea)?.SelectMany(f => f)?.GroupBy(f => f.PagesId)?.Select(v => v.FirstOrDefault()).ToList();

                        bookJson = (from a in booksFilteredList?.OrderByDescending(c => c.DateTimeCreated)?.ToList()
                        from r in rectAreasViewersList
                                    where r.BookId == a.BookId
                                    group a by a.BookId into g
                                    select new BooksJson
                                    {
                                        BookId = g.Key,                                        
                                        BookName = g.FirstOrDefault().BookName,
                                        Visible = g.FirstOrDefault()?.Visible,
                                        Tags = g.FirstOrDefault().Tags,
                                        DateTimeCreated = g.FirstOrDefault().DateTimeCreated.Value.ToString("dd.MM.yyyy"),                                        
                                        //Description = ,
                                        FileNamePreview = !string.IsNullOrEmpty (g.FirstOrDefault()?.BookCoverFileName) ? g.FirstOrDefault()?.BookCoverFileName : g.FirstOrDefault()?.Pages?.FirstOrDefault()?.FileNamePreview,
                                        searchAttrRectArea = rectSearchResult?.Where(c => c.BookId == g.Key).ToList(),
                                        HashFolder = g.FirstOrDefault()?.HashFolder,
                                        CountAll = rectSearchResult.Count(),
                                        Description = getShortText(g?.FirstOrDefault()?.Description),

                                        //Доп. атрибуты
                                        //Идентификатор коллекции
                                        CollectionId = g.FirstOrDefault().Attributes != null ? g.FirstOrDefault().Attributes?.CollectionId : null,

                                        //Для группировки в сборник
                                        isCollectedBooks = g.FirstOrDefault().Attributes != null ? g.FirstOrDefault().Attributes.isCollectedBooks.HasValue ? g.FirstOrDefault().Attributes.isCollectedBooks.Value : false : false,
                                        InventoryCollectedBook = g.FirstOrDefault().Attributes != null ? !string.IsNullOrEmpty(g.FirstOrDefault().Attributes.InventoryCollectedBook) ? g.FirstOrDefault().Attributes.InventoryCollectedBook : "-1" : "-1",
                                        CollectedNameText = g.FirstOrDefault().Attributes != null ? g.FirstOrDefault().Attributes?.CollectedNameText : string.Empty,

                                        //Регион
                                        Region = g.FirstOrDefault().Attributes != null ? g.FirstOrDefault().Attributes?.Regions : null,
                                        //Состояние
                                        State = g.FirstOrDefault().Attributes != null ? g.FirstOrDefault().Attributes?.States : null,
                                        //Тип
                                        TypeId = g.FirstOrDefault().Attributes!=null ? g.FirstOrDefault().Attributes?.TypesLanguagesListId:null,
                                        //Тематика
                                        ThemeId = g.FirstOrDefault().Attributes != null ? g.FirstOrDefault().Attributes?.ThemesLanguagesListId : null,
                                        //Кол-во страниц
                                        PageCount = g.FirstOrDefault().Attributes != null ? g.FirstOrDefault().Attributes?.PageCount : null
                                    })?.ToList();                        

                        bookJson = bookJson?.ToList();
                    }
                }
                else
                {                    
                    bookJson = (from a in booksFilteredList                                
                                select new BooksJson
                                {
                                    BookId = a.BookId,                                 
                                    BookName = a.BookName,
                                    Visible = a?.Visible,
                                    Tags = a.Tags,
                                    DateTimeCreated = a.DateTimeCreated.Value.ToString("dd.MM.yyyy"),                                                                   
                                    FileNamePreview = !string.IsNullOrEmpty(a.BookCoverFileName) ? a.BookCoverFileName : null,
                                    searchAttrRectArea = null,
                                    HashFolder = a.HashFolder,
                                    CountAll = booksFilteredCount,
                                    Description = getShortText(a.Description),
                                    
                                    //Доп. атрибуты
                                    //Идентификатор коллекции
                                    CollectionId = a.Attributes != null ? a.Attributes?.CollectionId : null,

                                    //Для группировки в сборник
                                    isCollectedBooks = a.Attributes != null ? a.Attributes.isCollectedBooks.HasValue ? a.Attributes.isCollectedBooks.Value : false : false,
                                    InventoryCollectedBook = a.Attributes!=null ? !string.IsNullOrEmpty (a.Attributes.InventoryCollectedBook) ? a.Attributes.InventoryCollectedBook : "-1":"-1",
                                    CollectedNameText = a.Attributes!=null ? a.Attributes?.CollectedNameText:string.Empty,

                                    //Регион
                                    Region = a.Attributes!=null ? a.Attributes?.Regions:null,
                                    //Состояние
                                    State = a.Attributes!=null ? a.Attributes?.States:null,
                                    //Тип
                                    TypeId = a.Attributes != null ? a.Attributes?.TypesLanguagesListId : null,
                                    //Тематика
                                    ThemeId = a.Attributes != null ? a.Attributes?.ThemesLanguagesListId : null,
                                    //Кол-во страниц
                                    PageCount = a.Attributes != null ? a.Attributes?.PageCount : null

                                })?.ToList();
                }
                return bookJson;
            }
            return null;
        }

        //Все издания
        public async Task<List<BooksJson>> searchBooks(SearchForm model)
        {
            try
            {
                List<Books> booksFilteredList = new List<Books>();
                if(model.isAccessEditor)
                {
                    //Показываем все книги, в том числе и скрытые
                    booksFilteredList = (from a in await _context?.Books?.Include(p => p.Pages)?.Where(b => !b.IsRemoved).AsNoTracking()?.ToListAsync()
                                         select a)?.ToList();
                }
                else
                {
                    //Скрытые книги не показываем
                    booksFilteredList = (from a in await _context?.Books?.Include(p => p.Pages)?.Where(b => b.Visible && !b.IsRemoved).AsNoTracking()?.ToListAsync()
                                         select a)?.ToList();
                }
                
                var bookJson = await getFilteredResult(model, booksFilteredList);
                return bookJson;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Autocomplete>> Autocomplete(string s, EnumTypes.AutoEnum type)
        {
            List<Autocomplete> autoList = new List<Autocomplete>();
            switch(type)
            {
                case EnumTypes.AutoEnum.Person:
                    {
                        var data = (from a in await Db?.PersonCatalog?.Distinct()?.ToListAsync()
                                    where utils.insensitiveContains(a.Person, s, StringComparison.OrdinalIgnoreCase)
                                    select a)?.Select(c => new Autocomplete
                                    {
                                        Id = c.PersonCatalogId,
                                        Value = c.Person?.Trim()?.Replace("\"", "'")
                                    })?.ToList();

                        autoList = data?.GroupBy(c => new { c.Value })?.ToList()?.Select(v => new Autocomplete
                        {
                            Value = v.Key.Value
                        })?.ToList();
                    }
                    break;
                case EnumTypes.AutoEnum.Event:
                    {
                        var data = (from a in await Db?.EventCatalog?.Distinct()?.ToListAsync()
                                    where utils.insensitiveContains(a.Event, s, StringComparison.OrdinalIgnoreCase)
                                    select a)?.Select(c => new Autocomplete
                                    {
                                        Id = c.EventCatalogId,
                                        Value = c.Event?.Trim()?.Replace("\"", "'")
                                    })?.ToList();

                        autoList = data?.GroupBy(c => new { c.Value })?.ToList()?.Select(v => new Autocomplete
                        {
                            Value = v.Key.Value
                        })?.ToList();
                    }
                    break;
                case EnumTypes.AutoEnum.Place:
                    {
                        var data = (from a in await Db?.PlaceCatalog?.Distinct()?.ToListAsync()
                                    where utils.insensitiveContains(a.Place, s, StringComparison.OrdinalIgnoreCase)
                                    select a)?.Select(c => new Autocomplete
                                    {
                                        Id = c.PlaceCatalogId,
                                        Value = c.Place?.Trim()?.Replace("\"", "'")
                                    })?.ToList();

                        autoList = data?.GroupBy(c => new { c.Value })?.ToList()?.Select(v => new Autocomplete
                        {
                            Value = v.Key.Value
                        })?.ToList();
                    }
                    break;
                case EnumTypes.AutoEnum.Organization:
                    {
                        var data = (from a in await Db?.OrganizationCatalog?.Distinct()?.ToListAsync()
                                             where utils.insensitiveContains(a.Organization, s, StringComparison.OrdinalIgnoreCase)
                                             select a)?.Select(c => new Autocomplete
                                             {
                                                 Id = c.OrganizationCatalogId,
                                                 Value = c.Organization?.Trim()?.Replace("\"", "'")
                                             })?.ToList();
                        
                        autoList = data?.GroupBy(c => new { c.Value })?.ToList()?.Select(v=>new Autocomplete
                        {
                            Value = v.Key.Value
                        })?.ToList();                        
                    }
                    break;

                //Attributes               
                case EnumTypes.AutoEnum.Theme:
                    {
                        //autoList = (from a in await Db?.ThemesLanguagesList?.Distinct()?.ToListAsync()
                        //            where utils.insensitiveContains(a.DescriptionThemeLanguage, s, StringComparison.OrdinalIgnoreCase)
                        //            select a)?.Select(c => new Autocomplete
                        //            {
                        //                Id = c.ThemesLanguagesListId,
                        //                Value = c.DescriptionThemeLanguage?.Trim()?.Replace("\"", "'")
                        //            })?.ToList();                        
                    }
                    break;
                case EnumTypes.AutoEnum.Type:
                    {
                        //autoList = (from a in await Db?.TypesLanguagesList?.Distinct()?.ToListAsync()
                        //            where utils.insensitiveContains(a.DescriptionTypeLanguage, s, StringComparison.OrdinalIgnoreCase)
                        //            select a)?.Select(c => new Autocomplete
                        //            {
                        //                Id = c.TypesLanguagesListId,
                        //                Value = c.DescriptionTypeLanguage?.Trim()?.Replace("\"", "'")
                        //            })?.ToList();                        
                    }
                    break;                
            }
            return autoList;
        }

        public async Task<AutocompleteDataSourceAdmin> getAutocompleteData()
        {
            AutocompleteDataSourceAdmin sourceData = new AutocompleteDataSourceAdmin();
            sourceData.Persons = await Db?.PersonCatalog?.Select(c => c.Person.Trim())?.Distinct().ToListAsync();
            sourceData.Events = await Db?.EventCatalog?.Select(c => c.Event.Trim())?.Distinct().ToListAsync();
            sourceData.Places = await Db?.PlaceCatalog?.Select(c => c.Place.Trim())?.Distinct().ToListAsync();
            sourceData.Organizations = await Db?.OrganizationCatalog?.Select(c => c.Organization.Trim())?.Distinct().ToListAsync();
            return sourceData;
        }

        public async Task<List<TagsCloud>> getTagsCloud()
        {
            List<TagsCloud> tagsList = new List<TagsCloud>();
            var persons = await Db?.PersonCatalog?.Select(c => c.Person.Trim())?.Distinct()?.ToListAsync();
            var organizations = await Db?.OrganizationCatalog?.Select(c => c.Organization.Trim())?.Distinct()?.ToListAsync();
            var events = await Db?.EventCatalog?.Select(c => c.Event.Trim())?.Distinct()?.ToListAsync();
            var places = await Db?.PlaceCatalog?.Select(c => c.Place.Trim())?.Distinct()?.ToListAsync();
            if(persons.Any())
            {
                var group = persons.GroupBy(c => new { c });
                foreach(var c in group)
                {
                    tagsList.Add(new TagsCloud
                    {
                        name = c.Key.c?.Replace("\"", "'"),
                        category = "Персоны",
                        param = "Person"
                    });
                }
            }
            if (organizations.Any())
            {
                var group = organizations.GroupBy(c => new { c });
                foreach (var c in group)
                {
                    tagsList.Add(new TagsCloud
                    {
                        name = c.Key.c?.Replace("\"","'"),
                        category = "Организации",
                        param = "Organization"
                    });
                }
            }
            if (events.Any())
            {
                var group = events.GroupBy(c => new { c });
                foreach (var c in group)
                {
                    tagsList.Add(new TagsCloud
                    {
                        name = c.Key.c?.Replace("\"", "'"),
                        category = "События",
                        param = "Event"
                    });
                }
            }
            if (places.Any())
            {
                var group = places.GroupBy(c => new { c });
                foreach (var c in group)
                {
                    tagsList.Add(new TagsCloud
                    {
                        name = c.Key.c?.Replace("\"", "'"),
                        category = "Места",
                        param = "Place"
                    });
                }
            }            
            return tagsList;
        }

        /// <summary>
        /// Обложка для книги
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<int> SetCoverBook(int bookId, bool currCheckState, string src)
        {
            try
            {
                var book = await Db?.Books?.Where(c => c.BookId == bookId)?.FirstOrDefaultAsync();
                if (book != null)
                {
                    //убираем текущую обложку
                    book.BookCoverFileName = null;
                    if(currCheckState)
                    {
                        book.BookCoverFileName = src;
                    }
                    await Db.SaveChangesAsync();
                    return 200;
                }
                else
                {
                    return 404;
                }
            }
            catch
            {
                return 500;
            }
        }
        
        /// <summary>
        /// Логирование действий
        /// </summary>
        /// <param name="userMember"></param>
        /// <param name="type"></param>
        /// <param name="book"></param>
        /// <returns></returns>
        public async Task addLog(int userId, LogType type, Books book, string pagesSeparator)
        {
            string logText = string.Empty;
            switch (type)
            {
                case (LogType.AddBook):
                {
                        logText += $"добавил(а) новую книгу с Id = {book.BookId}";
                }
                break;
                case (LogType.ChangeBook):
                {
                    logText += $"изменил(а) книгу с Id = {book.BookId}";
                }
                break;
                case (LogType.UploadFilePDF):
                {
                    logText += $"загрузил(а) вложение ({book.PdfFile}) в книгу с Id = {book.BookId}";
                }
                break;
                case (LogType.AddPage):
                {
                   logText += $"добавил(а) страницу в книгу с Id = {book.BookId} = {pagesSeparator}";
                }
                break;
                case (LogType.DeletePage):
                    {
                        logText += $"удалил(а) страницу с книги с Id = {book.BookId} = {pagesSeparator}";
                    }
                break;
                case (LogType.DeleteBook):
                    {
                        logText += $"удалил(а) книгу с Id = {book.BookId}";
                    }
                    break;
            }

            Log log = new Log();
            log.DateLog = DateTime.Now;
            log.UserId = userId;
            log.LogText = logText;

            Db.Log.Add(log);
            await Db.SaveChangesAsync();
        }

        public void removeFileInServer(Pages page)
        {
            try
            {
                string imageFull = HttpContext.Current.Server.MapPath($"~/Books/{page?.Book.HashFolder}/{page.FileName}");
                string imagePreview = HttpContext.Current.Server.MapPath($"~/Books/{page?.Book.HashFolder}/{page.FileNamePreview}");
                File.Delete(imageFull);
                File.Delete(imagePreview);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Удаление страницы -> удаление файла на сервере
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<int> RemovePage(int id, CustomAuthentication.CustomMembershipUser user)
        {
            try
            {
                var page = await Db?.Pages?.Where(c => c.Id == id)?.FirstOrDefaultAsync();
                if (page != null)
                {
                    //проверка на обложку
                    if(!string.IsNullOrEmpty(page.Book.BookCoverFileName))
                    {
                        if(page?.FileNamePreview == page.Book.BookCoverFileName)
                        {
                            page.Book.BookCoverFileName = null;
                        }
                    }
                    removeFileInServer(page);
                    page.Visible = false;
                    page.FileName = null;
                    page.FileNamePreview = null;
                    await Db.SaveChangesAsync();
                    await addLog(user.UserId, LogType.DeletePage, page.Book,page.Id.ToString());
                    return 200;
                }
                else
                {
                    return 404;
                }
            }
            catch
            {
                return 500;
            }
        }

        /// <summary>
        /// Удаление страниц массивом
        /// </summary>
        /// <returns></returns>
        public async Task<int?> RemovePageArray(int bookId, int[] pagesId, CustomAuthentication.CustomMembershipUser user)
        {
            try
            {        
                //все страницы
                var allPagesList = await Db?.Pages?.AsNoTracking()?.Where(b => b.Book.BookId == bookId && b.Visible)?.ToListAsync();
                //страницы с массива pagesId
                var pagesListFiltered = (from a in pagesId
                                     from b in allPagesList
                                     where b.Id == a
                                     select b)?.ToList();
                //Страницы для удаления
                var resPagesDeleted = allPagesList?.Except(pagesListFiltered);
                if(resPagesDeleted!=null && resPagesDeleted.Any())
                {                    
                    //удаляем
                    foreach(var f in resPagesDeleted)
                    {
                        await RemovePage(f.Id, user);                        
                    }
                }
                return 200;        
            }
            catch
            {
                return 500;
            }
        }

        public async Task<int> RemoveBook(int id, CustomAuthentication.CustomMembershipUser user)
        {
            try
            {
                var book = await Db?.Books?.Where(c => c.BookId == id)?.FirstOrDefaultAsync();
                if (book != null)
                {
                    book.IsRemoved = true;
                    await Db.SaveChangesAsync();
                    await addLog(user.UserId, LogType.DeleteBook, book, null);
                    return 200;
                }
                else
                {
                    return 404;
                }
            }
            catch
            {
                return 500;
            }
        }

        /// <summary>
        /// Получить последние сообщения
        /// </summary>
        /// <returns></returns>
        public async Task<List<ChatMessages>> getOldMessages(int BookId, int skip, int take)
        {            
            var select = await Db?.ChatMessages?.Where(b=>b.BookId == BookId)?.OrderByDescending(c=>c.Id)?.ToListAsync();
            if(select.Any())
            {
                return select.Skip(skip)?.Take(take)?.OrderByDescending(c=>c.Id)?.ToList();
            }
            return null;
        }
        

        /// <summary>
        /// Книги для Control Panel
        /// </summary>
        /// <param name="userMember"></param>
        /// <param name="isWaitOnly"></param>
        /// <returns></returns>
        public async Task<BooksControlPanelMainList> getBooksForAdminPanel(CustomAuthentication.CustomMembershipUser userMember)
        {
            BooksControlPanelMainList result = new BooksControlPanelMainList();
            if (userMember.Roles!=null && userMember.Roles.Any())
            {
                //администратор || редактор ? показываем все записи : только свои
                bool isMyOnly = true;
                foreach(var r in userMember.Roles)
                {
                    if(r.RoleName.Contains(EnumTypes.Roles.Admin) || r.RoleName.Contains(EnumTypes.Roles.Editor))
                    {
                        //все записи
                        isMyOnly = false;
                        break;
                    }                    
                }                                  
                
                string expr = string.Empty;
                if(isMyOnly)
                {
                    expr = $"and [UserExpert_UserId]  = {userMember.UserId}";
                }

                var allSelect = await _context.Database.SqlQuery<BooksControlPanelMain>($"select * from Books as b left join  Attributes attr on b.AttributesId = attr.AttributesId left join Pages as p on p.Book_BookId = [BookId] left join Users as u on UserExpert_UserId = u.UserId where [IsRemoved] = 0 {expr} order by b.DateTimeCreated desc")?.ToListAsync();
                var resultSelect = new List<BooksControlPanelMain>();
                if(allSelect.Any())
                {
                    resultSelect = allSelect?.GroupBy(f => f.BookId)?.Select(f => f.FirstOrDefault())?.ToList();
                }

                //ждут публикации
                var wait = resultSelect?.Where(c => !c.Visible)?.ToList();
                result.all = resultSelect;
                result.wait = wait;

                //информация с профиля
                result.Login = userMember.UserName;
                result.Name = userMember.FirstName;
                result.UserId = userMember.UserId;
                result.Roles = string.Join(",", userMember.Roles?.Select(c=>c.RoleName));
            }
            return result;
        }

        //Настройка роли для пользователя
        public async Task<int> setupRoleUser (int UserId, string[] roles)
        {
            var user = await Db?.Users?.Where(c => c.UserId == UserId)?.FirstOrDefaultAsync();
            if(user!=null)
            {                
                if(user?.Roles!=null && user.Roles.Any())
                {
                    //Очищаем
                    user.Roles.Clear();
                    await Db.SaveChangesAsync();
                }
                if (roles != null && roles.Any())
                {
                    foreach (var i in roles)
                    {
                        int roleId = int.Parse(i);
                        await Db.Database.ExecuteSqlCommandAsync(TransactionalBehavior.EnsureTransaction, $"INSERT INTO RoleUsers VALUES ({UserId}, {roleId});");
                    }
                }
                return 200;                
            }
            return 404;
        }

        public async Task<List<Role>> getRoles()
        {
            var res = await Db?.Roles?.ToListAsync();
            return res;
        }

        public async Task<List<UsersForControlPanel>> getUsers()
        {
            var user = await Db?.Users?.ToListAsync();
            var res = user?.Select(c => new UsersForControlPanel
            {
                UserId = c.UserId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                UserName = c.Username,
                IsActive = c.IsActive,
                RolesControl = c.Roles?.ToList()
            })?.ToList();
            return res;
        }

        //Все номера сборников сочинений
        public async Task<List<string>> getAllInventoryCollections()
        {
            var resList = new List<string>();
            var collectedBooks = await Db?.Attributes?.Where(c => c.isCollectedBooks.HasValue && c.isCollectedBooks.Value)?.ToListAsync();
            if(collectedBooks!=null && collectedBooks.Any())
            {
                resList = collectedBooks?.Where(c => !string.IsNullOrEmpty(c.InventoryCollectedBook))?.Select(c => c.InventoryCollectedBook)?.Distinct()?.OrderBy(c => c)?.ToList();
            }
            return resList;
        }

        //Все названия сборников сочинений
        public async Task<List<string>> getAllNamesCollections()
        {
            var resList = new List<string>();
            var collectedBooks = await Db?.Attributes?.Where(c => c.isCollectedBooks.HasValue && c.isCollectedBooks.Value)?.ToListAsync();
            if (collectedBooks != null && collectedBooks.Any())
            {
                resList = collectedBooks?.Where(c => !string.IsNullOrEmpty(c.CollectedNameText))?.Select(c => c.CollectedNameText)?.Distinct()?.OrderBy(c=>c)?.ToList();
            }
            return resList;
        }

        //**********************************************************************************
        //*************************** Логика для API BI ************************************
        //**********************************************************************************

        public async Task<List<ModelBI>> getDataBI(string securityCode)
        {
            if(!string.IsNullOrEmpty(securityCode))
            {
                if(securityCode == System.Configuration.ConfigurationManager.AppSettings["SecurityCodeAPI"])
                {
                    string currentSiteUrl = string.Format("{0}://{1}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority);
                    var res = await Db.Database?.SqlQuery<ModelBI>($"SELECT BookId, BookName, attr.NameOfBookCyrillic as BookNameRussian, attr.NameOfBookEnglish as BookNameEnglish, attr.NameOfBookArabic as BookNameArabic, CONCAT('{currentSiteUrl}','Books/', BookId) as BookUrl, CONCAT('{currentSiteUrl}','Books/',HashFolder,'/',BookCoverFileName) as BookCoverImageUrl, CONCAT('{currentSiteUrl}','Books/',HashFolder,'/',FileNamePreview) as PagePreviewImageUrl, CONCAT('{currentSiteUrl}','Books/',HashFolder,'/',FileName) as PageImageUrl, StateDescription as BookState, b.Description as BookDescription, b.Tags as BookTags, b.HashFolder as BookFolder, b.AttributesId as PageAttribute, b.isRtl as BookIsRtl, b.Comment as BookComment, b.Visible as BookIsVisible, b.IsRemoved as BookIsRemoved, attr.Inventory as BookInventoryNumber, attr.PageCount as BookPageCount, attr.Width as PageWidth, attr.Height as PageHeight, b.UserExpert_UserId as BookUserExpertId, b.UserIdCreator_UserId as BookUserCreatorId, attr.FIOScanner as BookScannerFIO, attr.FIOProcessing as BookProcessingFIO, attr.PercentWorkScanner as BookPercentScanner, attr.PercentWorkProcessing as BookPercentProcessing, attr.PercentWorkDescr as BookPercentWorkDescription, attr.AuthorOfCyrillic as BookAuthorOfRussian, attr.AuthorOfEnglish as BookAuthorOfEnglish, attr.AuthorOfArabic as BookAuthorOfArabic, regions.RegionName as BookRegionName, themes.DescriptionThemeRussian as BookThemeRussian, themes.DescriptionThemeEnglish as BookThemeEnglish, themes.DescriptionThemeArabic as BookThemeArabic, types.DescriptionTypeRussian as BookTypeRussian, types.DescriptionTypeEnglish as BookTypeEnglish, types.DescriptionTypeArabic as BookTypeArabic, attr.isCollectedBooks as BookIsCollection, attr.InventoryCollectedBook as BookCollectionInventoryNumber, attr.CollectedNameText as BookCollectionName, b.DateTimeCreated as BookCreatedDt, p.DateTimeCreated as PageCreatedDt, attr.LocalityOffDeliveryLat as LocalityOffDeliveryLat, attr.LocalityOffDeliveryLon as LocalityOffDeliveryLon, attr.PlaceCorrespondenceLat as PlaceCorrespondenceLat, attr.PlaceCorrespondenceLon as PlaceCorrespondenceLon FROM [Books] as b left join Pages as p on b.BookId = p.Book_BookId left join Attributes as attr on b.AttributesId = attr.AttributesId left join ThemesLanguagesLists as themes on attr.ThemesLanguagesListId = themes.ThemesLanguagesListId left join TypesLanguagesLists as types on attr.TypesLanguagesListId = types.TypesLanguagesListId left join Regions as regions on attr.RegionsId = regions.RegionsId left join States as states on attr.StatesId = states.StatesId where b.Visible = 1 and p.Visible = 1 and b.IsRemoved = 0")?.ToListAsync();
                    return res;
                }
            }
            return null;
        }

        //**********************************************************************************
        //*************************** History TimeLine *************************************
        //**********************************************************************************
        public async Task<List<HistoryModelObject>> getJSONHistoryMainPage()
        {
            var historyMapList = await Db?.HistoryModelObject?.Where(c => c.isVisible)?.OrderBy(c => c.Order)?.ToListAsync();
            return historyMapList;
        }

        //**********************************************************************************
        //*************************** Логика для справочников ******************************
        //**********************************************************************************

        public class universalListClass
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        #region StatesClass
        public async Task<Boolean?> deleteState(int StateId)
        {
            try
            {
                var t = await Db?.States?.Where(c => c.StatesId == StateId)?.FirstOrDefaultAsync();
                if(t!=null)
                {
                    Db?.States.Remove(t);
                    await Db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<universalListClass> addState(string Name)
        {
            try
            {
                var t = await Db?.States?.Where(c => c.StateDescription.Contains(Name))?.FirstOrDefaultAsync();
                if (t == null)
                {
                    var res = Db?.States.Add(new States
                    {                        
                        StateDescription = Name
                    });
                    await Db.SaveChangesAsync();
                    universalListClass uni = new universalListClass()
                    {
                        Id = res.StatesId,
                        Value = res.StateDescription
                    };
                    return uni;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        
        public async Task<Boolean?> updateState(int StateId, string Name)
        {
            try
            {
                var t = await Db?.States?.Where(c => c.StatesId == StateId)?.FirstOrDefaultAsync();
                if (t != null)
                {                    
                    t.StateDescription = Name;
                    await Db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return false;
            }
        }
        


        public async Task<List<States>> getAllState()
        {
            try
            {
                var t = await Db?.States?.OrderBy(c => c.StateDescription)?.ToListAsync();
                return t;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region RegionsClass
        public async Task<Boolean?> deleteRegion(int RegionId)
        {
            try
            {
                var t = await Db?.Regions?.Where(c => c.RegionsId == RegionId)?.FirstOrDefaultAsync();
                if (t != null)
                {
                    Db?.Regions.Remove(t);
                    await Db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<universalListClass> addRegion(string Name)
        {
            try
            {
                var t = await Db?.Regions?.Where(c => c.RegionName.Contains(Name))?.FirstOrDefaultAsync();
                if (t == null)
                {
                    var res = Db?.Regions.Add(new Regions
                    {                        
                        RegionName = Name
                    });
                    await Db.SaveChangesAsync();
                    universalListClass uni = new universalListClass()
                    {
                        Id = res.RegionsId,
                        Value = res.RegionName
                    };
                    return uni;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<Boolean?> updateRegion(int RegionId, string Name)
        {
            try
            {
                var t = await Db?.Regions?.Where(c => c.RegionsId == RegionId)?.FirstOrDefaultAsync();
                if (t != null)
                {                    
                    t.RegionName = Name;
                    await Db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Regions>> getAllRegions()
        {
            try
            {
                var t = await getRegionsList();
                return t;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region LangsClass
        public async Task<Boolean?> deleteLang(int LangId)
        {
            try
            {
                var t = await Db?.LanguageTableList?.Where(c => c.LanguageTableListId == LangId)?.FirstOrDefaultAsync();
                if (t != null)
                {
                    Db?.LanguageTableList.Remove(t);
                    await Db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<universalListClass> addLang(string Name)
        {
            try
            {
                var t = await Db?.LanguageTableList?.Where(c => c.LanguageTableName.Contains(Name))?.FirstOrDefaultAsync();
                if (t == null)
                {
                    var res = Db?.LanguageTableList.Add(new LanguageTableList
                    {                        
                        LanguageTableName = Name
                    });
                    await Db.SaveChangesAsync();
                    universalListClass uni = new universalListClass()
                    {
                        Id = res.LanguageTableListId,
                        Value = res.LanguageTableName
                    };
                    return uni;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<Boolean?> updateLang(int LangId, string Name)
        {
            try
            {
                var t = await Db?.LanguageTableList?.Where(c => c.LanguageTableListId == LangId)?.FirstOrDefaultAsync();
                if (t != null)
                {
                    t.LanguageTableName = Name;
                    await Db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return false;
            }
        }
      
        public async Task<List<LanguageTableList>> getAllLang()
        {
            try
            {
                var t = await Db?.LanguageTableList?.OrderBy(c=>c.LanguageTableName)?.ToListAsync();
                return t;
            }
            catch
            {
                return null;
            }
        }
        #endregion
         
        #region ThemesClass
        public async Task<Boolean?> deleteTheme(int ThemeId)
        {
            try
            {
                var t = await Db?.ThemesLanguagesList?.Where(c => c.ThemesLanguagesListId == ThemeId)?.FirstOrDefaultAsync();
                if (t != null)
                {
                    Db?.ThemesLanguagesList.Remove(t);
                    await Db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<universalListClass> addTheme(string ru, string en, string ar)
        {
            try
            {
                var res = Db?.ThemesLanguagesList.Add(new ThemesLanguagesList
                {
                    DescriptionThemeRussian = ru,
                    DescriptionThemeEnglish = en,
                    DescriptionThemeArabic = ar
                });
                await Db.SaveChangesAsync();
                universalListClass uni = new universalListClass()
                {
                    Id = res.ThemesLanguagesListId,
                    Value = res.DescriptionThemeRussian+"/"+res.DescriptionThemeEnglish+"/"+res.DescriptionThemeArabic
                };
                return uni;                
            }
            catch
            {
                return null;
            }
        }

        public async Task<Boolean?> updateTheme(int ThemeId, string ru, string en, string ar)
        {
            try
            {
                var t = await Db?.ThemesLanguagesList?.Where(c => c.ThemesLanguagesListId == ThemeId)?.FirstOrDefaultAsync();
                if (t != null)
                {
                    t.DescriptionThemeRussian = ru;
                    t.DescriptionThemeEnglish = en;
                    t.DescriptionThemeArabic = ar;
                    await Db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ThemesLanguagesList>> getAllThemes()
        {
            try
            {
                var t = await Db?.ThemesLanguagesList?.OrderBy(c => c.ThemesLanguagesListId)?.ToListAsync();
                return t;                
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region TypesClass
        public async Task<Boolean?> deleteType(int TypeId)
        {
            try
            {
                var t = await Db?.TypesLanguagesList?.Where(c => c.TypesLanguagesListId == TypeId)?.FirstOrDefaultAsync();
                if (t != null)
                {
                    Db?.TypesLanguagesList.Remove(t);
                    await Db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<universalListClass> addType(string ru, string en, string ar)
        {
            try
            {
                var res = Db?.TypesLanguagesList.Add(new TypesLanguagesList
                {
                    DescriptionTypeRussian = ru,
                    DescriptionTypeEnglish = en,
                    DescriptionTypeArabic = ar
                });
                await Db.SaveChangesAsync();
                universalListClass uni = new universalListClass()
                {
                    Id = res.TypesLanguagesListId,
                    Value = res.DescriptionTypeRussian+"/"+res.DescriptionTypeEnglish+"/"+res.DescriptionTypeArabic
                };
                return uni;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Boolean?> updateType(int TypeId, string ru, string en, string ar)
        {
            try
            {
                var t = await Db?.TypesLanguagesList?.Where(c => c.TypesLanguagesListId == TypeId)?.FirstOrDefaultAsync();
                if (t != null)
                {
                    t.DescriptionTypeRussian = ru;
                    t.DescriptionTypeEnglish = en;
                    t.DescriptionTypeArabic = ar;
                    await Db.SaveChangesAsync();
                    return true;                    
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<TypesLanguagesList>> getAllTypes()
        {
            try
            {
                var t = await Db?.TypesLanguagesList?.OrderBy(c => c.TypesLanguagesListId)?.ToListAsync();
                return t;                
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region CollectionsClass
        public async Task<Boolean?> deleteCollection(int CollectionId)
        {
            try
            {
                var t = await Db?.Collections?.Where(c => c.CollectionId == CollectionId)?.FirstOrDefaultAsync();
                if (t != null)
                {
                    Db?.Collections.Remove(t);
                    await Db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<universalListClass> addCollection(string Name)
        {
            try
            {
                var t = await Db?.Collections?.Where(c => c.CollectionName.Contains(Name))?.FirstOrDefaultAsync();
                if (t == null)
                {
                    var res = Db?.Collections.Add(new Collection
                    {                        
                        CollectionName = Name
                    });
                    await Db.SaveChangesAsync();
                    universalListClass uni = new universalListClass()
                    {
                        Id = res.CollectionId,
                        Value = res.CollectionName
                    };
                    return uni;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<Boolean?> updateCollection(int CollectionId, string Name)
        {
            try
            {
                var t = await Db?.Collections?.Where(c => c.CollectionId == CollectionId)?.FirstOrDefaultAsync();
                if (t != null)
                {                    
                    t.CollectionName = Name;
                    await Db.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Collection>> getAllCollections()
        {
            try
            {
                var res = await _context?.Collections?.ToListAsync();
                return res;
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }    
}