using System;
using System.Collections.Generic;

namespace inventario_coprotab.Models.DBSeguridadCoprotab;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public DateTime LastActivityDate { get; set; }

    public string? Image { get; set; }

    public byte[] Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool Active { get; set; }

    public virtual ICollection<UsersInApplication> UsersInApplications { get; set; } = new List<UsersInApplication>();

    public virtual ICollection<UsersInRole> UsersInRoles { get; set; } = new List<UsersInRole>();
}
