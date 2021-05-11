using System;

namespace PodcastAPI.Exceptions
{
    public class RateLimitException : Exception
    {
        public RateLimitException()
        {

        }

        public RateLimitException(string message) : base(message)
        {

        }

        public RateLimitException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
