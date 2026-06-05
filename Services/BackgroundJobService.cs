using asyncBack.Models;
using Microsoft.Extensions.Logging;

namespace asyncBack.Services;

public class BackgroundJobService(IJobStore jobStore, ILogger<BackgroundJobService> logger) : IBackgroundJobService
{
    public async Task StartJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        var running = jobStore.Get(jobId) ?? new JobStatusModel { JobId = jobId };
        running.Status = "Running";
        running.Result = null;
        jobStore.Upsert(running);

        try
        {
            logger.LogInformation("Job {JobId} started", jobId);

            await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);

            var completed = jobStore.Get(jobId) ?? new JobStatusModel { JobId = jobId };
            completed.Status = "Completed";
            completed.Result = $"Finished at {DateTimeOffset.UtcNow:O}";
            jobStore.Upsert(completed);

            logger.LogInformation("Job {JobId} completed", jobId);
        }
        catch (Exception ex)
        {
            var failed = jobStore.Get(jobId) ?? new JobStatusModel { JobId = jobId };
            failed.Status = "Failed";
            failed.Result = ex.Message;
            jobStore.Upsert(failed);

            logger.LogError(ex, "Job {JobId} failed", jobId);
        }
    }
}
