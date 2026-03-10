using Quartz;

namespace BackendApi.Jobs;

/// <summary>
/// Nightly job that recalculates CurrentQuantity and EstimatedDepletionDate
/// for all active inventories based on InventoryEvent history and AvgConsumptionRate.
/// </summary>
[DisallowConcurrentExecution]
public class InventoryRecalculationJob : IJob
{
    private readonly ILogger<InventoryRecalculationJob> _logger;

    public InventoryRecalculationJob(ILogger<InventoryRecalculationJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("InventoryRecalculationJob started at {Time}", DateTimeOffset.UtcNow);

        // TODO: Implement inventory recalculation logic

        _logger.LogInformation("InventoryRecalculationJob completed at {Time}", DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }
}
