using System;
using System.Collections.Generic;

namespace Jadin.Models;

public partial class User //Selected from database
{
    public int Userid { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
