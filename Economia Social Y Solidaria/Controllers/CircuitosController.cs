using Economia_Social_Y_Solidaria.Models;
using System.Linq;
using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class CircuitosController : Controller
    {
        public JsonResult Lista()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var lista = ctx.Circuitos.Select(a => new
            {
                DisplayText = a.nombre,
                Value = a.idCircuito
            });

            return Json(new { Result = "OK", Options = lista }, JsonRequestBehavior.AllowGet);
        }
    }
}