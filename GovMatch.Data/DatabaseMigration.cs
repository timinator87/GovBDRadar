using Microsoft.Data.Sqlite;

namespace GovMatch.Data;

public class DatabaseMigration
{
    private readonly DatabaseContext _context;

    public DatabaseMigration(DatabaseContext context)
    {
        _context = context;
    }

    public async Task ApplyMigrationsAsync()
    {
        var connection = _context.GetConnection();

        await EnsureSchemaVersionTableAsync(connection);

        var currentVersion = await GetCurrentSchemaVersionAsync(connection);

        if (currentVersion < 2)
        {
            await ApplyVersion2MigrationAsync(connection);
            await SetSchemaVersionAsync(connection, 2);
        }
    }

    private async Task EnsureSchemaVersionTableAsync(SqliteConnection connection)
    {
        var sql = @"
            CREATE TABLE IF NOT EXISTS SchemaVersion (
                Version INTEGER PRIMARY KEY,
                AppliedAtUtc TEXT NOT NULL
            );
        ";

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();

        command.CommandText = "SELECT COUNT(*) FROM SchemaVersion";
        var count = (long)await command.ExecuteScalarAsync();

        if (count == 0)
        {
            command.CommandText = "INSERT INTO SchemaVersion (Version, AppliedAtUtc) VALUES (1, @Now)";
            command.Parameters.AddWithValue("@Now", DateTime.UtcNow.ToString("O"));
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task<int> GetCurrentSchemaVersionAsync(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT MAX(Version) FROM SchemaVersion";
        var result = await command.ExecuteScalarAsync();
        return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 1;
    }

    private async Task SetSchemaVersionAsync(SqliteConnection connection, int version)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO SchemaVersion (Version, AppliedAtUtc) VALUES (@Version, @Now)";
        command.Parameters.AddWithValue("@Version", version);
        command.Parameters.AddWithValue("@Now", DateTime.UtcNow.ToString("O"));
        await command.ExecuteNonQueryAsync();
    }

    private async Task ApplyVersion2MigrationAsync(SqliteConnection connection)
    {
        var migrations = new[]
        {
            "ALTER TABLE Opportunities ADD COLUMN ContentHash TEXT",
            "ALTER TABLE Opportunities ADD COLUMN UpdatedAtUtc TEXT",
            "ALTER TABLE Opportunities ADD COLUMN LastFetchedAtUtc TEXT",
            "ALTER TABLE CompanyProfile ADD COLUMN CurrentVersionId INTEGER",
            @"CREATE TABLE IF NOT EXISTS ProfileVersions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CreatedAtUtc TEXT NOT NULL,
                DisplayNameSnapshot TEXT NOT NULL,
                CapabilityTextSnapshot TEXT,
                KeywordsSnapshot TEXT,
                NaicsSnapshot TEXT,
                PscSnapshot TEXT,
                TargetAgencyCodesSnapshot TEXT,
                SetAsidePreferencesSnapshot TEXT,
                PastPerformanceTextSnapshot TEXT
            )",
            @"CREATE TABLE IF NOT EXISTS OpportunityEvents (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NoticeId TEXT NOT NULL,
                EventType INTEGER NOT NULL,
                OldScore INTEGER,
                NewScore INTEGER,
                OccurredAtUtc TEXT NOT NULL,
                ProfileVersionId INTEGER,
                FOREIGN KEY (NoticeId) REFERENCES Opportunities(NoticeId),
                FOREIGN KEY (ProfileVersionId) REFERENCES ProfileVersions(Id)
            )",
            "CREATE INDEX IF NOT EXISTS idx_opportunityevents_noticeid ON OpportunityEvents(NoticeId)",
            "CREATE INDEX IF NOT EXISTS idx_opportunityevents_occurred ON OpportunityEvents(OccurredAtUtc)",
            "CREATE INDEX IF NOT EXISTS idx_opportunityevents_type ON OpportunityEvents(EventType)",
            @"CREATE TABLE IF NOT EXISTS Digests (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                GeneratedAtUtc TEXT NOT NULL,
                ProfileVersionId INTEGER,
                DigestJson TEXT,
                SentVia INTEGER NOT NULL DEFAULT 0,
                LastDigestCutoffUtc TEXT,
                FOREIGN KEY (ProfileVersionId) REFERENCES ProfileVersions(Id)
            )",
            "CREATE INDEX IF NOT EXISTS idx_digests_generated ON Digests(GeneratedAtUtc DESC)",
            "CREATE INDEX IF NOT EXISTS idx_opportunities_contenthash ON Opportunities(ContentHash)"
        };

        foreach (var migrationSql in migrations)
        {
            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = migrationSql;
                await command.ExecuteNonQueryAsync();
            }
            catch (SqliteException ex) when (ex.Message.Contains("duplicate column"))
            {
                continue;
            }
        }
    }
}
