using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class ReparacionDetalle
{
    public int IdReparacionDetalle { get; set; }

    public int IdReparacion { get; set; }

    public int? IdDispositivo { get; set; }

    public int? IdComponente { get; set; }

    public string TipoElemento { get; set; } = null!;

    public string? Accion { get; set; }

    public int? Cantidad { get; set; }

    public string? Observaciones { get; set; }

    public virtual Componente? IdComponenteNavigation { get; set; }

    public virtual Dispositivo? IdDispositivoNavigation { get; set; }

    public virtual Reparacione IdReparacionNavigation { get; set; } = null!;
}
