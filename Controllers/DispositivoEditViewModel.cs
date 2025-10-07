// DispositivoEditViewModel.cs
// ====================================
using System.ComponentModel.DataAnnotations;

namespace inventario_coprotab.ViewModels
{
    public class DispositivoEditViewModel
    {
        public int IdDispositivo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string? Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        public int? IdMarca { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public int? IdTipo { get; set; }

        // ✅ Solo lectura en edición
        public string? CodigoInventario { get; set; }

        public string? NroSerie { get; set; }
        public string? Estado { get; set; }

        // ✅ Cambiado de DateOnly? a DateTime?
        public DateTime? FechaAlta { get; set; }
        public DateTime? FechaBaja { get; set; }

        public int? CantidadInicial { get; set; }
        public int? StockActual { get; set; }
        public int? StockMinimo { get; set; }
    }
}