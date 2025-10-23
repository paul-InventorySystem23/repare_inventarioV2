using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBSeguridadCoprotab;

public partial class Application
{
    public int ApplicationId { get; set; }

    public string ApplicationName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Platform { get; set; } = null!;

    public int Campana { get; set; }

    public string Url { get; set; } = null!;

    public string Image { get; set; } = null!;

    public bool Active { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<UsersInApplication> UsersInApplications { get; set; } = new List<UsersInApplication>();
}
