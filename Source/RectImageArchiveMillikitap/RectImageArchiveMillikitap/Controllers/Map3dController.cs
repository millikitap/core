using RectImageArchiveMillikitap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RectImageArchiveMillikitap.Controllers
{
    public class Map3dController : Controller
    {
        /// <summary>
        /// Родовое поместье
        /// Отрисовка объектов в 3d
        /// Места, откуда были доставлены рукописи       
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index()
        {
            var JSONinfoPageHistory = await new BLogicMillikitap().getJSONHistoryMainPage();
            string d = JsonSerializer.Serialize(JSONinfoPageHistory);
            ViewData["JSONinfoPageHistory"] = JsonSerializer.Serialize(JSONinfoPageHistory);
            return View();
        }

        /// <summary>
        /// Страница Добавления/Редактирования
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Editor(int? HistoryMainPagesId)
        {
            return View();
        }
    }
}