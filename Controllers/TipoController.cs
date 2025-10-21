using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using inventario_coprotab.Models.DBInventario;

namespace inventario_coprotab.Controllers
{
    public class TipoController : Controller
    {
        private readonly SistemaInventarioContext _context;

        public TipoController(SistemaInventarioContext context)
        {
            _context = context;
        }

        // POST: /Tipo/CreateAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAjax([FromBody] TipoRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Nombre))
            {
                return Json(new { success = false, errors = new[] { "El nombre del tipo es obligatorio." } });
            }

            // Verificar si ya existe un tipo con el mismo nombre (ignorando mayúsculas/minúsculas)
            if (_context.TipoHardwares.Any(t => t.Descripcion.ToLower() == model.Nombre.ToLower()))
            {
                return Json(new { success = false, errors = new[] { "Ya existe un tipo con ese nombre." } });
            }

            var nuevoTipo = new TipoHardware
            {
                Descripcion = model.Nombre
            };

            _context.TipoHardwares.Add(nuevoTipo);
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                id = nuevoTipo.IdTipo,
                nombre = nuevoTipo.Descripcion
            });
        }
    }

    // Clase auxiliar para recibir el JSON del fetch
    public class TipoRequest
    {
        public string Nombre { get; set; }
    }
}