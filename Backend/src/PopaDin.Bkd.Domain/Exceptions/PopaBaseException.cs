namespace PopaDin.Bkd.Domain.Exceptions;

public class PopaBaseException : Exception
{
    public int StatusCode { get; set; }

    public PopaBaseException(string? message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}