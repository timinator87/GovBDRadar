using GovMatch.Core.Models;
using GovMatch.Data.Repositories;

namespace GovMatch.Core.Services;

public class ProfileVersioningService
{
    private readonly ProfileVersionRepository _versionRepo;
    private readonly CompanyProfileRepository _profileRepo;

    public ProfileVersioningService(
        ProfileVersionRepository versionRepo,
        CompanyProfileRepository profileRepo)
    {
        _versionRepo = versionRepo;
        _profileRepo = profileRepo;
    }

    public async Task<ProfileVersion> CreateSnapshotAsync(CompanyProfile profile)
    {
        var snapshot = new ProfileVersion
        {
            CreatedAtUtc = DateTime.UtcNow,
            DisplayNameSnapshot = profile.DisplayName,
            CapabilityTextSnapshot = profile.CapabilityText,
            KeywordsSnapshot = profile.KeywordsCsv,
            NaicsSnapshot = profile.NaicsCsv,
            PscSnapshot = profile.PscCsv,
            TargetAgencyCodesSnapshot = profile.TargetAgencyCodesCsv,
            SetAsidePreferencesSnapshot = profile.SetAsidePreferencesCsv,
            PastPerformanceTextSnapshot = profile.PastPerformanceText
        };

        snapshot.Id = await _versionRepo.InsertAsync(snapshot);

        profile.CurrentVersionId = snapshot.Id;
        await _profileRepo.UpdateProfileAsync(profile);

        return snapshot;
    }

    public async Task<ProfileVersion?> GetCurrentVersionAsync()
    {
        var profile = await _profileRepo.GetProfileAsync();
        if (profile.CurrentVersionId == null)
            return null;

        return await _versionRepo.GetByIdAsync(profile.CurrentVersionId.Value);
    }

    public CompanyProfile SnapshotToProfile(ProfileVersion snapshot)
    {
        return new CompanyProfile
        {
            Id = 1,
            DisplayName = snapshot.DisplayNameSnapshot,
            CapabilityText = snapshot.CapabilityTextSnapshot,
            KeywordsCsv = snapshot.KeywordsSnapshot,
            NaicsCsv = snapshot.NaicsSnapshot,
            PscCsv = snapshot.PscSnapshot,
            TargetAgencyCodesCsv = snapshot.TargetAgencyCodesSnapshot,
            SetAsidePreferencesCsv = snapshot.SetAsidePreferencesSnapshot,
            PastPerformanceText = snapshot.PastPerformanceTextSnapshot
        };
    }
}
