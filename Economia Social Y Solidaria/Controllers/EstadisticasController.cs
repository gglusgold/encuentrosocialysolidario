using Economia_Social_Y_Solidaria.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Economia_Social_Y_Solidaria.Controllers
{
    public class ComprasComuna
    {
        public int ComunaNumero;
        public string Comuna;
        public int Compras;
    }

    public class VecinosComuna
    {
        public int ComunaNumero;
        public string Comuna;
        public int Vecinos;
    }

    public class ProductosEst
    {
        string nombre { get; set; }
        public int[] valores { get; set; }
    }

    public class HistoricoTandas
    {
        public string xAxis { get; set; }
        public List<ProductosEst> Productos { get; set; }
    }

    public class Estadistica
    {
        public List<ComprasComuna> ComprasComuna { get; set; }
        public List<VecinosComuna> VecinosComuna { get; set; }
        public List<HistoricoTandas> HistoricoTandas { get; set; }

    }

    public class ProductosVecino
    {
        public string Nombre;
        public string Vecino;
        public int Cantidad;

    }

    [Authorize]
    public class EstadisticasController : Controller
    {

        public ActionResult Estadisticas()
        {
            TanoNEEntities ctx = new TanoNEEntities();
            Estadistica estadistica = new Estadistica();

            EstadosCompra Entregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 3);
            estadistica.ComprasComuna = ctx.Compras.Where(a => a.EstadosCompra.codigo >= Entregado.codigo).GroupBy(a => a.Locales.comuna).Select(b => new ComprasComuna { ComunaNumero = b.Key, Comuna = "Comuna " + b.Key, Compras = b.Count() }).ToList();
            estadistica.VecinosComuna = ctx.Vecinos.Where(a => a.verificado).GroupBy(a => a.comuna).Select(b => new VecinosComuna { ComunaNumero = b.Key.Value, Comuna = "Comuna " + b.Key, Vecinos = b.Count() }).ToList();

            //estadistica.HistoricoTandas = ctx.Tandas.Where(a => a.fechaCerrado.HasValue).ToList().Select(a => new HistoricoTandas
            //{
            //    xAxis = ComprasController.GetNextWeekday(a.fechaCerrado.Value, DayOfWeek.Saturday).ToShortDateString(),
            //    Productos = a.Compras.Select(c => c.CompraProducto.Select(b => new ProductosEst { valores = b.cantidad })).ToList()
            //}).ToList();
        

            //COMPRAS POR CICUITO

            return View(estadistica);
    }

    public JsonResult test()
    {
        TanoNEEntities ctx = new TanoNEEntities();
        var fechasCircuitos = ctx.Tandas.Where(a => a.circuitoId == 1 && a.fechaCerrado != null).ToList().Select(a => new
        {
            fechas = a.fechaCerrado.Value.ToString("dd/MM/yyyy"),
            productos = a.Compras.Select(b => b.CompraProducto.Select(c => c.Productos.producto)),
            ventas = a.Compras.Select(b => b.CompraProducto.Select(c => c.cantidad))
        }).ToList();

        return Json(fechasCircuitos, JsonRequestBehavior.AllowGet);
    }

    public JsonResult ProductoPorComuna(int comuna)
    {
        TanoNEEntities ctx = new TanoNEEntities();

        EstadosCompra Entregado = ctx.EstadosCompra.FirstOrDefault(a => a.codigo == 3);
        var compras = ctx.CompraProducto.Where(a => a.Compras.EstadosCompra.codigo >= Entregado.codigo && a.Compras.Locales.comuna == comuna)
            .GroupBy(a => a.Productos.producto)
            .Select(b => new
            {
                Nombre = b.Key,
                Cantidad = b.Sum(a => a.cantidad)
            });



        return Json(new { listado = compras, quienes = "" });
    }

}
}
