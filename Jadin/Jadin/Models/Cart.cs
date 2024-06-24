using System;
using System.Collections.Generic;

namespace Jadin.Models;

public partial class Cart //Selected from database
{
    public int CartId { get; set; }

    public int? ProductId { get; set; }

    public int? Userid { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
