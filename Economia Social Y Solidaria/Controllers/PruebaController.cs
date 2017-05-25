using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class PruebaController : Controller
    {
        //
        // GET: /Prueba/

        public ActionResult Prueba()
        {
            return View();
        }

        public JsonResult GenerarLink(int cantidad)
        {
            //MP mp = new MP("8814347100096265", "JtKadEIMTzl7tp7ZpvWAX1VH3xCNQqFg");

            //String preferenceData = "{\"items\":" +
            //    "[{" +
            //        "\"title\":\"Multicolor kite\"," +
            //        "\"quantity\":1," +
            //        "\"currency_id\":\"ARS\"," +
            //        "\"unit_price\":" + cantidad +
            //    "}]" +
            //"}";


            //Hashtable preference = mp.createPreference(preferenceData.ToString());
            //Hashtable resp = (Hashtable) preference["response"];
            //var link = resp["sandbox_init_point"];

            return Json(new { error = "", link = "" /*link*/ }, JsonRequestBehavior.DenyGet);
        }

    }
}
