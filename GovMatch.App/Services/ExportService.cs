using System.IO;
using System.Text;
using System.Text.Json;
using GovMatch.Core.Models;

namespace GovMatch.App.Services;

public class ExportService
{
    public async Task ExportToCsvAsync(IEnumerable<OpportunityViewModel> opportunities, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("NoticeId,Title,Score,Agency,PostedDate,ResponseDeadline,NAICS,PSC,SetAside,Type,Status,URL");

        foreach (var opp in opportunities)
        {
            sb.AppendLine($"\"{EscapeCsv(opp.NoticeId)}\",\"{EscapeCsv(opp.Title)}\",{opp.Score},\"{EscapeCsv(opp.Agency)}\",\"{opp.PostedDate:yyyy-MM-dd}\",\"{opp.ResponseDeadline:yyyy-MM-dd}\",\"{EscapeCsv(opp.NaicsCode)}\",\"{EscapeCsv(opp.ClassificationCode)}\",\"{EscapeCsv(opp.SetAside)}\",\"{EscapeCsv(opp.Type)}\",\"{opp.Status}\",\"{EscapeCsv(opp.UiUrl)}\"");
        }

        await File.WriteAllTextAsync(filePath, sb.ToString());
    }

    public async Task ExportToMarkdownAsync(Opportunity opportunity, MatchResult? matchResult, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {opportunity.Title}");
        sb.AppendLine();
        sb.AppendLine($"**Notice ID:** {opportunity.NoticeId}");
        sb.AppendLine($"**Solicitation Number:** {opportunity.SolicitationNumber}");
        sb.AppendLine($"**Posted Date:** {opportunity.PostedDate:yyyy-MM-dd}");
        sb.AppendLine($"**Response Deadline:** {opportunity.ResponseDeadline:yyyy-MM-dd}");
        sb.AppendLine($"**Type:** {opportunity.Type}");
        sb.AppendLine($"**Agency:** {opportunity.AgencyPathName}");
        sb.AppendLine($"**NAICS Code:** {opportunity.NaicsCode}");
        sb.AppendLine($"**Classification Code:** {opportunity.ClassificationCode}");
        sb.AppendLine($"**Set-Aside:** {opportunity.SetAsideDescription}");
        sb.AppendLine();

        if (matchResult != null)
        {
            sb.AppendLine($"## Match Score: {matchResult.Score}/100");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(matchResult.ScoreBreakdownJson))
            {
                var breakdown = JsonSerializer.Deserialize<ScoreBreakdown>(matchResult.ScoreBreakdownJson);
                if (breakdown != null)
                {
                    sb.AppendLine("### Why This Matched");
                    foreach (var factor in breakdown.Factors)
                    {
                        sb.AppendLine($"- **{factor.Name}:** {factor.Points}/{factor.MaxPoints} - {factor.Details}");
                    }
                    sb.AppendLine();
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(opportunity.PocJson))
        {
            sb.AppendLine("## Points of Contact");
            var pocs = JsonSerializer.Deserialize<List<PointOfContactDto>>(opportunity.PocJson);
            if (pocs != null)
            {
                foreach (var poc in pocs)
                {
                    sb.AppendLine($"- **{poc.FullName}** ({poc.Type})");
                    if (!string.IsNullOrWhiteSpace(poc.Email))
                        sb.AppendLine($"  - Email: {poc.Email}");
                    if (!string.IsNullOrWhiteSpace(poc.Phone))
                        sb.AppendLine($"  - Phone: {poc.Phone}");
                }
            }
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(opportunity.UiUrl))
        {
            sb.AppendLine($"## Links");
            sb.AppendLine($"- [View on SAM.gov]({opportunity.UiUrl})");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(opportunity.DescriptionText))
        {
            sb.AppendLine("## Description");
            sb.AppendLine(opportunity.DescriptionText);
        }

        await File.WriteAllTextAsync(filePath, sb.ToString());
    }

    private string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
        return value.Replace("\"", "\"\"");
    }
}

public class OpportunityViewModel
{
    public string NoticeId { get; set; } = "";
    public string Title { get; set; } = "";
    public int Score { get; set; }
    public string Agency { get; set; } = "";
    public DateTime PostedDate { get; set; }
    public DateTime? ResponseDeadline { get; set; }
    public string? NaicsCode { get; set; }
    public string? ClassificationCode { get; set; }
    public string? SetAside { get; set; }
    public string? Type { get; set; }
    public string Status { get; set; } = "";
    public string? UiUrl { get; set; }
}

public class PointOfContactDto
{
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? FullName { get; set; }
    public string? Type { get; set; }
}
