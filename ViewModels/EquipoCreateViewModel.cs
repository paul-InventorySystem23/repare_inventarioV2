// ViewModels/EquipoCreateViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace inventario_coprotab.ViewModels
{
    public class EquipoCreateViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        public int IdMarca { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public int IdTipo { get; set; } // Debe ser un tipo "Hardware"

        public string? NroSerie { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        public string Estado { get; set; } = "Nuevo";

        // Lista de componentes disponibles para seleccionar
        public List<int> ComponentesSeleccionados { get; set; } = new();

        // Para cargar en el formulario
        public List<SelectListItem> Marcas { get; set; } = new();
        public List<SelectListItem> TiposHardware { get; set; } = new();
        public List<ComponenteCheckboxItem> ComponentesDisponibles { get; set; } = new();
    }

    public class ComponenteCheckboxItem
    {
        public int IdComponente { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public bool Seleccionado { get; set; }
    }
}