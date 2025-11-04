using System.Collections.Generic;

namespace inventario_coprotab.ViewModels
{
    public class EquipoEditViewModel
    {
        public int IdRelacion { get; set; }
        public int IdDispositivo { get; set; }
        public string DispositivoNombre { get; set; } = null!;
        public List<int> ComponentesSeleccionados { get; set; } = new();
        public List<ComponenteCheckboxItem> ComponentesDisponibles { get; set; } = new();
    }
}