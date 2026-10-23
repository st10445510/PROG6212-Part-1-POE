using System;
using System.Collections.Generic;

namespace RaceDay.API.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Role { get; set; } = null!;

    public virtual ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
