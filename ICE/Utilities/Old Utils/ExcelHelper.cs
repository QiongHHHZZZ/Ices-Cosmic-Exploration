using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace ICE.Utilities;
internal static class ExcelHelper
{
    // Place to keep all the excel sheets that I'm currently grabbing from
    // Vs having to call it x20 times whenever I need it, it's just right there

    internal static ExcelSheet<Item>? ItemSheet;
    internal static ExcelSheet<Recipe>? RecipeSheet;
    internal static ExcelSheet<Weather>? WeatherSheet;
    internal static ExcelSheet<TerritoryType>? TerritorySheet;
    internal static ExcelSheet<ClassJob>? ClassJobSheet;
    internal static ExcelSheet<WKSDevGrade>? DevGrade;
    internal static ExcelSheet<WKSMissionUnit>? MoonMissionSheet;
    internal static ExcelSheet<WKSMissionRecipe>? wksMissionRecipe;
    internal static ExcelSheet<WKSMissionReward>? ExpSheet;
    internal static ExcelSheet<WKSMissionToDo>? ToDoSheet;
    internal static ExcelSheet<WKSItemInfo>? MoonItemInfoSheet;
    internal static ExcelSheet<WKSMissionMapMarker>? MarkerSheet;
    internal static ExcelSheet<LeveAssignmentType>? LeveAssignmentSheet;
    internal static SubrowExcelSheet<WKSMissionToDoEvalutionItem>? EvalSheet;
    internal static ExcelSheet<WKSScoreList>? WKSScoreListSheet;
    internal static ExcelSheet<Lumina.Excel.Sheets.Action>? ActionSheet;

    public static void Init() // Only need to grab once, they won't change
    {
        Svc.Data.GameData.Options.PanicOnSheetChecksumMismatch = false;
        ItemSheet ??= Svc.Data.GetExcelSheet<Item>();
        RecipeSheet ??= Svc.Data.GetExcelSheet<Recipe>();
        WeatherSheet ??= Svc.Data.GetExcelSheet<Weather>();
        TerritorySheet ??= Svc.Data.GetExcelSheet<TerritoryType>();
        ClassJobSheet ??= Svc.Data.GetExcelSheet<ClassJob>();
        DevGrade ??= Svc.Data.GetExcelSheet<WKSDevGrade>();
        MoonMissionSheet ??= Svc.Data.GetExcelSheet<WKSMissionUnit>();
        wksMissionRecipe ??= Svc.Data.GetExcelSheet<WKSMissionRecipe>();
        ExpSheet ??= Svc.Data.GetExcelSheet<WKSMissionReward>();
        ToDoSheet ??= Svc.Data.GetExcelSheet<WKSMissionToDo>();
        MoonItemInfoSheet ??= Svc.Data.GetExcelSheet<WKSItemInfo>();
        MarkerSheet ??= Svc.Data.GetExcelSheet<WKSMissionMapMarker>();
        LeveAssignmentSheet ??= Svc.Data.GetExcelSheet<LeveAssignmentType>(); // using this for icons
        EvalSheet ??= Svc.Data.GetSubrowExcelSheet<WKSMissionToDoEvalutionItem>();
        WKSScoreListSheet ??= Svc.Data.GetExcelSheet<WKSScoreList>();
        ActionSheet ??= Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>();
    }
}