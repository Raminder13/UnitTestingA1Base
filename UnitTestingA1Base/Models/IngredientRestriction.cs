using System.ComponentModel.DataAnnotations.Schema;

namespace UnitTestingA1Base.Models
{
    public class IngredientRestriction
    {
        public int Id { get; set; }

        [ForeignKey("DietaryRestriction")]
        public int DietaryRestrictionId { get; set; }

        [ForeignKey("Ingredient")]
        public int IngredientId { get; set; }

        public DietaryRestriction DietaryRestriction { get; set; }
        public Ingredient Ingredient { get; set; }
    }
}
