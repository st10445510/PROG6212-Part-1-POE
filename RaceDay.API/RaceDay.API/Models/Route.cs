using System;
using System.Collections.Generic;

namespace RaceDay.API.Models;

public partial class EventRoute
{
    public int RouteId { get; set; }

    public int EventId { get; set; }

    public string RouteName { get; set; } = null!;

    public decimal Distance { get; set; }

    public string? Description { get; set; }

    public string? MapUrl { get; set; }

    public virtual Event Event { get; set; } = null!;
}
