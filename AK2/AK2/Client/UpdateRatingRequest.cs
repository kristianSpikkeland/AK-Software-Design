using System.ComponentModel.DataAnnotations;

namespace AK2.Client
{
    public class UpdateRatingRequest
    {
        [Range(1, 6, ErrorMessage = "Terningkast må være mellom 1 og 6")]
        public int Rating { get; set; }
    }
}
