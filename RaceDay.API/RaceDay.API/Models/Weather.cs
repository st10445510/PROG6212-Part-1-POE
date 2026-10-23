using System;
using System.Collections.Generic;

namespace RaceDay.API.Models;

public partial class Weather
{
    public int WeatherId { get; set; }

    public int EventId { get; set; }

    public DateOnly WeatherDate { get; set; }

    public decimal? Temperature { get; set; }

    public string Condition { get; set; } = null!;

    public decimal? WindSpeed { get; set; }

    public virtual Event Event { get; set; } = null!;
}
