using Team1Abr.Core;
using Xunit;

namespace Team1Abr.Tests;

public class LookupBrowseTests
{
    [Fact]
    public void AllRecords_ReturnsEveryFixtureRecord()
    {
        Assert.Equal(4, Lookup.AllRecords().Count);
    }

    [Fact]
    public void FilterByState_ReturnsOnlyMatchingRecords()
    {
        var vic = Lookup.FilterByState("VIC");

        Assert.All(vic, record => Assert.Equal("VIC", record.State));
        Assert.NotEmpty(vic);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("All")]
    public void FilterByState_ReturnsAllRecords_ForNullEmptyOrAll(string? state)
    {
        Assert.Equal(4, Lookup.FilterByState(state).Count);
    }

    [Fact]
    public void FilterByState_IsCaseInsensitive()
    {
        Assert.Equal(Lookup.FilterByState("VIC").Count, Lookup.FilterByState("vic").Count);
        Assert.NotEmpty(Lookup.FilterByState("vic"));
    }

    [Fact]
    public void DistinctStates_ReturnsSortedUniqueStates()
    {
        Assert.Equal(new[] { "ACT", "NSW", "QLD", "VIC" }, Lookup.DistinctStates());
    }
}
