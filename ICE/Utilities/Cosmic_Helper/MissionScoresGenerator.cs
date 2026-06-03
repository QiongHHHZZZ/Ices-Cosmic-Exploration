using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ICE.Utilities.Cosmic_Helper;

/// <summary>
/// Fills in MissionScores.csv rows from bronze sheet values when a mission is not in the embedded CSV yet.
/// Debug → Table: Mission Info → "Copy Auxesia CSV" pastes rows to append to Resources/MissionScores.csv.
/// </summary>
public static class MissionScoresGenerator
{
    private static readonly Dictionary<uint, string> JobAbbreviations = new()
    {
        [8] = "CRP",
        [9] = "BSM",
        [10] = "ARM",
        [11] = "GSM",
        [12] = "LTW",
        [13] = "WVR",
        [14] = "ALC",
        [15] = "CUL",
        [16] = "MIN",
        [17] = "BTN",
        [18] = "FSH",
    };

    /// <summary>Missions in SheetMissionDict with no embedded CSV row — score from BronzeScore.</summary>
    public static List<string> GetMissingCsvRows(uint? territoryId = null)
    {
        var rows = new List<string>();

        foreach (var (missionId, info) in CosmicHelper.SheetMissionDict.OrderBy(x => x.Key))
        {
            if (CosmicHelper.MissionScoreDict.ContainsKey(missionId))
                continue;
            if (info.BronzeScore == 0)
                continue;
            if (territoryId is uint tid && info.TerritoryId != tid)
                continue;
            if (info.Jobs.Count == 0)
                continue;

            var jobCode = GetJobAbbreviation(info.Jobs[0]);
            var name = EscapeCsvField(info.Name);
            rows.Add($"{missionId},{jobCode},{name},{info.BronzeScore}");
        }

        return rows;
    }

    public static int CountMissing(uint? territoryId = null) =>
        GetMissingCsvRows(territoryId).Count;

    public static string BuildCsvText(uint? territoryId = null, bool includeHeader = true)
    {
        var rows = GetMissingCsvRows(territoryId);
        var sb = new StringBuilder();
        if (includeHeader)
            sb.AppendLine("MissionID,Class,MissionName,Score");
        foreach (var row in rows)
            sb.AppendLine(row);
        return sb.ToString();
    }

    public static bool TryExportMissingRows(string path, out string message, uint? territoryId = null)
    {
        var rows = GetMissingCsvRows(territoryId);
        if (rows.Count == 0)
        {
            message = "No missing missions — every sheet mission has a CSV row or bronze score is 0.";
            return false;
        }

        try
        {
            if (!path.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase))
                path += ".csv";

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, BuildCsvText(territoryId), Encoding.UTF8);
            message = $"Exported {rows.Count} missing rows to {path}";
            return true;
        }
        catch (System.Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }

    private static string GetJobAbbreviation(uint jobId)
    {
        if (CosmicHelper.ClassInfoDict.TryGetValue(jobId, out var jobClass)
            && !string.IsNullOrWhiteSpace(jobClass.shortName)
            && jobClass.shortName != "???")
            return jobClass.shortName;

        return JobAbbreviations.TryGetValue(jobId, out var abbr) ? abbr : jobId.ToString();
    }

    private static string EscapeCsvField(string field)
    {
        field = field.Replace("<nbsp>", " ").Replace("<->", "").Trim();
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
