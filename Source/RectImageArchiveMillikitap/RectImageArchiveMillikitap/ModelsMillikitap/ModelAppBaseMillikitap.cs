namespace RectImageArchiveMillikitap.Models
{
    using RectImageArchiveMillikitap.Models.CustomAuthentication;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;    

    public class ModelAppBaseMillikitap : DbContext
    {
        public ModelAppBaseMillikitap()
            : base("name=ModelMillikitap")
        {
        }

        public DbSet<User.User> Users { get; set; }
        public DbSet<Likes> Likes { get; set; }
        public DbSet<DataAccess.Role> Roles { get; set; }       

        public virtual DbSet<Books> Books { get; set; }
        public virtual DbSet<Pages> Pages { get; set; }
        public virtual DbSet<RectAreasViewer> RectAreasViewer { get; set; }
        public virtual DbSet<RectAreasEditor> RectAreasEditor { get; set; }

        public virtual DbSet<PersonCatalog> PersonCatalog { get; set; }
        public virtual DbSet<EventCatalog> EventCatalog { get; set; }
        public virtual DbSet<PlaceCatalog> PlaceCatalog { get; set; }
        public virtual DbSet<OrganizationCatalog> OrganizationCatalog { get; set; }
        public virtual DbSet<CustomAttributesApp> CustomAttributesApp { get; set; }
        public virtual DbSet<Log> Log { get; set; }

        public virtual DbSet<LanguageList> LanguageList { get; set; }
        public virtual DbSet<LanguageTableList> LanguageTableList { get; set; }

        public virtual DbSet<Attributes> Attributes { get; set; }

        public virtual DbSet<States> States { get; set; }
        public virtual DbSet<ThemesLanguagesList> ThemesLanguagesList { get; set; }
        public virtual DbSet<TypesLanguagesList> TypesLanguagesList { get; set; }
        public virtual DbSet<BookLanguages> BookLanguages { get; set; }
        public virtual DbSet<Regions> Regions { get; set; }
        public virtual DbSet<Collection> Collections { get; set; }
        public DbSet<ChatMessages> ChatMessages { get; set; }
        public DbSet<MessagesChatComplaints> MessagesChatComplaints { get; set; }
        public DbSet<MessagesChatBlackList> MessagesChatBlackList { get; set; }
        public DbSet<TranslateLanguage> TranslateLanguage { get; set; }

        public virtual DbSet<HistoryModelObject> HistoryModelObject { get; set; }
    }

    public class Books
    {
        [Key]
        public int BookId { get; set; }
        public string BookName { get; set; }
        public DateTime? DateTimeCreated { get; set; }        
        public string Description { get; set; }
        public string Tags { get; set; }
        public ICollection<Pages> Pages { get; set; }
        public bool Visible { get; set; }
        public string HashFolder { get; set; }
        public string PdfFile { get; set; }
        //public int UserIdExpert { get; set; }
        public virtual User.User UserExpert { get; set; }
        //public int UserCreator { get; set; }
        public virtual User.User UserIdCreator { get; set; }

        public int ProgressCompleteValue { get; set; } = 100; //def
        public bool IsRemoved { get; set; } = false;
        public string Comment { get; set; }

        public int? AttributesId { get; set; }
        public virtual Attributes Attributes { get; set; }

        //Языки
        public virtual ICollection<BookLanguages> BookLanguages { get; set; }
        //info
        [NotMapped]
        public string OperationInfo {get;set;}

        //Атрибуты
        [NotMapped]
        public string ThemeId { get; set; }
        [NotMapped]
        public string TypeId { get; set; }
                
        //rtl направление
        public bool? isRtl { get; set; }

        //Обложка
        public string BookCoverFileName { get; set; }
    }

    public class BookLanguages
    {
        [Key]
        public int BookId { get; set; }
        public virtual Books Book { get; set; }           
       
        [ForeignKey("LanguageTableList")]
        public int LanguageTableListsId { get; set; }
        public virtual LanguageTableList LanguageTableList { get; set; }
    }

    public class Attributes
    {
        public int AttributesId { get; set; }
        ///* Millikitap Custom attributes */
        public string Inventory { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? PageCount { get; set; }

        public DateTime? StartDateScan { get; set; }
        public DateTime? EndDateScan { get; set; }
        public string FIOScanner { get; set; }

        public DateTime? StartDateScanProcessing { get; set; }
        public DateTime? EndDateScanProcessing { get; set; }
        public string FIOProcessing { get; set; }
        public int? PercentWorkScanner { get; set; }
       

        public DateTime? StartDateDescriptionProcessing { get; set; }
        public DateTime? EndDateDescriptionProcessing { get; set; }
        public string FIODescriptionProcessing { get; set; }
        public int? PercentWorkDescr { get; set; }

        public int? PercentWorkProcessing { get; set; } //+

        //Характеристика бумаги
        public string ParametersPaper { get; set; }
        public string NameOfBookArabic { get; set; }
        public string NameOfBookCyrillic { get; set; }
        public string NameOfBookEnglish { get; set; }
        public string AuthorOfArabic { get; set; }
        public string AuthorOfCyrillic { get; set; }
        public string AuthorOfEnglish { get; set; }
        
        //Тип        
        public int? TypesLanguagesListId { get; set; }

        //Тема
        public int? ThemesLanguagesListId { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DateOfCorrespondenceGregorianCalendar { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime? DateOfCorrespondenceMusulmanCalendar { get; set; }
      
        //Переписчик
        public string CorrespondenceArabic { get; set; }
        public string CorrespondenceCyrillic { get; set; }
        public string CorrespondenceEnglish { get; set; }

        //Места переписки
        public string PlaceCorrespondenceArabic { get; set; }
        public string PlaceCorrespondenceTatar { get; set; }
        public string PlaceCorrespondenceRussian { get; set; }
        public string PlaceCorrespondenceEnglish { get; set; }
        //Координаты
        public float? PlaceCorrespondenceLat { get; set; }
        public float? PlaceCorrespondenceLon { get; set; }

        //Населенный пункт
        public string LocalityOffDeliveryTatar { get; set; }
        public string LocalityOffDeliveryRussian { get; set; }
        public string LocalityOffDeliveryEnglish { get; set; }

        //Координаты
        public float? LocalityOffDeliveryLat { get; set; }
        public float? LocalityOffDeliveryLon { get; set; }

        public string CommentExpert { get; set; }
        public string CommentEditor { get; set; }

        public bool FinishProcessing { get; set; }

        //Состояние               
        public int? StatesId { get; set; } 
        public virtual States States { get; set; }

        //Регион
        public int? RegionsId { get; set; }
        public virtual Regions Regions { get; set; }

        //Коллекция
        public int? CollectionId  { get; set; }

        //Дата на мусульманском в описательном виде
        public string ArabicDateText { get; set; }

        //Является частью сборником сочинений
        public bool? isCollectedBooks { get; set; }
        public string InventoryCollectedBook { get; set; }
        public string CollectedNameText { get; set; }

        //Содержит экстримистское содержание
        public bool? isExrimistStatements { get; set; }
        public string exrimistStatementsText { get; set; }
    }

    public class Collection
    {
        public int CollectionId { get; set; }
        public string CollectionName { get; set; }
    }

    public class Regions
    {
        public int RegionsId { get; set; }
        public string RegionName { get; set; }
    }

    public class States
    {                
        public int StatesId { get; set; }
        public string StateDescription { get; set; }
    }

    //Справочник языков
    public class LanguageTableList
    {
        [Key]        
        public int LanguageTableListId { get; set; }        
        public string LanguageTableName { get; set; }        
    }

    public class LanguageList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LanguageId { get; set; }

        //Ссылка на языки        
        [ForeignKey("LanguageTableList")]
        public int LanguageTableListId { get; set; }
        public LanguageTableList LanguageTableList { get; set; }

        //Ссылка на книгу        
        [ForeignKey("Books")]
        public int BooksId { get; set; }
        public Books Books { get; set; }
    }
    
    //Справочник тематик
    public class ThemesLanguagesList
    {
        public int ThemesLanguagesListId { get; set; }
        public string DescriptionThemeRussian { get; set; }
        public string DescriptionThemeEnglish { get; set; }
        public string DescriptionThemeArabic { get; set; }                 
    }

    //Справочник типов
    public class TypesLanguagesList
    {   
        public int TypesLanguagesListId { get; set; }
        public string DescriptionTypeRussian { get; set; }
        public string DescriptionTypeEnglish { get; set; }
        public string DescriptionTypeArabic { get; set; }
    }


    public class Pages
    {        
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DateTimeCreated { get; set; }
        public string BlobImage { get; set; }
        public string FileName { get; set; }
        public string FileNamePreview { get; set; }
        public virtual Books Book { get; set; }
        public virtual ICollection<RectAreasViewer> RectAreasViewer { get; set; }
        public virtual ICollection<RectAreasEditor> RectAreasEditor { get; set; }        
        public bool Visible { get; set; }
        [NotMapped]
        public string HashFolder { get; set; }
    }

    public class RectAreasViewer
    {
        [Key]
        public int RectAreasViewerId { get; set; }
        public int PagesId { get; set; }
        public virtual Pages Pages { get; set; }
        public string RectForViewerSVG { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DescriptionWithoutHTML { get; set; }
        public string Tag { get; set; }
        public string BlobFragment { get; set; }
        //Custom attributes
        public string Person { get; set; }
        public string Event { get; set; }
        public string Place { get; set; }
        public string Organization { get; set; }
        public string ArticleTitle { get; set; }
    }
    
    public class RectAreasEditor
    {
        [Key]
        public int RectAreasEditorId { get; set; }
        public int PagesId { get; set; }
        public virtual Pages Pages { get; set; }
        public string RectEditorSVGOriginal { get; set; }        
    }

    /* Autocomplete */
    public class PersonCatalog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int PersonCatalogId { get; set; }
        public string Person { get; set; }
    }

    public class EventCatalog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int EventCatalogId { get; set; }
        public string Event { get; set; }
    }

    public class PlaceCatalog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int PlaceCatalogId { get; set; }
        public string Place { get; set; }
    }

    public class OrganizationCatalog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int OrganizationCatalogId { get; set; }
        public string Organization { get; set; }
    }

    public class CustomAttributesApp
    {        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomStyleAppId { get; set; }
        public string StyleName { get; set; }
        public bool isActive { get; set; }
        public string StyleIndexCSS { get; set; }
        public string StyleOtherPagesCSS { get; set; }        
        public string TitleText { get; set; }
    }

    public class Log
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }
        public int UserId { get; set; }
        public virtual User.User User { get; set; }
        public string LogText { get; set; }
        public DateTime DateLog { get; set; }
    }

    public class ChatMessages
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string IdStringSocket { get; set; }
        public string MessageText { get; set; }
        public DateTime dt { get; set; }        
        public int UserId { get; set; }
        public virtual User.User User { get; set; }
        public bool isAnswer { get; set; }
        public string MessageAnswerId { get; set; }
        public int? PageId { get; set; }
    }

    public class MessagesChatComplaints
    {
        public int Id { get; set; }
        //Пользователь, который пожаловался
        public int UserIdVote { get; set; }
        //Идентификатор сообщения
        public string IdStringSocketId { get; set; }
        //Пользователь, на которого пожаловались
        public int UserHasVote { get; set; }
        public int BookId { get; set; }
        public DateTime DtReport { get; set; }
    }

    public class MessagesChatBlackList
    {
        public int Id { get; set; }        
        public int UserId{ get; set; }        
        public int BookId { get; set; }        
    }

    public class TranslateLanguage
    {
        public int Id { get; set; }
        public string NameOnPage { get; set; }
        public string Rus { get; set; }
        public string Eng { get; set; }
        public string Ar { get; set; }
        public string Tat { get; set; }
        /// <summary>
        /// Для какой страницы
        /// </summary>
        public int OnPageId { get; set; }
    }

    /// <summary>
    /// Не доделано
    /// </summary>
    [NotMapped]    
    public class RequestVolunteer
    {
        public int Id { get; set; }
        public int Message { get; set; }
        public DateTime RequestDt { get; set; }
        public int UserId { get; set; }
    }
}