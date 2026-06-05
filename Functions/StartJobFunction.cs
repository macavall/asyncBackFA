using System.Net;
using asyncBack.Models;
using asyncBack.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace asyncBack.Functions;

public class StartJobFunction(
    IJobStore jobStore,
    IBackgroundJobService backgroundJobService,
    ILogger<StartJobFunction> logger)
{
    [Function("StartJob")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "startjob")]
        HttpRequestData req)
    {
        var jobId = Guid.NewGuid().ToString("N");

        jobStore.Upsert(new JobStatusModel
        {
            JobId = jobId,
            Status = "Queued"
        });

        _ = Task.Run(() => backgroundJobService.StartJobAsync(jobId));

        logger.LogInformation("Accepted job {JobId}", jobId);

        var response = req.CreateResponse(HttpStatusCode.Accepted);
        await response.WriteAsJsonAsync(new
        {
            jobId,
            status = "Accepted",
            statusUrl = $"/api/jobstatus/{jobId}"
        });

        return response;
    }
}
