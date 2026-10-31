using TestDeIa.Domain.Modules.Saas.Services;

namespace TestDeIa.Tests.Saas;

public sealed class SaasConsumptionAnalysisTests
{
    [Fact]
    public void SuggestedPlanNeverRemovesModulesOrUnlimitedCapacity()
    {
        var current = new SaasConsumptionAnalysis.Capacity(100, 0, 5, true, true);
        Assert.False(SaasConsumptionAnalysis.CanUpgrade(current, new(1000, 15, 5, true, true), 200));
        Assert.False(SaasConsumptionAnalysis.CanUpgrade(current, new(1000, 0, 5, true, false), 200));
        Assert.False(SaasConsumptionAnalysis.CanUpgrade(current, new(1000, 0, 3, true, true), 200));
        Assert.False(SaasConsumptionAnalysis.CanUpgrade(current, new(150, 0, 5, true, true), 200));
        Assert.True(SaasConsumptionAnalysis.CanUpgrade(current, new(1000, 0, 5, true, true), 200));
        Assert.True(SaasConsumptionAnalysis.CanUpgrade(current, new(0, 0, 0, true, true), 2000));
        Assert.False(SaasConsumptionAnalysis.CanUpgrade(current with { Invoices = 0 }, new(1000, 0, 0, true, true), 200));
    }

    [Theory]
    [InlineData(80, 0, 100, 10, 20, 15, true)]
    [InlineData(1, 79, 100, 10, 20, 15, true)]
    [InlineData(100, 1, 100, 10, 20, 15, true)]
    [InlineData(10, 0, 100, 50, 20, 7, true)]
    [InlineData(1, 0, 100, 10, 20, 2, false)]
    [InlineData(500, 20, 0, 200, 20, 15, false)]
    [InlineData(10, 0, 100, 10, 20, 15, false)]
    public void RecommendationUsesOccupancyOrMatureProjection(int used, int reserved, int limit, int authorized, int previous, int day, bool expected)
        => Assert.Equal(expected, SaasConsumptionAnalysis.Evaluate(used, reserved, limit, authorized, previous, day, 31).SuggestUpgrade);

    [Fact]
    public void GrowthComparesForecastToCompletePreviousMonthAndAvoidsDivisionByZero()
    {
        var result = SaasConsumptionAnalysis.Evaluate(10, 0, 100, 20, 20, 15, 30);
        Assert.Equal(40, result.Forecast);
        Assert.Equal(100m, result.Growth);
        Assert.Null(SaasConsumptionAnalysis.Evaluate(1, 0, 100, 20, 0, 15, 30).Growth);
        Assert.Null(SaasConsumptionAnalysis.Evaluate(1, 0, 100, 20, 20, 2, 30).Growth);
    }

    [Fact]
    public void UnlimitedAndHighCountersDoNotOverflow()
    {
        var result = SaasConsumptionAnalysis.Evaluate(int.MaxValue, int.MaxValue, 0, int.MaxValue, 1, 1, 31);
        Assert.Equal(int.MaxValue, result.Forecast);
        Assert.Null(result.Usage);
        Assert.False(result.SuggestUpgrade);
    }
}
