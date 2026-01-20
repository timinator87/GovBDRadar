using System.Text.Json;
using GovMatch.Core.Models;
using GovMatch.Integrations.SamGov.DTOs;

namespace GovMatch.Integrations.SamGov.Mappers;

public static class OpportunityMapper
{
    public static Opportunity ToOpportunity(this OpportunityData dto)
    {
        var responseDeadline = ParseSamDate(dto.ResponseDeadline ?? dto.ResponseDeadLine);
        var postedDate = ParseSamDate(dto.PostedDate) ?? DateTime.UtcNow;

        return new Opportunity
        {
            NoticeId = dto.NoticeId ?? Guid.NewGuid().ToString(),
            Title = dto.Title ?? "Untitled",
            SolicitationNumber = dto.SolicitationNumber,
            PostedDate = postedDate,
            ResponseDeadline = responseDeadline,
            Type = dto.Type,
            BaseType = dto.BaseType,
            Active = dto.Active?.Equals("Yes", StringComparison.OrdinalIgnoreCase) ?? false,
            NaicsCode = dto.NaicsCode,
            ClassificationCode = dto.ClassificationCode,
            SetAsideCode = dto.TypeOfSetAside,
            SetAsideDescription = dto.TypeOfSetAsideDescription,
            AgencyPathName = dto.FullParentPathName,
            AgencyPathCode = dto.FullParentPathCode,
            PocJson = dto.PointOfContact != null ? JsonSerializer.Serialize(dto.PointOfContact) : null,
            DescriptionUrl = dto.Description,
            UiUrl = dto.UiLink ?? dto.Links?.FirstOrDefault(l => l.Rel == "self")?.Href,
            ResourceLinksJson = dto.ResourceLinks != null ? JsonSerializer.Serialize(dto.ResourceLinks) : null,
            RawJson = JsonSerializer.Serialize(dto),
            LastSeenAtUtc = DateTime.UtcNow,
            FirstSeenAtUtc = DateTime.UtcNow
        };
    }

    private static DateTime? ParseSamDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        if (DateTime.TryParse(dateString, out var result))
            return result;

        return null;
    }
}
