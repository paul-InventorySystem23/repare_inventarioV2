using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using inventario_coprotab.Models.DBInventario;
using inventario_coprotab.ViewModels;
using System.Linq;

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
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
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
            var movimientos = new List<Movimiento>();

             movimientos =  _context.Movimientos.ToList();

            return View(movimientos);
        }






    }
}