using BackendApi.Services;
using Quartz;

namespace BackendApi.Jobs;

/// <summary>
/// Nightly job that recalculates CurrentQuantity and EstimatedDepletionDate
/// for all active inventories based on InventoryEvent history and AvgConsumptionRate.
/// </summary>
[DisallowConcurrentExecution]
public class InventoryRecalculationJob : IJob
{
    private readonly InventoryRecalculationService _recalculationService;
    private readonly ILogger<InventoryRecalculationJob> _logger;

    public InventoryRecalculationJob(
        InventoryRecalculationService recalculationService,
        ILogger<InventoryRecalculationJob> logger)
    {
        _recalculationService = recalculationService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("InventoryRecalculationJob started at {Time}", DateTimeOffset.UtcNow);

        await _recalculationService.RecalculateAllAsync(context.CancellationToken);

        _logger.LogInformation("InventoryRecalculationJob completed at {Time}", DateTimeOffset.UtcNow);
    }
}
