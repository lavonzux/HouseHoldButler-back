using HouseHoldButler.Services;
using Quartz;

namespace HouseHoldButler.Jobs;

/// <summary>
/// Runs after InventoryRecalculationJob to evaluate trigger conditions
/// and create Reminder entities for inventories that meet LOW_STOCK,
/// EXPIRING, or DEPLETION_ESTIMATED thresholds.
/// </summary>
[DisallowConcurrentExecution]
public class ReminderEvaluationJob : IJob
{
    private readonly ReminderEvaluationService _evaluationService;
    private readonly ILogger<ReminderEvaluationJob> _logger;

    public ReminderEvaluationJob(
        ReminderEvaluationService evaluationService,
        ILogger<ReminderEvaluationJob> logger)
    {
        _evaluationService = evaluationService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("ReminderEvaluationJob started at {Time}", DateTimeOffset.UtcNow);

        await _evaluationService.EvaluateAllAsync(context.CancellationToken);

        _logger.LogInformation("ReminderEvaluationJob completed at {Time}", DateTimeOffset.UtcNow);
    }
}
