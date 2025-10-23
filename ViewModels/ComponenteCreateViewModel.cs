// ViewModels/ComponenteCreateViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace inventario_coprotab.ViewModels
{
    public class ComponenteCreateViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        public int IdMarca { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public int IdTipo { get; set; }

        public string? NroSerie { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public string? Estado { get; set; }

        [Display(Name = "Fecha de Instalación")]
        public DateTime? FechaInstalacion { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int? Cantidad { get; set; }

        [Display(Name = "Stock Mínimo")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        public int? StockMinimo { get; set; }

        public List<string> EstadosDisponibles { get; set; } = new List<string> { "Nuevo", "En uso", "Obsoleto" };
    }
}