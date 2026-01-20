using Dapper;
using GovMatch.Core.Interfaces;
using GovMatch.Core.Models;

namespace GovMatch.Data.Repositories;

public class OpportunityRepository : IRepository<Opportunity>
{
    private readonly DatabaseContext _context;

    public OpportunityRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Opportunity?> GetByIdAsync(int id)
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM Opportunities WHERE Id = @Id";
        var result = await connection.QueryFirstOrDefaultAsync<OpportunityDto>(sql, new { Id = id });
        return result?.ToModel();
    }

    public async Task<Opportunity?> GetByNoticeIdAsync(string noticeId)
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM Opportunities WHERE NoticeId = @NoticeId";
        var result = await connection.QueryFirstOrDefaultAsync<OpportunityDto>(sql, new { NoticeId = noticeId });
        return result?.ToModel();
    }

    public async Task<IEnumerable<Opportunity>> GetAllAsync()
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM Opportunities ORDER BY PostedDate DESC";
        var results = await connection.QueryAsync<OpportunityDto>(sql);
        return results.Select(r => r.ToModel());
    }

    public async Task<int> InsertAsync(Opportunity entity)
    {
        var connection = _context.GetConnection();
        var dto = OpportunityDto.FromModel(entity);
        var sql = @"
            INSERT INTO Opportunities (
                NoticeId, Title, SolicitationNumber, PostedDate, ResponseDeadline,
                Type, BaseType, Active, NaicsCode, ClassificationCode,
                SetAsideCode, SetAsideDescription, AgencyPathName, AgencyPathCode,
                PlaceOfPerformance, PocJson, DescriptionUrl, DescriptionText,
                UiUrl, ResourceLinksJson, RawJson, LastSeenAtUtc, FirstSeenAtUtc
            ) VALUES (
                @NoticeId, @Title, @SolicitationNumber, @PostedDate, @ResponseDeadline,
                @Type, @BaseType, @Active, @NaicsCode, @ClassificationCode,
                @SetAsideCode, @SetAsideDescription, @AgencyPathName, @AgencyPathCode,
                @PlaceOfPerformance, @PocJson, @DescriptionUrl, @DescriptionText,
                @UiUrl, @ResourceLinksJson, @RawJson, @LastSeenAtUtc, @FirstSeenAtUtc
            );
            SELECT last_insert_rowid();
        ";
        var id = await connection.ExecuteScalarAsync<int>(sql, dto);
        return id;
    }

    public async Task<bool> UpdateAsync(Opportunity entity)
    {
        var connection = _context.GetConnection();
        var dto = OpportunityDto.FromModel(entity);
        var sql = @"
            UPDATE Opportunities SET
                Title = @Title,
                SolicitationNumber = @SolicitationNumber,
                PostedDate = @PostedDate,
                ResponseDeadline = @ResponseDeadline,
                Type = @Type,
                BaseType = @BaseType,
                Active = @Active,
                NaicsCode = @NaicsCode,
                ClassificationCode = @ClassificationCode,
                SetAsideCode = @SetAsideCode,
                SetAsideDescription = @SetAsideDescription,
                AgencyPathName = @AgencyPathName,
                AgencyPathCode = @AgencyPathCode,
                PlaceOfPerformance = @PlaceOfPerformance,
                PocJson = @PocJson,
                DescriptionUrl = @DescriptionUrl,
                DescriptionText = @DescriptionText,
                UiUrl = @UiUrl,
                ResourceLinksJson = @ResourceLinksJson,
                RawJson = @RawJson,
                LastSeenAtUtc = @LastSeenAtUtc
            WHERE Id = @Id
        ";
        var rows = await connection.ExecuteAsync(sql, dto);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var connection = _context.GetConnection();
        var sql = "DELETE FROM Opportunities WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task<IEnumerable<Opportunity>> SearchAsync(OpportunitySearchCriteria criteria)
    {
        var connection = _context.GetConnection();
        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (criteria.MinScore.HasValue)
        {
            whereClauses.Add("EXISTS (SELECT 1 FROM MatchResults WHERE MatchResults.NoticeId = Opportunities.NoticeId AND MatchResults.Score >= @MinScore)");
            parameters.Add("MinScore", criteria.MinScore.Value);
        }

        if (criteria.ActiveOnly)
        {
            whereClauses.Add("Active = 1");
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            whereClauses.Add("(Title LIKE @SearchText OR SolicitationNumber LIKE @SearchText)");
            parameters.Add("SearchText", $"%{criteria.SearchText}%");
        }

        if (!string.IsNullOrWhiteSpace(criteria.NaicsCode))
        {
            whereClauses.Add("NaicsCode LIKE @NaicsCode");
            parameters.Add("NaicsCode", $"%{criteria.NaicsCode}%");
        }

        if (!string.IsNullOrWhiteSpace(criteria.SetAsideCode))
        {
            whereClauses.Add("SetAsideCode = @SetAsideCode");
            parameters.Add("SetAsideCode", criteria.SetAsideCode);
        }

        var whereClause = whereClauses.Any() ? "WHERE " + string.Join(" AND ", whereClauses) : "";
        var sql = $"SELECT * FROM Opportunities {whereClause} ORDER BY PostedDate DESC";

        var results = await connection.QueryAsync<OpportunityDto>(sql, parameters);
        return results.Select(r => r.ToModel());
    }
}

public class OpportunitySearchCriteria
{
    public int? MinScore { get; set; }
    public bool ActiveOnly { get; set; }
    public string? SearchText { get; set; }
    public string? NaicsCode { get; set; }
    public string? SetAsideCode { get; set; }
}

internal class OpportunityDto
{
    public int Id { get; set; }
    public string NoticeId { get; set; } = "";
    public string Title { get; set; } = "";
    public string? SolicitationNumber { get; set; }
    public string PostedDate { get; set; } = "";
    public string? ResponseDeadline { get; set; }
    public string? Type { get; set; }
    public string? BaseType { get; set; }
    public int Active { get; set; }
    public string? NaicsCode { get; set; }
    public string? ClassificationCode { get; set; }
    public string? SetAsideCode { get; set; }
    public string? SetAsideDescription { get; set; }
    public string? AgencyPathName { get; set; }
    public string? AgencyPathCode { get; set; }
    public string? PlaceOfPerformance { get; set; }
    public string? PocJson { get; set; }
    public string? DescriptionUrl { get; set; }
    public string? DescriptionText { get; set; }
    public string? UiUrl { get; set; }
    public string? ResourceLinksJson { get; set; }
    public string? RawJson { get; set; }
    public string LastSeenAtUtc { get; set; } = "";
    public string FirstSeenAtUtc { get; set; } = "";
    public string? ContentHash { get; set; }
    public string? UpdatedAtUtc { get; set; }
    public string? LastFetchedAtUtc { get; set; }

    public Opportunity ToModel()
    {
        return new Opportunity
        {
            Id = Id,
            NoticeId = NoticeId,
            Title = Title,
            SolicitationNumber = SolicitationNumber,
            PostedDate = DateTime.Parse(PostedDate),
            ResponseDeadline = string.IsNullOrWhiteSpace(ResponseDeadline) ? null : DateTime.Parse(ResponseDeadline),
            Type = Type,
            BaseType = BaseType,
            Active = Active == 1,
            NaicsCode = NaicsCode,
            ClassificationCode = ClassificationCode,
            SetAsideCode = SetAsideCode,
            SetAsideDescription = SetAsideDescription,
            AgencyPathName = AgencyPathName,
            AgencyPathCode = AgencyPathCode,
            PlaceOfPerformance = PlaceOfPerformance,
            PocJson = PocJson,
            DescriptionUrl = DescriptionUrl,
            DescriptionText = DescriptionText,
            UiUrl = UiUrl,
            ResourceLinksJson = ResourceLinksJson,
            RawJson = RawJson,
            LastSeenAtUtc = DateTime.Parse(LastSeenAtUtc),
            FirstSeenAtUtc = DateTime.Parse(FirstSeenAtUtc),
            ContentHash = ContentHash,
            UpdatedAtUtc = string.IsNullOrWhiteSpace(UpdatedAtUtc) ? null : DateTime.Parse(UpdatedAtUtc),
            LastFetchedAtUtc = string.IsNullOrWhiteSpace(LastFetchedAtUtc) ? null : DateTime.Parse(LastFetchedAtUtc)
        };
    }

    public static OpportunityDto FromModel(Opportunity model)
    {
        return new OpportunityDto
        {
            Id = model.Id,
            NoticeId = model.NoticeId,
            Title = model.Title,
            SolicitationNumber = model.SolicitationNumber,
            PostedDate = model.PostedDate.ToString("O"),
            ResponseDeadline = model.ResponseDeadline?.ToString("O"),
            Type = model.Type,
            BaseType = model.BaseType,
            Active = model.Active ? 1 : 0,
            NaicsCode = model.NaicsCode,
            ClassificationCode = model.ClassificationCode,
            SetAsideCode = model.SetAsideCode,
            SetAsideDescription = model.SetAsideDescription,
            AgencyPathName = model.AgencyPathName,
            AgencyPathCode = model.AgencyPathCode,
            PlaceOfPerformance = model.PlaceOfPerformance,
            PocJson = model.PocJson,
            DescriptionUrl = model.DescriptionUrl,
            DescriptionText = model.DescriptionText,
            UiUrl = model.UiUrl,
            ResourceLinksJson = model.ResourceLinksJson,
            RawJson = model.RawJson,
            LastSeenAtUtc = model.LastSeenAtUtc.ToString("O"),
            FirstSeenAtUtc = model.FirstSeenAtUtc.ToString("O"),
            ContentHash = model.ContentHash,
            UpdatedAtUtc = model.UpdatedAtUtc?.ToString("O"),
            LastFetchedAtUtc = model.LastFetchedAtUtc?.ToString("O")
        };
    }
}
