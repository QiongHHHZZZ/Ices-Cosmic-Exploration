using ECommons.GameHelpers;
using Lumina.Excel.Sheets;

namespace ICE.Utilities.Cosmic_Helper;

public static partial class CosmicHelper
{
    public class RecipeInfo
    {
        public int Durability { get; set; } = 0;
        public int Progress { get; set; } = 0;
        public int Quality { get; set; } = 0;
        public bool Expert { get; set; } = false;
    }
    public static RecipeInfo SpecificRecipeInfo(uint jobId, uint recipeId)
    {
        RecipeInfo info = new();

        Job CrafterJob = (Job)jobId;
        var level = Player.GetLevel(CrafterJob);
        var recipeSheet = Svc.Data.GetExcelSheet<Recipe>().GetRow(recipeId);
        var recipeLevelValue = recipeSheet.RecipeLevelTable.RowId;
        var levelTable = recipeLevelValue == 0 && level < 100 ? Svc.Data.GetExcelSheet<RecipeLevelTable>().First(x => x.ClassJobLevel == level) : recipeSheet.RecipeLevelTable.Value;
        info.Progress = recipeLevelValue == 0 ? RecipeDifficulty(recipeSheet, levelTable) : RecipeDifficulty(recipeSheet);
        info.Durability = RecipeDurability(recipeSheet);
        info.Quality = recipeLevelValue == 0 ? RecipeMaxQuality(recipeSheet, levelTable) : RecipeMaxQuality(recipeSheet);
        // info.Expert = recipeSheet.RecipeLevelTable.Value.ConditionsFlag != 15; // Use this if something breaks... but rn it's causing issues
        info.Expert = recipeSheet.IsExpert;

        /*
        var recipe = Svc.Data.GetExcelSheet<Recipe>().GetRow(recipeId);
        var level = 100;
        var lt = recipe.Number == 0 && level < 100 ? Svc.Data.GetExcelSheet<RecipeLevelTable>().First(x => x.ClassJobLevel == 100) : recipe.RecipeLevelTable.Value;

        info.Durability = RecipeDurability(recipe);
        info.Progress = recipe.Number == 0 ? RecipeDifficulty(recipe, lt) : RecipeDifficulty(recipe);
        info.Quality = recipe.Number == 0 ? RecipeMaxQuality(recipe, lt) : RecipeMaxQuality(recipe);
        */

        return info;
    }

    public static int RecipeDifficulty(Recipe recipe) => recipe.RecipeLevelTable.Value.Difficulty * recipe.DifficultyFactor / 100;
    public static int RecipeMaxQuality(Recipe recipe) => (int)(recipe.RecipeLevelTable.Value.Quality * recipe.QualityFactor / 100);
    public static int RecipeDurability(Recipe recipe) => recipe.RecipeLevelTable.Value.Durability * recipe.DurabilityFactor / 100;

    public static int RecipeDifficulty(Recipe recipe, RecipeLevelTable leveltable) => leveltable.Difficulty * recipe.DifficultyFactor / 100;
    public static int RecipeMaxQuality(Recipe recipe, RecipeLevelTable leveltable) => (int)(leveltable.Quality * recipe.QualityFactor / 100);
    public static int RecipeDurability(Recipe recipe, RecipeLevelTable leveltable) => leveltable.Durability * recipe.DurabilityFactor / 100;
}
