using System;
using System.Collections.Generic;

namespace inventario_coprotab.ViewModels
{
    public class DashboardViewModel
    {
        // Datos de usuario
        public string NombreUsuario { get; set; }
        public string EmailUsuario { get; set; }
        public DateTime FechaActual { get; set; }

        // Estadísticas generales
        public int TotalDispositivos { get; set; }
        public int TotalComponentes { get; set; }
        public int TotalUbicaciones { get; set; }
        public int MovimientosHoy { get; set; }

        // Alertas
        public int DispositivosSinUbicacion { get; set; }
        public int ComponentesStockBajo { get; set; }
        public int DispositivosStockBajo { get; set; }

        // Gráficos
        public List<ChartData> DispositivosPorUbicacion { get; set; } = new();
        public List<ChartData> DispositivosPorTipo { get; set; } = new();

        // Actividad reciente
        public List<ActividadReciente> ActividadesRecientes { get; set; } = new();
    }


    public class ActividadReciente
    {
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
    }
}
