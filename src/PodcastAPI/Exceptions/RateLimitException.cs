namespace PodcastAPI.Exceptions;

public class RateLimitException : ListenApiException
{
    public RateLimitException() { }
    public RateLimitException(string message) : base(message) { }
    public RateLimitException(string message, Exception inner) : base(message, inner) { }
}
