using System.Collections.Generic;

namespace inventario_coprotab.ViewModels
{
    public class EquipoEditViewModel
    {
        public int IdRelacion { get; set; }
        public int IdDispositivo { get; set; }
        public string DispositivoNombre { get; set; } = null!;
        public List<int> ComponentesSeleccionados { get; set; } = new();

        // ✅ Nueva propiedad para los componentes actuales del equipo
        public List<ComponenteEquipoItem> ComponentesActuales { get; set; } = new();

        // Para el buscador
        public List<ComponenteCheckboxItem> ComponentesDisponibles { get; set; } = new();
    }

    // ✅ Nueva clase para mostrar componentes en la tabla
    public class ComponenteEquipoItem
    {
        public int IdComponente { get; set; }
        public string Nombre { get; set; } = null!;
        public string? NroSerie { get; set; }
        public string? Tipo { get; set; }
        public string? Marca { get; set; }
    }
}