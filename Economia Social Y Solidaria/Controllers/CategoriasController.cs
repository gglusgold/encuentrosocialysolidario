using Economia_Social_Y_Solidaria.Models;
using System.Linq;
using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class CategoriasController : Controller
    {
        //
        // GET: /Categorias/

        public ActionResult Categorias()
        {
            return View();
        }

        public JsonResult ListaCategorias(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = "Id ASC", string nombre = null)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var listaFinal = ctx.Categorias.ToList().Select(a => new
            {
                idCategoria = a.idCategoria,
                nombre = a.nombre
            });

            switch (jtSorting)
            {
                default:
                    listaFinal = listaFinal.OrderByDescending(a => a.idCategoria);
                    break;

                case "nombre ASC":
                    listaFinal = listaFinal.OrderBy(a => a.nombre);
                    break;

                case "nombre DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.nombre);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(nombre))
                listaFinal = listaFinal.Where(a => a.nombre.Contains(nombre)).ToList();

            int size = listaFinal.Count();

            if (jtPageSize > 0 || jtStartIndex > 0)
                listaFinal = listaFinal.Skip(jtStartIndex).Take(jtPageSize).ToList();

            return Json(new { Result = "OK", Records = listaFinal, TotalRecordCount = size });
        }

        public JsonResult Crear(string nombre)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Categorias cat = ctx.Categorias.FirstOrDefault(a => a.nombre.Trim().ToLower() == nombre.Trim().ToLower());
            if (cat == null)
            {
                cat = new Categorias { nombre = nombre };
                ctx.Categorias.Add(cat);
                ctx.SaveChanges();

                var devuelta = new
                {
                    idCategoria = cat.idCategoria,
                    nombre = cat.nombre
                };

                return Json(new { Result = "OK", Record = devuelta }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(new { Result = "ERROR", Message = "Ya existe la categoria" }, JsonRequestBehavior.DenyGet);
            }
        }

        public JsonResult Editar(int idCategoria, string nombre)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Categorias cat = ctx.Categorias.FirstOrDefault(a => a.idCategoria == idCategoria);
            if (cat != null)
            {
                cat.nombre = nombre;
                ctx.SaveChanges();

                var devuelta = new
                {
                    idCategoria = cat.idCategoria,
                    nombre = cat.nombre
                };

                return Json(new { Result = "OK", Record = devuelta }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                return Json(new { Result = "ERROR", Message = "No existe la categoria" }, JsonRequestBehavior.DenyGet);
            }
        }

        public JsonResult Borrar(int idCategoria)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Categorias cat = ctx.Categorias.FirstOrDefault(a => a.idCategoria == idCategoria);
            if (cat.Productos.Count > 0)
            {
                return Json(new { Result = "ERROR", Message = "La categoria contiene productos, no se puede eliminar" }, JsonRequestBehavior.DenyGet);
            }
            else
            {
                ctx.Categorias.Remove(cat);
                ctx.SaveChanges();

                return Json(new { Result = "OK" }, JsonRequestBehavior.DenyGet);
            }
           
        }

        public JsonResult ListaProductos(int idCategoria)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            var listado = ctx.Productos.Where(a => a.categoriaId == idCategoria).Select(a => new
            {
                idProducto = a.idProducto,
                nombre = a.producto + " - " + a.presentacion + (a.marca != null ? "\n" + a.marca : "")
            }).ToList();

            return Json(new { Result = "OK", Records = listado }, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Lista()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var lista = ctx.Categorias.Select(a => new
            {
                DisplayText = a.nombre,
                Value = a.idCategoria
            });

            return Json(new { Options = lista });
        }

        public JsonResult ListaJ()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var lista = ctx.Categorias.Select(a => new
            {
                DisplayText = a.nombre,
                Value = a.idCategoria
            });

            return Json(new { Result = "OK", Options = lista }, JsonRequestBehavior.AllowGet);
        }

    }
}
