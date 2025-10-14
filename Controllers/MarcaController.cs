using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using inventario_coprotab.Models.DBInventario;

namespace inventario_coprotab.Controllers
{
    public class MarcaController : Controller
    {
        private readonly SistemaInventarioContext _context;

        public MarcaController(SistemaInventarioContext context)
        {
            _context = context;
        }

        // GET: Marca/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Marca/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Marca marca)
        {
            if (ModelState.IsValid)
            {
                _context.Add(marca);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create)); // O a Index si prefieres
            }
            return View(marca);
        }
        // GET: Marca/GetMarcas
        [HttpGet]
        public async Task<IActionResult> GetMarcas()
        {
            var marcas = await _context.Marcas
                .Select(m => new { id = m.IdMarca, nombre = m.Nombre })
                .ToListAsync();
            return Json(marcas);
        }
        // POST: Marca/Create (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax([FromBody] Marca marca)
        {
            if (ModelState.IsValid)
            {
                _context.Marcas.Add(marca);
                await _context.SaveChangesAsync();
                return Json(new { success = true, id = marca.IdMarca, nombre = marca.Nombre });
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, errors = errors });
        }

    }

}
