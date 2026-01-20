using Dapper;
using GovMatch.Core.Interfaces;
using GovMatch.Core.Models;

namespace GovMatch.Data.Repositories;

public class UserActionRepository : IRepository<UserAction>
{
    private readonly DatabaseContext _context;

    public UserActionRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<UserAction?> GetByIdAsync(int id)
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM UserActions WHERE Id = @Id";
        var result = await connection.QueryFirstOrDefaultAsync<UserActionDto>(sql, new { Id = id });
        return result?.ToModel();
    }

    public async Task<UserAction?> GetByNoticeIdAsync(string noticeId)
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM UserActions WHERE NoticeId = @NoticeId ORDER BY UpdatedAtUtc DESC LIMIT 1";
        var result = await connection.QueryFirstOrDefaultAsync<UserActionDto>(sql, new { NoticeId = noticeId });
        return result?.ToModel();
    }

    public async Task<IEnumerable<UserAction>> GetAllAsync()
    {
        var connection = _context.GetConnection();
        var sql = "SELECT * FROM UserActions ORDER BY UpdatedAtUtc DESC";
        var results = await connection.QueryAsync<UserActionDto>(sql);
        return results.Select(r => r.ToModel());
    }

    public async Task<int> InsertAsync(UserAction entity)
    {
        var connection = _context.GetConnection();
        var dto = UserActionDto.FromModel(entity);
        var sql = @"
            INSERT INTO UserActions (NoticeId, ActionType, Notes, UpdatedAtUtc)
            VALUES (@NoticeId, @ActionType, @Notes, @UpdatedAtUtc);
            SELECT last_insert_rowid();
        ";
        var id = await connection.ExecuteScalarAsync<int>(sql, dto);
        return id;
    }

    public async Task<bool> UpdateAsync(UserAction entity)
    {
        var connection = _context.GetConnection();
        var dto = UserActionDto.FromModel(entity);
        var sql = @"
            UPDATE UserActions SET
                ActionType = @ActionType,
                Notes = @Notes,
                UpdatedAtUtc = @UpdatedAtUtc
            WHERE Id = @Id
        ";
        var rows = await connection.ExecuteAsync(sql, dto);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var connection = _context.GetConnection();
        var sql = "DELETE FROM UserActions WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}

internal class UserActionDto
{
    public int Id { get; set; }
    public string NoticeId { get; set; } = "";
    public int ActionType { get; set; }
    public string? Notes { get; set; }
    public string UpdatedAtUtc { get; set; } = "";

    public UserAction ToModel()
    {
        return new UserAction
        {
            Id = Id,
            NoticeId = NoticeId,
            ActionType = (UserActionType)ActionType,
            Notes = Notes,
            UpdatedAtUtc = DateTime.Parse(UpdatedAtUtc)
        };
    }

    public static UserActionDto FromModel(UserAction model)
    {
        return new UserActionDto
        {
            Id = model.Id,
            NoticeId = model.NoticeId,
            ActionType = (int)model.ActionType,
            Notes = model.Notes,
            UpdatedAtUtc = model.UpdatedAtUtc.ToString("O")
        };
    }
}
