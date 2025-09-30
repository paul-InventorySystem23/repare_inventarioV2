// ViewModels/DispositivoCreateViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace inventario_coprotab.ViewModels
{
    public class DispositivoCreateViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        public int IdMarca { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public int IdTipo { get; set; }

        public string? CodigoInventario { get; set; }
        public string? NroSerie { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public string? Estado { get; set; } // ✅ Ahora es opcional (string?)

        // Campo dinámico: solo visible si es consumible
        [Display(Name = "Cantidad Inicial")]
        public int? CantidadInicial { get; set; }

        // ✅ Nueva propiedad para los estados disponibles
        public List<string> EstadosDisponibles { get; set; } = new List<string> { "Nuevo", "En uso", "Obsoleto" };
    }
}