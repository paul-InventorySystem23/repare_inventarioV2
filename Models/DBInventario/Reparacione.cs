using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class Reparacione
{
    public int IdReparacion { get; set; }

    public int IdDispositivo { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFinalizacion { get; set; }

    public string TipoReparacion { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public int? IdResponsable { get; set; }

    public string? Observaciones { get; set; }

    public virtual Dispositivo IdDispositivoNavigation { get; set; } = null!;

    public virtual Responsable? IdResponsableNavigation { get; set; }

    public virtual ICollection<ReparacionConsumible> ReparacionConsumibles { get; set; } = new List<ReparacionConsumible>();
}
