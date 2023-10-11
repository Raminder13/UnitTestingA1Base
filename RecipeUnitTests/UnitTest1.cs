using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestingA1Base.Data;
using UnitTestingA1Base.Models;
using System;
using System.Linq;
using Moq;

[TestClass]
public class BusinessLogicLayerTests
{
    private Mock<AppStorage> appStorageMock;
    private BusinessLogicLayer businessLogicLayer;

    [TestInitialize]
    public void TestInitialize()
    {
        appStorageMock = new Mock<AppStorage>();
        businessLogicLayer = new BusinessLogicLayer(appStorageMock.Object);
    }

    [TestMethod]
    public void GetRecipesByIngredient_WithValidIngredientId_ReturnsRecipes()
    {
        // Arrange
        var ingredientId = 1;
        var ingredient = new Ingredient { Id = ingredientId, Name = "TestIngredient" };
        var recipe1 = new Recipe { Id = 1, Name = "Recipe1" };
        var recipe2 = new Recipe { Id = 2, Name = "Recipe2" };
        var recipeIngredients = new HashSet<RecipeIngredient>
    {
        new RecipeIngredient { IngredientId = ingredientId, RecipeId = 1, Amount = 100, MeasurementUnit = MeasurementUnit.Grams },
        new RecipeIngredient { IngredientId = ingredientId, RecipeId = 2, Amount = 150, MeasurementUnit = MeasurementUnit.Grams },
    };
        appStorageMock.Setup(a => a.Ingredients).Returns(new HashSet<Ingredient> { ingredient });
        appStorageMock.Setup(a => a.RecipeIngredients).Returns(recipeIngredients);
        appStorageMock.Setup(a => a.Recipes).Returns(new HashSet<Recipe> { recipe1, recipe2 });

        // Act
        var result = businessLogicLayer.GetRecipesByIngredient(ingredientId, null);

        // Assert
        CollectionAssert.Contains(result.ToArray(), recipe1);
        CollectionAssert.Contains(result.ToArray(), recipe2);
    }


    [TestMethod]
    public void AddRecipeAndIngredients_WithNewIngredients_AddsRecipeAndIngredients()
    {
        // Arrange
        var recipeInput = new BusinessLogicLayer.RecipeInput
        {
            Recipe = new Recipe { Name = "NewRecipe" },
            Ingredients = new List<Ingredient>
                {
                    new Ingredient { Name = "Ingredient1" },
                    new Ingredient { Name = "Ingredient2" }
                }
        };
        appStorageMock.Setup(a => a.GeneratePrimaryKey()).Returns(123);

        // Act
        businessLogicLayer.AddRecipeAndIngredients(recipeInput);

        // Assert
        appStorageMock.Verify(a => a.GeneratePrimaryKey(), Times.Exactly(3));
        appStorageMock.Verify(a => a.Recipes.Add(It.Is<Recipe>(r => r.Name == "NewRecipe")), Times.Once);
        appStorageMock.Verify(a => a.Ingredients.Add(It.Is<Ingredient>(i => i.Name == "Ingredient1")), Times.Once);
        appStorageMock.Verify(a => a.Ingredients.Add(It.Is<Ingredient>(i => i.Name == "Ingredient2")), Times.Once);
        appStorageMock.Verify(a => a.RecipeIngredients.Add(It.IsAny<RecipeIngredient>()), Times.Exactly(2));
    }

    [TestMethod]
    public void GetRecipesByDietaryRestriction_WithValidDietaryRestriction_ReturnsRecipes()
    {
        // Arrange
        var dietaryRestriction = new DietaryRestriction { Id = 1, Name = "TestRestriction" };
        var recipe1 = new Recipe { Id = 1, Name = "Recipe1" };
        var recipe2 = new Recipe { Id = 2, Name = "Recipe2" };
        var ingredientRestrictions = new HashSet<IngredientRestriction>
    {
        new IngredientRestriction { DietaryRestrictionId = 1, IngredientId = 1 },
        new IngredientRestriction { DietaryRestrictionId = 1, IngredientId = 2 },
    };
        appStorageMock.Setup(a => a.DietaryRestrictions).Returns(new HashSet<DietaryRestriction> { dietaryRestriction });
        appStorageMock.Setup(a => a.Recipes).Returns(new HashSet<Recipe> { recipe1, recipe2 });
        appStorageMock.Setup(a => a.IngredientRestrictions).Returns(ingredientRestrictions);

        // Act
        var result = businessLogicLayer.GetRecipesByDietaryRestriction("TestRestriction", 0);

        // Assert
        Assert.IsTrue(result.Any(r => r.Id == 1));
        Assert.IsTrue(result.Any(r => r.Id == 2));
    }



    [TestMethod]
    public void GetRecipesByDietaryRestriction_WithInvalidDietaryRestriction_ReturnsEmptyList()
    {
        // Arrange
        appStorageMock.Setup(a => a.DietaryRestrictions).Returns(new HashSet<DietaryRestriction>());

        // Act
        var result = businessLogicLayer.GetRecipesByDietaryRestriction("InvalidRestriction", 0);

        // Assert
        CollectionAssert.AreEqual(result, new HashSet<Recipe>());
    }

    [TestMethod]
    public void GetRecipesByNameOrId_WithValidRecipeName_ReturnsMatchingRecipes()
    {
        // Arrange
        var recipe1 = new Recipe { Id = 1, Name = "Recipe1" };
        var recipe2 = new Recipe { Id = 2, Name = "Recipe2" };
        appStorageMock.Setup(a => a.Recipes).Returns(new HashSet<Recipe> { recipe1, recipe2 });

        // Act
        var result = businessLogicLayer.GetRecipesByNameOrId("Recipe1", 0);

        // Assert
        Assert.IsTrue(result.Any(r => r.Name == "Recipe1"));
        Assert.IsTrue(result.Any(r => r.Name == "Recipe2"));
    }

    [TestMethod]
    public void GetRecipesByNameOrId_WithValidRecipeId_ReturnsMatchingRecipe()
    {
        // Arrange
        var recipe1 = new Recipe { Id = 1, Name = "Recipe1" };
        var recipe2 = new Recipe { Id = 2, Name = "Recipe2" };
        var recipe3 = new Recipe { Id = 3, Name = "AnotherRecipe" };
        appStorageMock.Setup(a => a.Recipes).Returns(new HashSet<Recipe> { recipe1, recipe2, recipe3 });

        // Act
        var result = businessLogicLayer.GetRecipesByNameOrId("", 2);

        // Assert
        CollectionAssert.Contains(result, recipe2);
    }

    [TestMethod]
    public void GetRecipesByNameOrId_WithInvalidNameAndId_ReturnsEmptyList()
    {
        // Arrange
        var recipe1 = new Recipe { Id = 1, Name = "Recipe1" };
        var recipe2 = new Recipe { Id = 2, Name = "Recipe2" };
        appStorageMock.Setup(a => a.Recipes).Returns(new HashSet<Recipe> { recipe1, recipe2 });

        // Act
        var result = businessLogicLayer.GetRecipesByNameOrId("InvalidName", 3);

        // Assert
        CollectionAssert.AreEqual(result, new HashSet<Recipe>());
    }

    [TestMethod]
    public void DoesRecipeExist_WithExistingRecipeName_ReturnsTrue()
    {
        // Arrange
        var recipeName = "ExistingRecipe";
        var recipe1 = new Recipe { Id = 1, Name = recipeName };
        appStorageMock.Setup(a => a.Recipes).Returns(new HashSet<Recipe> { recipe1 });

        // Act
        var result = businessLogicLayer.DoesRecipeExist(recipeName);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void DoesRecipeExist_WithNonExistingRecipeName_ReturnsFalse()
    {
        // Arrange
        appStorageMock.Setup(a => a.Recipes).Returns(new HashSet<Recipe>());

        // Act
        var result = businessLogicLayer.DoesRecipeExist("NonExistingRecipe");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void DeleteIngredient_WithValidIngredientId_DeletesIngredient()
    {
        // Arrange
        var ingredientId = 1;
        var ingredientToDelete = new Ingredient { Id = ingredientId, Name = "IngredientToDelete" };
        var recipe1 = new Recipe { Id = 1, Name = "Recipe1" };
        appStorageMock.Setup(a => a.Ingredients).Returns(new HashSet<Ingredient> { ingredientToDelete });
        appStorageMock.Setup(a => a.RecipeIngredients).Returns(new HashSet<RecipeIngredient> { new RecipeIngredient { IngredientId = ingredientId, RecipeId = 1 } });
        appStorageMock.Setup(a => a.Recipes).Returns(new HashSet<Recipe> { recipe1 });

        // Act
        var result = businessLogicLayer.DeleteIngredient(ingredientId, null);

        // Assert
        Assert.IsTrue(result);
        appStorageMock.Verify(a => a.Ingredients.Remove(ingredientToDelete), Times.Once);
    }

    [TestMethod]
    public void DeleteIngredient_WithNonExistingIngredient_ReturnsFalse()
    {
        // Arrange
        appStorageMock.Setup(a => a.Ingredients).Returns(new HashSet<Ingredient>());

        // Act
        var result = businessLogicLayer.DeleteIngredient(1, "NonExistingIngredient");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void DeleteIngredient_WithMultipleRecipesUsingIngredient_ReturnsFalse()
    {
        // Arrange
        var ingredientId = 1;
        var ingredientToDelete = new Ingredient { Id = ingredientId, Name = "IngredientToDelete" };
        appStorageMock.Setup(a => a.Ingredients).Returns(new HashSet<Ingredient> { ingredientToDelete });
        appStorageMock.Setup(a => a.RecipeIngredients).Returns(new HashSet<RecipeIngredient>
            {
                new RecipeIngredient { IngredientId = ingredientId, RecipeId = 1 },
                new RecipeIngredient { IngredientId = ingredientId, RecipeId = 2 }
            });

        // Act
        var result = businessLogicLayer.DeleteIngredient(ingredientId, null);

        // Assert
        Assert.IsFalse(result);
        appStorageMock.Verify(a => a.Ingredients.Remove(ingredientToDelete), Times.Never);
    }

    [TestMethod]
    public void DeleteIngredient_WithOneRecipeUsingIngredient_DeletesIngredientAndRecipe()
    {
        // Arrange
        var ingredientId = 1;
        var ingredientToDelete = new Ingredient { Id = ingredientId, Name = "IngredientToDelete" };
        var recipe1 = new Recipe { Id = 1, Name = "Recipe1" };
        appStorageMock.Setup(a => a.Ingredients).Returns(new HashSet<Ingredient> { ingredientToDelete });
        appStorageMock.Setup(a => a.RecipeIngredients).Returns(new HashSet<RecipeIngredient> { new RecipeIngredient { IngredientId = ingredientId, RecipeId = 1 } });
        appStorageMock.Setup(a => a.Recipes).Returns(new HashSet<Recipe> { recipe1 });

        // Act
        var result = businessLogicLayer.DeleteIngredient(ingredientId, null);

        // Assert
        Assert.IsTrue(result);
        appStorageMock.Verify(a => a.Ingredients.Remove(ingredientToDelete), Times.Once);
        appStorageMock.Verify(a => a.Recipes.Remove(recipe1), Times.Once);
        appStorageMock.Verify(a => a.RecipeIngredients.Remove(It.IsAny<RecipeIngredient>()), Times.Once);
    }

    [TestMethod]
    public void DeleteRecipe_WithValidRecipeId_DeletesRecipe()
    {
        // Arrange
        var recipeId = 1;
        var recipeToDelete = new Recipe { Id = recipeId, Name = "RecipeToDelete" };
        appStorageMock.Setup(a => a.Recipes).Returns(new HashSet<Recipe> { recipeToDelete });
        appStorageMock.Setup(a => a.RecipeIngredients).Returns(new HashSet<RecipeIngredient> { new RecipeIngredient { RecipeId = recipeId, IngredientId = 1 } });

        // Act
        var result = businessLogicLayer.DeleteRecipe(recipeId, null);

        // Assert
        Assert.IsTrue(result);
        appStorageMock.Verify(a => a.Recipes.Remove(recipeToDelete), Times.Once);
        appStorageMock.Verify(a => a.RecipeIngredients.Remove(It.IsAny<RecipeIngredient>()), Times.Once);
    }

    [TestMethod]
    public void DeleteRecipe_WithNonExistingRecipe_ReturnsFalse()
    {
        // Arrange
        appStorageMock.Setup(a => a.Recipes).Returns(new HashSet<Recipe>());

        // Act
        var result = businessLogicLayer.DeleteRecipe(1, "NonExistingRecipe");

        // Assert
        Assert.IsFalse(result);
    }

}
