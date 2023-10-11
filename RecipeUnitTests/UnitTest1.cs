using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestingA1Base.Data;
using UnitTestingA1Base.Models;
using System.Collections.Generic;

namespace RecipeUnitTests { 
[TestClass]
public class UnitTest1
{
        private BusinessLogicLayer _bll;
        private AppStorage _appStorage;

        [TestInitialize]
        public void Initialize()
        {
            _appStorage = new AppStorage();
            _bll = new BusinessLogicLayer(_appStorage);
        }

        [TestMethod]
        public void GetRecipesByIngredient_ValidIngredientId_ReturnsRecipes()
        {
            // Arrange
            int validIngredientId = 1;

            // Act
            var recipes = _bll.GetRecipesByIngredient(validIngredientId, null);

            // Assert
            Assert.IsNotNull(recipes);
            Assert.IsTrue(recipes.Count > 0);
        }

        public HashSet<Recipe> GetRecipesByIngredient(int? id, string? name)
        {
            Ingredient ingredient;
            HashSet<Recipe> recipes = new HashSet<Recipe>();

            if (id != null)
            {
                ingredient = _appStorage.Ingredients.FirstOrDefault(i => i.Id == id);
                if (ingredient != null)
                {
                    HashSet<RecipeIngredient> recipeIngredients = _appStorage.RecipeIngredients.Where(rI => rI.IngredientId == ingredient.Id).ToHashSet();
                    recipes = _appStorage.Recipes.Where(r => recipeIngredients.Any(ri => ri.RecipeId == r.Id)).ToHashSet();
                }
            }

            return recipes;
        }


        [TestMethod]
        public void GetRecipesByIngredient_InvalidIngredientName_ReturnsEmptyHashSet()
        {
            // Arrange
            string invalidIngredientName = "Nonexistent Ingredient";

            // Act
            var recipes = _bll.GetRecipesByIngredient(null, invalidIngredientName);

            // Assert
            Assert.IsNotNull(recipes);
            Assert.AreEqual(0, recipes.Count);
        }

        [TestMethod]
        public void GetRecipesByDietaryRestriction_ValidDietaryRestrictionName_ReturnsRecipes()
        {
            // Arrange
            string validDietaryRestrictionName = "Vegetarian";

            // Act
            var recipes = _bll.GetRecipesByDietaryRestriction(validDietaryRestrictionName, 0);

            // Assert
            Assert.IsNotNull(recipes);
            Assert.IsTrue(recipes.Count > 0);
        }

        [TestMethod]
        public void GetRecipesByDietaryRestriction_InvalidDietaryRestrictionName_ReturnsEmptyHashSet()
        {
            // Arrange
            string invalidDietaryRestrictionName = "InvalidDiet";

            // Act
            var recipes = _bll.GetRecipesByDietaryRestriction(invalidDietaryRestrictionName, 0);

            // Assert
            Assert.IsNotNull(recipes);
            Assert.AreEqual(0, recipes.Count);
        }

        [TestMethod]
        public void GetRecipesByNameOrId_ValidRecipeId_ReturnsRecipe()
        {
            // Arrange
            int validRecipeId = 1;

            // Act
            var recipes = _bll.GetRecipesByNameOrId(null, validRecipeId);

            // Assert
            Assert.IsNotNull(recipes);
            Assert.AreEqual(1, recipes.Count);
        }

        [TestMethod]
        public void GetRecipesByNameOrId_InvalidRecipeId_ReturnsEmptyHashSet()
        {
            // Arrange
            int invalidRecipeId = -1;

            // Act
            var recipes = _bll.GetRecipesByNameOrId(null, invalidRecipeId);

            // Assert
            Assert.IsNotNull(recipes);
            Assert.AreEqual(0, recipes.Count);
        }

        [TestMethod]
        public void GetRecipesByNameOrId_ValidRecipeName_ReturnsRecipes()
        {
            // Arrange: Create the test input with a valid recipe name
            string validRecipeName = "Spaghetti Carbonara";

            // Act: Call the method with the test input
            var recipes = _bll.GetRecipesByNameOrId(validRecipeName, 0);

            // Assert: Check if the returned recipes contain the expected recipe
            Assert.IsNotNull(recipes);
            Assert.IsTrue(recipes.Count > 0);
        }


        [TestMethod]
        public void GetRecipesByNameOrId_InvalidRecipeName_ReturnsEmptyHashSet()
        {
            // Arrange
            string invalidRecipeName = "Nonexistent Recipe";

            // Act
            var recipes = _bll.GetRecipesByNameOrId(invalidRecipeName, 0);

            // Assert
            Assert.IsNotNull(recipes);
            Assert.AreEqual(0, recipes.Count);
        }

        [TestMethod]
        public void AddRecipeAndIngredients_ValidData_AddsRecipeAndIngredients()
        {
            // Arrange
            var recipe = new Recipe
            {
                Name = "New Recipe",
                // Add other required properties
            };

            var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Ingredient1" },
            new Ingredient { Name = "Ingredient2" },
        };

            var input = new BusinessLogicLayer.RecipeInput
            {
                Recipe = recipe,
                Ingredients = ingredients,
            };

            // Act
            _bll.AddRecipeAndIngredients(input);

            // Assert
            Assert.IsTrue(_bll.DoesRecipeExist("New Recipe"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddRecipeAndIngredients_DuplicateRecipeName_ThrowsException()
        {
            // Arrange
            var recipe = new Recipe
            {
                Name = "Spaghetti Carbonara",
                // Add other required properties
            };

            var ingredients = new List<Ingredient>
        {
            new Ingredient { Name = "Ingredient1" },
            new Ingredient { Name = "Ingredient2" },
        };

            var input = new BusinessLogicLayer.RecipeInput
            {
                Recipe = recipe,
                Ingredients = ingredients,
            };

            // Act
            _bll.AddRecipeAndIngredients(input);
        }

        [TestMethod]
        public void DoesRecipeExist_ExistingRecipeName_ReturnsTrue()
        {
            // Arrange
            string existingRecipeName = "Chicken Alfredo";

            // Act
            bool exists = _bll.DoesRecipeExist(existingRecipeName);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void DoesRecipeExist_NonExistingRecipeName_ReturnsFalse()
        {
            // Arrange
            string nonExistingRecipeName = "Nonexistent Recipe";

            // Act
            bool exists = _bll.DoesRecipeExist(nonExistingRecipeName);

            // Assert
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void DeleteRecipe_ValidRecipeId_DeletesRecipe()
        {
            // Arrange
            int validRecipeId = 1;

            // Act
            bool deleted = _bll.DeleteRecipe(validRecipeId, null);

            // Assert
            Assert.IsTrue(deleted);
        }

        [TestMethod]
        public void DeleteRecipe_ValidRecipeName_DeletesRecipe()
        {
            // Arrange
            string validRecipeName = "Spaghetti Carbonara"; // Replace with the correct existing recipe name

            // Act
            bool deleted = _bll.DeleteRecipe(0, validRecipeName);

            // Assert
            Assert.IsTrue(deleted);
        }


        [TestMethod]
        public void DeleteRecipe_InvalidRecipeId_ReturnsFalse()
        {
            // Arrange
            int invalidRecipeId = -1;

            // Act
            bool deleted = _bll.DeleteRecipe(invalidRecipeId, null);

            // Assert
            Assert.IsFalse(deleted);
        }

        [TestMethod]
        public void DeleteRecipe_InvalidRecipeName_ReturnsFalse()
        {
            // Arrange
            string invalidRecipeName = "Nonexistent Recipe";

            // Act
            bool deleted = _bll.DeleteRecipe(0, invalidRecipeName);

            // Assert
            Assert.IsFalse(deleted);
        }

    }
}