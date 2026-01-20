using Dapper;
using GovMatch.Core.Interfaces;
using GovMatch.Core.Models;

namespace GovMatch.Data.Repositories;

public class RetrievalRuleRepository : IRepository<RetrievalRule>
{
    private readonly DatabaseContext _context;

    public RetrievalRuleRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<RetrievalRule?> GetByIdAsync(int id)
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM RetrievalRules WHERE Id = @Id";
        var result = await connection.QueryFirstOrDefaultAsync<RetrievalRuleDto>(sql, new { Id = id });
        return result?.ToModel();
    }

    public async Task<IEnumerable<RetrievalRule>> GetAllAsync()
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM RetrievalRules ORDER BY Name";
        var results = await connection.QueryAsync<RetrievalRuleDto>(sql);
        return results.Select(r => r.ToModel());
    }

    public async Task<IEnumerable<RetrievalRule>> GetEnabledRulesAsync()
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM RetrievalRules WHERE Enabled = 1 ORDER BY Name";
        var results = await connection.QueryAsync<RetrievalRuleDto>(sql);
        return results.Select(r => r.ToModel());
    }

    public async Task<int> InsertAsync(RetrievalRule entity)
    {
        var connection = _context.GetConnection();
        var dto = RetrievalRuleDto.FromModel(entity);
        var sql = @"
            INSERT INTO RetrievalRules (
                Name, Enabled, DaysBack, PType, TitleKeywords, NaicsCodes,
                ClassificationCodes, AgencyNames, AgencyCodes, SetAsideCodes,
                State, Zip, ResponseDeadlineFrom, ResponseDeadlineTo,
                MaxPages, RequestBudget, LastRunAtUtc
            ) VALUES (
                @Name, @Enabled, @DaysBack, @PType, @TitleKeywords, @NaicsCodes,
                @ClassificationCodes, @AgencyNames, @AgencyCodes, @SetAsideCodes,
                @State, @Zip, @ResponseDeadlineFrom, @ResponseDeadlineTo,
                @MaxPages, @RequestBudget, @LastRunAtUtc
            );
            SELECT last_insert_rowid();
        ";
        var id = await connection.ExecuteScalarAsync<int>(sql, dto);
        return id;
    }

    public async Task<bool> UpdateAsync(RetrievalRule entity)
    {
        var connection = _context.GetConnection();
        var dto = RetrievalRuleDto.FromModel(entity);
        var sql = @"
            UPDATE RetrievalRules SET
                Name = @Name,
                Enabled = @Enabled,
                DaysBack = @DaysBack,
                PType = @PType,
                TitleKeywords = @TitleKeywords,
                NaicsCodes = @NaicsCodes,
                ClassificationCodes = @ClassificationCodes,
                AgencyNames = @AgencyNames,
                AgencyCodes = @AgencyCodes,
                SetAsideCodes = @SetAsideCodes,
                State = @State,
                Zip = @Zip,
                ResponseDeadlineFrom = @ResponseDeadlineFrom,
                ResponseDeadlineTo = @ResponseDeadlineTo,
                MaxPages = @MaxPages,
                RequestBudget = @RequestBudget,
                LastRunAtUtc = @LastRunAtUtc
            WHERE Id = @Id
        ";
        var rows = await connection.ExecuteAsync(sql, dto);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var connection = _context.GetConnection();
        var sql = "DELETE FROM RetrievalRules WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}

internal class RetrievalRuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Enabled { get; set; }
    public int DaysBack { get; set; }
    public string? PType { get; set; }
    public string? TitleKeywords { get; set; }
    public string? NaicsCodes { get; set; }
    public string? ClassificationCodes { get; set; }
    public string? AgencyNames { get; set; }
    public string? AgencyCodes { get; set; }
    public string? SetAsideCodes { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string? ResponseDeadlineFrom { get; set; }
    public string? ResponseDeadlineTo { get; set; }
    public int MaxPages { get; set; }
    public int RequestBudget { get; set; }
    public string? LastRunAtUtc { get; set; }

    public RetrievalRule ToModel()
    {
        return new RetrievalRule
        {
            Id = Id,
            Name = Name,
            Enabled = Enabled == 1,
            DaysBack = DaysBack,
            PType = PType,
            TitleKeywords = TitleKeywords,
            NaicsCodes = NaicsCodes,
            ClassificationCodes = ClassificationCodes,
            AgencyNames = AgencyNames,
            AgencyCodes = AgencyCodes,
            SetAsideCodes = SetAsideCodes,
            State = State,
            Zip = Zip,
            ResponseDeadlineFrom = string.IsNullOrWhiteSpace(ResponseDeadlineFrom) ? null : DateTime.Parse(ResponseDeadlineFrom),
            ResponseDeadlineTo = string.IsNullOrWhiteSpace(ResponseDeadlineTo) ? null : DateTime.Parse(ResponseDeadlineTo),
            MaxPages = MaxPages,
            RequestBudget = RequestBudget,
            LastRunAtUtc = string.IsNullOrWhiteSpace(LastRunAtUtc) ? null : DateTime.Parse(LastRunAtUtc)
        };
    }

    public static RetrievalRuleDto FromModel(RetrievalRule model)
    {
        return new RetrievalRuleDto
        {
            Id = model.Id,
            Name = model.Name,
            Enabled = model.Enabled ? 1 : 0,
            DaysBack = model.DaysBack,
            PType = model.PType,
            TitleKeywords = model.TitleKeywords,
            NaicsCodes = model.NaicsCodes,
            ClassificationCodes = model.ClassificationCodes,
            AgencyNames = model.AgencyNames,
            AgencyCodes = model.AgencyCodes,
            SetAsideCodes = model.SetAsideCodes,
            State = model.State,
            Zip = model.Zip,
            ResponseDeadlineFrom = model.ResponseDeadlineFrom?.ToString("O"),
            ResponseDeadlineTo = model.ResponseDeadlineTo?.ToString("O"),
            MaxPages = model.MaxPages,
            RequestBudget = model.RequestBudget,
            LastRunAtUtc = model.LastRunAtUtc?.ToString("O")
        };
    }
}
