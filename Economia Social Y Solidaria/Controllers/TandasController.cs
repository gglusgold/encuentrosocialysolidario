using DocumentFormat.OpenXml.Spreadsheet;
using Economia_Social_Y_Solidaria.Models;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{

    public class ConfCircuitos
    {
        public int idTanda;
        public int idCircuito;
        public int codigo;
        public string circuito;
        public string fechaAbierto;
        public string leyenda;
        public bool abrir = true;
        public string error;
    }


    public class TandasController : Controller
    {


        public ActionResult Tandas()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            List<Tandas> total = ctx.Tandas.ToList();


            ConfCircuitos conf = new ConfCircuitos();
            if (total.Count == 0)
            {
                conf.leyenda = "No existen tandas creadas";
                conf.abrir = true;
            }
            else
            {
                Tandas ultima = total.LastOrDefault(a => a.fechaCerrado == null);
                if (ultima == null)
                {
                    ultima = total.LastOrDefault();
                    conf.abrir = true;
                    conf.codigo = ultima.Circuitos.codigo;
                    conf.leyenda = "No hay tanda abierta, último abierto: " + ultima.Circuitos.nombre;
                }
                else
                {
                    conf.leyenda = "Circuito Abierto el " + ultima.fechaAbierto.ToString("dd/MM/yyyy") + " por : " + ultima.Vecinos.nombres;
                    conf.idCircuito = ultima.Circuitos.idCircuito;
                    conf.abrir = false;
                }

                conf.idTanda = ultima.idTanda;
                conf.circuito = ultima.Circuitos.nombre;
                conf.fechaAbierto = ultima.fechaAbierto.ToString("dd/MM/yyyy");

            }

            return View(conf);
        }

        public ActionResult Modificar(int codigo, bool abrir, string fechaVenta, int idTanda = -1)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            ConfCircuitos conf = new ConfCircuitos();

            Circuitos circuito = ctx.Circuitos.FirstOrDefault(a => a.codigo == codigo);
            Vecinos responsable = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
            RolesVecinos rol = responsable.RolesVecinos.FirstOrDefault(a => a.Roles.codigoRol == 2);
            if (rol == null)
            {
                conf.error = "Su ususario no tiene permisos";
            }
            else
            {
                if (abrir)
                {
                    Tandas ultima = ctx.Tandas.ToList().LastOrDefault();
                    if (ultima != null && ultima.fechaCerrado == null)
                    {
                        conf.error = "No puede haber 2 tandas abiertas";
                    }
                    else
                    {
                        Tandas tanda = new Tandas();
                        tanda.fechaAbierto = DateTime.Now;
                        tanda.fechaVenta = DateTime.ParseExact(fechaVenta, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        tanda.Circuitos = circuito;
                        tanda.Vecinos = responsable;
                        conf.abrir = false;
                        conf.circuito = tanda.Circuitos.nombre;
                        conf.codigo = tanda.Circuitos.codigo;
                        conf.idTanda = tanda.idTanda;
                        conf.idCircuito = tanda.Circuitos.idCircuito;
                        conf.leyenda = "Circuito Abierto: ";

                        if (!bool.Parse(ConfigurationManager.AppSettings["debug"]))
                        { 
                            ApiProductosController.mandarNotificacion("Ya poder pedir!", "Desde hoy tenés la posibilidad de hacer tu pedido", "CARRITO", ctx.AlertasVecinxs.Where(a => a.Vecinos.token != null &&  a.Alertas.codigo == 3).Select(a => a.Vecinos.token).Distinct().ToArray());
                        }

                        ctx.Tandas.Add(tanda);
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    Tandas ultima = ctx.Tandas.ToList().LastOrDefault();
                    if (ultima != null && ultima.fechaCerrado != null)
                    {
                        conf.error = "Ya se encuentra cerrado";
                    }
                    else
                    {
                        if (idTanda == -1)
                            conf.error = "No se encontró la tanda";
                        else
                        {
                            Tandas tanda = ctx.Tandas.FirstOrDefault(a => a.idTanda == idTanda);
                            tanda.fechaCerrado = DateTime.Now;
                            tanda.Vecinos = responsable;

                            conf.leyenda = "No hay tanda abierta, último abierto:";
                            conf.abrir = true;
                            conf.idCircuito = ultima.Circuitos.idCircuito;
                            conf.codigo = ultima.Circuitos.codigo;

                            //CERRAR TODOS LOS PEDIDOS DE ESTA TANDA

                            EstadosCompra confirmado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 2);
                            foreach (var compraEnTanda in tanda.Compras)
                            {
                                compraEnTanda.EstadosCompra = confirmado;
                            }
                            ctx.SaveChanges();

                            //if (!bool.Parse(ConfigurationManager.AppSettings["debug"]))
                            //    MandarMailConfirmandoCompraTodos(tanda);

                            ctx.SaveChanges();
                        }
                    }

                }
            }



            return RedirectToAction("Tandas", "Tandas");
        }



        public JsonResult Lista()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var lista = ctx.Tandas.OrderByDescending(a => a.idTanda).ToList().Select(a => new
            {
                idTanda = a.idTanda,
                fechaAbierto = a.fechaAbierto.ToString("dd/MM/yyyy"),
                fechaCerrado = a.fechaCerrado.HasValue ? a.fechaCerrado.Value.ToString("dd/MM/yyyy") : "-",
                circuito = a.Circuitos.nombre,
                responsable = a.Vecinos.nombres,
            });
            return Json(lista, JsonRequestBehavior.DenyGet);
        }

        private void MandarMailConfirmandoCompraTodos(Tandas tandaActual)
        {
            DateTime ProximaEntrea = ApiProductosController.GetNextWeekday();


            TanoNEEntities ctx = new TanoNEEntities();
            EstadosCompra confirmado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 2);
            var vecinosQueCompraron = tandaActual.Compras.Where(a => a.EstadosCompra.idEstadoCompra == confirmado.idEstadoCompra).GroupBy(a => a.vecinoId).Select(a => a.FirstOrDefault(b => b.vecinoId == a.Key));

            //ctx.AlertasVecinxs.Where(a => a.Vecinos.token != null && a.Alertas.codigo == 3).Select(a => a.vecinxId.ToString()).Distinct().ToArray()
            //    vecinosQueCompraron.Where(a => a.Vecinos.token != null).Select(a => a.Vecinos.token)
            ApiProductosController.mandarNotificacion("Pedido Confirmado", "Gracias por colaborar con la economía social y solidaria. No te olvides de venir con cambio!", "MISCOMPRAS", vecinosQueCompraron.Where(a => a.Vecinos.token != null).Select(a => a.Vecinos.token).ToArray());
            foreach (var actualTanda in vecinosQueCompraron)
            {
                string fecha = ProximaEntrea.ToString("dd/MM/yyyy") + " - " + actualTanda.Locales.horario;
                string nombre = actualTanda.Vecinos.nombres;
                string correo = actualTanda.Vecinos.correo;

                using (MailMessage mail = new MailMessage())
                {

                    mail.From = new MailAddress("economiasocial@encuentrocapital.com.ar", "Economía Social y Solidaria");
                    mail.To.Add(correo);
                    //mail.To.Add("julianlionti@hotmail.com");
                    mail.Subject = "Economia Social y Solidaria -- Nuevo Encuentro";
                    mail.Body = "<p>Se han confirmado las siguientes compras</p>";
                    mail.BodyEncoding = System.Text.Encoding.UTF8;

                    List<Compras> totalCompras = ctx.Compras.Where(a => a.tandaId == actualTanda.tandaId && a.vecinoId == actualTanda.Vecinos.idVecino && a.EstadosCompra.idEstadoCompra == confirmado.idEstadoCompra).ToList();
                    foreach (var compras in totalCompras)
                    {
                        mail.Body += "<p>-------------------</p>";
                        mail.Body += "<p>Compra N° " + (totalCompras.IndexOf(compras) + 1) + "</p>";
                        mail.Body += "<p><b>Lo tenés que pasar a retirar el dia " + fecha + " Por nuestro local en " + actualTanda.Locales.direccion + "</b></p>";

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
        }

        public ActionResult VerCompras(bool admin = false)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
            ViewBag.Admin = vecino.RolesVecinos.Any(a => a.Roles.codigoRol == 2) && admin;
            return View();
        }

        public JsonResult ListaCompras(bool ad, string jtSorting)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
            ad = vecino.RolesVecinos.Any(a => a.Roles.codigoRol == 2) && ad;

            Tandas ultimaAbierta = ctx.Tandas.FirstOrDefault(a => a.fechaCerrado == null);
            if (ultimaAbierta == null)
                ultimaAbierta = ctx.Tandas.ToList().Last();

            var lista = ctx.Compras.Where(a => (a.tandaId == ultimaAbierta.idTanda) && (ad ? a.localId > 0 : (vecino.localId == null ? vecino.comuna == a.Locales.comuna : vecino.localId == a.localId))).ToList().Select(a => new
            {
                idCompra = a.idCompra,
                vecine = a.Vecinos.nombres,
                productos = string.Join("<br/>", a.CompraProducto.Select(ab => string.Format("{0}: {1} {2} - {3}", ab.cantidad, ab.Productos.producto, ab.Productos.marca, ab.Productos.presentacion))),
                local = a.Locales.direccion,
                fecha = a.fecha.ToString("dd/MM/yyyy")
            });

            switch (jtSorting)
            {
                case "idCompra ASC":
                    lista = lista.OrderBy(a => a.idCompra);
                    break;
                case "idCompra DESC":
                    lista = lista.OrderByDescending(a => a.idCompra);
                    break;

                case "vecine ASC":
                    lista = lista.OrderBy(a => a.vecine);
                    break;
                case "vecine DESC":
                    lista = lista.OrderByDescending(a => a.vecine);
                    break;

                case "productos ASC":
                    lista = lista.OrderBy(a => a.productos);
                    break;
                case "productos DESC":
                    lista = lista.OrderByDescending(a => a.productos);
                    break;

                case "local ASC":
                    lista = lista.OrderBy(a => a.local);
                    break;
                case "local DESC":
                    lista = lista.OrderByDescending(a => a.local);
                    break;

                case "fecha ASC":
                    lista = lista.OrderBy(a => a.fecha);
                    break;
                case "fecha DESC":
                    lista = lista.OrderByDescending(a => a.fecha);
                    break;
            }

            return Json(new { Result = "OK", Records = lista }, JsonRequestBehavior.DenyGet);
        }

        public ActionResult CrearExcel(int idTanda, bool porLocal = false)
        {

            /*string handle = Guid.NewGuid().ToString();

            StringBuilder csv = new StringBuilder();
            string Columnas = null;
            if (porLocal)
                Columnas = ""; // string.Format("{0};{1};{2};{3}", "N", "Local", "Productos");
            else
                Columnas = string.Format("{0};{1};{2};{3}", "N", "Producto", "Presentacion", "Cantidad", "Costo(Aprox)");
            csv.AppendLine(Columnas);


            TanoNEEntities ctx = new TanoNEEntities();
            Tandas actual = ctx.Tandas.FirstOrDefault(a => a.idTanda == idTanda);

            if (porLocal)
            {
                decimal costoTotal = 0;
                var locales = ctx.Compras.Where(a => a.tandaId == idTanda).Select(a => a.Locales).Distinct().ToList();
                foreach (var local in locales)
                {
                    csv.AppendLine(local.direccion);
                    var listado = ctx.CompraProducto.Where(a => a.Compras.localId == local.idLocal && a.Compras.tandaId == idTanda).GroupBy(a => a.productoId).Select(a => new { idProducto = a.Key, Cantidad = a.Sum(b => b.cantidad) }).ToArray();
                    for (int x = 0; x < listado.Count(); x++)
                    {
                        var compra = listado[x];
                        Productos prod = ctx.Productos.FirstOrDefault(a => a.idProducto == compra.idProducto);
                        Costos ultimo = prod.Costos.Count > 1 ? prod.Costos.LastOrDefault(a => a.fecha.Date <= actual.fechaAbierto.Date) : prod.Costos.FirstOrDefault();
                        decimal costo = ultimo.costo * compra.Cantidad;
                        costoTotal += costo;
                        string filas = string.Format(";{0};{1};${2}", compra.Cantidad, string.Format("{0} - {1} - {2}", prod.producto, prod.marca, prod.presentacion), costo.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                        csv.AppendLine(filas);
                    }
                }
                string cierre = string.Format("Total;;;${0}", costoTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                csv.AppendLine(cierre);
            }
            else
            {
                var listado = ctx.CompraProducto.Where(a => a.Compras.tandaId == idTanda).GroupBy(a => a.productoId).Select(a => new { idProducto = a.Key, Cantidad = a.Sum(b => b.cantidad) }).ToArray();
                decimal costoTotal = 0;
                for (int x = 0; x < listado.Count(); x++)
                {
                    var compra = listado[x];
                    Productos prod = ctx.Productos.FirstOrDefault(a => a.idProducto == compra.idProducto);
                    Costos ultimo = prod.Costos.Count > 1 ? (prod.Costos.LastOrDefault(a => a.fecha.Date <= actual.fechaAbierto.Date) == null ? prod.Costos.LastOrDefault() : prod.Costos.FirstOrDefault()) : prod.Costos.FirstOrDefault();
                    decimal costo = ultimo.costo * compra.Cantidad;
                    //decimal costo = prod.Costos.FirstOrDefault(a => a.fecha <= actual.fechaAbierto).costo * compra.Cantidad;
                    costoTotal += costo;
                    string filas = string.Format("{0};{1};{2};{3};${4}", x + 1, prod.producto, prod.presentacion, compra.Cantidad, costo.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                    csv.AppendLine(filas);
                }

                string cierre = string.Format("{0};{1};{2};{3};${4}", "", "", "", "", costoTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                csv.AppendLine(cierre);
            }



            using (MemoryStream memoryStream = new MemoryStream(Encoding.Default.GetBytes(csv.ToString())))
            {
                memoryStream.Position = 0;
                TempData[handle] = memoryStream.ToArray();
            }

            return Json(new { FileGuid = handle, FileName = "Reporte.csv" });*/

            string handle = Guid.NewGuid().ToString();

            TanoNEEntities ctx = new TanoNEEntities();
            Tandas actual = ctx.Tandas.FirstOrDefault(a => a.idTanda == idTanda);

            using (MemoryStream mem = new MemoryStream())
            using (SLDocument sl = new SLDocument())
            {
                sl.RenameWorksheet(SLDocument.DefaultFirstSheetName, porLocal ? "Local" : "Total");

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
                saltoLinea.SetWrapText(true);

                SLStyle rojo = sl.CreateStyle();
                rojo.Fill.SetPatternType(PatternValues.Solid);//.BottomBorder.Color = System.Drawing.Color.Red;
                rojo.Font.FontColor = System.Drawing.Color.White;
                rojo.Fill.SetPatternForegroundColor(System.Drawing.Color.Red);

                SLStyle centrado = sl.CreateStyle();
                centrado.FormatCode = "$ * #,##0.00";
                centrado.Font.FontSize = 10;
                centrado.SetHorizontalAlignment(HorizontalAlignmentValues.Right);

                SLStyle negrita = sl.CreateStyle();
                negrita.Font.Bold = true;

                int row = 3;

                if (porLocal)
                {
                    sl.SetColumnWidth(1, 16);
                    sl.SetColumnWidth(2, 10);
                    sl.SetColumnWidth(3, 65);
                    sl.SetColumnWidth(4, 15);
                    sl.SetColumnWidth(5, 15);

                    sl.SetCellValue(1, 1, "Local");
                    sl.SetCellValue(1, 2, "Cantidad");
                    sl.SetCellValue(1, 3, "Producto");
                    sl.SetCellValue(1, 4, "Costo");
                    sl.SetCellValue(1, 5, "Costo Total");

                    sl.SetCellStyle(1, 1, 1, 5, bordeNegrita);

                    decimal total = 0;
                    var locales = ctx.Compras.Where(a => a.tandaId == idTanda).Select(a => a.Locales).Distinct().OrderBy(a => new { a.comuna, a.nombre }).ToList();
                    foreach (var local in locales)
                    {
                        var listado = ctx.CompraProducto.Where(a => a.Compras.localId == local.idLocal && a.Compras.tandaId == idTanda).OrderBy(a => a.Productos.producto).GroupBy(a => a.productoId).Select(a => new { idProducto = a.Key, cp = a.FirstOrDefault(), Cantidad = a.Sum(b => b.cantidad) });
                        int totalVecinx = (row + listado.Count() - 1);

                        sl.SetCellStyle(row, 1, totalVecinx, 1, bordeIz);
                        sl.SetCellStyle(row, 1, row, 5, bordeAr);
                        sl.SetCellStyle(totalVecinx, 1, totalVecinx, 5, bordeAb);
                        for (int x = 1; x < 6; x++)
                        {
                            sl.SetCellStyle(row, x, totalVecinx, x, bordeDe);
                        }

                        sl.SetCellValue(row, 1, local.direccion + (local.nombre != null ? "\n" + local.nombre : ""));
                        sl.SetCellStyle(row, 1, saltoLinea);
                        sl.MergeWorksheetCells(row, 1, totalVecinx, 1);

                        decimal subTotalLocal = 0;
                        foreach (var productos in listado)
                        {
                            Productos prod = ctx.Productos.FirstOrDefault(a => a.idProducto == productos.idProducto);

                            Costos ultimo = productos.cp.Costos;
                            decimal costoTotal = ultimo.costo * productos.Cantidad;
                            subTotalLocal += costoTotal;

                            sl.SetCellValue(row, 2, productos.Cantidad);
                            sl.SetCellValue(row, 3, prod.producto + " - " + prod.marca + " - " + prod.presentacion);

                            sl.SetCellValue(row, 4, ultimo.costo);
                            sl.SetCellValue(row, 5, costoTotal);

                            sl.SetCellStyle(row, 4, centrado);
                            sl.SetCellStyle(row, 5, centrado);

                            row++;
                        }

                        total += subTotalLocal;

                        sl.SetCellValue(row, 4, "Subtotal local: ");
                        sl.SetCellStyle(row, 4, negrita);
                        sl.SetCellValue(row, 5, subTotalLocal);
                        sl.SetCellStyle(row, 5, centrado);
                        sl.SetCellStyle(row, 5, negrita);

                        row++;
                        row++;

                    }

                    row++;
                    sl.SetCellValue(row, 4, "Total: ");
                    sl.SetCellStyle(row, 4, negrita);
                    sl.SetCellValue(row, 5, total);
                    sl.SetCellStyle(row, 5, centrado);
                    sl.SetCellStyle(row, 5, negrita);

                }
                else
                {
                    sl.SetColumnWidth(1, 5);
                    sl.SetColumnWidth(2, 10);
                    sl.SetColumnWidth(3, 65);
                    sl.SetColumnWidth(4, 20);
                    sl.SetColumnWidth(5, 20);
                    sl.SetColumnWidth(6, 15);
                    sl.SetColumnWidth(7, 15);

                    sl.SetCellValue(1, 1, "N°");
                    sl.SetCellValue(1, 2, "Cantidad");
                    sl.SetCellValue(1, 3, "Producto");
                    sl.SetCellValue(1, 4, "Marca");
                    sl.SetCellValue(1, 5, "Presentacion");
                    sl.SetCellValue(1, 6, "Costo");
                    sl.SetCellValue(1, 7, "Costo Total");

                    sl.SetCellStyle(1, 1, 1, 7, bordeNegrita);

                    var listado = ctx.CompraProducto.Where(a => a.Compras.tandaId == idTanda).GroupBy(a => a.productoId).Select(a => new { idProducto = a.Key, Cantidad = a.Sum(b => b.cantidad), Compra = a.FirstOrDefault() }).OrderBy(a => new { a.Compra.Productos.Categorias.nombre, a.Compra.Productos.producto }).ToList();

                    decimal subTotal = 0;
                    for (int x = 0; x < listado.Count(); x++)
                    {
                        var compra = listado.ElementAt(x);

                        Costos ultimo = compra.Compra.Costos; //.Costos.Count > 1 ? (compra.Compra.Productos.Costos.LastOrDefault(a => a.fecha.Date <= actual.fechaAbierto.Date) == null ? compra.Compra.Productos.Costos.LastOrDefault() : compra.Compra.Productos.Costos.FirstOrDefault()) : compra.Compra.Productos.Costos.FirstOrDefault();
                        decimal costoTotal = ultimo.costo * compra.Cantidad;
                        subTotal += costoTotal;

                        sl.SetCellValue(row, 1, x + 1);
                        sl.SetCellValue(row, 2, compra.Cantidad);
                        sl.SetCellValue(row, 3, compra.Compra.Productos.producto + " - " + compra.Compra.Productos.marca + " - " + compra.Compra.Productos.presentacion);
                        sl.SetCellValue(row, 4, compra.Compra.Productos.marca);
                        sl.SetCellValue(row, 5, compra.Compra.Productos.presentacion);
                        sl.SetCellValue(row, 6, ultimo.costo);
                        sl.SetCellValue(row, 7, costoTotal);

                        sl.SetCellStyle(row, 6, centrado);
                        sl.SetCellStyle(row, 7, centrado);

                        row++;


                    }

                    centrado.Font.Bold = true;
                    sl.SetCellValue(row, 5, "Total: ");
                    sl.SetCellValue(row, 7, subTotal);
                    sl.SetCellStyle(row, 7, centrado);
                }

                sl.SaveAs(mem);
                mem.Position = 0;


                TempData[handle] = mem.ToArray();

                return Json(new { FileGuid = handle, FileName = porLocal ? "Local.xlsx" : "Total.xlsx" });
            }


        }

        public ActionResult CrearExcelFinanzas(int idTanda)
        {

            /*string handle = Guid.NewGuid().ToString();

            StringBuilder csv = new StringBuilder();
            string Columnas = string.Format(";{0};{1};{2};{3};{4}", "Cantidad", "Producto", "Costo", "Precio", "Diferencia");
            csv.AppendLine(Columnas);


            TanoNEEntities ctx = new TanoNEEntities();
            Tandas actual = ctx.Tandas.FirstOrDefault(a => a.idTanda == idTanda);


            var locales = ctx.Compras.Where(a => a.tandaId == idTanda).Select(a => a.Locales).Distinct().ToList();
            foreach (var local in locales)
            {
                csv.AppendLine(local.direccion);
                decimal costoLocal = 0;
                decimal precioLocal = 0;
                var listado = ctx.CompraProducto.Where(a => a.Compras.localId == local.idLocal && a.Compras.tandaId == idTanda).GroupBy(a => a.productoId).Select(a => new { idProducto = a.Key, Cantidad = a.Sum(b => b.cantidad) }).ToArray();
                for (int x = 0; x < listado.Count(); x++)
                {
                    var compra = listado[x];
                    Productos prod = ctx.Productos.FirstOrDefault(a => a.idProducto == compra.idProducto);

                    Costos ultimoc = prod.Costos.Count > 1 ? prod.Costos.LastOrDefault(a => a.fecha.Date <= actual.fechaAbierto.Date) : prod.Costos.FirstOrDefault();
                    decimal costo = ultimoc.costo * compra.Cantidad;
                    //decimal costo = prod.Costos.FirstOrDefault(a => a.fecha <= actual.fechaAbierto).costo * compra.Cantidad;

                    Precios ultimop = prod.Precios.Count > 1 ? prod.Precios.LastOrDefault(a => a.fecha.Date <= actual.fechaAbierto.Date) : prod.Precios.FirstOrDefault();
                    decimal precio = ultimop.precio * compra.Cantidad;
                    //decimal precio = prod.Precios.FirstOrDefault(a => a.fecha <= actual.fechaAbierto).precio * compra.Cantidad;
                    costoLocal += costo;
                    precioLocal += precio;
                    string filas = string.Format(";{0};{1};${2};${3};${4}", compra.Cantidad, string.Format("{0} - {1} - {2}", prod.producto, prod.marca, prod.presentacion), costo.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), precio.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), (precio - costo).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                    csv.AppendLine(filas);
                }
                csv.AppendLine(string.Format(";;;${0};${1};${2}", costoLocal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), precioLocal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), (precioLocal - costoLocal).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)));
            }
            //string cierre = string.Format("Total;;;${0}", costoTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
            //csv.AppendLine(cierre);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.Default.GetBytes(csv.ToString())))
            {
                memoryStream.Position = 0;
                TempData[handle] = memoryStream.ToArray();
            }

            return Json(new { FileGuid = handle, FileName = "Reporte.csv" });*/


            string handle = Guid.NewGuid().ToString();

            TanoNEEntities ctx = new TanoNEEntities();
            Tandas actual = ctx.Tandas.FirstOrDefault(a => a.idTanda == idTanda);

            EstadosCompra entre = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 3);

            using (MemoryStream mem = new MemoryStream())
            using (SLDocument sl = new SLDocument())
            {
                sl.RenameWorksheet(SLDocument.DefaultFirstSheetName, "Finanzas");

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
                saltoLinea.SetWrapText(true);

                SLStyle rojo = sl.CreateStyle();
                rojo.Fill.SetPatternType(PatternValues.Solid);//.BottomBorder.Color = System.Drawing.Color.Red;
                rojo.Font.FontColor = System.Drawing.Color.White;
                rojo.Fill.SetPatternForegroundColor(System.Drawing.Color.Red);

                SLStyle centrado = sl.CreateStyle();
                centrado.FormatCode = "$ * #,##0.00";
                centrado.Font.FontSize = 10;
                centrado.SetHorizontalAlignment(HorizontalAlignmentValues.Right);

                SLStyle negrita = sl.CreateStyle();
                negrita.Font.Bold = true;

                int row = 3;

                sl.SetColumnWidth(1, 16);
                sl.SetColumnWidth(2, 10);
                sl.SetColumnWidth(3, 65);
                sl.SetColumnWidth(4, 15);
                sl.SetColumnWidth(5, 15);
                sl.SetColumnWidth(6, 15);

                sl.SetCellValue(1, 1, "Local");
                sl.SetCellValue(1, 2, "Cantidad");
                sl.SetCellValue(1, 3, "Producto");
                sl.SetCellValue(1, 4, "Costo");
                sl.SetCellValue(1, 5, "Precio");
                sl.SetCellValue(1, 6, "Diferencia");

                sl.SetCellStyle(1, 1, 1, 5, bordeNegrita);

                var locales = ctx.Compras.Where(a => a.tandaId == idTanda && a.estadoId != entre.idEstadoCompra).Select(a => a.Locales).Distinct().OrderBy(a => new { a.comuna, a.nombre }).ToList();
                foreach (var local in locales)
                {
                    var listado = ctx.CompraProducto.Where(a => a.Compras.localId == local.idLocal && a.Compras.tandaId == idTanda).GroupBy(a => a.productoId).Select(a => new { idProducto = a.Key, Cantidad = a.Sum(b => b.cantidad), CompraProducto = a.FirstOrDefault() }).ToArray();

                    int totalVecinx = (row + listado.Count() - 1);

                    sl.SetCellStyle(row, 1, totalVecinx, 1, bordeIz);
                    sl.SetCellStyle(row, 1, row, 6, bordeAr);
                    sl.SetCellStyle(totalVecinx, 1, totalVecinx, 6, bordeAb);
                    for (int x = 1; x < 7; x++)
                    {
                        sl.SetCellStyle(row, x, totalVecinx, x, bordeDe);
                    }

                    sl.SetCellValue(row, 1, local.direccion + (local.nombre != null ? "\n" + local.nombre : ""));
                    sl.SetCellStyle(row, 1, saltoLinea);
                    sl.MergeWorksheetCells(row, 1, totalVecinx, 1);

                    decimal costoLocal = 0;
                    decimal precioLocal = 0;
                    for (int x = 0; x < listado.Count(); x++)
                    {
                        var compra = listado[x];
                        Productos prod = ctx.Productos.FirstOrDefault(a => a.idProducto == compra.idProducto);

                        Costos ultimoc = compra.CompraProducto.Costos;
                        decimal costo = ultimoc.costo * compra.Cantidad;
                        //decimal costo = prod.Costos.FirstOrDefault(a => a.fecha <= actual.fechaAbierto).costo * compra.Cantidad;

                        //Precios ultimop = prod.Precios.Count > 1 ? prod.Precios.LastOrDefault(a => a.fecha.Date <= actual.fechaAbierto.Date) : prod.Precios.FirstOrDefault();
                        decimal precio = compra.CompraProducto.Precios.precio * compra.Cantidad;
                        //decimal precio = prod.Precios.FirstOrDefault(a => a.fecha <= actual.fechaAbierto).precio * compra.Cantidad;
                        costoLocal += costo;
                        precioLocal += precio;

                        //string filas = string.Format(";{0};{1};${2};${3};${4}", compra.Cantidad, string.Format("{0} - {1} - {2}", prod.producto, prod.marca, prod.presentacion), costo.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), precio.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture), (precio - costo).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));


                        sl.SetCellValue(row, 2, compra.Cantidad);
                        sl.SetCellValue(row, 3, prod.producto + " - " + prod.marca + " - " + prod.presentacion);
                        sl.SetCellValue(row, 4, costo);
                        sl.SetCellValue(row, 5, precio);
                        sl.SetCellValue(row, 6, precio - costo);

                        sl.SetCellStyle(row, 4, centrado);
                        sl.SetCellStyle(row, 5, centrado);
                        sl.SetCellStyle(row, 6, centrado);


                        row++;
                    }

                    sl.SetCellValue(row, 3, "Totales por local: ");
                    sl.SetCellValue(row, 4, costoLocal);
                    sl.SetCellValue(row, 5, precioLocal);
                    sl.SetCellValue(row, 6, precioLocal - costoLocal);

                    sl.SetCellStyle(row, 3, negrita);
                    sl.SetCellStyle(row, 4, negrita);
                    sl.SetCellStyle(row, 5, negrita);
                    sl.SetCellStyle(row, 6, negrita);

                    sl.SetCellStyle(row, 4, centrado);
                    sl.SetCellStyle(row, 5, centrado);
                    sl.SetCellStyle(row, 6, centrado);


                    row++;
                    row++;


                }


                sl.SaveAs(mem);
                mem.Position = 0;


                TempData[handle] = mem.ToArray();

                return Json(new { FileGuid = handle, FileName = "Finanzas.xlsx" });
            }

        }


        [HttpGet]
        public virtual ActionResult Descargar(string fileGuid, string fileName)
        {
            if (TempData[fileGuid] != null)
            {
                byte[] data = TempData[fileGuid] as byte[];
                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            else
            {
                // Problem - Log the error, generate a blank file,
                //           redirect to another controller action - whatever fits with your application
                return new EmptyResult();
            }
        }



        /*public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }*/

    }
}
