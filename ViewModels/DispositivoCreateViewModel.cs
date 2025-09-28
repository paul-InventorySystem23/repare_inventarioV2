using System.ComponentModel.DataAnnotations;

namespace inventario_coprotab.ViewModels
{
    public class DispositivoCreateViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string? Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        [Display(Name = "Marca")]
        public int IdMarca { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        [Display(Name = "Tipo")]
        public int IdTipo { get; set; }

        [Display(Name = "Código de Inventario")]
        public string? CodigoInventario { get; set; }

        [Display(Name = "Número de Serie")]
        public string? NroSerie { get; set; }

        [Display(Name = "Estado")]
        public string? Estado { get; set; }

        // Campo dinámico: solo visible si es consumible
        [Display(Name = "Cantidad Inicial")]
        public int? CantidadInicial { get; set; }

        // Para mostrar info del tipo
        public string TipoDescripcion { get; set; } = string.Empty;
    }
}