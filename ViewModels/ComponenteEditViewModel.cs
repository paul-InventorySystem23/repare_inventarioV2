// ViewModels/ComponenteEditViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace inventario_coprotab.ViewModels
{
    public class ComponenteEditViewModel
    {
        public int IdComponente { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        public int? IdMarca { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public int? IdTipo { get; set; }

        public string? NroSerie { get; set; }

        public string? Estado { get; set; }

        [Display(Name = "Fecha de Instalación")]
        public DateTime? FechaInstalacion { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }
    }
}