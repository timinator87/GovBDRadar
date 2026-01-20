-- Migration: Add Digest and Profile Versioning Features

-- Add new columns to Opportunities table
ALTER TABLE Opportunities ADD COLUMN ContentHash TEXT;
ALTER TABLE Opportunities ADD COLUMN UpdatedAtUtc TEXT;
ALTER TABLE Opportunities ADD COLUMN LastFetchedAtUtc TEXT;

-- Add new column to CompanyProfile
ALTER TABLE CompanyProfile ADD COLUMN CurrentVersionId INTEGER;

-- Create ProfileVersions table
CREATE TABLE IF NOT EXISTS ProfileVersions (
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
);

-- Create OpportunityEvents table
CREATE TABLE IF NOT EXISTS OpportunityEvents (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    NoticeId TEXT NOT NULL,
    EventType INTEGER NOT NULL,
    OldScore INTEGER,
    NewScore INTEGER,
    OccurredAtUtc TEXT NOT NULL,
    ProfileVersionId INTEGER,
    FOREIGN KEY (NoticeId) REFERENCES Opportunities(NoticeId),
    FOREIGN KEY (ProfileVersionId) REFERENCES ProfileVersions(Id)
);

CREATE INDEX IF NOT EXISTS idx_opportunityevents_noticeid ON OpportunityEvents(NoticeId);
CREATE INDEX IF NOT EXISTS idx_opportunityevents_occurred ON OpportunityEvents(OccurredAtUtc);
CREATE INDEX IF NOT EXISTS idx_opportunityevents_type ON OpportunityEvents(EventType);

-- Create Digests table
CREATE TABLE IF NOT EXISTS Digests (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    GeneratedAtUtc TEXT NOT NULL,
    ProfileVersionId INTEGER,
    DigestJson TEXT,
    SentVia INTEGER NOT NULL DEFAULT 0,
    LastDigestCutoffUtc TEXT,
    FOREIGN KEY (ProfileVersionId) REFERENCES ProfileVersions(Id)
);

CREATE INDEX IF NOT EXISTS idx_digests_generated ON Digests(GeneratedAtUtc DESC);

-- Create index on Opportunities.ContentHash for faster lookups
CREATE INDEX IF NOT EXISTS idx_opportunities_contenthash ON Opportunities(ContentHash);
