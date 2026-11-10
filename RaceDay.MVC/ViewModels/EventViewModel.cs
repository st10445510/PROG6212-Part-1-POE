
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.ViewModels
{
    public class EventViewModel
    {
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Event Name")]
        public string EventName { get; set; } = "";

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        public string Location { get; set; } = "";

        public string? Description { get; set; }

        [Range(0.1, 1000)]
        public decimal? Distance { get; set; }

        [Display(Name = "Event Type")]
        public string? EventType { get; set; }
    }
}
