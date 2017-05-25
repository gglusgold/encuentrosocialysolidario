using Economia_Social_Y_Solidaria.Models;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class VecinosController : Controller
    {
        //
        // GET: /Vecinos/

        public ActionResult Vecino()
        {
            return View();
        }

        public JsonResult Perfil()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var perfil = ctx.Vecinos.Select(a => new
            {
                idVecinx = a.idVecino,
                nombre = a.nombres,
                comuna = a.comuna,
                correo = a.correo,
                telefono = a.telefono,
                totalCompras = a.Compras.Count,
                totalProductosComprados= 0,
                alertas = ctx.Alertas.Where(b=> b.android == false || b.android == (a.token != null)).Select(b => new { tipo = b.tipo, codigo = b.codigo, activa = a.AlertasVecinxs.Any( c=> c.alertaId == b.idAlerta) })
            }).FirstOrDefault(a => a.correo == User.Identity.Name);

            if (perfil == null)
                return Json(new { Error = "No se ha iniciado sesion", JsonRequestBehavior.DenyGet });

            return Json(new { Perfil = perfil, JsonRequestBehavior.DenyGet });
        }

        public JsonResult ModificarPerfil(string emailperfil, string nombreperfil, string telefonoperfil, int comunaperfil, int[] idsalertas, bool[] valalertas)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var perfil = ctx.Vecinos.FirstOrDefault(a => a.correo == User.Identity.Name);
            if (perfil != null)
            {
                if (ctx.Vecinos.FirstOrDefault(a => a.correo == emailperfil && a.idVecino != perfil.idVecino) != null )
                    return Json(new { Error = "El correo ingresado ya existe", JsonRequestBehavior.DenyGet });

                perfil.nombres = nombreperfil;
                perfil.telefono = telefonoperfil;
                perfil.comuna = comunaperfil;

                for ( int x = 0; x < idsalertas.Length; x++)
                {
                    int idAlerta = idsalertas[x];
                    if (valalertas[x])
                    {
                        perfil.AlertasVecinxs.Add(new AlertasVecinxs { alertaId = idAlerta });
                    }
                    else
                    {
                        var alerta = ctx.AlertasVecinxs.FirstOrDefault(a => a.alertaId == idAlerta && a.vecinxId == perfil.idVecino);
                        if (alerta != null)
                            ctx.AlertasVecinxs.Remove(alerta);
                    }
                }

                if (emailperfil != perfil.correo)
                {
                    perfil.correo = emailperfil;
                    perfil.verificado = false;

                    using (MailMessage mail = new MailMessage())
                    {
                        string urla = ConfigurationManager.AppSettings["UrlSitio"];

                        //string datos = "Datos para entrar:<br/>Usuario: " + email;
                        // string datos = "Datos para entrar:<br/>Usuario: " + email + " Contraseña :" + password;
                        mail.From = new MailAddress("economiasocial@encuentrocapital.com.ar", "Economía Social y Solidaria");
                        mail.To.Add(emailperfil);
                        mail.Subject = "Economia Social y Solidaria -- Nuevo Encuentro";
                        mail.Body = "<p>Gracias por usar nuestro sistema. Hacé click en el link de abajo para activar la cuenta</p>";
                        mail.Body += "<p><a href='" + urla + "Inicio/ActivarCuenta?k=" + perfil.hash + "'>Activar Cuenta</a></p>";
                        // mail.Body += "<p>-------------------</p><p><p>" + datos + "</p><p>-------------------</p>";
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

                    Response.Cookies["Info"].Value = "Te mandamos un mail para confirmar la cuenta!";
                }

                ctx.SaveChanges();

                return Json(new { sacar = !(perfil.verificado) }, JsonRequestBehavior.DenyGet);
            }

            return Json(new { Error = "No se encontró el usuario", JsonRequestBehavior.DenyGet });


        }

        public JsonResult Lista()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var lista = ctx.Vecinos.ToList().Select(a => new
            {
                idVecino = a.idVecino,
                nombres = a.nombres,
                correo = a.correo,
                telefono = a.telefono,
                comuna = a.comuna,
                fechaCreado = a.fechaCreado.HasValue ? a.fechaCreado.Value.ToString("dd/MM/yyyy") : "-",
                roles = a.RolesVecinos.Select(b => b.Roles.codigoRol),
                administrador = a.RolesVecinos.Any(b => b.Roles.codigoRol == 2),
                contador = a.RolesVecinos.Any(b => b.Roles.codigoRol == 3),
                encargado = a.RolesVecinos.Any(b => b.Roles.codigoRol == 4),
                noticias = a.RolesVecinos.Any(b => b.Roles.codigoRol == 5),
            });

            return Json(lista, JsonRequestBehavior.DenyGet);
        }

        public JsonResult Editar(int idVecino, bool? administrador = null, bool? contador = null, bool? encargado = null, bool? noticias = null)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vec = ctx.Vecinos.FirstOrDefault(a => a.idVecino == idVecino);

            Roles admin = ctx.Roles.FirstOrDefault(a => a.codigoRol == 2);
            Roles cont = ctx.Roles.FirstOrDefault(a => a.codigoRol == 3);
            Roles enc = ctx.Roles.FirstOrDefault(a => a.codigoRol == 4);
            Roles noti = ctx.Roles.FirstOrDefault(a => a.codigoRol == 5);

            //var rolesVecino = ctx.RolesVecinos.Where(a => a.vecinoId == vec.idVecino).ToList<RolesVecinos>();
            //rolesVecino.ForEach(cs => ctx.RolesVecinos.Remove(cs));

            if (administrador.HasValue && administrador.Value)
                ctx.RolesVecinos.Add(new RolesVecinos { Roles = admin, vecinoId = vec.idVecino });
            else if (administrador.HasValue && !administrador.Value)
                ctx.RolesVecinos.Remove(vec.RolesVecinos.FirstOrDefault(a => a.rolId == admin.idRol));

            if (contador.HasValue && contador.Value)
                ctx.RolesVecinos.Add(new RolesVecinos { Roles = cont, vecinoId = vec.idVecino });
            else if (contador.HasValue && !contador.Value)
                ctx.RolesVecinos.Remove(vec.RolesVecinos.FirstOrDefault(a => a.rolId == cont.idRol));

            if (encargado.HasValue && encargado.Value)
                ctx.RolesVecinos.Add(new RolesVecinos { Roles = enc, vecinoId = vec.idVecino });
            else if (encargado.HasValue && !encargado.Value)
                ctx.RolesVecinos.Remove(vec.RolesVecinos.FirstOrDefault(a => a.rolId == enc.idRol));

            if (noticias.HasValue && noticias.Value)
                ctx.RolesVecinos.Add(new RolesVecinos { Roles = noti, vecinoId = vec.idVecino });
            else if (noticias.HasValue && !noticias.Value)
                ctx.RolesVecinos.Remove(vec.RolesVecinos.FirstOrDefault(a => a.rolId == noti.idRol));

            ctx.SaveChanges();

            return Json(new { error = false, admin = administrador, contador = contador, encargado = encargado, noticias = noticias }, JsonRequestBehavior.DenyGet);
        }

        public void Borrar(int idVecino)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos vec = ctx.Vecinos.FirstOrDefault(a => a.idVecino == idVecino);
            ctx.Vecinos.Remove(vec);
            ctx.SaveChanges();
        }
    }
}
