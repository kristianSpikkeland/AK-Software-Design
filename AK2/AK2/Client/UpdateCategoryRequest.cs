using System.ComponentModel.DataAnnotations;


namespace AK2.Client
{
    public sealed class UpdateCategoryRequest
    {
        [Required(ErrorMessage = "Kategori må fylles ut")]
        public string Category { get; set; }

    }
}
