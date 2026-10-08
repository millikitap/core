using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace RectImageArchiveMillikitap.Models
{
    public class Helper
    {
        public class MemoryPostedFile : HttpPostedFileBase
        {
            private readonly byte[] fileBytes;

            public MemoryPostedFile(byte[] fileBytes, string fileName = null)
            {
                this.fileBytes = fileBytes;
                this.FileName = fileName;
                this.InputStream = new MemoryStream(fileBytes);
            }

            public override int ContentLength => fileBytes.Length;

            public override string FileName { get; }

            public override Stream InputStream { get; }
        }

        /// <summary>
        /// Список поддерживаемых языков на сайте
        /// </summary>
        public static class LanguagesIdAStatic
        {
            public static int Ru = 1;
            public static int En = 2;
            public static int Ar = 3;
            public static int Ta = 4;
        }

        /// <summary>
        /// Идентификаторы представлений для выборки языка
        /// </summary>
        public static class PagesIdStatic
        {           
            /// Авторизация/Регистрация            
            public static int SignInSignOut = 1; //+
           
            /// Двухфакторная аутентификация           
            public static int TwoFactorAuth = 2; //+
            
            /// Просмотр/Редактирование профиля            
            public static int Account = 3;//+
            
            /// Просмотр книги            
            public static int View = 4;//+
            
            /// Главная страница с поиском            
            public static int Index = 5; // +

            /// Частичное представление поиска и группировки результата           
            public static int SearchPartialMain = 6;//+
            
            /// Алфавитный указатель            
            public static int TagsCloud = 7; // +-доработать перевод в скриптах

            /// Алфавитный указатель            
            public static int Favorite = 8; //+
        }        

        public static string CreateMD5(string input)
        {
            //unique id
            int unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(string.Join("{0}{1}", unixTimestamp.ToString(),input));
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        
        public static byte[] getBytesFromStream (Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public enum dtFormat
        {
            timeOnly = 0,
            dateTime = 1
        }     

        public static string getUTCStringTime(dtFormat format)
        {
            var now = DateTime.UtcNow;            
            switch (format)
            {
                case dtFormat.timeOnly:
                    {
                        return now.ToString("HH:mm");
                    };
                case dtFormat.dateTime:
                    {
                        return now.ToString("dd.MM.yyyy HH:mm");
                    };
            }
            return "";
        }
        
        /// <summary>
        /// Сохранение в файл для импорта CSV
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="path"></param>
        public static void saveToCsv<T>(List<T> data, string path)
        {
            var lines = new List<string>();
            IEnumerable<PropertyDescriptor> props = TypeDescriptor.GetProperties(typeof(T)).OfType<PropertyDescriptor>();
            var header = string.Join("$", props.ToList().Select(x => x.Name));
            lines.Add(header);
            var valueLines = data.Select(row => string.Join("$", header.Split('$').Select(a => row.GetType().GetProperty(a).GetValue(row, null))));
            lines.AddRange(valueLines);
            string serverPathCSV = HttpContext.Current.Server.MapPath($"~/CSV/{"CSV_export"}.csv");
            File.WriteAllLines(serverPathCSV, lines.ToArray(), Encoding.UTF8);
        }
        
        /// <summary>
        /// Создание одноразового пароля на основе счетчика
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="iterationNumber"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        public static string GeneratePassword(string secret, long iterationNumber, int digits = 6)
        {
            byte[] counter = BitConverter.GetBytes(iterationNumber);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(counter);
            byte[] key = Encoding.ASCII.GetBytes(secret);
            HMACSHA1 hmac = new HMACSHA1(key, true);
            byte[] hash = hmac.ComputeHash(counter);
            int offset = hash[hash.Length - 1] & 0xf;
            int binary =
                ((hash[offset] & 0x7f) << 24)
                | ((hash[offset + 1] & 0xff) << 16)
                | ((hash[offset + 2] & 0xff) << 8)
                | (hash[offset + 3] & 0xff);
            int password = binary % (int)Math.Pow(10, digits);
            return password.ToString(new string('0', digits));
        }


        /// <summary>
        /// Отправка кода на email
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool SendToEmailCode(User.User user, string theme, string message)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("@millicitap.com");
            mailMessage.To.Add(new MailAddress(user.Username));
            mailMessage.Subject = theme;
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = message;
            SmtpClient client = new SmtpClient();
            client.Credentials = new System.Net.NetworkCredential("server SMTP", "password SMTP");
            client.Host = "smtp.secureserver.net";
            client.Port = 443;
            try
            {
                client.Send(mailMessage);
                return true;
            }
            catch
            {
                return false;
            }            
        }        
    }

    public static class EnumTypes
    {
        public enum AutoEnum
        {
            Person,
            Event,
            Place,
            Organization,

            //Attributes
            Theme,
            Type
        }

        public enum LogType
        {
            AddBook,
            ChangeBook,
            DeleteBook,
            AddPage,
            ChangePage,
            DeletePage,
            UploadFilePDF
        }

        /// <summary>
        /// Список ролей
        /// </summary>
        public static class Roles
        {   
            public static string User = "User";
            public static string Editor = "Editor";            
            public static string Admin = "Admin";
            public static string Expert = "Expert";
            public static string Uploader = "Uploader";
        }

        /// <summary>
        /// Список действий пользователя для логирования
        /// </summary>
        public enum MethodTypeList
        {
            Add,
            Update,
            Delete,
            GetAll
        }        
    }

    public static class utils
    {
        public static bool insensitiveContains(this string text, string value,
        StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {   
            if(text == null || value == null)
            {
                return false;
            }
            return text.IndexOf(value, stringComparison) >= 0;
        }

        public static DateTime? GregorianToUmAlQura(this DateTime? gregorianDate)
        {   
            if(!gregorianDate.HasValue)
            {
                return null;
            }
            Calendar umAlQura = new UmAlQuraCalendar();
            return new DateTime(umAlQura.GetYear(gregorianDate.Value), umAlQura.GetMonth(gregorianDate.Value), umAlQura.GetDayOfMonth(gregorianDate.Value), umAlQura);
        }

        /// <summary>
        /// Удалить все переносы из строки
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RemoveLineEndings(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }
            string lineSeparator = ((char)0x2028).ToString();
            string paragraphSeparator = ((char)0x2029).ToString();

            return value.Replace("\r\n", string.Empty)
                        .Replace("\n", string.Empty)
                        .Replace("\r", string.Empty)
                        .Replace(lineSeparator, string.Empty)
                        .Replace(paragraphSeparator, string.Empty);
        }
    }

    public static class messages
    {
        public static string NotPermission = "У Вас нет разрешения для данной операции";
    }  
}