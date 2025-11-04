using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using inventario_coprotab.Models.DBInventario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Relacion = inventario_coprotab.Models.DBInventario.Relacion;

namespace inventario_coprotab.Controllers
{
    public class EquiposController : Controller
    {
        private readonly SistemaInventarioContext _context;

        public EquiposController(SistemaInventarioContext context)
        {
            _context = context;
        }

        // GET: Equipos
        public async Task<IActionResult> Index()
        {
            var equipos = await _context.Relacions
                .Include(r => r.RelacionDetalles)
                    .ThenInclude(rd => rd.IdDispositivoNavigation)
                        .ThenInclude(d => d.IdTipoNavigation)  
                .Include(r => r.RelacionDetalles)
                    .ThenInclude(rd => rd.IdComponenteNavigation)
                .OrderByDescending(r => r.IdRelacion)
                .ToListAsync();

            return View(equipos);
        }

        // GET: Detalle del equipo
        [HttpGet]
        public async Task<IActionResult> DetalleEquipo(int id)
        {
            var equipo = await _context.Relacions
                .Include(r => r.RelacionDetalles)
                    .ThenInclude(rd => rd.IdDispositivoNavigation)
                        .ThenInclude(d => d.IdTipoNavigation)
                .Include(r => r.RelacionDetalles)
                    .ThenInclude(rd => rd.IdComponenteNavigation)
                        .ThenInclude(c => c.IdTipoNavigation)
                .FirstOrDefaultAsync(r => r.IdRelacion == id);

            if (equipo == null)
            {
                return Json(new { success = false, message = "Equipo no encontrado" });
            }

            var dispositivo = equipo.RelacionDetalles.FirstOrDefault()?.IdDispositivoNavigation;
            var componentes = equipo.RelacionDetalles
                .Select(rd => new
                {
                    id = rd.IdComponente,
                    nombre = rd.IdComponenteNavigation.Nombre,
                    nroSerie = rd.IdComponenteNavigation.NroSerie,
                    tipo = rd.IdComponenteNavigation.IdTipoNavigation?.Descripcion ?? "Sin tipo",
                    estado = rd.IdComponenteNavigation.Estado
                })
                .ToList();

            var resultado = new
            {
                success = true,
                idRelacion = equipo.IdRelacion,
                fecha = equipo.Fecha.ToString("dd/MM/yyyy"),
                dispositivo = new
                {
                    id = dispositivo?.IdDispositivo,
                    nombre = dispositivo?.Nombre,
                    nroSerie = dispositivo?.NroSerie,
                    tipo = dispositivo?.IdTipoNavigation?.Descripcion ?? "Sin tipo",
                    estado = dispositivo?.Estado
                },
                componentes = componentes
            };

            return Json(resultado);
        }

        // GET: Obtener dispositivos para el dropdown
        [HttpGet]
        public async Task<IActionResult> ObtenerDispositivos()
        {
            var dispositivos = await _context.Dispositivos
                .Include(d => d.IdTipoNavigation)
                .Where(d => d.EstadoRegistro == true)
                .Select(d => new
                {
                    id = d.IdDispositivo,
                    nombre = d.Nombre + " - " + d.NroSerie
                })
                .ToListAsync();

            return Json(dispositivos);
        }

        // GET: Buscar componentes
        [HttpGet]
        public async Task<IActionResult> BuscarComponentes(string termino)
        {
            if (string.IsNullOrEmpty(termino))
            {
                return Json(new List<object>());
            }

            var componentes = await _context.Componentes
                .Include(c => c.IdTipoNavigation)
                .Where(c => c.EstadoRegistro == true &&
                           (c.Nombre.Contains(termino) ||
                            c.NroSerie.Contains(termino)))
                .Select(c => new
                {
                    id = c.IdComponente,
                    nombre = c.Nombre,
                    nroSerie = c.NroSerie,
                    tipo = c.IdTipoNavigation.Descripcion
                })
                .Take(10)
                .ToListAsync();

            return Json(componentes);
        }

        // POST: Crear equipo
        [HttpPost]
        public async Task<IActionResult> CrearEquipo([FromBody] CrearEquipoViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Datos inválidos" });
            }

            if (modelo.Componentes == null || !modelo.Componentes.Any())
            {
                return Json(new { success = false, message = "Debe agregar al menos un componente" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Crear la relación
                var relacion = new inventario_coprotab.Models.DBInventario.Relacion
                {
                    Fecha = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.Relacions.Add(relacion);
                await _context.SaveChangesAsync();

                // Crear los detalles de la relación
                foreach (var componenteId in modelo.Componentes)
                {
                    var detalle = new inventario_coprotab.Models.DBInventario.RelacionDetalle
                    {
                        IdRelacion = relacion.IdRelacion,
                        IdDispositivo = modelo.IdDispositivo,
                        IdComponente = componenteId
                    };

                    _context.RelacionDetalles.Add(detalle);
                }

                await _context.SaveChangesAsync();

                // Actualizar estado del dispositivo
                var dispositivo = await _context.Dispositivos.FindAsync(modelo.IdDispositivo);
                if (dispositivo != null)
                {
                    dispositivo.EstadoRegistro = false;
                    await _context.SaveChangesAsync();
                }

                // Actualizar estado de los componentes
                var componentes = await _context.Componentes
                    .Where(c => modelo.Componentes.Contains(c.IdComponente))
                    .ToListAsync();

                foreach (var componente in componentes)
                {
                    componente.EstadoRegistro = false;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Equipo creado exitosamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                return Json(new
                {
                    success = false,
                    message = "Error al crear el equipo: " + ex.Message,
                    innerError = innerMessage
                });
            }
        }
    }

    // Modelo de vista para crear equipo
    public class CrearEquipoViewModel
    {
        public int IdDispositivo { get; set; }
        public List<int> Componentes { get; set; }
    }
}