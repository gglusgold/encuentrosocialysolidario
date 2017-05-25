using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Economia_Social_Y_Solidaria.Models;
using SpreadsheetLight;
using SpreadsheetLight.Drawing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class VecinxsComunaAux
    {
        public int Comuna { get; internal set; }
        public string ComunaNombre { get; internal set; }
        public IEnumerable<VecinxsAux> Vecinos { get; set; }

        public class VecinxsAux
        {
            public int IdVecinx { get; internal set; }
            public string Nombre { get; set; }
        }
    }



    public class ComentariosAux
    {
        public string vecinx;
        public string comentario;
        public int estrellas;
        public int haceCuanto;
    }

    public class Changuito
    {
        public int idProducto { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public decimal precio = 0;

        public int comentarios = 0;
        public double rating = 0;
        public int vendidos;

        public int categoria { get; set; }

        public int stock { get; set; }
        public List<ComentariosAux> comentario { get; set; }
    }

    public class ChanguitoCompleta
    {
        public int totalCompraTandaUsuario = 0;
        public string proxFecha { get; set; }
        public List<Locales> locales = new List<Locales>();

        public List<Cat> categorias = new List<Cat>();
        public IEnumerable<Changuito> changuito;
    }

    public class Cat
    {
        public int idCategoria;
        public string nombre;
    }

    public class ProductosComprados
    {
        public int idProducto;
        public int cantidad;
        public string nombre;
        public string marca;
        public string presentacion;
        public bool comentado;
        public decimal precioUnidad;
    }

    public class Comprados
    {
        public int idCompra;
        public int comuna;
        public int idLocal;
        public string local;
        public string barrio;
        public string fecha;
        public bool editar = false;
        public bool comentar = false;
        public List<ProductosComprados> productos;
        public string estado;
        public bool comentado { get; set; }
    }

    public class MisCompras
    {
        public List<Comprados> Compras = new List<Comprados>();
    }

    public class ComprasController : Controller
    {
        public ActionResult Carrito(int idCategoria = -1, int idLocal = -1)
        {

            ChanguitoCompleta completo = new ChanguitoCompleta();
            crearChango(completo, idCategoria, idLocal);

            DateTime ProximaEntrea = ApiProductosController.GetNextWeekday();
            completo.proxFecha = ProximaEntrea.ToString("dd/MM/yyyy");



            return View(completo);
        }

        public ActionResult cambioCategoria(int idCategoria, int idLocal = -1, int ordenar = 2)
        {
            ChanguitoCompleta completo = new ChanguitoCompleta();
            crearChango(completo, idCategoria, idLocal, ordenar);

            DateTime ProximaEntrea = ApiProductosController.GetNextWeekday();
            completo.proxFecha = ProximaEntrea.ToString("dd/MM/yyyy");

            return Json(new { lista = completo.changuito });
        }

        public ChanguitoCompleta crearChango(ChanguitoCompleta completo, int idCategoria = -1, int idLocal = -1, int ordenar = 2)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Tandas ultima = ctx.Tandas.ToList().LastOrDefault();
            if (ultima != null && ultima.fechaCerrado == null)
                completo.locales = ultima.Circuitos.Locales.Where(a => a.activo).OrderBy(a => a.comuna).ToList();

            completo.categorias = ctx.Categorias.Where(a => a.Productos.Any(b => b.activo)).Select(a => new Cat { idCategoria = a.idCategoria, nombre = a.nombre }).OrderBy(a => a.nombre).ToList();

            if (User.Identity.IsAuthenticated && ultima != null)
            {
                EstadosCompra EstadoEntregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);

                Vecinos actual = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
                completo.totalCompraTandaUsuario = ctx.Compras.Where(a => a.vecinoId == actual.idVecino && a.tandaId == ultima.idTanda && a.estadoId == EstadoEntregado.idEstadoCompra).Count();
            }

            Categorias cat = ctx.Categorias.FirstOrDefault(a => a.nombre == "Bolsones");
            if (idCategoria != -1)
            {
                cat = ctx.Categorias.FirstOrDefault(a => a.idCategoria == idCategoria);
                if (!cat.Productos.Any(a => a.activo))
                {
                    int cate = completo.categorias.ToArray()[0].idCategoria;
                    cat = ctx.Categorias.FirstOrDefault(a => a.idCategoria == cate);
                }
            }


            ViewBag.categoria = idCategoria == -1 ? idCategoria : cat.idCategoria;
            ViewBag.ordenar = ordenar;

            if (idLocal == -1)
            {
                completo.changuito = ctx.Productos.Where(a => (idCategoria < 0 ? a.categoriaId > idCategoria : a.categoriaId == cat.idCategoria) && a.activo).OrderBy(a => a.producto).ToList().Select(a => new Changuito()
                {
                    idProducto = a.idProducto,
                    stock = a.stock,
                    nombre = a.producto + " - " + a.presentacion + (a.marca != null ? "\n" + a.marca : ""),
                    descripcion = a.descripcion == null ? "" : a.descripcion,//.Replace("\n", "<br/>"),
                    precio = a.Precios.LastOrDefault().precio,
                    comentarios = a.ComentariosProducto.Where(comentarios => comentarios.visible).Count(),
                    vendidos = a.CompraProducto.GroupBy(b => b.productoId).Select(c => new { Id = c.Key, Cantidad = c.Count() }).Sum(d => d.Cantidad),
                    rating = a.ComentariosProducto.Where(comentarios => comentarios.visible).Count() == 0 ? 0 : a.ComentariosProducto.Where(comentarios => comentarios.visible).Average(b => b.estrellas)
                });
            }
            else
            {
                completo.changuito = ctx.Productos.Where(a => (idCategoria < 0 ? a.categoriaId > idCategoria : a.categoriaId == cat.idCategoria) && a.ProductosLocales.Any(b => b.localId == idLocal) && a.activo).OrderBy(a => a.producto).ToList().Select(a => new Changuito()
                {
                    idProducto = a.idProducto,
                    stock = a.stock,
                    nombre = a.producto + " - " + a.presentacion + (a.marca != null ? "\n" + a.marca : ""),
                    descripcion = a.descripcion == null ? "" : a.descripcion,//.Replace("\n", "<br/>"),
                    precio = a.Precios.LastOrDefault().precio,
                    comentarios = a.ComentariosProducto.Where(comentarios => comentarios.visible).Count(),
                    vendidos = a.CompraProducto.GroupBy(b => b.productoId).Select(c => new { Id = c.Key, Cantidad = c.Count() }).Sum(d => d.Cantidad),
                    rating = a.ComentariosProducto.Where(comentarios => comentarios.visible).Count() == 0 ? 0 : a.ComentariosProducto.Where(comentarios => comentarios.visible).Average(b => b.estrellas)
                });
            }

            switch (ordenar)
            {
                case 1:
                    completo.changuito = completo.changuito.OrderBy(a => a.nombre);
                    break;
                case 2:
                    completo.changuito = completo.changuito.OrderByDescending(a => a.vendidos).ThenByDescending(a => a.precio);
                    break;
                case 3:
                    completo.changuito = completo.changuito.OrderByDescending(a => a.rating).ThenByDescending(a => a.precio);
                    break;
            }



            return completo;
        }

        public ActionResult Detalle(int id)
        {
            DateTime hoy = DateTime.Today;
            TanoNEEntities ctx = new TanoNEEntities();

            Productos actual = ctx.Productos.FirstOrDefault(a => a.idProducto == id);

            return View(new Changuito()
            {
                idProducto = actual.idProducto,
                nombre = actual.producto,
                descripcion = actual.descripcion == null ? "" : actual.descripcion.Replace("\n", "<br/>"),
                precio = actual.Precios.LastOrDefault().precio,
                comentarios = actual.ComentariosProducto.Count,
                rating = actual.ComentariosProducto.Count == 0 ? 0 : actual.ComentariosProducto.Average(b => b.estrellas),
                categoria = actual.Categorias.idCategoria,
                comentario = actual.ComentariosProducto.Where(a => a.visible == true).Select(a => new ComentariosAux { comentario = a.comentario, vecinx = a.Compras.Vecinos.nombres, estrellas = a.estrellas, haceCuanto = (hoy - a.fecha).Days }).ToList()
            });
        }

        public ActionResult Historial()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
            if (vecino == null)
                return RedirectToAction("Carrito", "Compras");

            EstadosCompra EstadoEntregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);
            EstadosCompra confirmado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 3);
            EstadosCompra comentado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 4);

            //Precios ultimop = prod.Precios.Count > 1 ? prod.Precios.LastOrDefault(a => a.fecha.Date <= actual.fechaAbierto.Date) : prod.Precios.FirstOrDefault();
            Tandas ultimaTanda = ctx.Tandas.ToList().LastOrDefault(a => a.fechaCerrado == null);


            MisCompras compras = new MisCompras();
            compras.Compras = ctx.Compras.Where(a => a.vecinoId == vecino.idVecino).OrderByDescending(a => a.fecha).ToList().Select(a => new Comprados
            {
                idCompra = a.idCompra,
                estado = a.EstadosCompra.nombre,
                fecha = a.fecha.ToString("hh:mm dd/MM/yyyy"),
                idLocal = a.Locales.idLocal,
                local = a.Locales.direccion,
                barrio = a.Locales.barrio,
                editar = (ultimaTanda != null && a.Tandas.idTanda == ultimaTanda.idTanda) && a.estadoId == EstadoEntregado.idEstadoCompra,
                comentar = a.estadoId == confirmado.idEstadoCompra,
                comentado = a.estadoId == comentado.idEstadoCompra,
                comuna = a.Locales.comuna,
                productos = a.CompraProducto.ToList().Select(b => new ProductosComprados
                {
                    idProducto = b.Productos.idProducto,
                    nombre = b.Productos.producto,
                    marca = b.Productos.marca,
                    presentacion = b.Productos.presentacion,
                    cantidad = b.cantidad,
                    comentado = a.ComentariosProducto.FirstOrDefault(c => c.productoId == b.productoId) != null,  // a.Comentarios.Count == 1 ? a.Comentarios.FirstOrDefault().ComentariosProducto.FirstOrDefault(cp => cp.productoId == b.productoId).Productos != null : false,
                    precioUnidad = b.Precios.precio

                }).ToList()
            }).ToList();

            return View(compras);
        }

        public ActionResult Entregar()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos actual = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);

            var vecino = ctx.Vecinos.Where(a => a.comuna != actual.comuna).GroupBy(a => a.comuna).ToList().Select(a => new VecinxsComunaAux
            {
                Comuna = a.Key.Value, // == -1 ? "Gran Bs. As" : a.Key.ToString(),
                ComunaNombre = a.Key == -1 ? "Gran Bs. As" : a.Key.ToString(),
                Vecinos = a.Select(b => new VecinxsComunaAux.VecinxsAux { IdVecinx = b.idVecino, Nombre = new CultureInfo("en-Us", false).TextInfo.ToTitleCase(b.nombres.ToLower()) }).ToList()
            }).ToList();

            vecino.InsertRange(0, ctx.Vecinos.Where(a => a.comuna == actual.comuna).GroupBy(a => a.comuna).ToList().Select(a => new VecinxsComunaAux
            {
                Comuna = a.Key.Value, // == -1 ? "Gran Bs. As" : a.Key.ToString(),
                ComunaNombre = a.Key == -1 ? "Gran Bs. As" : a.Key.ToString(),
                Vecinos = a.Select(b => new VecinxsComunaAux.VecinxsAux { IdVecinx = b.idVecino, Nombre = new CultureInfo("en-Us", false).TextInfo.ToTitleCase(b.nombres.ToLower()) }).ToList()
            }));

            return View(vecino);
        }



        public ActionResult CancelarPedido(int idCompra)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);

            EstadosCompra EstadoEntregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);
            Compras cancelar = ctx.Compras.FirstOrDefault(a => a.idCompra == idCompra && vecino.idVecino == a.vecinoId);
            if (cancelar != null)
            {
                ctx.Compras.Remove(cancelar);
                ctx.SaveChanges();
            }

            MisCompras compras = new MisCompras();
            compras.Compras = ctx.Compras.Where(a => a.vecinoId == vecino.idVecino).ToList().Select(a => new Comprados
            {
                idCompra = a.idCompra,
                estado = a.EstadosCompra.nombre,
                fecha = a.fecha.ToString("hh:mm dd/MM/yyyy"),
                local = a.Locales.direccion,
                barrio = a.Locales.barrio,
                editar = a.estadoId == EstadoEntregado.idEstadoCompra,
                comuna = a.Locales.comuna,
                productos = a.CompraProducto.ToList().Select(b => new ProductosComprados
                {
                    idProducto = b.Productos.idProducto,
                    nombre = b.Productos.producto,
                    cantidad = b.cantidad,
                    marca = b.Productos.marca,
                    presentacion = b.Productos.presentacion,
                    //precioUnidad = b.Productos.Precios.FirstOrDefault(precio => a.fecha > precio.fecha).precio
                    precioUnidad = b.Precios.precio
                }).ToList()
            }).ToList();

            return Json(compras);
        }

        public ActionResult ConfirmarPedido(int local, int[] idProducto, int[] cantidad, int? idCompra = null)
        {
            string error = null;

            TanoNEEntities ctx = new TanoNEEntities();

            Locales localCompro = ctx.Locales.FirstOrDefault(a => a.idLocal == local);
            if (localCompro == null)
                error = "No se indico en que local va a retirar lo pedido";

            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
            if (vecino == null)
                error = "Hay que iniciar sesion para realizar un pedido";

            Tandas ultimaTanda = ctx.Tandas.ToList().LastOrDefault(a => a.fechaCerrado == null);
            if (ultimaTanda == null)
                error = "No hay circuitos abiertos en este momento";

            //Encargado
            EstadosCompra estado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);

            Compras compra = new Compras();
            if (idCompra.HasValue)
            {
                compra = ctx.Compras.FirstOrDefault(a => a.idCompra == idCompra.Value);
                compra.CompraProducto.ToList().ForEach(cs => ctx.CompraProducto.Remove(cs));
            }
            else
            {
                compra.fecha = DateTime.Now;
            }

            compra.Locales = localCompro;
            compra.Vecinos = vecino;
            compra.Tandas = ultimaTanda;
            compra.EstadosCompra = estado;

            if (!idCompra.HasValue)
                ctx.Compras.Add(compra);


            for (int x = 0; x < idProducto.Length; x++)
            {
                int prodActual = idProducto[x];
                int cantActual = cantidad[x];

                Productos prod = ctx.Productos.FirstOrDefault(a => a.idProducto == prodActual);
                if (prod.stock != -1)
                {
                    int stockrestante = prod.stock - cantActual;
                    if (stockrestante < 0)
                    {
                        error = string.Format("{0} del siguiente producto:<br/>{1} - {2} - {3}", prod.stock == 0 ? "No contamos con stock" : "Solo contamos con " + prod.stock + " articulos", prod.producto, prod.presentacion, prod.marca);
                        break;
                    }
                    else
                        prod.stock = stockrestante;
                }


                CompraProducto productos = new CompraProducto();
                productos.Productos = prod;
                productos.Compras = compra;
                productos.cantidad = cantidad[x];
                productos.Precios = prod.Precios.LastOrDefault();
                productos.Costos = prod.Costos.LastOrDefault();

                ctx.CompraProducto.Add(productos);
            }



            if (string.IsNullOrEmpty(error))
            {
                ctx.SaveChanges();
                if (!bool.Parse(ConfigurationManager.AppSettings["debug"]))
                    MandarMailConfirmandoCompra(compra.idCompra);
            }

            return Json(new { error = error }, JsonRequestBehavior.DenyGet);
        }

        private void MandarMailConfirmandoCompra(int idCompra)
        {
            DateTime ProximaEntrea = ApiProductosController.GetNextWeekday();


            TanoNEEntities ctx = new TanoNEEntities();

            Compras actual = ctx.Compras.FirstOrDefault(a => a.idCompra == idCompra);

            var vecino = ctx.AlertasVecinxs.FirstOrDefault(a => a.Vecinos.token != null && a.Alertas.codigo == 3 && a.vecinxId == actual.Vecinos.idVecino);
            string token = vecino != null ? vecino.Vecinos.token : null;
            if (token != null)
                ApiProductosController.mandarNotificacion("Pedido Confirmado", "Gracias por colaborar con la economía social y solidaria. No te olvides de venir con cambio!", "MISCOMPRAS", new string[] { token });

            string fecha = ProximaEntrea.ToString("dd/MM/yyyy") + " - " + actual.Locales.horario;
            string nombre = actual.Vecinos.nombres;
            string correo = actual.Vecinos.correo;

            using (MailMessage mail = new MailMessage())
            {

                mail.From = new MailAddress("economiasocial@encuentrocapital.com.ar", "Economía Social y Solidaria");
                mail.To.Add(correo);
                //mail.To.Add("julianlionti@hotmail.com");
                mail.Subject = "Economia Social y Solidaria -- Nuevo Encuentro";
                mail.Body = "<p>Se han confirmado las siguientes compras</p>";
                mail.BodyEncoding = System.Text.Encoding.UTF8;

                List<Compras> totalCompras = ctx.Compras.Where(a => a.tandaId == actual.Tandas.idTanda && a.vecinoId == actual.Vecinos.idVecino).ToList();
                foreach (var compras in totalCompras)
                {
                    mail.Body += "<p>-------------------</p>";
                    mail.Body += "<p>Compra N° " + (totalCompras.IndexOf(compras) + 1) + "</p>";
                    mail.Body += "<p><b>Lo tenés que pasar a retirar el dia " + fecha + " Por nuestro local en " + actual.Locales.direccion + "</b></p>";

                    decimal total = 0;
                    foreach (CompraProducto prod in compras.CompraProducto)
                    {
                        mail.Body += "<p>" + prod.cantidad + " - " + prod.Productos.producto + " - " + prod.Productos.presentacion + " - " + prod.Productos.marca + " - " + (prod.Precios.precio * prod.cantidad) + "</p>";
                        total += prod.cantidad * prod.Precios.precio;
                    }
                    mail.Body += "<p>-------------------</p>";
                    mail.Body += "<p>Total : " + total + "</p>";
                    mail.Body += "<br/><br/>";

                }

                mail.Body += "<p>o	Sujeto a disponibilidad de stock</p>";
                mail.Body += "<p>o	No te olvides de venir con cambio. Y con bolsa, changuito o lo que te parezca donde poder llevarte tu compra</p>";
                mail.Body += "<p>o	Pasada el horario de entrega no se aceptan reclamos. Cualcuier problema avisanos con tiempo</p>";
                mail.Body += "<p>o	Tené en cuenta que cada producto que se pide por este medio lo abonamos al productor (con el dinero de los militantes) y no tenemos devolución posible. No nos claves, gracias.</p>";

                mail.Body += "<p>Muchas gracias! Te esperamos</p>";
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("economiasocial@encuentrocapital.com.ar", "Frent3355");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }

        public ActionResult Calificar(int idCompra, int[] idProducto, string[] tComentarios, int[] rating)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);

            EstadosCompra comentado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 4);

            Compras compra = ctx.Compras.FirstOrDefault(a => a.idCompra == idCompra);

            var totalProductos = compra.CompraProducto.Where(a => a.compraId == idCompra).Count();

            var ids = new List<int>();
            for (int x = 0; x < idProducto.Length; x++)
            {
                int usar = idProducto[x];
                Productos prod = ctx.Productos.FirstOrDefault(a => a.idProducto == usar);


                string com = tComentarios[x];
                int rat = rating[x];
                if (com != "" && rat > 0)
                {
                    ComentariosProducto cp = new ComentariosProducto();
                    cp.comentario = com;
                    cp.estrellas = rat;
                    cp.Productos = prod;
                    cp.fecha = DateTime.Now;
                    ids.Add(usar);
                    compra.ComentariosProducto.Add(cp);
                }
            }

            ctx.SaveChanges();

            bool todoComentado = false;
            var totalComendados = ctx.ComentariosProducto.Where(a => a.compraId == idCompra).Count();
            if (totalProductos == totalComendados)
            {
                compra.EstadosCompra = comentado;
                todoComentado = true;
                ctx.SaveChanges();
            }


            return Json(new { bien = true, idCompra = idCompra, comentario = comentado.nombre, ids = ids, todoComentado = todoComentado });

            //return Json(new { bien = false, mensaje = "No se puede comentar la misma compra 2 veces" });
        }

        public ActionResult ListaTanda()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);

            Tandas ultima = ctx.Tandas.ToList().LastOrDefault(a => a.fechaCerrado != null);
            var lista = ctx.Compras.Where(a => (a.tandaId == ultima.idTanda) && (vecino.localId == null ? vecino.comuna == a.Locales.comuna : vecino.localId == a.localId)).ToList().Select(a => new
            {
                idCompra = a.idCompra,
                nombre = a.Vecinos.nombres,
                productos = string.Join("<br/>", a.CompraProducto.ToList().Select(b => "<span class='idca' style='display:none'>[" + b.productoId + "|" + b.cantidad + "]</span>(" + b.cantidad + ") " + b.Productos.producto + " - " + b.Productos.marca + " - " + b.Productos.presentacion)),
                precio = a.CompraProducto.ToList().Sum(b => b.cantidad * b.Precios.precio),
                retiro = a.EstadosCompra.codigo
            });
            return Json(lista, JsonRequestBehavior.DenyGet);
        }

        public FileResult ExportarEncargado()
        {
            /*StringBuilder csv = new StringBuilder();
            string Columnas = string.Format("{0};{1};{2};{3};{4}", "N", "Nombre", "Productos", "Precio", "Local");
            csv.AppendLine(Columnas);

            decimal costoTotal = 0;
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);

            Tandas ultima = ctx.Tandas.ToList().LastOrDefault(a => a.fechaCerrado != null);
            var lista = ctx.Compras.Where(a => a.tandaId == ultima.idTanda && (vecino.localId == null ? vecino.comuna == a.Locales.comuna : vecino.localId == a.localId)).OrderBy(a => new { a.Locales.idLocal, a.Vecinos.nombres }).ToList().Select(a => new
            {
                idCompra = a.idCompra,
                nombre = a.Vecinos.nombres,
                productos = string.Join(" - ", a.CompraProducto.ToList().Select(b => "(" + b.cantidad + ") " + b.Productos.producto + " - " + b.Productos.marca + " - " + b.Productos.presentacion + " - " + b.Productos.Precios.LastOrDefault(precio => a.fecha > precio.fecha).precio + "\015")),
                precio = a.CompraProducto.ToList().Sum(b => b.cantidad * b.Productos.Precios.LastOrDefault(precio => a.fecha >= precio.fecha).precio),
                retiro = a.EstadosCompra.codigo,
                local = a.Locales.direccion
            }).ToArray();

            for (int x = 0; x < lista.Count(); x++)
            {
                var compra = lista[x];
                string filas = string.Format("{0};{1};{2};${3};{4}", compra.idCompra, compra.nombre, compra.productos, compra.precio, compra.local);
                csv.AppendLine(filas);
            }

            using (MemoryStream memoryStream = new MemoryStream(Encoding.Default.GetBytes(csv.ToString())))
            {
                memoryStream.Position = 0;
                return File(memoryStream.ToArray() as byte[], "application/vnd.ms-excel", "Reporte.csv");
            }*/


            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);

            Tandas ultima = ctx.Tandas.ToList().LastOrDefault(a => a.fechaCerrado != null);

            DateTime ProximaEntrea = ultima.fechaVenta.HasValue ? ultima.fechaVenta.Value : DateTime.Now;
            string nombreLibro = ProximaEntrea.ToString("dd-MM-yyyy") + " Entrega";

            using (MemoryStream mem = new MemoryStream())
            using (SLDocument sl = new SLDocument())
            {
                sl.RenameWorksheet(SLDocument.DefaultFirstSheetName, nombreLibro);

                SLStyle bordeNegrita = sl.CreateStyle();
                bordeNegrita.Border.LeftBorder.Color = System.Drawing.Color.Black;
                bordeNegrita.Border.TopBorder.Color = System.Drawing.Color.Black;
                bordeNegrita.Border.RightBorder.Color = System.Drawing.Color.Black;
                bordeNegrita.Border.BottomBorder.Color = System.Drawing.Color.Black;

                bordeNegrita.Font.Bold = true;

                bordeNegrita.Border.LeftBorder.BorderStyle = BorderStyleValues.Thin;
                bordeNegrita.Border.TopBorder.BorderStyle = BorderStyleValues.Thin;
                bordeNegrita.Border.RightBorder.BorderStyle = BorderStyleValues.Thin;
                bordeNegrita.Border.BottomBorder.BorderStyle = BorderStyleValues.Thin;

                SLStyle bordeIz = sl.CreateStyle();
                bordeIz.Border.LeftBorder.Color = System.Drawing.Color.Black;
                bordeIz.Border.LeftBorder.BorderStyle = BorderStyleValues.Thin;

                SLStyle bordeDe = sl.CreateStyle();
                bordeDe.Border.RightBorder.Color = System.Drawing.Color.Black;
                bordeDe.Border.RightBorder.BorderStyle = BorderStyleValues.Thin;

                SLStyle bordeAr = sl.CreateStyle();
                bordeAr.Border.TopBorder.Color = System.Drawing.Color.Black;
                bordeAr.Border.TopBorder.BorderStyle = BorderStyleValues.Thin;

                SLStyle bordeAb = sl.CreateStyle();
                bordeAb.Border.BottomBorder.Color = System.Drawing.Color.Black;
                bordeAb.Border.BottomBorder.BorderStyle = BorderStyleValues.Thin;

                SLStyle saltoLinea = sl.CreateStyle();
                saltoLinea.SetVerticalAlignment(VerticalAlignmentValues.Center);
                saltoLinea.SetHorizontalAlignment(HorizontalAlignmentValues.Center);
                saltoLinea.SetWrapText(true);

                SLStyle rojo = sl.CreateStyle();
                rojo.Fill.SetPatternType(PatternValues.Solid);//.BottomBorder.Color = System.Drawing.Color.Red;
                rojo.Font.FontColor = System.Drawing.Color.White;
                rojo.Fill.SetPatternForegroundColor(System.Drawing.Color.Red);

                SLStyle centrado = sl.CreateStyle();
                centrado.FormatCode = "$ * #,##0.00";
                centrado.Font.FontSize = 10;
                centrado.SetHorizontalAlignment(HorizontalAlignmentValues.Right);

                sl.SetColumnWidth(1, 30);
                sl.SetColumnWidth(2, 65);
                sl.SetColumnWidth(3, 10);
                sl.SetColumnWidth(4, 10);
                sl.SetColumnWidth(5, 35);

                sl.SetCellValue(1, 1, "Nombre");
                sl.SetCellValue(1, 2, "Producto");
                sl.SetCellValue(1, 3, "Precio");
                sl.SetCellValue(1, 4, "Total");
                sl.SetCellValue(1, 5, "Observaciones");

                sl.SetCellStyle(1, 1, 1, 5, bordeNegrita);


                string urla = ConfigurationManager.AppSettings["UrlSitio"];

                int row = 3;

                bordeNegrita.Font.Bold = false;

                var lista = ctx.Compras.Where(a => a.tandaId == ultima.idTanda && (vecino.localId == null ? vecino.comuna == a.Locales.comuna : vecino.localId == a.localId)).OrderBy(a => new { a.Locales.idLocal, a.Vecinos.nombres });
                foreach (var compra in lista)
                {
                    int totalVecinx = (row + compra.CompraProducto.Count - 1);
                    sl.SetCellStyle(row, 1, totalVecinx, 1, bordeIz);
                    sl.SetCellStyle(row, 1, row, 5, bordeAr);
                    sl.SetCellStyle(totalVecinx, 1, totalVecinx, 5, bordeAb);
                    for (int x = 1; x < 6; x++)
                    {
                        sl.SetCellStyle(row, x, totalVecinx, x, bordeDe);
                    }

                    sl.SetCellValue(row, 1, new System.Globalization.CultureInfo("en-US", false).TextInfo.ToTitleCase(compra.Vecinos.nombres.ToLower()) + "\n" + compra.Vecinos.telefono + "\n" + compra.Vecinos.correo);
                    sl.SetCellStyle(row, 1, saltoLinea);
                    sl.MergeWorksheetCells(row, 1, totalVecinx, 1);

                    var ordenado = compra.CompraProducto.OrderBy(a => a.Productos.producto);
                    decimal totaltotal = 0;
                    foreach (var compraProducto in ordenado)
                    {
                        decimal total = compraProducto.Precios.precio * compraProducto.cantidad;
                        totaltotal += total;
                        centrado.Font.Bold = false;
                        sl.SetCellValue(row, 2, compraProducto.cantidad + ": " + compraProducto.Productos.producto + " - " + compraProducto.Productos.marca + " - " + compraProducto.Productos.presentacion);

                        sl.SetCellValue(row, 3, total);
                        sl.SetCellStyle(row, 3, centrado);

                        if (compraProducto.cantidad > 6)
                        {
                            sl.SetCellStyle(row, 2, row, 5, rojo);
                            sl.SetCellValue(row, 6, "Alerta! Mucha cantidad");
                        }

                        row++;
                    }

                    centrado.Font.Bold = true;
                    sl.SetCellValue(row - 1, 4, totaltotal);
                    sl.SetCellStyle(row - 1, 4, centrado);
                }


                sl.SaveAs(mem);
                mem.Position = 0;


                return File(mem.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte.xlsx");
            }

        }

        private Cell crearCelda(string value, CellValues dataType)
        {
            return new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataType),
            };
        }

        public ActionResult EnregarPedido(int idCompra, bool entregado, int[] ids = null, int[] noUsados = null, string[] vecinxs = null)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);

            EstadosCompra nopaso = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 5);
            EstadosCompra entre = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 3);
            EstadosCompra confirmado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 2);
            Compras compra = ctx.Compras.FirstOrDefault(a => a.idCompra == idCompra && a.estadoId == confirmado.idEstadoCompra);

            if (ids == null)
                compra.EstadosCompra = entregado ? entre : nopaso;
            else
            {
                var productos = compra.CompraProducto;
                for (int x = 0; x < productos.Count; x++)
                {
                    var producto = productos.ElementAt(x);
                    int posicion = Array.IndexOf(ids, producto.productoId);


                    if (noUsados[posicion] > 0)
                    {
                        //Vino menos cantidad
                        if (producto.cantidad - noUsados[posicion] == 0)
                            ctx.CompraProducto.Remove(producto);
                        else
                            producto.cantidad = producto.cantidad - noUsados[posicion];
                    }
                    if (!string.IsNullOrEmpty(vecinxs[posicion]))
                    {
                        //REUBICADO A UN VEXINX
                        var arrayvecino = vecinxs[posicion].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList().GroupBy(a => a);
                        foreach (var idVecinx in arrayvecino)
                        {
                            Vecinos reubicado = ctx.Vecinos.FirstOrDefault(a => a.idVecino == idVecinx.Key);
                            Compras vecinoCompra = ctx.Compras.FirstOrDefault(a => a.vecinoId == idVecinx.Key && a.tandaId == compra.tandaId);
                            if (vecinoCompra == null)
                            {
                                vecinoCompra = new Compras();
                                vecinoCompra.fecha = ApiProductosController.GetNextWeekday();
                                vecinoCompra.EstadosCompra = entre;
                                vecinoCompra.localId = compra.localId;
                                vecinoCompra.tandaId = compra.tandaId;
                                vecinoCompra.Vecinos = reubicado;

                                ctx.Compras.Add(vecinoCompra);

                            }

                            CompraProducto cp = new CompraProducto();
                            cp.cantidad = idVecinx.Count();
                            cp.Compras = vecinoCompra;
                            cp.Costos = producto.Costos;
                            cp.Precios = producto.Precios;
                            cp.Productos = ctx.Productos.FirstOrDefault(a => a.idProducto == producto.productoId);

                            vecinoCompra.CompraProducto.Add(cp);
                        }

                        var cantidadTotal = arrayvecino.Sum(a => a.Count());
                        if (producto.cantidad - cantidadTotal == 0)
                            ctx.CompraProducto.Remove(producto);
                        else
                            producto.cantidad = producto.cantidad - cantidadTotal;
                    }

                    compra.EstadosCompra = entregado ? entre : nopaso;
                }


                /*var productos = compra.CompraProducto;
                for (int x = 0; x < ids.Count(); x++)
                {
                    var prod = productos.FirstOrDefault(a => a.productoId == ids[x]);

                    if (vecinxs[x] != null)
                    {
                        var arrayvecino = vecinxs[x].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList().GroupBy( a => a);
                        foreach ( var idVecinx in arrayvecino )
                        {
                            Vecinos reubicado = ctx.Vecinos.FirstOrDefault(a => a.idVecino == idVecinx.Key);
                            Compras vecinoCompra = ctx.Compras.FirstOrDefault(a => a.vecinoId == idVecinx.Key && a.tandaId == compra.tandaId);

                            //SOLO SE ESTA LLEVANDO COSAS
                            if ( vecinoCompra == null)
                            {
                                vecinoCompra = new Compras();
                                vecinoCompra.fecha = ApiProductosController.GetNextWeekday();
                                vecinoCompra.EstadosCompra = entre;
                                vecinoCompra.localId = compra.localId;
                                vecinoCompra.tandaId = compra.tandaId;
                                vecinoCompra.Vecinos = reubicado;

                                prod.cantidad = idVecinx.Count();
                                compra.CompraProducto.Remove(prod);
                                vecinoCompra.CompraProducto.Add(prod);
                                ctx.Compras.Add(vecinoCompra);

                            }
                            else
                            {
                                prod.cantidad = idVecinx.Count();
                                compra.CompraProducto.Remove(prod);
                                vecinoCompra.CompraProducto.Add(prod);
                                ctx.Compras.Add(vecinoCompra);
                            }
                        }
                    }
                    else
                    {
                        prod.cantidad = cantidad[x];

                        if (prod.cantidad == 0)
                            ctx.CompraProducto.Remove(prod);
                    }


                }
                compra.EstadosCompra = entregado ? entre : nopaso;*/
            }

            ctx.SaveChanges();

            return Json(new { error = false }, JsonRequestBehavior.DenyGet);
        }

        /*public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }*/

        public string MandarMailCalificacion()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var compras = ctx.Compras.Where(a => a.EstadosCompra.codigo == 3).Distinct().ToList();

            var vecinosQueCompraron = ctx.Compras.Where(a => a.EstadosCompra.codigo == 3).GroupBy(a => a.vecinoId).Select(a => a.FirstOrDefault(b => b.vecinoId == a.Key)).ToList();
            foreach (var actualTanda in vecinosQueCompraron)
            {
                string nombre = actualTanda.Vecinos.nombres;
                string correo = actualTanda.Vecinos.correo;

                using (MailMessage mail = new MailMessage())
                {

                    mail.From = new MailAddress("economiasocial@encuentrocapital.com.ar", "Economía Social y Solidaria");
                    mail.To.Add(correo);
                    //mail.To.Add("julianlionti@hotmail.com");
                    mail.Subject = "Economia Social y Solidaria -- Nuevo Encuentro";
                    mail.Body = "<p>No te olvides de dejarnos tus comentarios sobre los productos comprados</p>";
                    mail.BodyEncoding = System.Text.Encoding.UTF8;

                    List<Compras> totalCompras = ctx.Compras.Where(a => a.vecinoId == actualTanda.Vecinos.idVecino && a.EstadosCompra.codigo == 3).ToList();
                    foreach (var c in totalCompras)
                    {
                        mail.Body += "<p>-------------------</p>";
                        mail.Body += "<p>Compra N° " + (totalCompras.IndexOf(c) + 1) + "</p>";

                        foreach (CompraProducto prod in c.CompraProducto)
                        {
                            if (c.ComentariosProducto.FirstOrDefault(a => a.productoId == prod.productoId) == null)
                                mail.Body += "<p>" + prod.Productos.producto + " - " + prod.Productos.presentacion + " - " + prod.Productos.marca + " - Cantidad: " + prod.cantidad + "</p>";
                        }
                        mail.Body += "<p>-------------------</p>";
                        mail.Body += "<br/><br/>";
                    }

                    mail.Body += "<p>Muchas gracias! Te esperamos</p>";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("economiasocial@encuentrocapital.com.ar", "Frent3355");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
            }

            return "Bien!";
        }
    }
}
