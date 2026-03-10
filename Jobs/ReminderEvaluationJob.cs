using Quartz;

namespace BackendApi.Jobs;

/// <summary>
/// Runs after InventoryRecalculationJob to evaluate trigger conditions
/// and create Reminder entities for inventories that meet LOW_STOCK,
/// EXPIRING, or DEPLETION_ESTIMATED thresholds.
/// </summary>
[DisallowConcurrentExecution]
public class ReminderEvaluationJob : IJob
{
    private readonly ILogger<ReminderEvaluationJob> _logger;

    public ReminderEvaluationJob(ILogger<ReminderEvaluationJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("ReminderEvaluationJob started at {Time}", DateTimeOffset.UtcNow);

        // TODO: Implement reminder evaluation logic

        _logger.LogInformation("ReminderEvaluationJob completed at {Time}", DateTimeOffset.UtcNow);
        return Task.CompletedTask;
    }
}
