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
        public string? Estado { get; set; }

        // Campo dinámico: solo visible si es consumible
        [Display(Name = "Cantidad Inicial")]
        public int? CantidadInicial { get; set; }

        // Para mostrar info del tipo (opcional)
        public string TipoDescripcion { get; set; } = string.Empty;
    }
}