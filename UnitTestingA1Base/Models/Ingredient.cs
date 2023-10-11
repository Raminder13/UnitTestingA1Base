using System.ComponentModel.DataAnnotations;

namespace UnitTestingA1Base.Models
{
    public class Ingredient
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public ICollection<IngredientRestriction> IngredientRestrictions { get; set; }

        public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
    }
}
