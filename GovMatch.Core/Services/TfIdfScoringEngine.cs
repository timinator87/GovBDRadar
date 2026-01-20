using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using GovMatch.Core.Interfaces;
using GovMatch.Core.Models;

namespace GovMatch.Core.Services;

public class TfIdfScoringEngine : IScoringEngine
{
    private readonly double _structuredWeight;
    private readonly double _textWeight;
    private readonly double _agencyWeight;

    public TfIdfScoringEngine(double structuredWeight = 0.4, double textWeight = 0.6, double agencyWeight = 1.0)
    {
        _structuredWeight = structuredWeight;
        _textWeight = textWeight;
        _agencyWeight = agencyWeight;
    }

    public MatchResult ScoreOpportunity(CompanyProfile profile, Opportunity opportunity, string? fullTextIfAvailable = null)
    {
        var breakdown = new ScoreBreakdown();

        var naicsScore = CalculateNaicsScore(profile, opportunity);
        breakdown.Factors.Add(new ScoreFactor
        {
            Name = "NAICS Match",
            Points = naicsScore.Points,
            MaxPoints = 15,
            Details = naicsScore.Details
        });

        var pscScore = CalculatePscScore(profile, opportunity);
        breakdown.Factors.Add(new ScoreFactor
        {
            Name = "PSC/Classification Match",
            Points = pscScore.Points,
            MaxPoints = 10,
            Details = pscScore.Details
        });

        var setAsideScore = CalculateSetAsideScore(profile, opportunity);
        breakdown.Factors.Add(new ScoreFactor
        {
            Name = "Set-Aside Match",
            Points = setAsideScore.Points,
            MaxPoints = 10,
            Details = setAsideScore.Details
        });

        var agencyScore = CalculateAgencyScore(profile, opportunity);
        breakdown.Factors.Add(new ScoreFactor
        {
            Name = "Target Agency Match",
            Points = agencyScore.Points,
            MaxPoints = 5,
            Details = agencyScore.Details
        });

        var structuredTotal = naicsScore.Points + pscScore.Points + setAsideScore.Points + agencyScore.Points;
        var structuredNormalized = (int)(structuredTotal * _structuredWeight);

        var textScore = CalculateTextSimilarity(profile, opportunity, fullTextIfAvailable);
        breakdown.Factors.Add(new ScoreFactor
        {
            Name = "Text Similarity",
            Points = textScore.Points,
            MaxPoints = 60,
            Details = textScore.Details
        });

        var textNormalized = (int)(textScore.Points * _textWeight);

        var totalScore = Math.Min(100, structuredNormalized + textNormalized);
        breakdown.TotalScore = totalScore;

        return new MatchResult
        {
            NoticeId = opportunity.NoticeId,
            Score = totalScore,
            ScoreBreakdownJson = JsonSerializer.Serialize(breakdown),
            ScoredAtUtc = DateTime.UtcNow
        };
    }

    private (int Points, string Details) CalculateNaicsScore(CompanyProfile profile, Opportunity opportunity)
    {
        var profileNaics = profile.GetNaicsCodes().ToHashSet();
        if (profileNaics.Count == 0 || string.IsNullOrWhiteSpace(opportunity.NaicsCode))
            return (0, "No NAICS codes to compare");

        var oppNaics = opportunity.NaicsCode.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var matches = oppNaics.Where(n => profileNaics.Any(p => n.StartsWith(p) || p.StartsWith(n))).ToList();

        if (matches.Any())
        {
            var points = Math.Min(15, matches.Count * 8);
            return (points, $"Matched NAICS: {string.Join(", ", matches)}");
        }

        return (0, "No NAICS match");
    }

    private (int Points, string Details) CalculatePscScore(CompanyProfile profile, Opportunity opportunity)
    {
        var profilePsc = profile.GetPscCodes().ToHashSet();
        if (profilePsc.Count == 0 || string.IsNullOrWhiteSpace(opportunity.ClassificationCode))
            return (0, "No PSC codes to compare");

        var oppPsc = opportunity.ClassificationCode.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var matches = oppPsc.Where(p => profilePsc.Contains(p)).ToList();

        if (matches.Any())
        {
            var points = Math.Min(10, matches.Count * 5);
            return (points, $"Matched PSC: {string.Join(", ", matches)}");
        }

        return (0, "No PSC match");
    }

    private (int Points, string Details) CalculateSetAsideScore(CompanyProfile profile, Opportunity opportunity)
    {
        var preferences = profile.GetSetAsidePreferences().ToHashSet();
        if (preferences.Count == 0)
            return (0, "No set-aside preferences configured");

        if (string.IsNullOrWhiteSpace(opportunity.SetAsideCode))
            return (0, "Opportunity has no set-aside");

        if (preferences.Contains(opportunity.SetAsideCode))
            return (10, $"Matched set-aside: {opportunity.SetAsideDescription ?? opportunity.SetAsideCode}");

        return (0, "Set-aside does not match preferences");
    }

    private (int Points, string Details) CalculateAgencyScore(CompanyProfile profile, Opportunity opportunity)
    {
        var targetCodes = profile.GetTargetAgencyCodes().ToHashSet();
        if (targetCodes.Count == 0)
            return (0, "No target agencies configured");

        if (!string.IsNullOrWhiteSpace(opportunity.AgencyPathCode))
        {
            var oppCodes = opportunity.AgencyPathCode.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (oppCodes.Any(c => targetCodes.Contains(c)))
            {
                var points = (int)(5 * _agencyWeight);
                return (points, $"Matched agency: {opportunity.AgencyPathName ?? opportunity.AgencyPathCode}");
            }
        }

        return (0, "No agency match");
    }

    private (int Points, string Details) CalculateTextSimilarity(CompanyProfile profile, Opportunity opportunity, string? fullText)
    {
        var profileText = profile.GetCombinedProfileText();
        if (string.IsNullOrWhiteSpace(profileText))
            return (0, "No profile text to compare");

        var opportunityText = fullText ?? opportunity.Title ?? "";
        if (string.IsNullOrWhiteSpace(opportunityText))
            return (0, "No opportunity text to compare");

        var profileTerms = Tokenize(profileText);
        var oppTerms = Tokenize(opportunityText);

        var profileVector = BuildTfIdfVector(profileTerms);
        var oppVector = BuildTfIdfVector(oppTerms);

        var similarity = CosineSimilarity(profileVector, oppVector);

        var points = (int)(similarity * similarity * 60);

        var topMatches = FindTopMatchingTerms(profileVector, oppVector, 5);
        var details = topMatches.Any()
            ? $"Similarity: {similarity:P0}. Top terms: {string.Join(", ", topMatches)}"
            : $"Similarity: {similarity:P0}";

        return (points, details);
    }

    private List<string> Tokenize(string text)
    {
        var normalized = text.ToLowerInvariant();
        var words = Regex.Matches(normalized, @"\b[a-z]{3,}\b")
            .Select(m => m.Value)
            .Where(w => !IsStopWord(w))
            .ToList();
        return words;
    }

    private bool IsStopWord(string word)
    {
        var stopWords = new HashSet<string> { "the", "and", "for", "that", "this", "with", "from", "have", "will", "are", "can", "not", "was", "were", "been", "being", "has", "had", "does", "did", "but", "his", "her", "its", "their", "than", "then", "there", "here", "when", "where", "why", "how", "all", "each", "any", "some", "such" };
        return stopWords.Contains(word);
    }

    private Dictionary<string, double> BuildTfIdfVector(List<string> terms)
    {
        var termCounts = new Dictionary<string, int>();
        foreach (var term in terms)
        {
            termCounts[term] = termCounts.GetValueOrDefault(term) + 1;
        }

        var vector = new Dictionary<string, double>();
        foreach (var kvp in termCounts)
        {
            var tf = (double)kvp.Value / terms.Count;
            var idf = 1.0;
            vector[kvp.Key] = tf * idf;
        }

        return vector;
    }

    private double CosineSimilarity(Dictionary<string, double> v1, Dictionary<string, double> v2)
    {
        var allTerms = v1.Keys.Union(v2.Keys).ToList();
        if (allTerms.Count == 0) return 0;

        var dotProduct = allTerms.Sum(t => v1.GetValueOrDefault(t) * v2.GetValueOrDefault(t));
        var mag1 = Math.Sqrt(v1.Values.Sum(v => v * v));
        var mag2 = Math.Sqrt(v2.Values.Sum(v => v * v));

        if (mag1 == 0 || mag2 == 0) return 0;

        return dotProduct / (mag1 * mag2);
    }

    private List<string> FindTopMatchingTerms(Dictionary<string, double> v1, Dictionary<string, double> v2, int count)
    {
        return v1.Keys.Intersect(v2.Keys)
            .OrderByDescending(t => v1[t] * v2[t])
            .Take(count)
            .ToList();
    }
}
