namespace asyncBack.Models;

public class JobStatusModel
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = "Queued";
    public string? Result { get; set; }
}
