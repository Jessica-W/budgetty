using System.Diagnostics.CodeAnalysis;

namespace Budgetty.Mvc;

[ExcludeFromCodeCoverage]
public class AppInsightsConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableQuickPulseMetricStream { get; set; }
    public bool EnableEventCounterCollectionModule { get; set; }
    public bool EnablePerformanceCounterCollectionModule { get; set; }
    public bool EnableHeartbeat { get; set; }
}