using inventario_coprotab.Models.DBInventario;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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
            // 🔹 Cargar movimientos con sus relaciones
            var Lista = await _context.Movimientos
                .Include(d => d.IdUbicacionNavigation)
                .Include(d => d.IdDispositivoNavigation)
                .Include(d => d.IdResponsableNavigation)
                .Include(d => d.IdComponenteNavigation)
                .Where(d => d.IdUbicacionNavigation != null)
                .ToListAsync();

            // 🔹 Función local para normalizar texto (quita espacios y mayúsculas)
            string Normalize(string value) =>
                string.IsNullOrWhiteSpace(value) ? "" : value.ToLower().Replace(" ", "");

            // 🔹 Filtros (ignorando mayúsculas y espacios)
            if (!string.IsNullOrEmpty(searchUbicacion))
            {
                var filtro = Normalize(searchUbicacion);
                Lista = Lista.Where(d =>
                    d.IdUbicacionNavigation?.Nombre != null &&
                    Normalize(d.IdUbicacionNavigation.Nombre).Contains(filtro)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(searchDispositivo))
            {
                var filtro = Normalize(searchDispositivo);
                Lista = Lista.Where(d =>
                    d.IdDispositivoNavigation?.Nombre != null &&
                    Normalize(d.IdDispositivoNavigation.Nombre).Contains(filtro)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(searchResponsable))
            {
                var filtro = Normalize(searchResponsable);
                Lista = Lista.Where(d =>
                    d.IdResponsableNavigation?.Nombre != null &&
                    Normalize(d.IdResponsableNavigation.Nombre).Contains(filtro)
                ).ToList();
            }

            if (!string.IsNullOrEmpty(searchComponente))
            {
                var filtro = Normalize(searchComponente);
                Lista = Lista.Where(d =>
                    d.IdComponenteNavigation?.Nombre != null &&
                    Normalize(d.IdComponenteNavigation.Nombre).Contains(filtro)
                ).ToList();
            }

            // 🔹 Mantener valores de búsqueda en los inputs
            ViewBag.SearchUbicacion = searchUbicacion;
            ViewBag.SearchDispositivo = searchDispositivo;
            ViewBag.SearchResponsable = searchResponsable;
            ViewBag.SearchComponente = searchComponente;

            return View(Lista);
        }
    }
}
