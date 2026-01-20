using Microsoft.Data.Sqlite;

namespace GovMatch.Data;

public class DatabaseContext : IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;

    public DatabaseContext(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public SqliteConnection GetConnection()
    {
        if (_connection == null)
        {
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();
        }
        return _connection;
    }

    public async Task InitializeDatabaseAsync()
    {
        var connection = GetConnection();

        var createTablesScript = @"
            CREATE TABLE IF NOT EXISTS Opportunities (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NoticeId TEXT NOT NULL UNIQUE,
                Title TEXT NOT NULL,
                SolicitationNumber TEXT,
                PostedDate TEXT NOT NULL,
                ResponseDeadline TEXT,
                Type TEXT,
                BaseType TEXT,
                Active INTEGER NOT NULL DEFAULT 1,
                NaicsCode TEXT,
                ClassificationCode TEXT,
                SetAsideCode TEXT,
                SetAsideDescription TEXT,
                AgencyPathName TEXT,
                AgencyPathCode TEXT,
                PlaceOfPerformance TEXT,
                PocJson TEXT,
                DescriptionUrl TEXT,
                DescriptionText TEXT,
                UiUrl TEXT,
                ResourceLinksJson TEXT,
                RawJson TEXT,
                LastSeenAtUtc TEXT NOT NULL,
                FirstSeenAtUtc TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_opportunities_noticeid ON Opportunities(NoticeId);
            CREATE INDEX IF NOT EXISTS idx_opportunities_posted ON Opportunities(PostedDate);
            CREATE INDEX IF NOT EXISTS idx_opportunities_active ON Opportunities(Active);

            CREATE TABLE IF NOT EXISTS MatchResults (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NoticeId TEXT NOT NULL,
                Score INTEGER NOT NULL,
                ScoreBreakdownJson TEXT,
                ScoredAtUtc TEXT NOT NULL,
                FOREIGN KEY (NoticeId) REFERENCES Opportunities(NoticeId)
            );

            CREATE INDEX IF NOT EXISTS idx_matchresults_noticeid ON MatchResults(NoticeId);
            CREATE INDEX IF NOT EXISTS idx_matchresults_score ON MatchResults(Score DESC);

            CREATE TABLE IF NOT EXISTS CompanyProfile (
                Id INTEGER PRIMARY KEY CHECK (Id = 1),
                DisplayName TEXT NOT NULL DEFAULT 'My Company',
                CapabilityText TEXT,
                KeywordsCsv TEXT,
                NaicsCsv TEXT,
                PscCsv TEXT,
                TargetAgencyCodesCsv TEXT,
                TargetAgencyNamesCsv TEXT,
                SetAsidePreferencesCsv TEXT,
                PastPerformanceText TEXT
            );

            INSERT OR IGNORE INTO CompanyProfile (Id, DisplayName) VALUES (1, 'My Company');

            CREATE TABLE IF NOT EXISTS UserActions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                NoticeId TEXT NOT NULL,
                ActionType INTEGER NOT NULL,
                Notes TEXT,
                UpdatedAtUtc TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_useractions_noticeid ON UserActions(NoticeId);

            CREATE TABLE IF NOT EXISTS SyncState (
                Id INTEGER PRIMARY KEY CHECK (Id = 1),
                LastSyncAtUtc TEXT,
                LastPostedToDateUsed TEXT,
                LastPostedFromDateUsed TEXT,
                LastError TEXT,
                RequestsLast24Hours INTEGER DEFAULT 0,
                RequestCounterResetAtUtc TEXT
            );

            INSERT OR IGNORE INTO SyncState (Id) VALUES (1);

            CREATE TABLE IF NOT EXISTS RetrievalRules (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Enabled INTEGER NOT NULL DEFAULT 1,
                DaysBack INTEGER NOT NULL DEFAULT 3,
                PType TEXT,
                TitleKeywords TEXT,
                NaicsCodes TEXT,
                ClassificationCodes TEXT,
                AgencyNames TEXT,
                AgencyCodes TEXT,
                SetAsideCodes TEXT,
                State TEXT,
                Zip TEXT,
                ResponseDeadlineFrom TEXT,
                ResponseDeadlineTo TEXT,
                MaxPages INTEGER NOT NULL DEFAULT 10,
                RequestBudget INTEGER NOT NULL DEFAULT 50,
                LastRunAtUtc TEXT
            );

            CREATE TABLE IF NOT EXISTS AppSettings (
                Id INTEGER PRIMARY KEY CHECK (Id = 1),
                EncryptedApiKey TEXT,
                Environment TEXT NOT NULL DEFAULT 'Production',
                DailyRateLimitTier INTEGER NOT NULL DEFAULT 10,
                EnablePrefetch INTEGER NOT NULL DEFAULT 0,
                PrefetchThreshold INTEGER NOT NULL DEFAULT 75,
                EnableNotifications INTEGER NOT NULL DEFAULT 1,
                NotificationThreshold INTEGER NOT NULL DEFAULT 80,
                StructuredWeight REAL NOT NULL DEFAULT 0.4,
                TextWeight REAL NOT NULL DEFAULT 0.6,
                AgencyWeight REAL NOT NULL DEFAULT 1.0
            );

            INSERT OR IGNORE INTO AppSettings (Id) VALUES (1);

            CREATE TABLE IF NOT EXISTS RateLimitRequests (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RequestedAtUtc TEXT NOT NULL,
                Endpoint TEXT
            );

            CREATE INDEX IF NOT EXISTS idx_ratelimit_time ON RateLimitRequests(RequestedAtUtc);
        ";

        using var command = connection.CreateCommand();
        command.CommandText = createTablesScript;
        await command.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
