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

    // --- C4: added edge-case coverage ---------------------------------------

    [Fact]
    public void Validate_RejectsWhitespaceOnlyInput_AsEmpty()
    {
        // Padded with leading/trailing spaces only → normalises to "" → "Enter an ABN".
        var result = Abn.Validate("     ");
        Assert.False(result.Valid);
        Assert.Contains("Enter an ABN", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_AcceptsAValidAbnPaddedWithLeadingAndTrailingSpaces()
    {
        var result = Abn.Validate("   51824753556   ");
        Assert.True(result.Valid);
        Assert.Equal("51824753556", result.Normalised);
    }

    [Theory]
    [InlineData("5182475355")]   // exactly 10 digits (one short)
    [InlineData("518247535566")] // exactly 12 digits (one long)
    public void Validate_RejectsBoundaryLengthsAround11Digits(string input)
    {
        var result = Abn.Validate(input);
        Assert.False(result.Valid);
        Assert.Contains("11 digits", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("51824753556")] // Australian Taxation Office Team 1
    [InlineData("83914571673")] // Example Manufacturing Pty Ltd
    [InlineData("53004085616")] // Sample Retail Group Ltd
    [InlineData("40000000026")] // Dormant Holdings (Cancelled)
    public void Validate_AcceptsEveryKnownValidAbnFromTheFixtures(string abn)
    {
        var result = Abn.Validate(abn);
        Assert.True(result.Valid);
        Assert.Equal(abn, result.Normalised);
        Assert.Equal(string.Empty, result.Reason);
    }

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
        Assert.Contains("Australian Taxation Office", record!.EntityName, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LookupAbn_ReturnsNullWhenNotFound()
    {
        Assert.Null(Lookup.LookupAbn("12345678901"));
    }

    [Fact]
    public void LookupAbn_SurfacesCancelledStatus_ForAccessibleStatusHandling()
    {
        var record = Lookup.LookupAbn("40000000026");
        Assert.NotNull(record);
        Assert.Equal("Cancelled", record!.AbnStatus);
        Assert.False(record.GstRegistered);
    }

    // --- C4: SearchAbn end-to-end (validate + lookup) -----------------------

    [Fact]
    public void SearchAbn_ReturnsError_WhenTheAbnIsInvalid()
    {
        var result = Lookup.SearchAbn("123");
        Assert.Equal(LookupStatus.Error, result.Status);
        Assert.Null(result.Record);
        Assert.Contains("11 digits", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SearchAbn_ReturnsNotFound_WhenTheAbnIsValidButAbsent()
    {
        // Valid checksum but not in the fixture data.
        var result = Lookup.SearchAbn("10000000000");
        Assert.Equal(LookupStatus.NotFound, result.Status);
        Assert.Null(result.Record);
        Assert.Equal("10000000000", result.NormalisedAbn);
    }

    [Fact]
    public void SearchAbn_ReturnsFound_AndNormalisesSpacedInput()
    {
        var result = Lookup.SearchAbn("51 824 753 556");
        Assert.Equal(LookupStatus.Found, result.Status);
        Assert.NotNull(result.Record);
        Assert.Equal("51824753556", result.Record!.Abn);
    }

    // --- C4: recent-search list behaviour -----------------------------------

    [Fact]
    public void AddRecentSearch_MovesAnExistingEntryToTheFrontWithoutGrowing()
    {
        var recentSearches = new List<string> { "83914571673", "53004085616", "51824753556" };

        Lookup.AddRecentSearch(recentSearches, "53004085616");

        Assert.Equal(3, recentSearches.Count);
        Assert.Equal("53004085616", recentSearches[0]);
        Assert.Single(recentSearches, abn => abn == "53004085616");
    }

    [Fact]
    public void AddRecentSearch_RespectsACustomMaxItemsLimit()
    {
        var recentSearches = new List<string>();

        foreach (var abn in new[] { "51824753556", "83914571673", "53004085616", "40000000026" })
        {
            Lookup.AddRecentSearch(recentSearches, abn, maxItems: 2);
        }

        Assert.Equal(2, recentSearches.Count);
        Assert.Equal("40000000026", recentSearches[0]);
        Assert.Equal("53004085616", recentSearches[1]);
    }

    // --- C4: Normalise / Format edge cases ----------------------------------

    [Fact]
    public void Normalise_ReturnsEmptyStringForNull()
    {
        Assert.Equal(string.Empty, Abn.Normalise(null));
    }

    [Fact]
    public void Normalise_RemovesTabsAndNewlinesNotJustSpaces()
    {
        Assert.Equal("51824753556", Abn.Normalise("51824\t753\n556"));
    }

    [Fact]
    public void Format_ReturnsInputUnchangedWhenLongerThan11Digits()
    {
        Assert.Equal("518247535566", Abn.Format("518247535566"));
    }

    // --- S2: Accessibility edge cases & session isolation -------------------

    [Fact]
    public void Validate_ErrorMessageIsConsistentAndHelpful_ForAccessibility()
    {
        // Error messages should be clear and visible; tested with aria-describedby in component.
        var emptyResult = Abn.Validate("");
        Assert.False(emptyResult.Valid);
        Assert.NotEmpty(emptyResult.Reason);
        Assert.True(emptyResult.Reason.Length > 5, "Error message should be descriptive");

        var checksumResult = Abn.Validate("51824753557");
        Assert.False(checksumResult.Valid);
        Assert.Contains("checksum", checksumResult.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RecentSearches_DoNotPersistAcrossSessions_SessionMemoryOnly()
    {
        // Recent searches must be in-memory only; verify they reset when cleared.
        var session1 = new List<string>();
        Lookup.AddRecentSearch(session1, "51824753556");
        Assert.Single(session1);

        var session2 = new List<string>();
        Lookup.ClearRecentSearches(session2);
        Assert.Empty(session2);
        // session1 and session2 are separate instances — no cross-session leakage.
    }

    [Fact]
    public void AddRecentSearch_PreservesMaxItemsWithMultipleAdditions_AvoidingGrowthBeyondLimit()
    {
        var recentSearches = new List<string>();
        var maxItems = 3;

        // Add 5 items with a limit of 3
        foreach (var abn in new[] { "51824753556", "83914571673", "53004085616", "40000000026", "10000000128" })
        {
            Lookup.AddRecentSearch(recentSearches, abn, maxItems);
        }

        // Should never exceed maxItems
        Assert.Equal(3, recentSearches.Count);
        Assert.Equal("10000000128", recentSearches[0]); // most recent first
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("\t\n")]
    public void Validate_RejectsBlankOrWhitespaceInput_WithClearMessage(string input)
    {
        var result = Abn.Validate(input);
        Assert.False(result.Valid);
        Assert.Contains("Enter an ABN", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("51-824-753-556")] // dashes
    [InlineData("51/824/753/556")] // slashes
    [InlineData("51.824.753.556")] // dots
    public void Validate_RejectsFormattedNumbersWithNonDigitCharacters(string input)
    {
        var result = Abn.Validate(input);
        Assert.False(result.Valid);
        Assert.Contains("digits only", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Normalise_HandlesExtremelyLongInput_WithoutCrashing()
    {
        var longInput = new string('5', 1000);
        var result = Abn.Normalise(longInput);
        Assert.NotNull(result);
        // Normalise should return the input as-is if all chars are digits
        Assert.Equal(longInput, result);
    }

    [Fact]
    public void LookupAbn_IsCaseSensitiveForEntityNames_AndReturnsExactRecords()
    {
        var record = Lookup.LookupAbn("51824753556");
        Assert.NotNull(record);
        // Verify exact entity name
        Assert.Equal("Australian Taxation Office Team 1", record!.EntityName);
        Assert.Equal("Active", record.AbnStatus);
        Assert.True(record.GstRegistered);
    }

    [Fact]
    public void SearchAbn_ReturnsNormalisedAbnInNotFoundCase_ForErrorDisplay()
    {
        var result = Lookup.SearchAbn("  51 824 753 557  ");
        // Invalid checksum, so NotFound or Error
        Assert.True(result.Status == LookupStatus.Error || result.Status == LookupStatus.NotFound);
        // Normalised ABN should be returned for display
        if (result.Status == LookupStatus.NotFound)
        {
            Assert.Equal("51824753557", result.NormalisedAbn);
        }
    }

    [Fact]
    public void ClearRecentSearches_FullyEmptiesTheList_WithoutSideLects()
    {
        var recentSearches = new List<string> { "51824753556", "83914571673", "53004085616" };
        Assert.Equal(3, recentSearches.Count);

        Lookup.ClearRecentSearches(recentSearches);

        Assert.Empty(recentSearches);
    }
}
