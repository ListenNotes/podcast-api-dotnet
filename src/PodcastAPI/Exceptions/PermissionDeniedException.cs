namespace PodcastAPI.Exceptions;

public class PermissionDeniedException : ListenApiException
{
    public PermissionDeniedException() { }
    public PermissionDeniedException(string message) : base(message) { }
    public PermissionDeniedException(string message, Exception inner) : base(message, inner) { }
}
