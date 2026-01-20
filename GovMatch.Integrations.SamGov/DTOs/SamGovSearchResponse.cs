using System.Text.Json.Serialization;

namespace GovMatch.Integrations.SamGov.DTOs;

public class SamGovSearchResponse
{
    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("opportunitiesData")]
    public List<OpportunityData> OpportunitiesData { get; set; } = new();
}

public class OpportunityData
{
    [JsonPropertyName("noticeId")]
    public string? NoticeId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("solicitationNumber")]
    public string? SolicitationNumber { get; set; }

    [JsonPropertyName("postedDate")]
    public string? PostedDate { get; set; }

    [JsonPropertyName("responseDeadLine")]
    public string? ResponseDeadLine { get; set; }

    [JsonPropertyName("responseDeadline")]
    public string? ResponseDeadline { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("baseType")]
    public string? BaseType { get; set; }

    [JsonPropertyName("active")]
    public string? Active { get; set; }

    [JsonPropertyName("naicsCode")]
    public string? NaicsCode { get; set; }

    [JsonPropertyName("classificationCode")]
    public string? ClassificationCode { get; set; }

    [JsonPropertyName("typeOfSetAside")]
    public string? TypeOfSetAside { get; set; }

    [JsonPropertyName("typeOfSetAsideDescription")]
    public string? TypeOfSetAsideDescription { get; set; }

    [JsonPropertyName("fullParentPathName")]
    public string? FullParentPathName { get; set; }

    [JsonPropertyName("fullParentPathCode")]
    public string? FullParentPathCode { get; set; }

    [JsonPropertyName("pointOfContact")]
    public List<PointOfContact>? PointOfContact { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("resourceLinks")]
    public List<ResourceLink>? ResourceLinks { get; set; }

    [JsonPropertyName("uiLink")]
    public string? UiLink { get; set; }

    [JsonPropertyName("links")]
    public List<LinkInfo>? Links { get; set; }
}

public class PointOfContact
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("fax")]
    public string? Fax { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class ResourceLink
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }
}

public class LinkInfo
{
    [JsonPropertyName("rel")]
    public string? Rel { get; set; }

    [JsonPropertyName("href")]
    public string? Href { get; set; }
}
