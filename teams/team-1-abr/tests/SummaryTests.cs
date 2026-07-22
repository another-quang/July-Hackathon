using Team1Abr.Core;
using Xunit;

namespace Team1Abr.Tests;

public class SummaryTests
{
    private static BusinessRecord Active() => new(
        "51824753556", "Australian Taxation Office Team 1",
        "Commonwealth Government Entity", "Active", "ACT", "2600", true);

    private static BusinessRecord Cancelled() => new(
        "40000000026", "Dormant Holdings (Deregistered)",
        "Australian Private Company", "Cancelled", "QLD", "4000", false);

    [Fact]
    public void Generate_ActiveGstRecord_DescribesActiveAndGstRegistered()
    {
        var text = Summary.Generate(Active());

        Assert.Contains("Australian Taxation Office Team 1", text);
        Assert.Contains("Commonwealth Government Entity", text);
        Assert.Contains("currently active", text);
        Assert.Contains("registered for GST", text);
    }

    [Fact]
    public void Generate_CancelledRecord_LeadsWithCautionAndNoGst()
    {
        var text = Summary.Generate(Cancelled());

        Assert.Contains("cancelled", text, System.StringComparison.OrdinalIgnoreCase);
        Assert.Contains("may no longer be trading", text);
        Assert.Contains("not registered for GST", text);
    }

    [Theory]
    [InlineData("51824753556")]
    [InlineData("83914571673")]
    [InlineData("53004085616")]
    [InlineData("40000000026")]
    public void Generate_EveryFixtureRecord_ProducesTextNamingTheEntity(string abn)
    {
        var record = Lookup.LookupAbn(abn);
        Assert.NotNull(record);

        var text = Summary.Generate(record!);

        Assert.False(string.IsNullOrWhiteSpace(text));
        Assert.Contains(record!.EntityName, text);
    }
}
