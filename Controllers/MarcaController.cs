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
    }
}