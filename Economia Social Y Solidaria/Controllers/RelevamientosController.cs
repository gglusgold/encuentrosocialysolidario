using System;
using System.Collections.Generic;
using Economia_Social_Y_Solidaria.Models;
using System.Web.Http;
using System.Web;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class RelevamientosController : ApiController
    {



        // POST api/relevamientos
        public Respuesta Post()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            var valores = HttpContext.Current.Request.Form;
            var total = valores.Count / 7;

            Respuesta res = new Respuesta();
            List<int> ids = new List<int>();
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            for ( int x = 0; x < total; x++)
            {
                int cantidad = int.Parse(valores["lista[" + x + "][cantidad]"]);
                string donde = valores["lista[" + x + "][donde]"];
                long fecha = long.Parse(valores["lista[" + x + "][fechaRelevado]"]);
                DateTime date = start.AddMilliseconds(fecha).ToLocalTime();

                string fruta = valores["lista[" + x + "][fruta]"];
                int idCel = int.Parse(valores["lista[" + x + "][idCel]"]);
                decimal precio = decimal.Parse(valores["lista[" + x + "][precio]"]);
                string tipo = valores["lista[" + x + "][tipo]"];

                Relevamientos rel = new Relevamientos();
                rel.cantidad = cantidad;
                rel.donde = donde;
                rel.fechaRelevado = date;
                rel.fechaSincronizado = DateTime.Now;
                rel.precio = precio;
                rel.tipo = tipo;
                ctx.Relevamientos.Add(rel);
                ids.Add(idCel);
            }

            res.ids = ids.ToArray();
            ctx.SaveChanges();
            return res;
        }

    }

    public class Respuesta
    {
        public string error = null;
        public int[] ids { get; set; }
    }
}
