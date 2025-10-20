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
        public async Task<IActionResult> Index(string searchUbicacion, string searchDispositivo, string searchResponsable, string searchComponente)
        {
            var Lista = await _context.Movimientos
                .Include(d => d.IdUbicacionNavigation)
                .Include(d => d.IdDispositivoNavigation)
                .Include(d => d.IdResponsableNavigation)
                .Include(d => d.IdComponenteNavigation)
                .Where(d => d.IdUbicacionNavigation != null)
                .ToListAsync();

            if (!string.IsNullOrEmpty(searchUbicacion))
                Lista = Lista.Where(d => d.IdUbicacionNavigation != null && 
                                         d.IdUbicacionNavigation.Nombre != null &&
                                         d.IdUbicacionNavigation.Nombre.Contains(searchUbicacion)).ToList();

            if (!string.IsNullOrEmpty(searchDispositivo))
                Lista = Lista.Where(d => d.IdDispositivoNavigation != null &&
                                         d.IdDispositivoNavigation.Nombre != null &&
                                         d.IdDispositivoNavigation.Nombre.Contains (searchDispositivo)).ToList();

            if (!string.IsNullOrEmpty(searchResponsable))
                Lista = Lista.Where(d => d.IdResponsableNavigation != null && 
                                         d.IdResponsableNavigation.Nombre != null &&
                                         d.IdResponsableNavigation.Nombre.Contains(searchResponsable)).ToList();
            if (!string.IsNullOrEmpty(searchComponente))
                Lista = Lista.Where(d => d.IdComponenteNavigation != null &&
                                         d.IdComponenteNavigation.Nombre != null &&
                                         d.IdComponenteNavigation.Nombre.Contains(searchComponente)).ToList();


            var ListaFiltrada =  Lista;

            ViewBag.SearchCode = searchUbicacion;
            ViewBag.SearchSerie = searchDispositivo;
            ViewBag.SearchTipo = searchResponsable;
            ViewBag.SearchSeriec = searchComponente;


            return View(ListaFiltrada);
        }
    }
}
