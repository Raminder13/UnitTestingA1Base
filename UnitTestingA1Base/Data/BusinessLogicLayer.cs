using UnitTestingA1Base.Models;

namespace UnitTestingA1Base.Data
{
    public class BusinessLogicLayer
    {
        private AppStorage _appStorage;

        public BusinessLogicLayer(AppStorage appStorage) {
            _appStorage = appStorage;
        }
        public HashSet<Recipe> GetRecipesByIngredient(int? id, string? name)
        {
            Ingredient ingredient;
            HashSet<Recipe> recipes = new HashSet<Recipe>();

            if (id != null)
            {
                ingredient = _appStorage.Ingredients.First(i => i.Id == id);

                HashSet<RecipeIngredient> recipeIngredients = _appStorage.RecipeIngredients.Where(rI => rI.IngredientId == ingredient.Id).ToHashSet();

                recipes = _appStorage.Recipes.Where(r => recipeIngredients.Any(ri => ri.RecipeId == r.Id)).ToHashSet();
            }

            return recipes;
        }

        public HashSet<Recipe> GetRecipesByDietaryRestriction(string name, int id)
        {
            // Find the dietary restriction based on either name or ID
            DietaryRestriction dietaryRestriction = null;
            if (!string.IsNullOrEmpty(name))
            {
                dietaryRestriction = _appStorage.DietaryRestrictions.FirstOrDefault(dr => dr.Name == name);
            }
            else if (id > 0)
            {
                dietaryRestriction = _appStorage.DietaryRestrictions.FirstOrDefault(dr => dr.Id == id);
            }

            if (dietaryRestriction == null)
            {
                return new HashSet<Recipe>();
            }

            // Find all recipes that contain ingredients satisfying the dietary restriction
            var recipesWithDietaryRestriction = _appStorage.RecipeIngredients
                .Where(ri => _appStorage.IngredientRestrictions
                    .Any(ir => ir.DietaryRestrictionId == dietaryRestriction.Id &&
                               ir.IngredientId == ri.IngredientId))
                .Select(ri => ri.Recipe)
                .ToHashSet();

            return recipesWithDietaryRestriction;
        }

        public HashSet<Recipe> GetRecipesByNameOrId(string name, int id)
        {
            // If an ID is provided, search for the recipe by ID
            if (id > 0)
            {
                var recipeById = _appStorage.Recipes.FirstOrDefault(recipe => recipe.Id == id);
                if (recipeById != null)
                {
                    var result = new HashSet<Recipe> { recipeById };
                    return result;
                }
            }

            // If a name is provided, search for recipes by name (case-insensitive)
            if (!string.IsNullOrEmpty(name))
            {
                var recipesByName = _appStorage.Recipes
                    .Where(recipe => recipe.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                    .ToHashSet();

                return recipesByName;
            }

            // If neither ID nor name is provided or no matching recipe is found, return an empty HashSet
            return new HashSet<Recipe>();
        }

        public class RecipeInput
        {
            public Recipe Recipe { get; set; }
            public List<Ingredient> Ingredients { get; set; }
        }

        public void AddRecipeAndIngredients(RecipeInput input)
        {
            // Check if a Recipe with the same name already exists
            if (_appStorage.Recipes.Any(r => r.Name.Equals(input.Recipe.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("A recipe with the same name already exists.");
            }

            // Process and add new Ingredients
            foreach (var ingredient in input.Ingredients)
            {
                // Check if an Ingredient with the same name already exists
                var existingIngredient = _appStorage.Ingredients.FirstOrDefault(i => i.Name.Equals(ingredient.Name, StringComparison.OrdinalIgnoreCase));
                if (existingIngredient == null)
                {
                    // If the Ingredient is not found, add it to the database
                    ingredient.Id = _appStorage.GeneratePrimaryKey();
                    _appStorage.Ingredients.Add(ingredient);
                }
                else
                {
                    // Use the existing Ingredient's ID
                    ingredient.Id = existingIngredient.Id;
                }
            }

            // Generate a new ID for the Recipe
            input.Recipe.Id = _appStorage.GeneratePrimaryKey();

            // Add the new Recipe to the database
            _appStorage.Recipes.Add(input.Recipe);

            // Create RecipeIngredient objects and add them to the database
            foreach (var ingredient in input.Ingredients)
            {
                _appStorage.RecipeIngredients.Add(new RecipeIngredient
                {
                    IngredientId = ingredient.Id,
                    RecipeId = input.Recipe.Id,
                    Amount = 1.0, // You can set the amount as needed
                    MeasurementUnit = MeasurementUnit.Grams // You can set the measurement unit as needed
                });
            }
        }

        public bool DoesRecipeExist(string recipeName)
        {
            // Check if a recipe with the provided name already exists.
            return _appStorage.Recipes.Any(recipe => recipe.Name.Equals(recipeName, StringComparison.OrdinalIgnoreCase));
        }

        public bool DeleteIngredient(int id, string name)
        {
            // Find the ingredient by ID or name
            Ingredient ingredientToDelete = _appStorage.Ingredients.FirstOrDefault(i => i.Id == id || i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (ingredientToDelete == null)
            {
                // Ingredient not found
                return false;
            }

            // Check how many recipes use this ingredient
            int recipeCount = _appStorage.RecipeIngredients.Count(ri => ri.IngredientId == ingredientToDelete.Id);

            if (recipeCount > 1)
            {
                // Ingredient is used in multiple recipes and cannot be deleted
                return false;
            }

            // Delete the ingredient
            _appStorage.Ingredients.Remove(ingredientToDelete);

            // If there's only one recipe using the ingredient, delete the recipe and associated RecipeIngredients
            if (recipeCount == 1)
            {
                Recipe recipeUsingIngredient = _appStorage.Recipes.FirstOrDefault(r => _appStorage.RecipeIngredients.Any(ri => ri.RecipeId == r.Id && ri.IngredientId == ingredientToDelete.Id));

                if (recipeUsingIngredient != null)
                {
                    // Delete associated RecipeIngredients
                    var recipeIngredientsToDelete = _appStorage.RecipeIngredients.Where(ri => ri.RecipeId == recipeUsingIngredient.Id).ToList();
                    foreach (var recipeIngredient in recipeIngredientsToDelete)
                    {
                        _appStorage.RecipeIngredients.Remove(recipeIngredient);
                    }

                    // Delete the recipe
                    _appStorage.Recipes.Remove(recipeUsingIngredient);
                }
            }

            return true; // Ingredient deleted successfully
        }

        public bool DeleteRecipe(int id, string name)
        {
            // Find the recipe by ID or name
            Recipe recipeToDelete = null;

            if (id > 0)
            {
                recipeToDelete = _appStorage.Recipes.FirstOrDefault(recipe => recipe.Id == id);
            }
            else if (!string.IsNullOrEmpty(name))
            {
                recipeToDelete = _appStorage.Recipes.FirstOrDefault(recipe => recipe.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }

            if (recipeToDelete == null)
            {
                return false; // Recipe not found
            }

            // Delete associated RecipeIngredient entries
            var recipeIngredientsToDelete = _appStorage.RecipeIngredients.Where(ri => ri.RecipeId == recipeToDelete.Id).ToList();
            foreach (var recipeIngredient in recipeIngredientsToDelete)
            {
                _appStorage.RecipeIngredients.Remove(recipeIngredient);
            }

            // Delete the recipe
            _appStorage.Recipes.Remove(recipeToDelete);

            return true; // Recipe deleted successfully
        }


    }
}
