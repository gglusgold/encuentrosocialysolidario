using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Economia_Social_Y_Solidaria.Models;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using System.Configuration;
using System.IO;
using Newtonsoft.Json;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class ApiProductosController : ApiController
    {
        public static DateTime GetNextWeekday()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Tandas ultimaTanda = ctx.Tandas.ToList().LastOrDefault(a => a.fechaCerrado == null);
            if (ultimaTanda == null)
            {
                return DateTime.Now;
            }
            else
            {
                return ultimaTanda.fechaVenta.HasValue ? ultimaTanda.fechaVenta.Value : DateTime.Now;
            }

            /*int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);*/
        }

        // GET api/apiproductos
        [ActionName("Productos")]
        public IHttpActionResult Get(int idLocal)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            var categorias = ctx.ProductosLocales.Where(a => idLocal == -1 ? a.Locales.activo : a.localId == idLocal).ToList().Select(b => new
            {
                idProducto = b.Productos.idProducto,
                nombre = b.Productos.producto,
                marca = b.Productos.marca,
                presentacion = b.Productos.presentacion,
                precio = b.Productos.Precios.ToArray().Last().precio,
                stock = b.Productos.stock,
                idCategoria = b.Productos.categoriaId,
                categoria = b.Productos.Categorias.nombre
            });

            var groupedCustomerList = categorias
            .GroupBy(u => u.categoria)
            .Select(grp => new
            {
                idCategoria = grp.FirstOrDefault().idCategoria,
                nombre = grp.Key,
                productos = grp.ToList()
            })
            .ToList();

            //DateTime ProximaEntrea = GetNextWeekday(DateTime.Now, DayOfWeek.Saturday);
            //return Json(new { Proxima = ProximaEntrea.ToString("dd/MM/yyyy"), Lista = groupedCustomerList });
            return Json(groupedCustomerList);
        }

        [ActionName("Detalle")]
        public IHttpActionResult Detalle()
        {
            TanoNEEntities ctx = new TanoNEEntities();

            var form = HttpContext.Current.Request.Form;

            int idProducto = int.Parse(form["id"]);

            return Json(ctx.Productos.Select(a => new
            {
                idProducto = a.idProducto,
                producto = a.producto,
                descripcion = a.descripcion,
                marca = a.marca,
                presentacion = a.presentacion,
                proveedor = a.proveedor,
                variedad = a.variedad,
                precio = a.Precios.OrderByDescending(b => b.idPrecio).FirstOrDefault().precio,
            }).FirstOrDefault(a => a.idProducto == idProducto));
        }


        [ActionName("Locales")]
        public IHttpActionResult Get()
        {
            string error = null;
            TanoNEEntities ctx = new TanoNEEntities();

            int idCircuito;
            Tandas ultimaTanda = ctx.Tandas.ToList().LastOrDefault(a => a.fechaCerrado == null);
            if (ultimaTanda == null)
            {
                idCircuito = ctx.Tandas.OrderByDescending(p => p.idTanda).FirstOrDefault().circuitoId;
            }
            else
                idCircuito = ultimaTanda.circuitoId;

            var locales = ctx.ProductosLocales.Where(a => a.Locales.activo && a.Locales.circuitoId == idCircuito).ToList().Select(b => new
            {
                idLocal = b.Locales.idLocal,
                nombre = b.Locales.nombre,
                horario = b.Locales.horario,
                direccion = b.Locales.direccion,
                comuna = b.Locales.comuna,
                barrio = b.Locales.barrio,
            }).Distinct();

            DateTime ProximaEntrea = GetNextWeekday();

            return Json(new { Proxima = ProximaEntrea.ToString("dd/MM/yyyy"), Lista = locales, TandaAbierta = ultimaTanda != null });

            //return Json(locales);
        }

        [ActionName("Usuario")]
        public IHttpActionResult Usuario() // string correo, string pass)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            var form = HttpContext.Current.Request.Form;

            string correo = form["correo"];
            string pass = form["pass"];
            string token = form["token"];

            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == correo);
            if (vecino != null)
            {

                if (!vecino.verificado)
                {
                    return Json(new { Error = "Hay que verificar la cuenta. Revisá tu mail" });
                }

                pass = InicioController.GetCrypt(pass);
                if (vecino.contrasena == pass)
                {
                    if ( vecino.token == null )
                    {
                        foreach (Alertas alerta in ctx.Alertas.Where(a => a.android))
                        {
                            vecino.AlertasVecinxs.Add(new AlertasVecinxs { Alertas = alerta });
                        }
                    }
                    vecino.token = token;
                    ctx.SaveChanges();

                    EstadosCompra EstadoEntregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);
                    EstadosCompra confirmado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 3);
                    EstadosCompra comentado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 4);

                    //Precios ultimop = prod.Precios.Count > 1 ? prod.Precios.LastOrDefault(a => a.fecha.Date <= actual.fechaAbierto.Date) : prod.Precios.FirstOrDefault();


                    var json = ctx.Compras.Where(a => a.vecinoId == vecino.idVecino).OrderByDescending(a => a.fecha).ToList().Select(a => new
                    {
                        idCompra = a.idCompra,
                        estado = a.EstadosCompra.nombre,
                        fecha = a.fecha.ToString("hh:mm dd/MM/yyyy"),
                        idLocal = a.Locales.idLocal,
                        local = a.Locales.direccion,
                        barrio = a.Locales.barrio,
                        editar = a.estadoId == EstadoEntregado.idEstadoCompra,
                        comentar = a.estadoId == confirmado.idEstadoCompra,
                        comentado = a.estadoId == comentado.idEstadoCompra,
                        comuna = a.Locales.comuna,
                        productos = a.CompraProducto.ToList().Select(b => new
                        {
                            idProducto = b.Productos.idProducto,
                            nombre = b.Productos.producto,
                            marca = b.Productos.marca,
                            presentacion = b.Productos.presentacion,
                            cantidad = b.cantidad,
                            comentado = a.ComentariosProducto.FirstOrDefault(c => c.productoId == b.productoId) != null,  // a.Comentarios.Count == 1 ? a.Comentarios.FirstOrDefault().ComentariosProducto.FirstOrDefault(cp => cp.productoId == b.productoId).Productos != null : false,
                            precioUnidad = b.Precios.precio //.Precios.Count > 1 ? b.Productos.Precios.LastOrDefault(precio => a.fecha.Date >= precio.fecha.Date).precio : b.Productos.Precios.FirstOrDefault().precio,

                        })
                    }).ToList();

                    var jsonvecino = new
                    {
                        idVecino = vecino.idVecino,
                        nombre = vecino.nombres,
                        telefono = vecino.telefono,
                        comuna = vecino.comuna
                    };

                    return Json(new { Historico = json, Vecino = jsonvecino });
                }
                else
                    return Json(new { Error = "Contraseña incorrecta" });
            }
            else
                return Json(new { Error = "No existe el usuario, para usar la plataforma es necesario registrarse " });

        }


        [ActionName("ActualizarToken")]
        public IHttpActionResult ActualizarToken()
        {
            TanoNEEntities ctx = new TanoNEEntities();

            var form = HttpContext.Current.Request.Form;

            int idVecino = int.Parse(form["idVecino"]);
            string token = form["token"];

            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.idVecino == idVecino);
            if (vecino != null)
                vecino.token = token;

            ctx.SaveChanges();

            return Json(new { });
        }

        [ActionName("Registrarse")]
        public IHttpActionResult Registrarse()
        {
            string urla = ConfigurationManager.AppSettings["UrlSitio"];

            TanoNEEntities ctx = new TanoNEEntities();

            var form = HttpContext.Current.Request.Form;


            string email = form["mail"];
            string nombres = form["nombres"];
            string telefono = form["telefono"];
            string password = form["password"];
            int comuna = int.Parse(form["comuna"]);
            string token = form["token"];

            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == email);
            if (vecino == null)
            {
                vecino = new Vecinos();
                vecino.correo = email;
                vecino.nombres = nombres;
                vecino.telefono = telefono;
                vecino.contrasena = InicioController.GetCrypt(password);
                vecino.comuna = comuna;
                vecino.fechaCreado = DateTime.Now;
                vecino.token = token;

                List<string> hashes = ctx.Vecinos.Select(a => a.hash).ToList();
                string hash = InicioController.RandomString(25);
                while (hashes.Contains(hash))
                {
                    hash = InicioController.RandomString(25);
                }

                vecino.hash = hash;
                ctx.Vecinos.Add(vecino);


                using (MailMessage mail = new MailMessage())
                {
                    //string datos = "Datos para entrar:<br/>Usuario: " + email;
                    string datos = "Datos para entrar:<br/>Usuario: " + email + " Contraseña :" + password;
                    mail.From = new MailAddress("economiasocial@encuentrocapital.com.ar", "Economía Social y Solidaria");
                    mail.To.Add(email);
                    mail.Subject = "Economia Social y Solidaria -- Nuevo Encuentro";
                    mail.Body = "<p>Gracias por usar nuestro sistema. Hacé click en el link de abajo para activar la cuenta</p>";
                    mail.Body += "<p><a href='" + urla + "Inicio/ActivarCuenta?k=" + hash + "'>Activar Cuenta</a></p>";
                    mail.Body += "<p>-------------------</p><p><p>" + datos + "</p><p>-------------------</p>";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    //using (SmtpClient smtp = new SmtpClient("smtp.encuentrocapital.com.ar", 587))
                    {
                        // smtp.Credentials = new NetworkCredential("racinglocura07@gmail.com", "kapanga34224389,");
                        smtp.Credentials = new NetworkCredential("economiasocial@encuentrocapital.com.ar", "Frent3355");
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }

                ctx.SaveChanges();

                return Json(new { respuesta = "Te enviamos un mail para activar la cuenta" });
            }
            else
            {
                return Json(new { error = "Ya existe el usuario" });
            }
        }

        [ActionName("Pedir")]
        public IHttpActionResult Pedir()
        {
            try
            {
                string error = "";

                var form = HttpContext.Current.Request.Form;

                int idVecino = int.Parse(form["idVecino"]);
                int local = int.Parse(form["local"]);
                string prods = form["productos"];
                int idCompra = int.Parse(form["idCompra"]);

                dynamic dyn = JArray.Parse(prods);

                int hash = (idVecino + "-" + local + "-" + prods).GetHashCode();

                TanoNEEntities ctx = new TanoNEEntities();

                Locales localCompro = ctx.Locales.FirstOrDefault(a => a.idLocal == local);
                Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.idVecino == idVecino);
                if (vecino == null)
                    error = "Hay que iniciar sesion para realizar un pedido";

                Tandas ultimaTanda = ctx.Tandas.ToList().LastOrDefault(a => a.fechaCerrado == null);
                if (ultimaTanda == null)
                    error = "No hay circuitos abiertos en este momento";

                //Encargado
                EstadosCompra estado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);

                bool editando = false;
                Compras compra = new Compras();
                if (idCompra != -1)
                {
                    compra = ctx.Compras.FirstOrDefault(a => a.idCompra == idCompra);
                    compra.CompraProducto.ToList().ForEach(cs => ctx.CompraProducto.Remove(cs));
                    editando = true;
                }
                else
                {
                    compra.fecha = DateTime.Now;
                }

                compra.Locales = localCompro;
                compra.Vecinos = vecino;
                compra.Tandas = ultimaTanda;
                compra.EstadosCompra = estado;

                if (idCompra == -1)
                    ctx.Compras.Add(compra);

                foreach (dynamic producto in dyn)
                {
                    int prodActual = producto.idProducto;
                    int cantActual = producto.pedidos;

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
                    productos.cantidad = cantActual;

                    ctx.CompraProducto.Add(productos);
                }

                var test = ctx.Compras.FirstOrDefault(a => a.hash == hash);
                if (test != null && !editando)
                    error = "Ya ha comprado los mismos productos para esta tanda";

                if (string.IsNullOrEmpty(error))
                {
                    compra.hash = hash;
                    ctx.SaveChanges();
                }

                return Json(new { error = error });

            }
            catch (Exception)
            {
                return Json(new { error = "Error al grabar, si te sigue pasando, ¡avisanos!" });
            }


        }


        [ActionName("MisCompras")]
        public IHttpActionResult MisCompras()
        {
            try
            {
                var form = HttpContext.Current.Request.Form;

                int idVecino = int.Parse(form["idVecino"]);

                TanoNEEntities ctx = new TanoNEEntities();

                Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.idVecino == idVecino);

                Tandas ultimaTanda = ctx.Tandas.ToList().LastOrDefault(a => a.fechaCerrado == null);

                EstadosCompra EstadoEntregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);
                EstadosCompra confirmado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 3);
                EstadosCompra comentado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 4);

                //Precios ultimop = prod.Precios.Count > 1 ? prod.Precios.LastOrDefault(a => a.fecha.Date <= actual.fechaAbierto.Date) : prod.Precios.FirstOrDefault();


                var json = ctx.Compras.Where(a => a.vecinoId == idVecino).OrderByDescending(a => a.fecha).ToList().Select(a => new
                {
                    idCompra = a.idCompra,
                    estado = a.EstadosCompra.nombre,
                    fecha = a.fecha.ToString("hh:mm dd/MM/yyyy"),
                    idLocal = a.Locales.idLocal,
                    local = a.Locales.direccion,
                    barrio = a.Locales.barrio,
                    editar = ultimaTanda == null ? false : a.estadoId == EstadoEntregado.idEstadoCompra,
                    comentar = a.estadoId == confirmado.idEstadoCompra,
                    comentado = a.estadoId == comentado.idEstadoCompra,
                    comuna = a.Locales.comuna,
                    productos = a.CompraProducto.ToList().Select(b => new
                    {
                        idProducto = b.Productos.idProducto,
                        nombre = b.Productos.producto,
                        marca = b.Productos.marca,
                        presentacion = b.Productos.presentacion,
                        cantidad = b.cantidad,
                        comentado = a.ComentariosProducto.FirstOrDefault(c => c.productoId == b.productoId) != null,  // a.Comentarios.Count == 1 ? a.Comentarios.FirstOrDefault().ComentariosProducto.FirstOrDefault(cp => cp.productoId == b.productoId).Productos != null : false,
                        precioUnidad = b.Precios.precio //.Precios.Count > 1 ? b.Productos.Precios.LastOrDefault(precio => a.fecha.Date >= precio.fecha.Date).precio : b.Productos.Precios.FirstOrDefault().precio,

                    })
                }).ToList();



                return Json(new { Historico = json });

            }
            catch (Exception)
            {
                return Json(new { error = "Error al grabar, si te sigue pasando, ¡avisanos!" });
            }


        }

        [ActionName("CancelarPedido")]
        public IHttpActionResult CancelarPedido()
        {
            try
            {
                string error = "";
                var form = HttpContext.Current.Request.Form;

                int idVecino = int.Parse(form["idVecino"]);
                int idCompra = int.Parse(form["idCompra"]);

                TanoNEEntities ctx = new TanoNEEntities();
                Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.idVecino == idVecino);

                EstadosCompra EstadoEntregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 1);
                Compras cancelar = ctx.Compras.FirstOrDefault(a => a.idCompra == idCompra && vecino.idVecino == a.vecinoId);
                if (cancelar.estadoId != EstadoEntregado.idEstadoCompra)
                {
                    error = "No se puede cancelar la compra, ya ha sido encargada!";
                }
                if (cancelar != null && string.IsNullOrWhiteSpace(error))
                {
                    ctx.Compras.Remove(cancelar);
                    ctx.SaveChanges();
                }

                return Json(new { error = error });

            }
            catch (Exception)
            {
                return Json(new { error = "Error al grabar, si te sigue pasando, ¡avisanos!" });
            }
        }

        [ActionName("Noticias")]
        public IHttpActionResult Noticias()
        {
            TanoNEEntities ctx = new TanoNEEntities();

            return Json(new
            {
                Noticias = ctx.Noticias.OrderBy(a => a.fecha).ToList().Select(a => new
                {
                    titulo = a.titulo,
                    copete = a.copete,
                    link = a.link,
                    imagen = a.imagen,
                    categoria = a.categoria,
                    tags = a.tags,
                    autor = a.autor,
                    fecha = a.fecha.HasValue ? a.fecha.Value.ToShortDateString() : "",
                }).Take(10)
            });
        }

        [ActionName("MandarNotificacion")]
        public IHttpActionResult MandarNotificacion()
        {
            var form = HttpContext.Current.Request.Form;

            string titulo = form["titulo"];
            string mensaje = form["mensaje"];

            mandarNotificacion(titulo, mensaje);
            //mandarNotificacion("Ya poder pedir!", "Desde hoy tenés la posibilidad de hacer tu pedido", "CARRITO");
            return Json("Si");
        }


        public static void mandarNotificacion(string titulo, string mensaje, string donde = null, string[] regIds = null)
        {
            if (regIds == null)
            {
                TanoNEEntities ctx = new TanoNEEntities();
                regIds = ctx.Vecinos.Where(x => x.token != null).Select(x => x.token).ToArray();
            }

            if (regIds.Count() == 0)
                return;

            string authkey = "AIzaSyB34E8CF1abOR9LvGBIuT9nfniWkY_DEyE";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("Authorization", "key=" + authkey);


            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = new
                {
                    data = new
                    {
                        titulo = titulo,
                        mensaje = mensaje,
                        donde = donde
                    },
                    registration_ids = regIds
                };

                streamWriter.Write(JsonConvert.SerializeObject(json));
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }
    }

}
