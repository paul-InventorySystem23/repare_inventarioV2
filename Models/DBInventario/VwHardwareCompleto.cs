using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBInventario;

public partial class VwHardwareCompleto
{
    public int IdHardware { get; set; }

    public string CodigoInventario { get; set; } = null!;

    public string? NroSerie { get; set; }

    public string? DescripcionHardware { get; set; }

    public string Estado { get; set; } = null!;

    public DateOnly FechaAlta { get; set; }

    public DateOnly? FechaBaja { get; set; }

    public string NombreModelo { get; set; } = null!;

    public string TipoHardware { get; set; } = null!;

    public string Marca { get; set; } = null!;
}
