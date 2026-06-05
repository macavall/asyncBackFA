using asyncBack.Models;

namespace asyncBack.Services;

public interface IJobStore
{
    JobStatusModel? Get(string jobId);
    void Upsert(JobStatusModel job);
    bool Exists(string jobId);
}
