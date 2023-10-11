using UnitTestingA1Base.Models;

public interface IAppStorage
{
    IEnumerable<Recipe> Recipes { get; }
    IEnumerable<Ingredient> Ingredients { get; }
    // Define other properties/methods as needed
}
