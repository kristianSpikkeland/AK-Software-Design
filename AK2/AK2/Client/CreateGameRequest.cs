using System.ComponentModel.DataAnnotations;

namespace AK2.Client
{
    public class CreateGameRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Kategori må fylles ut")]
        public string Category { get; set; }


        [Required(ErrorMessage = "Navn må fylles ut")]
        public string Name { get; set; }

        [Range(1, 4, ErrorMessage ="Ugyldig minimum antall spillere")]
        public int MinPlayers { get; set; }

        [Range(2, 20, ErrorMessage ="Ugyldig maksimum antall spillere")]
        public int MaxPlayers { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MaxPlayers < MinPlayers)
            {
                yield return new ValidationResult(
                    "Maksimum spillere kan ikke være lavere enn minimum",
                    new[] { nameof(MaxPlayers) });
            }
        }
    }
}
