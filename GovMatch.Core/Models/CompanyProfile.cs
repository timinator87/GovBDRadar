namespace GovMatch.Core.Models;

public class CompanyProfile
{
    public int Id { get; set; }
    public int? CurrentVersionId { get; set; }
    public string DisplayName { get; set; } = "My Company";
    public string? CapabilityText { get; set; }
    public string? KeywordsCsv { get; set; }
    public string? NaicsCsv { get; set; }
    public string? PscCsv { get; set; }
    public string? TargetAgencyCodesCsv { get; set; }
    public string? TargetAgencyNamesCsv { get; set; }
    public string? SetAsidePreferencesCsv { get; set; }
    public string? PastPerformanceText { get; set; }

    public IEnumerable<string> GetKeywords()
    {
        if (string.IsNullOrWhiteSpace(KeywordsCsv))
            return Array.Empty<string>();
        return KeywordsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public IEnumerable<string> GetNaicsCodes()
    {
        if (string.IsNullOrWhiteSpace(NaicsCsv))
            return Array.Empty<string>();
        return NaicsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public IEnumerable<string> GetPscCodes()
    {
        if (string.IsNullOrWhiteSpace(PscCsv))
            return Array.Empty<string>();
        return PscCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public IEnumerable<string> GetTargetAgencyCodes()
    {
        if (string.IsNullOrWhiteSpace(TargetAgencyCodesCsv))
            return Array.Empty<string>();
        return TargetAgencyCodesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public IEnumerable<string> GetSetAsidePreferences()
    {
        if (string.IsNullOrWhiteSpace(SetAsidePreferencesCsv))
            return Array.Empty<string>();
        return SetAsidePreferencesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public string GetCombinedProfileText()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(CapabilityText))
            parts.Add(CapabilityText);
        if (!string.IsNullOrWhiteSpace(PastPerformanceText))
            parts.Add(PastPerformanceText);
        if (!string.IsNullOrWhiteSpace(KeywordsCsv))
            parts.Add(KeywordsCsv.Replace(",", " "));
        return string.Join(" ", parts);
    }
}
