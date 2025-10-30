using System.Collections.Generic;
using inventario_coprotab.Models.DBInventario;

namespace inventario_coprotab.ViewModels
{
    public class UbicacionViewModel
    {
        public List<Movimiento> Movimientos { get; set; } = new();
        public List<ChartData> DispositivosPorUbicacion { get; set; } = new();
        public List<ChartData> DispositivosPorTipo { get; set; } = new();
    }

  

}
