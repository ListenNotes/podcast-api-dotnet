namespace PodcastAPI.Exceptions;

public class InvalidRequestException : ListenApiException
{
    public InvalidRequestException() { }
    public InvalidRequestException(string message) : base(message) { }
    public InvalidRequestException(string message, Exception inner) : base(message, inner) { }
}
