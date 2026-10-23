
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class CreateResultRequest
    {
        [Range(1, int.MaxValue)]
        public int EnrolmentId { get; set; }

        public TimeOnly? FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        public int? FinishingPosition { get; set; }
    }
}
