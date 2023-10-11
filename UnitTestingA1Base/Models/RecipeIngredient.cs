using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UnitTestingA1Base.Models
{
    public class RecipeIngredient
    {
        public int Id { get; set; }

        [ForeignKey("Ingredient")]
        public int IngredientId { get; set; }

        [ForeignKey("Recipe")]
        public int RecipeId { get; set; }

        [Range(0.1, double.MaxValue)]
        public double Amount { get; set; }

        public MeasurementUnit MeasurementUnit { get; set; }

        public Ingredient Ingredient { get; set; }
        public Recipe Recipe { get; set; }
    }

    public enum MeasurementUnit
    {
        Grams,
        Milliletres
    }
}
