using inventario_coprotab.Models.DBInventario;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace inventario_coprotab.Controllers
{
    public class UbicacionController : Controller
    {
        private readonly SistemaInventarioContext _context;

        public UbicacionController(SistemaInventarioContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchUbicacion, string searchDispositivo, string searchResponsable)
        {
            var Lista = _context.Movimientos
                .Include(d => d.oUbicaion)
                .Include(d => d.oDispositivo)
                .Include(d => d.oResponsable)
                .Where(d => d.oUbicaion != null)
                .ToList();

            if (!string.IsNullOrEmpty(searchUbicacion))
                Lista = Lista.Where(d => d.oUbicaion != null && 
                                         d.oUbicaion.Nombre != null &&
                                         d.oUbicaion.Nombre.Contains(searchUbicacion)).ToList();

            if (!string.IsNullOrEmpty(searchDispositivo))
                Lista = Lista.Where(d => d.oDispositivo != null &&
                                         d.oDispositivo.Nombre != null &&
                                         d.oDispositivo.Nombre.Contains (searchDispositivo)).ToList();

            if (!string.IsNullOrEmpty(searchResponsable))
                Lista = Lista.Where(d => d.oResponsable != null && 
                                         d.oResponsable.Nombre != null &&
                                         d.oResponsable.Nombre.Contains(searchResponsable)).ToList();


            var ListaFiltrada =  Lista;

            ViewBag.SearchCode = searchUbicacion;
            ViewBag.SearchSerie = searchDispositivo;
            ViewBag.SearchTipo = searchResponsable;
            

            return View(ListaFiltrada);
        }
    }
}
