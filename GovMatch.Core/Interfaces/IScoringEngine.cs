using GovMatch.Core.Models;

namespace GovMatch.Core.Interfaces;

public interface IScoringEngine
{
    MatchResult ScoreOpportunity(CompanyProfile profile, Opportunity opportunity, string? fullTextIfAvailable = null);
}
