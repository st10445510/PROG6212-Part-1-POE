
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.ViewModels
{
    public class ResultViewModel
    {
        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        public TimeOnly FinishTime { get; set; }

        public int? FinishingPosition { get; set; }
    }
}
