using System.Collections.Concurrent;
using asyncBack.Models;

namespace asyncBack.Services;

public class InMemoryJobStore : IJobStore
{
    private readonly ConcurrentDictionary<string, JobStatusModel> _jobs = new();

    public JobStatusModel? Get(string jobId)
    {
        return _jobs.TryGetValue(jobId, out var job) ? job : null;
    }

    public void Upsert(JobStatusModel job)
    {
        _jobs[job.JobId] = job;
    }

    public bool Exists(string jobId)
    {
        return _jobs.ContainsKey(jobId);
    }
}
