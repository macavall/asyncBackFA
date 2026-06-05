using System.Net;
using asyncBack.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace asyncBack.Functions;

public class JobStatusFunction(IJobStore jobStore)
{
    [Function("JobStatus")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "jobstatus/{jobId}")]
        HttpRequestData req,
        string jobId)
    {
        var job = jobStore.Get(jobId);

        if (job is null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteAsJsonAsync(new
            {
                jobId,
                status = "NotFound"
            });

            return notFound;
        }

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(new
        {
            jobId = job.JobId,
            status = job.Status,
            result = job.Result
        });

        return ok;
    }
}
