namespace PodcastAPI.Exceptions;

public class AuthenticationException : ListenApiException
{
    public AuthenticationException() { }
    public AuthenticationException(string message) : base(message) { }
    public AuthenticationException(string message, Exception inner) : base(message, inner) { }
}
