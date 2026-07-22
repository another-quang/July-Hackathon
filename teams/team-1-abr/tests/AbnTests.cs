using Team1Abr.Core;
using Xunit;

namespace Team1Abr.Tests;

/// <summary>
/// Starter unit tests for ABN validation.
///
/// These give you a working baseline. During the hackathon (Objective C4), use
/// Copilot + the <c>generate-unit-tests</c> prompt to ADD more edge cases and to
/// critique/strengthen these. Look for the TODOs.
/// </summary>
public class AbnTests
{
    [Fact]
    public void Validate_AcceptsAValidAbn()
    {
        var result = Abn.Validate("51824753556");
        Assert.True(result.Valid);
        Assert.Equal("51824753556", result.Normalised);
    }

    [Fact]
    public void AddRecentSearch_KeepsTheMostRecentFiveAndMovesDuplicatesToTheFront()
    {
        var recentSearches = new List<string>();

        foreach (var abn in new[] { "51824753556", "51824753557", "51824753558", "51824753559", "51824753550", "51824753551" })
        {
            Lookup.AddRecentSearch(recentSearches, abn);
        }

        Assert.Equal(5, recentSearches.Count);
        Assert.Equal("51824753551", recentSearches[0]);
        Assert.DoesNotContain("51824753556", recentSearches);
    }

    [Fact]
    public void ClearRecentSearches_RemovesAllEntries()
    {
        var recentSearches = new List<string>();
        Lookup.AddRecentSearch(recentSearches, "51824753556");

        Lookup.ClearRecentSearches(recentSearches);

        Assert.Empty(recentSearches);
    }

    [Fact]
    public void Validate_AcceptsAValidAbnThatContainsSpaces()
    {
        var result = Abn.Validate("51 824 753 556");
        Assert.True(result.Valid);
    }

    [Fact]
    public void Validate_RejectsAnEmptyString()
    {
        var result = Abn.Validate("");
        Assert.False(result.Valid);
        Assert.Contains("Enter an ABN", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_RejectsAnAbnThatIsNot11Digits()
    {
        var result = Abn.Validate("123");
        Assert.False(result.Valid);
        Assert.Contains("11 digits", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_RejectsAnAbnContainingLetters()
    {
        var result = Abn.Validate("5182475355A");
        Assert.False(result.Valid);
        Assert.Contains("digits only", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_RejectsAn11DigitNumberThatFailsTheChecksum()
    {
        // Valid ABN with its last digit changed → checksum should fail.
        var result = Abn.Validate("51824753557");
        Assert.False(result.Valid);
        Assert.Contains("checksum", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_RejectsNullInput_UntrustedBoundary()
    {
        // Validate accepts a nullable string and validates the type itself — the
        // C# equivalent of guarding against non-string input.
        var result = Abn.Validate(null);
        Assert.False(result.Valid);
    }

    // TODO (C4): with Copilot, add tests for:
    //  - an ABN padded with leading/trailing spaces only
    //  - the boundary case of exactly 10 and exactly 12 digits
    //  - at least one MORE known-valid ABN from the fixtures

    [Fact]
    public void Normalise_RemovesAllWhitespace()
    {
        Assert.Equal("51824753556", Abn.Normalise("  51 824 753 556 "));
    }

    [Fact]
    public void Format_FormatsAn11DigitAbn()
    {
        Assert.Equal("51 824 753 556", Abn.Format("51824753556"));
    }

    [Fact]
    public void Format_ReturnsInputUnchangedWhenNot11Digits()
    {
        Assert.Equal("123", Abn.Format("123"));
    }

    [Fact]
    public void LookupAbn_FindsAKnownRecordFromTheFixture()
    {
        var record = Lookup.LookupAbn("51824753556");
        Assert.NotNull(record);
        Assert.Equal("Australian Taxation Office", record!.EntityName);
    }

    [Fact]
    public void LookupAbn_ReturnsNullWhenNotFound()
    {
        Assert.Null(Lookup.LookupAbn("12345678901"));
    }
}
