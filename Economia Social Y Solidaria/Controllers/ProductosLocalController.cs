using Economia_Social_Y_Solidaria.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class ProductosLocalController : Controller
    {
        //
        // GET: /ProductosLocal/

        public JsonResult Lista(int idLocal)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            var listado = ctx.ProductosLocales.Where(a => a.localId == idLocal).Select(a => new
            {
                idProducto = a.Productos.idProducto,
                nombre = a.Productos.producto + " - " + a.Productos.presentacion + (a.Productos.marca != null ? "\n" + a.Productos.marca : "")
            }).ToList();

            return Json(new { Result = "OK", Records = listado }, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Asignar(int idLocal, int[] ids)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Locales local = ctx.Locales.FirstOrDefault(a => a.idLocal == idLocal);

            List<ProductosLocales> todos = ctx.ProductosLocales.Where(a => a.localId == local.idLocal).ToList();
            foreach (ProductosLocales prod in todos)
            {
                ctx.ProductosLocales.Remove(prod);
            }

            foreach (int idprod in ids)
            {
                ProductosLocales pl = new ProductosLocales();
                pl.Locales = local;
                pl.Productos = ctx.Productos.FirstOrDefault(a => a.idProducto == idprod);
                ctx.ProductosLocales.Add(pl);
            }

            ctx.SaveChanges();


            return Json(new { bien = true }, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Replicar(int idLocalc, int[] ids)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Locales acopiar = ctx.Locales.FirstOrDefault(a => a.idLocal == idLocalc);

            foreach (int idlocal in ids)
            {

                List<ProductosLocales> todos = ctx.ProductosLocales.Where(a => a.localId == idlocal).ToList();
                foreach (ProductosLocales prod in todos)
                {
                    ctx.ProductosLocales.Remove(prod);
                }

                if (idlocal != idLocalc)
                {

                    List<ProductosLocales> productos = ctx.ProductosLocales.Where(a => a.localId == acopiar.idLocal).ToList();
                    foreach (ProductosLocales prod in productos)
                    {
                        ProductosLocales pl = new ProductosLocales();
                        pl.Locales = ctx.Locales.FirstOrDefault(a => a.idLocal == idlocal);
                        pl.Productos = prod.Productos;
                        ctx.ProductosLocales.Add(pl);
                    }
                }

            }

            ctx.SaveChanges();


            return Json(new { bien = true }, JsonRequestBehavior.DenyGet);
        }



    }
}
