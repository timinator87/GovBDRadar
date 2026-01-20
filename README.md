# GovMatch - Government Opportunity Discovery Application

A Windows desktop application for discovering and matching government opportunities from SAM.gov with your company's capabilities.

## Overview

GovMatch automatically ingests opportunities from SAM.gov, scores them against your company profile using hybrid matching (structured + TF-IDF text similarity), and presents them in a prioritized inbox. The application includes rate limiting, scheduled syncs, lazy description fetching, comprehensive opportunity management, daily digests, and profile versioning with automatic re-scoring.

## Features

### Core Features
- **SAM.gov Integration**: Search and retrieve opportunities using the SAM.gov Opportunities v2 API
- **Smart Matching**: Hybrid scoring engine combining:
  - Structured matching (NAICS, PSC, Set-Aside, Agency)
  - TF-IDF text similarity for capability matching
  - Configurable weights and auto-tuning potential
- **Rate Limiting**: Built-in protection against API rate limits with configurable tiers (10/1,000/10,000 requests per day)
- **Lazy Loading**: Descriptions and attachments fetched on-demand to minimize API usage
- **Scheduled Sync**: Quartz.NET-based job scheduling (default: daily at 6:00 AM)
- **Retrieval Rules**: Create custom SAM.gov query templates with filters
- **Export**: CSV export for analysis, Markdown export for individual opportunities
- **Secure Storage**: API keys encrypted using Windows DPAPI
- **User Actions**: Track opportunities as Saved/Pursue/Ignore

### Advanced Features
- **Daily Digest**: Automatic daily summary of new, updated, and newly relevant opportunities
- **Profile Versioning**: Track changes to your company profile with full history
- **Auto Re-Scoring**: When you update your profile, all active opportunities are automatically re-scored
- **Change Detection**: Tracks when SAM.gov opportunities are updated
- **Score Trend Analysis**: See which opportunities became more/less relevant over time
- **Smart Notifications**: Toast notifications for important digest items

## Prerequisites

- **Windows 10 or Windows 11**
- **.NET 8 SDK** (for building from source)
  - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
- **SAM.gov API Key**
  - Obtain from: https://sam.gov/data-services
  - Free tier: 10 requests/day
  - Registered tier: 1,000 requests/day
  - Premium tier: 10,000 requests/day

## Quick Start

### Install on Windows (Easy Method)

1. **Install .NET 8 SDK**: Download from https://dotnet.microsoft.com/download/dotnet/8.0
2. **Clone or download** this repository
3. **Double-click**: `BUILD.bat`

That's it! The script will automatically restore packages, build, test, and launch the application.

**Alternative options:**
- `simple-build.bat` - Same as BUILD.bat, alternative name
- See `WHICH-SCRIPT-TO-USE.txt` for all options

### Create Standalone Executable

**Double-click**: `PUBLISH.bat`

This creates a standalone executable in the `publish` folder (~80-100 MB).

The script will automatically open the publish folder when done. Your .exe will be:
```
publish\GovMatch.App.exe
```

### Manual Installation

See [INSTALL.md](INSTALL.md) for detailed installation instructions.

See [USAGE_GUIDE.md](USAGE_GUIDE.md) for detailed usage scenarios.

See [BUILD.md](BUILD.md) for advanced build and deployment options.

## Solution Structure

```
GovMatch/
├── GovMatch.Core/              # Domain models, interfaces, scoring engine
├── GovMatch.Data/              # SQLite persistence layer
├── GovMatch.Integrations.SamGov/  # SAM.gov API client
├── GovMatch.App/               # WPF UI application
└── GovMatch.Tests/             # xUnit tests
```

## Documentation

- **[README.md](README.md)** - This file (overview)
- **[INSTALL.md](INSTALL.md)** - Windows installation guide
- **[USAGE_GUIDE.md](USAGE_GUIDE.md)** - Detailed user guide
- **[BUILD.md](BUILD.md)** - Build and deployment instructions

## License

This project is provided as-is for educational and commercial use.

## Version

**v1.1.0** - Added Daily Digest, Profile Versioning, and Change Detection
