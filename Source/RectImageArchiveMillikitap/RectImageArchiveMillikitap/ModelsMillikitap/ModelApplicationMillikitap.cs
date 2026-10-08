using Newtonsoft.Json;
using RectImageArchiveMillikitap.Models.CustomAuthentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using static RectImageArchiveMillikitap.ModelsMillikitap.Map3d.Map3dModel;

namespace RectImageArchiveMillikitap.Models
{
    public class TranslateModel
    {
        public string NameOnPage { get; set; }
        public string Translate { get; set; }
    }

    public class BookDatesSearch
    {
        public string minDateISO { get; set; }
        public string currentDateISO { get; set; }
        public string minDate { get; set; }
        public string currentDate { get; set; }
    }

    public class DescriptionContent
    {
        public string descriptionHtml { get; set; }
        public string descriptionBook { get; set; }
        public DateTime dtCreated { get; set; }
        public DateTime dtPublish { get; set; }
        public string bookTag { get; set; }

        public CustomAttributes otherAttributes { get; set; }
        public bool isLike { get; set; }
    }

    public class CustomAttributes
    {
        public int Order { get; set; }
        public string Person { get; set; }
        public string Event { get; set; }
        public string Place { get; set; }
        public string Organization { get; set; }
        public string ArticleTitle { get; set; }
    }

    public class SearchForm
    {
        public string DtStart { get; set; }
        public string DtEnd { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Text { get; set; }

        public string Person { get; set; }
        public string Event { get; set; }
        public string Place { get; set; }
        public string Organization { get; set; }
        public string ArticleTitle { get; set; }

        public bool isAccessEditor { get; set; }

        //Атрибуты        
        public Attributes Attributes { get; set; }
    }

    public class searchAttrRectArea
    {
        public int PagesId { get; set; }
        public string Name { get; set; }
        public string Preview { get; set; }
        public int BookId { get; set; }
        public string HashFolder { get; set; }
        /// <summary>
        /// Текст, который есть по совпадению
        /// </summary>
        public string FinderText { get; set; }
        public int? Data_Viewerid { get; set; }
    }

    public class BooksJson
    {
        public int BookId { get; set; }
        public List<searchAttrRectArea> searchAttrRectArea { get; set; }
        public string BookName { get; set; }

        public string DateTimeCreated { get; set; }
        public DateTime DateTimeCreatedDt { get; set; }

        public string Description { get; set; }
        public string Tags { get; set; }
        public string FileNamePreview { get; set; }
        public string HashFolder { get; set; }
        public string FinderText { get; set; }
        public int CountAll { get; set; }
        public bool? Visible { get; set; }
        public List<Pages> PagesList { get; set; }

        //Доп. атрибуты
        public bool? isCollectedBooks { get; set; }
        public int? CollectionId { get; set; }

        //Инв. номер сборника (для группировки)
        public string InventoryCollectedBook { get; set; }
        //Название сборника
        public string CollectedNameText { get; set; }

        //Регион
        public Regions Region { get; set; }
        //Состояние
        public States State { get; set; }
        //Тип
        public int? TypeId { get; set; }
        //Тематика
        public int? ThemeId { get; set; }
        //Кол-во страниц
        public int? PageCount { get; set; }
    }

    public class Likes
    {
        [Key]
        public int LikeId { get; set; }
        public int RectAreasViewerId { get; set; }
        public virtual RectAreasViewer RectAreasViewer { get; set; }
        public int UserId { get; set; }
        public virtual User.User User { get; set; }
    }

    public class LikeInfo
    {
        public bool isLike { get; set; }
    }

    public class Favorite
    {
        public int AreaViewerId { get; set; }
        public string BookName { get; set; }
        public int BookId { get; set; }
        public int PagesId { get; set; }
        public string Description { get; set; }
        public string PreviewImage { get; set; }
        public string HashFolder { get; set; }
        public string DateTimeCreated { get; set; }
        public string DateTimePublish { get; set; }
        public string ArticleTitle { get; set; }
    }

    public class Autocomplete
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class AutocompleteDataSourceAdmin
    {
        public List<string> Persons { get; set; } = new List<string>();
        public List<string> Events { get; set; } = new List<string>();
        public List<string> Places { get; set; } = new List<string>();
        public List<string> Organizations { get; set; } = new List<string>();
    }

    public class CustomStyleAppClass
    {
        public string IndexCSS { get; set; }
        public string OtherPagesCSS { get; set; }
        public string TextTitle { get; set; }
    }

    public class TagsCloud
    {
        public string name { get; set; }
        public string category { get; set; }
        public string param { get; set; }
    }

    public class SearchResult
    {
        public int RectAreasViewerId { get; set; }
        public int PagesId { get; set; }
        public string RectForViewerSVG { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DescriptionWithoutHTML { get; set; }
        public string Tag { get; set; }
        public string BlobFragment { get; set; }
        public string Person { get; set; }
        public string Event { get; set; }
        public string Place { get; set; }
        public string Organization { get; set; }
        public string ArticleTitle { get; set; }
        public int Id { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public string BlobImage { get; set; }
        public string FileName { get; set; }
        public bool Visible { get; set; }
        public int Book_BookId { get; set; }
        public string FileNamePreview { get; set; }
        public string BookCoverFileName { get; set; }
        //Атрибуты
        public string Inventory { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? StateId { get; set; }
    }

    public class BooksControlPanelMain
    {
        public int BookId { get; set; }
        public string Inventory { get; set; }
        public string BookName { get; set; }
        public DateTime DateTimePublish { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public string Description { get; set; }
        public bool Visible { get; set; }
        public int ProgressCompleteValue { get; set; }
        public string HashFolder { get; set; }
        public string FileNamePreview { get; set; }
        
        public int? PercentWorkScanner { get; set; }
        public int? PercentWorkProcessing { get; set; }
        public int? PercentWorkDescr { get; set; }

        public string BookCoverFileName { get; set; }
        public string Username { get; set; }//ExpertEmail
        public string FirstName { get; set; }//Expert FirstName

        public string InventoryCollectedBook { get; set; }
        public int? PageCount { get; set; }

        public bool? isExrimistStatements { get; set; }
    }

    public class BooksControlPanelMainList
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Roles { get; set; }
        public List<BooksControlPanelMain> all { get; set; }
        public List<BooksControlPanelMain> wait { get; set; }
    }

    public class UsersForControlBook
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Roles { get; set; }
    }

    public class SaveModelBook
    {
        public int? BookId { get; set; } //+
        public string bookName { get; set; } //+
        public string description { get; set; } //+
        public string tags { get; set; } //+
        public string visible { get; set; }
        public int progress { get; set; }
        public string comment { get; set; }
        public int? userId { get; set; } //Эксперт
        public HttpPostedFileBase fileBook { get; set; }

        //Атрибуты
        //Первичное описание
        public string Inventory { get; set; }//+
        public int? Width { get; set; } //+
        public int? Height { get; set; } // +
        public int? PageCount { get; set; } //+
        public int? StatesId { get; set; } //+

        //Сканирование
        public DateTime? StartDateScan { get; set; } = new DateTime(); //+
        public DateTime? EndDateScan { get; set; } //+
        public string FIOScanner { get; set; } //+
        public int? PercentWorkScanner { get; set; } //+

        //Обработка
        public DateTime? StartDateScanProcessing { get; set; } = new DateTime(); //+
        public DateTime? EndDateScanProcessing { get; set; } = new DateTime(); //+
        public string FIOProcessing { get; set; } // +  
        public int? PercentWorkProcessing { get; set; } // +

        //Описание
        public DateTime? StartDateDescriptionProcessing { get; set; } = new DateTime();//+
        public DateTime? EndDateDescriptionProcessing { get; set; } = new DateTime();//+
        public string FIODescriptionProcessing { get; set; }//+
        public int? PercentWorkDescr { get; set; } //+



        public string ParametersPaper { get; set; } // +
        public string NameOfBookArabic { get; set; } //+
        public string NameOfBookCyrillic { get; set; } //+
        public string NameOfBookEnglish { get; set; } //+

        public string AuthorOfArabic { get; set; } //+
        public string AuthorOfCyrillic { get; set; } //+
        public string AuthorOfEnglish { get; set; } //+

        public int? ThemesLanguagesListId { get; set; } //+      

        public int? TypesLanguagesListId { get; set; } //+        

        public DateTime? DateOfCorrespondenceGregorianCalendar { get; set; } = new DateTime(); //+
        public DateTime? DateOfCorrespondenceMusulmanCalendar { get; set; } = new DateTime(); //+

        public string CorrespondenceArabic { get; set; } //+
        public string CorrespondenceCyrillic { get; set; } //+
        public string CorrespondenceEnglish { get; set; } //+
        public string CorrespondenceTatar { get; set; }
        public string PlaceCorrespondenceArabic { get; set; } //+
        public string PlaceCorrespondenceTatar { get; set; } //+
        public string PlaceCorrespondenceRussian { get; set; } //+
        public string PlaceCorrespondenceEnglish { get; set; } //+

        public float? PlaceCorrespondenceLat { get; set; } //+
        public float? PlaceCorrespondenceLon { get; set; } //+

        public string LocalityOffDeliveryTatar { get; set; } //+
        public string LocalityOffDeliveryRussian { get; set; } //+
        public string LocalityOffDeliveryEnglish { get; set; } //+
        public float? LocalityOffDeliveryLat { get; set; } //+
        public float? LocalityOffDeliveryLon { get; set; } //+

        public string CommentExpert { get; set; } //+
        public string CommentEditor { get; set; } //+
        public bool? FinishProcessing { get; set; }

        public List<int?> LanguageId { get; set; }

        //Место переписки координаты String
        public string PlaceCorrespondenceLatLonString { get; set; }
        //Населенный пункт откуда доставлена рукопись
        public string LocalityOffDeliveryLatLonString { get; set; }

        //Пролистывание справа налево
        public string isRtl { get; set; }

        public int? RegionsId { get; set; }

        //Коллекция
        public string isCollection { get; set; }

        //Дата на мусульманском в описательном виде
        public string ArabicDateText { get; set; }

        //Часть сборника сочинений
        public string isCollectedBooks { get; set; }
        public string InventoryCollectedBook { get; set; }
        public string CollectedNameText { get; set; }

        public int? CollectionId { get; set; }

        //Содержит экстримистские содержания
        public string isExrimistStatements { get; set; }
        public string exrimistStatementsText { get; set; }        
    }

    internal class RolesControl
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class UsersForControlPanel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public List<DataAccess.Role> RolesControl { get; set; }
    }

    public class BookInfo
    {
        public int Id { get; set; }
        public string BookName { get; set; }
        public string Description { get; set; }
        public bool isRtl { get; set; }

        //Атрибуты
        public string Inventory { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? PageCount { get; set; }
        public int? StateId { get; set; }
        public string StateName { get; set; }
        public string ParametersPaper { get; set; }

        public string NameOfBookArabic { get; set; }
        public string AuthorOfArabic { get; set; }

        public string NameOfBookCyrillic { get; set; }
        public string AuthorOfCyrillic { get; set; }

        public string NameOfBookEnglish { get; set; }
        public string AuthorOfEnglish { get; set; }

        public string ThemeOfArabic { get; set; }
        public string ThemeOfRussian { get; set; }
        public string ThemeOfEnglish { get; set; }

        public DateTime? DateOfCorrespondenceGregorianCalendar { get; set; }
        public DateTime? DateOfCorrespondenceMusulmanCalendar { get; set; }

        public string CorrespondenceArabic { get; set; }
        public string CorrespondenceCyrillic { get; set; }
        public string CorrespondenceEnglish { get; set; }
    }

    public class ModelBI
    {          
        public int BookId { get; set; }
        public string BookName { get; set; }        
        public string BookNameRussian { get; set; }
        public string BookNameEnglish { get; set; }
        public string BookNameArabic { get; set; }
        public string BookUrl { get; set; }
        public string BookCoverImageUrl { get; set; }
        public string PagePreviewImageUrl { get; set; }
        public string PageImageUrl { get; set; }
        public string BookState { get; set; }
        public string BookDescription { get; set; }
        public string BookTags { get; set; }
        public string BookFolder { get; set; }
        public int? PageAttribute { get; set; }
        public bool BookIsRtl { get; set; }
        public string BookComment { get; set; }
        public bool BookIsVisible { get; set; }
        public bool BookIsRemoved { get; set; }
        public string BookInventoryNumber { get; set; }
        public int? BookPageCount { get; set; }
        public int? PageWidth { get; set; }
        public int? PageHeight { get; set; }
        public int? BookUserExpertId { get; set; }
        public int? BookUserCreatorId { get; set; }
        public string BookScannerFIO { get; set; }
        public string BookProcessingFIO { get; set; }
        public int? BookPercentScanner { get; set; }
        public int? BookPercentProcessing { get; set; }
        public int? BookPercentWorkDescription { get; set; }
        public string BookAuthorOfRussian { get; set; }
        public string BookAuthorOfEnglish { get; set; }
        public string BookAuthorOfArabic { get; set; }
        public string BookRegionName { get; set; }
        public string BookThemeRussian { get; set; }
        public string BookThemeEnglish { get; set; }
        public string BookThemeArabic { get; set; }
        public string BookTypeRussian { get; set; }
        public string BookTypeEnglish { get; set; }
        public string BookTypeArabic { get; set; }
        public bool? BookIsCollection { get; set; }
        public string BookCollectionInventoryNumber { get; set; }
        public string BookCollectionName { get; set; }
        public DateTime BookCreatedDt { get; set; }
        public DateTime PageCreatedDt { get; set; }
        public float? LocalityOffDeliveryLat { get; set; }
        public float? LocalityOffDeliveryLon { get; set; }
        public float? PlaceCorrespondenceLat { get; set; }
        public float? PlaceCorrespondenceLon { get; set; }        
    }

    /// <summary>
    /// Модель для визуализации карты LeafLet с маркерами
    /// </summary>
    public class HistoryModelObject
    {
        public int HistoryModelObjectId { get; set; }
        public int Order { get; set; } = 1;
        public string Title { get; set; }
        /// <summary>
        /// Изображение для иконки
        /// </summary>
        public string UrlIconMarker { get; set; }
        /// <summary>
        /// Основное изображение обложки
        /// </summary>
        public string UrlMedia { get; set; }
        public string TypeMedia { get; set; }
        public string Description { get; set; }        

        public bool isCover { get; set; } = false;
        public bool isVisible { get; set; }
        /// <summary>
        /// Широта
        /// </summary>
        public double? Lat { get; set; }
        /// <summary>
        /// Долгота
        /// </summary>
        public double? Lon { get; set; }
    }
}