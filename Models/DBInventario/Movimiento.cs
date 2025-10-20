using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class Movimiento
{
    public int IdMovimiento { get; set; }

    // ✅ Ahora es nullable - puede ser dispositivo O componente
    public int? IdDispositivo { get; set; }

    // ✅ NUEVO: Soporte para movimientos de componentes
    public int? IdComponente { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public int? IdUbicacion { get; set; }

    public int? IdResponsable { get; set; }

    public int Cantidad { get; set; }

    public string? Observaciones { get; set; }

    // Navegación
    public virtual Dispositivo? IdDispositivoNavigation { get; set; }

    // ✅ NUEVO: Navegación a componente
    public virtual Componente? IdComponenteNavigation { get; set; }

    public virtual Responsable? IdResponsableNavigation { get; set; }

    public virtual Ubicacione? IdUbicacionNavigation { get; set; }
}