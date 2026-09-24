namespace PodcastAPI.Exceptions;

public class ListenApiException : Exception
{
    public ApiResponse? Response { get; internal set; }
    public ListenApiException() { }
    public ListenApiException(string message) : base(message) { }
    public ListenApiException(string message, Exception inner) : base(message, inner) { }
}
