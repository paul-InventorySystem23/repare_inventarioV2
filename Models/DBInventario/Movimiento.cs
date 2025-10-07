using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class Movimiento
{
    public int IdMovimiento { get; set; }               
    public int IdDispositivo { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    // ✅ Cambiado de DateOnly a DateTime
    public DateTime Fecha { get; set; }

    public int? IdUbicacion { get; set; }

    public int? IdResponsable { get; set; }

    public int Cantidad { get; set; }

    public string? Observaciones { get; set; }

    public virtual Dispositivo oDispositivo { get; set; } = null!;

    public virtual Responsable? oResponsable { get; set; }

    public virtual Ubicacione? oUbicaion { get; set; }

 
}
