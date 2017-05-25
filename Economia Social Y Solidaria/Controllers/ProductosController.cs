using Economia_Social_Y_Solidaria.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{

    public class Asignacion
    {
        public int idProducto { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }

        public int idLocal { get; set; }
        public string direccion { get; set; }

    }

    public class ProductosController : Controller
    {
        private string rutaImagen = AppDomain.CurrentDomain.BaseDirectory + "Imagenes\\";
        //
        // GET: /Productos/

        public ActionResult Productos()
        {
            return View();
        }

        public JsonResult Lista(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = "Id ASC", string nombre = null, string marca = null, string descripcion = null, string cantidad = null, decimal precio = -1, decimal costo = -1, int stock = -2, int idCategoria = -1, bool todos = true, int idLocal = -1)
        {
            //string path = HttpContext.Request.Url.AbsolutePath;
            //bool todos = true;
            TanoNEEntities ctx = new TanoNEEntities();
            var listaFinal = ctx.Productos.Where(a => todos == true ? (a.activo == true || a.activo == false) : a.activo == !todos)/*.OrderBy(a => a.idProducto).Skip(offset).Take(limit)*/.ToList().Select(a => new
            {
                idProducto = a.idProducto,
                nombre = a.producto,
                stock = a.stock,
                marca = a.marca,
                descripcion = a.descripcion != null ? a.descripcion.Replace("\n", "<br/>") : "",
                proveedor = a.proveedor,
                variedad = a.variedad,
                cantidad = a.presentacion,
                imagen = System.IO.File.Exists(HttpContext.Server.MapPath("/Imagenes/Producto-" + a.idProducto + ".jpg")) ? "/Imagenes/Producto-" + a.idProducto + ".jpg" : "/Imagenes/Fijas/pp.jpeg",
                idCategoria = a.categoriaId,
                categoria = a.Categorias.nombre,
                precio = a.Precios.LastOrDefault().precio,
                costo = a.Costos.LastOrDefault().costo,
                borrar = a.activo,
                selected = idLocal != -1 ? a.ProductosLocales.Count > 0 : false
            });

            switch (jtSorting)
            {
                default:
                    listaFinal = listaFinal.OrderByDescending(a => a.idProducto);
                    break;

                case "nombre ASC":
                    listaFinal = listaFinal.OrderBy(a => a.nombre);
                    break;

                case "nombre DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.nombre);
                    break;

                case "marca ASC":
                    listaFinal = listaFinal.OrderBy(a => a.marca);
                    break;

                case "marca DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.marca);
                    break;

                case "descripcion ASC":
                    listaFinal = listaFinal.OrderBy(a => a.descripcion);
                    break;

                case "descripcion DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.descripcion);
                    break;

                case "cantidad ASC":
                    listaFinal = listaFinal.OrderBy(a => a.cantidad);
                    break;

                case "cantidad DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.cantidad);
                    break;

                case "categoria ASC":
                    listaFinal = listaFinal.OrderBy(a => a.categoria);
                    break;

                case "categoria DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.categoria);
                    break;

                case "precio ASC":
                    listaFinal = listaFinal.OrderBy(a => a.precio);
                    break;

                case "precio DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.precio);
                    break;

                case "costo ASC":
                    listaFinal = listaFinal.OrderBy(a => a.costo);
                    break;

                case "costo DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.costo);
                    break;

                case "stock ASC":
                    listaFinal = listaFinal.OrderBy(a => a.stock);
                    break;

                case "stock DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.stock);
                    break;

                case "borrar ASC":
                    listaFinal = listaFinal.OrderBy(a => a.borrar);
                    break;

                case "borrar DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.borrar);
                    break;

            }

            if (!string.IsNullOrWhiteSpace(nombre))
                listaFinal = listaFinal.Where(a => a.nombre.Contains(nombre)).ToList();

            if (!string.IsNullOrWhiteSpace(marca))
                listaFinal = listaFinal.Where(a => a.marca.Contains(marca)).ToList();

            if (!string.IsNullOrWhiteSpace(descripcion))
                listaFinal = listaFinal.Where(a => a.marca.Contains(descripcion)).ToList();

            if (!string.IsNullOrWhiteSpace(cantidad))
                listaFinal = listaFinal.Where(a => a.nombre.Contains(cantidad)).ToList();

            if (idCategoria > 0)
                listaFinal = listaFinal.Where(a => a.idCategoria == idCategoria).ToList();

            if (precio > 0)
                listaFinal = listaFinal.Where(a => a.precio == precio).ToList();

            if (costo > 0)
                listaFinal = listaFinal.Where(a => a.costo == costo).ToList();

            if (stock > -2)
                listaFinal = listaFinal.Where(a => a.stock == stock).ToList();

            int size = listaFinal.Count();

            if (jtPageSize > 0 || jtStartIndex > 0)
                listaFinal = listaFinal.Skip(jtStartIndex).Take(jtPageSize).ToList();

            return Json(new { Result = "OK", Records = listaFinal, TotalRecordCount = size });

            //return Json(new { total = lista.Count(), rows = lista }, JsonRequestBehavior.DenyGet);
            //return Json(lista, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Crear(string nombre, string marca, string descripcion, string proveedor, string variedad, HttpPostedFileBase imagen, string cantidad, int idCategoria, decimal precio, decimal costo, int stock)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Productos item = ctx.Productos.FirstOrDefault(a => a.producto == nombre && a.marca == marca && a.presentacion == cantidad);
            if (item == null)
            {
                Categorias categoria = ctx.Categorias.FirstOrDefault(a => a.idCategoria == idCategoria);
                item = new Productos();
                item.producto = nombre;
                item.marca = marca;
                item.descripcion = descripcion;
                item.stock = stock;
                item.proveedor = proveedor;
                item.variedad = variedad;
                item.presentacion = cantidad;
                item.Categorias = categoria;
                item.activo = true;

                Precios pre = new Precios();
                pre.precio = precio;
                pre.fecha = DateTime.Now;
                item.Precios.Add(pre);

                Costos cos = new Costos();
                cos.costo = costo;
                cos.fecha = DateTime.Now;
                item.Costos.Add(cos);

                ctx.Productos.Add(item);
                ctx.SaveChanges();

                if (imagen != null)
                {
                    string savedFileName = rutaImagen + "Producto-" + item.idProducto + imagen.FileName.Substring(imagen.FileName.LastIndexOf("."));
                    imagen.SaveAs(savedFileName);
                }
            }
            else
            {
                return Json(new { error = true, mensaje = "Ya existe el producto" }, JsonRequestBehavior.DenyGet);
            }

            var devuelta = new
            {
                idProducto = item.idProducto,
                nombre = item.producto,
                stock = item.stock,
                marca = item.marca,
                descripcion = item.descripcion != null ? item.descripcion.Replace("\n", "<br/>") : "",
                proveedor = item.proveedor,
                variedad = item.variedad,
                cantidad = item.presentacion,
                imagen = System.IO.File.Exists(HttpContext.Server.MapPath("/Imagenes/Producto-" + item.idProducto + ".jpg")) ? "/Imagenes/Producto-" + item.idProducto + ".jpg" : "/Imagenes/Fijas/pp.jpeg",
                idCategoria = item.categoriaId,
                categoria = item.Categorias.nombre,
                precio = item.Precios.LastOrDefault().precio,
                costo = item.Costos.LastOrDefault().costo,
                borrar = item.activo
            };

            return Json(new { Result = "OK", Record = devuelta }, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Editar(int idProducto, string nombre, string marca, string descripcion, string proveedor, string variedad, HttpPostedFileBase imagen, string cantidad, int idCategoria, decimal precio, decimal costo, int stock)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Productos item = ctx.Productos.FirstOrDefault(a => a.idProducto == idProducto);
            if (item != null)
            {
                if (ctx.Productos.FirstOrDefault(a => a.producto == nombre && a.marca == marca && a.presentacion == cantidad && a.idProducto != idProducto) != null)
                {
                    return Json(new { error = true, mensaje = "Ya existe otro producto de esas caracteristicas" }, JsonRequestBehavior.DenyGet);
                }

                Categorias categoria = ctx.Categorias.FirstOrDefault(a => a.idCategoria == idCategoria);
                item.marca = marca;
                item.descripcion = descripcion;
                item.presentacion = cantidad;
                item.stock = stock;
                item.proveedor = proveedor;
                item.variedad = variedad;
                item.Categorias = categoria;

                Precios pre = new Precios();
                pre.precio = precio;
                pre.fecha = DateTime.Now;
                item.Precios.Add(pre);

                Costos cos = new Costos();
                cos.costo = costo;
                cos.fecha = DateTime.Now;
                item.Costos.Add(cos);

                ctx.SaveChanges();

                if (imagen != null)
                {
                    string savedFileName = rutaImagen + "Producto-" + item.idProducto + imagen.FileName.Substring(imagen.FileName.LastIndexOf("."));
                    imagen.SaveAs(savedFileName);
                }
            }
            else
            {
                return Json(new { error = true, mensaje = "No existe el producto" }, JsonRequestBehavior.DenyGet);
            }

            var devuelta = new
            {
                idProducto = item.idProducto,
                nombre = item.producto,
                stock = item.stock,
                marca = item.marca,
                descripcion = item.descripcion != null ? item.descripcion.Replace("\n", "<br/>") : "",
                cantidad = item.presentacion,
                imagen = System.IO.File.Exists(HttpContext.Server.MapPath("/Imagenes/Producto-" + item.idProducto + ".jpg")) ? "/Imagenes/Producto-" + item.idProducto + ".jpg" : "/Imagenes/Fijas/pp.jpeg",
                idCategoria = item.categoriaId,
                categoria = item.Categorias.nombre,
                precio = item.Precios.LastOrDefault().precio,
                costo = item.Costos.LastOrDefault().costo,
                borrar = item.activo
            };

            return Json(new { Result = "OK", Record = devuelta }, JsonRequestBehavior.DenyGet);

            //return Json(new { error = false, mensaje = "Producto editado satisfactoriamente" }, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Borrar(int idProducto)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Productos item = ctx.Productos.FirstOrDefault(a => a.idProducto == idProducto);
            if (item != null)
            {
                item.activo = !item.activo;
                ctx.SaveChanges();

                //string savedFileName = rutaImagen + "Producto-" + item.idProducto + ".jpg";
                //if (System.IO.File.Exists(savedFileName))
                //{
                //    System.IO.File.Delete(savedFileName);
                //}
            }
            else
            {
                return Json(new { error = true, mensaje = "No existe el producto" }, JsonRequestBehavior.DenyGet);
            }

            return Json(new { Result = "OK" }, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Muchos(HttpPostedFileBase archivo)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            string[] result = new StreamReader(archivo.InputStream).ReadToEnd().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string linea in result)
            {
                string[] usar = linea.Replace("\r", "").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                string nombre = usar[0];
                string marca = usar[1];
                string presentacion = usar[2];
                int cate = int.Parse(usar[3]);
                decimal precio = decimal.Parse(usar[4], new NumberFormatInfo() { NumberDecimalSeparator = "," });
                decimal costo = decimal.Parse(usar[5], new NumberFormatInfo() { NumberDecimalSeparator = "," });

                Productos item = ctx.Productos.FirstOrDefault(a => a.producto == nombre && a.marca == marca && a.presentacion == presentacion);
                if (item == null)
                {
                    Categorias categoria = ctx.Categorias.FirstOrDefault(a => a.idCategoria == cate);
                    item = new Productos();
                    item.producto = nombre;
                    item.marca = marca;
                    item.presentacion = presentacion;
                    item.Categorias = categoria;
                    item.activo = true;

                    Precios pre = new Precios();
                    pre.precio = precio;
                    pre.fecha = DateTime.Now;
                    item.Precios.Add(pre);

                    Costos cos = new Costos();
                    cos.costo = costo;
                    cos.fecha = DateTime.Now;
                    item.Costos.Add(cos);

                    ctx.Productos.Add(item);
                }

            }

            ctx.SaveChanges();

            return Json(new { error = false, mensaje = "Producto grabado satisfactoriamente" }, JsonRequestBehavior.DenyGet);
        }

        public ActionResult Asignar()
        {
            //List
            return View();
        }

    }

}
