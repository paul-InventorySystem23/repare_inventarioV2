using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class RelacionDispositivoComponente
{
    public int IdRelacion { get; set; }

    public int IdDispositivo { get; set; }

    public int IdComponente { get; set; }

    public string? Observaciones { get; set; }

    public int? IdResponsable { get; set; }

    public virtual Componente IdComponenteNavigation { get; set; } = null!;

    public virtual Dispositivo IdDispositivoNavigation { get; set; } = null!;

    public virtual Responsable? IdResponsableNavigation { get; set; }
}
