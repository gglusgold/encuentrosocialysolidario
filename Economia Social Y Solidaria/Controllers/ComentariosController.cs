using Economia_Social_Y_Solidaria.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class ComentariosController : Controller
    {
        // GET: Comentarios
        public ActionResult Comentarios()
        {
            return View();
        }

        public JsonResult Lista(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = "Id ASC", string local = null, int comuna = -1,  string producto = null, string comentario = null, int estrellas = -1)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var listaFinal = ctx.ComentariosProducto.ToList().Select(a => new
            {
                idProductoComentario = a.idProductoComentario,
                local = a.Compras.Locales.nombre == null ? a.Compras.Locales.direccion : a.Compras.Locales.nombre,
                comuna = a.Compras.Locales.comuna,
                vecinx = a.Compras.Vecinos.nombres + " - " + a.Compras.Vecinos.telefono,
                producto = a.Productos.producto + " - " + a.Productos.presentacion + (a.Productos.marca != null ? "\n" + a.Productos.marca : ""),
                comentario = a.comentario,
                estrellas = a.estrellas,
                fecha = a.fecha,
                visible =a.visible
            });

            switch (jtSorting)
            {
                default:
                    listaFinal = listaFinal.OrderByDescending(a => a.idProductoComentario);
                    break;

                case "local ASC":
                    listaFinal = listaFinal.OrderBy(a => a.local);
                    break;

                case "local DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.local);
                    break;

                case "comuna ASC":
                    listaFinal = listaFinal.OrderBy(a => a.comuna);
                    break;

                case "comuna DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.comuna);
                    break;

                case "producto ASC":
                    listaFinal = listaFinal.OrderBy(a => a.producto);
                    break;

                case "producto DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.producto);
                    break;

                case "comentario ASC":
                    listaFinal = listaFinal.OrderBy(a => a.comentario);
                    break;

                case "comentario DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.comentario);
                    break;

                case "estrellas ASC":
                    listaFinal = listaFinal.OrderBy(a => a.estrellas);
                    break;

                case "estrellas DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.estrellas);
                    break;

                case "fecha ASC":
                    listaFinal = listaFinal.OrderBy(a => a.fecha);
                    break;

                case "fecha DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.fecha);
                    break;

                case "visible ASC":
                    listaFinal = listaFinal.OrderBy(a => a.visible);
                    break;

                case "visible DESC":
                    listaFinal = listaFinal.OrderByDescending(a => a.visible);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(local))
                listaFinal = listaFinal.Where(a => a.local.Contains(local)).ToList();

            if (comuna > 0)
                listaFinal = listaFinal.Where(a => a.comuna == comuna).ToList();

            if (!string.IsNullOrWhiteSpace(producto))
                listaFinal = listaFinal.Where(a => a.producto.Contains(producto)).ToList();

            if (!string.IsNullOrWhiteSpace(comentario))
                listaFinal = listaFinal.Where(a => a.comentario.Contains(comentario)).ToList();

            if (estrellas > 0)
                listaFinal = listaFinal.Where(a => a.estrellas == estrellas).ToList();

            

            int size = listaFinal.Count();

            if (jtPageSize > 0 || jtStartIndex > 0)
                listaFinal = listaFinal.Skip(jtStartIndex).Take(jtPageSize).ToList();

            return Json(new { Result = "OK", Records = listaFinal, TotalRecordCount = size });
        }

        public JsonResult Cambiar(int idProductoComentario)
        {
            TanoNEEntities ctx = new TanoNEEntities();
            ComentariosProducto comentario =  ctx.ComentariosProducto.FirstOrDefault(a => a.idProductoComentario == idProductoComentario);
            if (comentario != null)
            {
                comentario.visible = !comentario.visible;
                ctx.SaveChanges();
                return Json(comentario.visible);
            }
            else
                return Json("Error");
        }
    }
}