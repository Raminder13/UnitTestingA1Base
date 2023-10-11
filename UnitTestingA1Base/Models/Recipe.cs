using System.ComponentModel.DataAnnotations;

namespace UnitTestingA1Base.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Range(1, int.MaxValue)]
        public int Servings { get; set; }

        public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
    }
}
