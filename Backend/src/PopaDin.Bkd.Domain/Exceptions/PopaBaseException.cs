namespace PopaDin.Bkd.Domain.Exceptions;

public abstract class PopaBaseException : ApplicationException
{
    public int StatusCode { get; set; }

    public PopaBaseException(string? message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}