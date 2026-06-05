namespace asyncBack.Services;

public interface IBackgroundJobService
{
    Task StartJobAsync(string jobId, CancellationToken cancellationToken = default);
}
