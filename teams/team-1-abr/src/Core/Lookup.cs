using System.Reflection;
using System.Text.Json;

namespace Team1Abr.Core;

public enum LookupStatus
{
    Idle,
    Error,
    NotFound,
    Found,
}

public sealed record LookupResult(LookupStatus Status, string Reason, string NormalisedAbn, BusinessRecord? Record)
{
    public static LookupResult Invalid(string reason) => new(LookupStatus.Error, reason, string.Empty, null);
    public static LookupResult NotFound(string normalisedAbn) => new(LookupStatus.NotFound, string.Empty, normalisedAbn, null);
    public static LookupResult Found(BusinessRecord record) => new(LookupStatus.Found, string.Empty, record.Abn, record);
}

/// <summary>
/// Lookup of ABN details from LOCAL sample data only.
///
/// IMPORTANT: This does NOT call the live Australian Business Register
/// (abr.business.gov.au). For the hackathon we use a small static fixture so the
/// app and tests are fast, deterministic, and safe. Replacing this with a real
/// API call (with proper validation, error handling, and rate limiting) could be
/// a follow-up beyond the event.
/// </summary>
public static class Lookup
{
    private static readonly IReadOnlyList<BusinessRecord> Records = LoadRecords();

    /// <summary>
    /// Find a business by its normalised 11-digit ABN. Returns null when not found.
    /// Assumes the ABN has already passed <see cref="Abn.Validate"/>.
    /// </summary>
    public static BusinessRecord? LookupAbn(string normalisedAbn) =>
        Records.FirstOrDefault(record => record.Abn == normalisedAbn);

    /// <summary>All business records from the local sample data.</summary>
    public static IReadOnlyList<BusinessRecord> AllRecords() => Records;

    /// <summary>
    /// Records whose state matches <paramref name="state"/>. A null/empty value or
    /// "All" (case-insensitive) returns every record. Matching is case-insensitive.
    /// </summary>
    public static IReadOnlyList<BusinessRecord> FilterByState(string? state)
    {
        if (string.IsNullOrWhiteSpace(state) ||
            string.Equals(state, "All", StringComparison.OrdinalIgnoreCase))
        {
            return Records;
        }

        return Records
            .Where(record => string.Equals(record.State, state, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>The sorted, unique set of states present in the sample data.</summary>
    public static IReadOnlyList<string> DistinctStates() =>
        Records
            .Select(record => record.State)
            .Distinct()
            .OrderBy(state => state, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Validate a user-entered ABN and look up the matching record from the local sample data.
    /// </summary>
    public static LookupResult SearchAbn(string input)
    {
        var result = Abn.Validate(input);
        if (!result.Valid)
        {
            return LookupResult.Invalid(result.Reason);
        }

        var record = LookupAbn(result.Normalised);
        return record is null
            ? LookupResult.NotFound(result.Normalised)
            : LookupResult.Found(record);
    }

    public static void AddRecentSearch(IList<string> recentSearches, string normalisedAbn, int maxItems = 5)
    {
        if (recentSearches.Contains(normalisedAbn))
        {
            recentSearches.Remove(normalisedAbn);
        }

        recentSearches.Insert(0, normalisedAbn);

        if (recentSearches.Count > maxItems)
        {
            recentSearches.RemoveAt(recentSearches.Count - 1);
        }
    }

    public static void ClearRecentSearches(ICollection<string> recentSearches) => recentSearches.Clear();

    private static IReadOnlyList<BusinessRecord> LoadRecords()
    {
        var assembly = typeof(Lookup).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .Single(name => name.EndsWith("abn-sample-data.json", StringComparison.Ordinal));

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("Embedded sample data not found.");

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<List<BusinessRecord>>(stream, options) ?? [];
    }
}

/// <summary>A single business record from the sample data.</summary>
public sealed record BusinessRecord(
    string Abn,
    string EntityName,
    string EntityType,
    string AbnStatus,
    string State,
    string Postcode,
    bool GstRegistered);
