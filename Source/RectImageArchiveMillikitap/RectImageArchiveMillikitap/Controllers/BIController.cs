using Newtonsoft.Json;
using RectImageArchiveMillikitap.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using static RectImageArchiveMillikitap.Models.CustomAuthentication.AttributesMethod;

namespace RectImageArchiveMillikitap.Controllers
{    
    public class BIController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> generateCSV(string securityCode)
        {
            var bl = new BLogicMillikitap();
            var res = await bl.getDataBI(securityCode);
            Helper.saveToCsv(res, string.Empty);
            return new JsonResult
            {
                MaxJsonLength = int.MaxValue,
                Data = new { status = "OK", count = res?.Count() },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        /// <summary>
        /// Плоская таблица для PowerBI
        /// </summary>
        /// <returns></returns>        
        public FileStreamResult getFile()
        {
            string serverPathCSV = HttpContext.Server.MapPath($"~/CSV/{"CSV_export"}.csv");
            var stream = new FileStream(serverPathCSV, FileMode.Open);
            var fileName = "CSV_export.csv";
            var mimeType = "text/csv";
            return new FileStreamResult(stream, mimeType)
            {
                FileDownloadName = fileName
            };
        }
    }
}