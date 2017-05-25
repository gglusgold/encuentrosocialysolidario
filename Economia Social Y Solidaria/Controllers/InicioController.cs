using Economia_Social_Y_Solidaria.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Economia_Social_Y_Solidaria.Controllers
{

    public class InicioController : Controller
    {
        //
        // GET: /Inicio/

        public ActionResult IniciarSesion()
        {

            return View();
        }


        [HttpPost]
        public ActionResult IniciarSesion(string usuario, string pass, string email, string nombres, string telefono, string password, string url, int comuna = -1)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Response.Cookies.Add(new HttpCookie("Error"));
            Response.Cookies.Add(new HttpCookie("Info"));
            Response.Cookies.Add(new HttpCookie("Mensaje"));


            if (usuario != null)
            {
                //login
                Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == usuario);
                if (vecino != null)
                {
                    if (vecino.contrasena == null)
                    {
                        Response.Cookies["Error"].Value = "Se ha reseteado su contraseña. Revise su mail";
                        return RedirectToAction("Portada", "Noticias");
                    }

                    if (!vecino.verificado)
                    {
                        Response.Cookies["Error"].Value = "Hay que verificar la cuenta. Revisa tu mail";
                        return RedirectToAction("Portada", "Noticias");
                    }

                    pass = GetCrypt(pass);
                    if (vecino.contrasena == pass)
                    {
                        Response.Cookies.Remove("Error");
                        Response.Cookies.Remove("Info");
                        Response.Cookies.Remove("Mensaje");

                        FormsAuthentication.SetAuthCookie(vecino.correo, true);
                        //if (url.Split('/').Length == 3)
                        //    return RedirectToAction(url.Split('/')[2], url.Split('/')[1]);
                        //else
                        return RedirectToAction("Portada", "Noticias");
                    }
                    else
                        Response.Cookies["Error"].Value = "Contraseña no válida";
                }
                else
                    Response.Cookies["Error"].Value = "Usuario no encontrado, registrese";
            }
            else
            {
                string urla = ConfigurationManager.AppSettings["UrlSitio"];
                //registrar
                Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == email);
                if (vecino == null)
                {
                    vecino = new Vecinos();
                    vecino.correo = email;
                    vecino.nombres = nombres;
                    vecino.telefono = telefono;
                    vecino.contrasena = GetCrypt(password);
                    vecino.comuna = comuna;
                    vecino.fechaCreado = DateTime.Now;

                    List<string> hashes = ctx.Vecinos.Select(a => a.hash).ToList();
                    string hash = RandomString(25);
                    while (hashes.Contains(hash))
                    {
                        hash = RandomString(25);
                    }

                    vecino.hash = hash;
                    ctx.Vecinos.Add(vecino);


                    using (MailMessage mail = new MailMessage())
                    {
                        //string datos = "Datos para entrar:<br/>Usuario: " + email;
                       // string datos = "Datos para entrar:<br/>Usuario: " + email + " Contraseña :" + password;
                        mail.From = new MailAddress("economiasocial@encuentrocapital.com.ar", "Economía Social y Solidaria");
                        mail.To.Add(email);
                        mail.Subject = "Economia Social y Solidaria -- Nuevo Encuentro";
                        mail.Body = "<p>Gracias por usar nuestro sistema. Hacé click en el link de abajo para activar la cuenta</p>";
                        mail.Body += "<p><a href='" + urla + "Inicio/ActivarCuenta?k=" + hash + "'>Activar Cuenta</a></p>";
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

                    ctx.SaveChanges();

                    Response.Cookies["Info"].Value = "Te mandamos un mail para confirmar la cuenta!";
                }
            }
            return RedirectToAction("Portada", "Noticias");
        }

        public ActionResult ActivarCuenta(string k)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos confirmar = ctx.Vecinos.FirstOrDefault(a => a.hash == k);
            if (confirmar != null)
            {
                confirmar.verificado = true;
                ctx.SaveChanges();
                Response.Cookies["Mensaje"].Value = "Se activó tu cuenta, volvé a iniciar sesion";
            }
            else
                Response.Cookies["Mensaje"].Value = ViewBag.Error = "No se encontró la cuenta para activar";

            return RedirectToAction("Portada", "Noticias");
        }


        public ActionResult ResetearCuenta(string k)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos confirmar = ctx.Vecinos.FirstOrDefault(a => a.hash == k);
            if (confirmar != null)
            {
                confirmar.contrasena = null;
                ctx.SaveChanges();
                Response.Cookies["Mensaje"].Value = "Ingrese su nuevas credenciales";
                Response.Cookies["Mail"].Value = confirmar.correo;
                Response.Cookies["k"].Value = k;
            }
            else
                Response.Cookies["Mensaje"].Value = ViewBag.Error = "No se encontró la cuenta para activar";

            return RedirectToAction("Portada", "Noticias");
        }

        public ActionResult Cambiar(string k, string emaila, string password1)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Vecinos confirmar = ctx.Vecinos.FirstOrDefault(a => a.hash == k && a.correo == emaila);
            if (confirmar != null)
            {
                confirmar.contrasena = GetCrypt(password1);
                ctx.SaveChanges();

                Response.Cookies["Info"].Value = "Se ha cambiado la contraseña satisfactoriamente";
            }
            else
                Response.Cookies["Info"].Value = ViewBag.Error = "No se encontró la cuenta para activar";

            return RedirectToAction("Portada", "Noticias");
        }

        public ActionResult OlvidePass(string emailolvido)
        {
            TanoNEEntities ctx = new TanoNEEntities();

            Vecinos vecino = ctx.Vecinos.FirstOrDefault(a => a.correo == emailolvido);
            if (vecino == null)
            {
                Response.Cookies["Info"].Value = "No existe el mail ingresado";
                return RedirectToAction("Portada", "Noticias");
            }

            string urla = ConfigurationManager.AppSettings["UrlSitio"];

            using (MailMessage mail = new MailMessage())
            {
                //string datos = "Datos para entrar:<br/>Usuario: " + email;
                mail.From = new MailAddress("economiasocial@encuentrocapital.com.ar", "Economía Social y Solidaria");
                mail.To.Add(emailolvido);
                mail.Subject = "Economia Social y Solidaria -- Nuevo Encuentro";
                mail.Body = "<p>Gracias por usar nuestro sistema. Hacé click en el link de abajo para resetear la contraseña</p>";
                mail.Body += "<p><a href='" + urla + "Inicio/ResetearCuenta?k=" + vecino.hash + "'>Resetear contraseña</a></p>";
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                //using (SmtpClient smtp = new SmtpClient("smtp.encuentrocapital.com.ar", 587))
                {
                    // smtp.Credentials = new NetworkCredential("racinglocura07@gmail.com", "kapanga34224389,");
                    smtp.Credentials = new NetworkCredential("economiasocial@encuentrocapital.com.ar", "Frent3355");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }

                /*vecino.contrasena = null;
                ctx.SaveChanges();*/
            }


            Response.Cookies["Info"].Value = "Te mandamos un mail para resetear la cuenta!";

            return RedirectToAction("Portada", "Noticias");
        }

        public ActionResult CerrarSesion()
        {
            Session.Abandon();
            FormsAuthentication.SignOut();
            return RedirectToAction("Portada", "Noticias");
        }


        public JsonResult Logueado()
        {
            return Json(new { nombre = User.Identity.Name, logueado = User.Identity.IsAuthenticated }, JsonRequestBehavior.DenyGet);
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GetCrypt(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            var sb = new StringBuilder();

            using (var sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(bytes);

                foreach (byte b in hashBytes)
                {
                    var hex = b.ToString("x2");
                    sb.Append(hex);
                }
            }

            return sb.ToString();
        }
    }
}
