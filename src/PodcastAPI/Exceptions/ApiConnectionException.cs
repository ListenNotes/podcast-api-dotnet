namespace PodcastAPI.Exceptions;

public class ApiConnectionException : ListenApiException
{
    public ApiConnectionException() { }
    public ApiConnectionException(string message) : base(message) { }
    public ApiConnectionException(string message, Exception inner) : base(message, inner) { }
}
