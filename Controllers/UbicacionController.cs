using inventario_coprotab.Models.DBInventario;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Text;
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

        // ✅ Vista principal con búsqueda inteligente (por nombre o descripción)
        [HttpGet]
        public async Task<IActionResult> Create_Ubicacion(string searchUbicacion)
        {
            // Primero obtenemos todos los registros (ordenados)
            var ubicaciones = await _context.Ubicaciones.OrderBy(u => u.Nombre).ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchUbicacion))
            {
                // 🔹 Función para normalizar texto (sin acentos, espacios ni mayúsculas)
                string Normalize(string text)
                {
                    if (string.IsNullOrEmpty(text)) return "";
                    var normalized = text.ToLowerInvariant().Trim();
                    normalized = new string(normalized
                        .Normalize(NormalizationForm.FormD)
                        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                        .ToArray());
                    return normalized.Replace(" ", "");
                }

                string filtro = Normalize(searchUbicacion);

                // 🔹 Aplicar el filtro en memoria (no en SQL)
                ubicaciones = ubicaciones.Where(u =>
                    (u.Nombre != null && Normalize(u.Nombre).Contains(filtro)) ||
                    (u.Descripcion != null && Normalize(u.Descripcion).Contains(filtro))
                ).ToList();
            }

            ViewBag.SearchUbicacion = searchUbicacion;
            return View("create_ubicacion", ubicaciones);
        }


        // Resto de métodos (Create, Edit, Delete) se mantienen igual
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] Ubicacione ubicacion)
        {
            if (!ModelState.IsValid)
                return BadRequest("Datos inválidos");

            _context.Add(ubicacion);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Ubicación creada correctamente ✅",
                id = ubicacion.IdUbicacion,
                nombre = ubicacion.Nombre,
                descripcion = ubicacion.Descripcion
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] Ubicacione ubicacion)
        {
            var ubic = await _context.Ubicaciones.FindAsync(ubicacion.IdUbicacion);
            if (ubic == null) return NotFound();

            ubic.Nombre = ubicacion.Nombre;
            ubic.Descripcion = ubicacion.Descripcion;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ubicación actualizada correctamente ✏️" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ubicacion = await _context.Ubicaciones.FindAsync(id);
            if (ubicacion == null) return NotFound();

            _context.Ubicaciones.Remove(ubicacion);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Create_Ubicacion));
        }
    }
}
