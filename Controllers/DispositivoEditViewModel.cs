// ViewModels/DispositivoEditViewModel.cs
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
        public int? IdMarca { get; set; } // ✅ Ahora es nullable

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public int? IdTipo { get; set; } // ✅ Ahora es nullable

        public string? CodigoInventario { get; set; }
        public string? NroSerie { get; set; }
        public string? Estado { get; set; }
        public DateOnly? FechaAlta { get; set; }
        public DateOnly? FechaBaja { get; set; }

        // Campo opcional solo para consumibles
        public int? CantidadInicial { get; set; }

        public int? StockActual { get; set; } // ✅ Ahora es nullable
        public int? StockMinimo { get; set; } // ✅ Ahora es nullable
        
    }
}