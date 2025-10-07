using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using inventario_coprotab.Models.DBInventario;
using inventario_coprotab.ViewModels;
using System.Linq;
using System.Threading.Tasks;
namespace inventario_coprotab.Controllers
{
    public class MovimientoController : Controller
    {
        private readonly SistemaInventarioContext _context;

        public MovimientoController(SistemaInventarioContext context)
        {
            _context = context;
        }

        // GET: Movimiento/Create
        public IActionResult Create()
        {
            ViewBag.Dispositivos = _context.Dispositivos.Where(d => d.EstadoRegistro).ToList();
            ViewBag.Ubicaciones = _context.Ubicaciones.ToList();
            ViewBag.Responsables = _context.Responsables.ToList();
            return View();
        }

        // POST: Movimiento/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovimientoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var dispositivo = await _context.Dispositivos.FindAsync(model.IdDispositivo);
                if (dispositivo == null)
                {
                    ModelState.AddModelError("IdDispositivo", "Dispositivo no encontrado");
                    return View(model);
                }

                // Validar que el stock no quede negativo en salidas
                if (model.TipoMovimiento == "Salida" && dispositivo.StockActual < model.Cantidad)
                {
                    ModelState.AddModelError("Cantidad", $"No hay suficiente stock. Stock actual: {dispositivo.StockActual}");
                    return View(model);
                }

                var movimiento = new Movimiento
                {
                    IdDispositivo = model.IdDispositivo,
                    TipoMovimiento = model.TipoMovimiento ?? "Sin nombre",
                    Fecha = DateTime.Now,
                    IdUbicacion = model.IdUbicacion,
                    IdResponsable = model.IdResponsable,
                    Cantidad = model.Cantidad,
                    Observaciones = model.Observaciones
                };

                _context.Movimientos.Add(movimiento);

                // Actualizar stock
                if (model.TipoMovimiento == "Entrada")
                {
                    dispositivo.StockActual += model.Cantidad;
                }
                else if (model.TipoMovimiento == "Salida")
                {
                    dispositivo.StockActual -= model.Cantidad;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), "Dispositivo");
            }

            ViewBag.Dispositivos = _context.Dispositivos.Where(d => d.EstadoRegistro).ToList();
            ViewBag.Ubicaciones = _context.Ubicaciones.ToList();
            ViewBag.Responsables = _context.Responsables.ToList();
            return View(model);
        }

        public async Task<IActionResult> Movimientos()
        {
            List<Movimiento> Lista = _context.Movimientos.Include(d => d.oUbicaion)
                .Include(d => d.oDispositivo)
                .Include(d => d.oResponsable)
                .ToList();
            return View(Lista);
        }

        // GET: Movimiento/Edit
        public async Task<IActionResult> Edit(int id)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento == null)
                return NotFound();

            var vm = new MovimientoViewModel
            {
                IdMovimiento = movimiento.IdMovimiento,
                IdDispositivo = movimiento.IdDispositivo,
                TipoMovimiento = movimiento.TipoMovimiento,
                Cantidad = movimiento.Cantidad,
                IdUbicacion = movimiento.IdUbicacion,
                IdResponsable = movimiento.IdResponsable,
                Observaciones = movimiento.Observaciones,
                NombreDispositivo = _context.Dispositivos
                         .Where(d => d.IdDispositivo == movimiento.IdDispositivo)
                         .Select(d => d.Nombre).FirstOrDefault(),
                StockActual = _context.Dispositivos
                         .Where(d => d.IdDispositivo == movimiento.IdDispositivo)
                         .Select(d => (int?)d.StockActual).FirstOrDefault() ?? 0
            };


            ViewBag.Dispositivos = _context.Dispositivos.Where(d => d.EstadoRegistro).ToList();
            ViewBag.Ubicaciones = _context.Ubicaciones.ToList();
            ViewBag.Responsables = _context.Responsables.ToList();

            return PartialView("_EditMovimiento", vm);
            return View(Movimientos);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MovimientoViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_EditMovimiento", model);

            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento == null)
                return Json(new { success = false, message = "Movimiento no encontrado" });

            movimiento.TipoMovimiento = model.TipoMovimiento;
            movimiento.Cantidad = model.Cantidad;
            movimiento.IdUbicacion = model.IdUbicacion;
            movimiento.IdResponsable = model.IdResponsable;
            movimiento.Observaciones = model.Observaciones;

            _context.Update(movimiento);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                movimiento = new
                {
                    id = movimiento.IdMovimiento,
                    tipo = movimiento.TipoMovimiento,
                    fecha = movimiento.Fecha.ToString("dd/MM/yyyy"),
                    ubicacion = movimiento.IdUbicacion,
                    responsable = movimiento.IdResponsable,
                    cantidad = movimiento.Cantidad,
                    observaciones = movimiento.Observaciones
                }
            });
        }

    }
}