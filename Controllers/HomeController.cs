using System.Diagnostics;
using inventario_coprotab.Models;
using inventario_coprotab.Models.DBInventario;
using inventario_coprotab.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace inventario_coprotab.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SistemaInventarioContext _context;

        public HomeController(ILogger<HomeController> logger, SistemaInventarioContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Obtener información del usuario desde la sesión
            var nombreUsuario = HttpContext.Session.GetString("UserName") ?? "Usuario";
            var emailUsuario = HttpContext.Session.GetString("Email") ?? "";

            var viewModel = new DashboardViewModel
            {
                NombreUsuario = nombreUsuario,
                EmailUsuario = emailUsuario,
                FechaActual = DateTime.Now
            };

            // ============ ESTADÍSTICAS GENERALES ============
            viewModel.TotalDispositivos = await _context.Dispositivos
                .Where(d => d.EstadoRegistro)
                .CountAsync();

            viewModel.TotalComponentes = await _context.Componentes
                .Where(c => c.EstadoRegistro)
                .CountAsync();

            viewModel.TotalUbicaciones = await _context.Ubicaciones.CountAsync();

            // Movimientos de hoy
            var hoy = DateTime.Today;
            viewModel.MovimientosHoy = await _context.Movimientos
                .Where(m => m.Fecha.Date == hoy)
                .CountAsync();

            // ============ ALERTAS ============
            // Dispositivos sin ubicación (sin movimientos)
            var dispositivosConUbicacion = await _context.Movimientos
                .Where(m => m.IdDispositivo != null)
                .Select(m => m.IdDispositivo)
                .Distinct()
                .CountAsync();

            viewModel.DispositivosSinUbicacion = viewModel.TotalDispositivos - dispositivosConUbicacion;

            // Componentes con stock bajo
            viewModel.ComponentesStockBajo = await _context.Componentes
                .Where(c => c.EstadoRegistro && c.Cantidad <= c.StockMinimo)
                .CountAsync();

            // Dispositivos con stock bajo
            viewModel.DispositivosStockBajo = await _context.Dispositivos
                .Where(d => d.EstadoRegistro && d.StockActual <= d.StockMinimo)
                .CountAsync();

            // ===================== Gráficos =====================
            // Dispositivos por ubicación
            var dispositivosPorUbicacion = await _context.Movimientos
                .Include(m => m.IdUbicacionNavigation)
                .Where(m => m.IdDispositivo != null && m.IdUbicacionNavigation != null)
                .GroupBy(m => m.IdUbicacionNavigation.Nombre)
                .Select(g => new ChartData
                {
                    Label = g.Key ?? "Sin ubicación",
                    Cantidad = g.Select(x => x.IdDispositivo).Distinct().Count()
                })
                .ToListAsync();

            viewModel.DispositivosPorUbicacion = dispositivosPorUbicacion;

            // Dispositivos por tipo
            var dispositivosPorTipo = await _context.Dispositivos
                .Include(d => d.IdTipoNavigation)
                .Where(d => d.EstadoRegistro)
                .GroupBy(d => d.IdTipoNavigation.Descripcion)
                .Select(g => new ChartData
                {
                    Label = g.Key ?? "Sin tipo",
                    Cantidad = g.Count()
                })
                .ToListAsync();

            viewModel.DispositivosPorTipo = dispositivosPorTipo;

            // ============ ACTIVIDAD RECIENTE ============
            var movimientosRecientes = await _context.Movimientos
                .Include(m => m.IdDispositivoNavigation)
                .Include(m => m.IdComponenteNavigation)
                .Include(m => m.IdUbicacionNavigation)
                .OrderByDescending(m => m.Fecha)
                .Take(5)
                .ToListAsync();

            foreach (var mov in movimientosRecientes)
            {
                var descripcion = "";
                var icono = "bi-arrow-left-right";

                if (mov.IdDispositivoNavigation != null)
                {
                    descripcion = $"{mov.IdDispositivoNavigation.Nombre} movido a {mov.IdUbicacionNavigation?.Nombre ?? "ubicación desconocida"}";
                    icono = "bi-laptop";
                }
                else if (mov.IdComponenteNavigation != null)
                {
                    descripcion = $"{mov.IdComponenteNavigation.Nombre} - {mov.TipoMovimiento}";
                    icono = "bi-gear";
                }

                viewModel.ActividadesRecientes.Add(new ActividadReciente
                {
                    Descripcion = descripcion,
                    Fecha = mov.Fecha,
                    Tipo = "Movimiento",
                    Icono = icono
                });
            }

            // Dispositivos creados recientemente
            var dispositivosRecientes = await _context.Dispositivos
                .Where(d => d.EstadoRegistro && d.FechaAlta.HasValue)
                .OrderByDescending(d => d.FechaAlta)
                .Take(3)
                .ToListAsync();

            foreach (var disp in dispositivosRecientes)
            {
                viewModel.ActividadesRecientes.Add(new ActividadReciente
                {
                    Descripcion = $"Nuevo dispositivo agregado: {disp.Nombre}",
                    Fecha = disp.FechaAlta ?? DateTime.Now,
                    Tipo = "Dispositivo",
                    Icono = "bi-plus-circle"
                });
            }

            // Ordenar por fecha descendente
            viewModel.ActividadesRecientes = viewModel.ActividadesRecientes
                .OrderByDescending(a => a.Fecha)
                .Take(5)
                .ToList();

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}