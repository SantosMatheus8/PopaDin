namespace PopaDin.Bkd.Domain.Exceptions;

public class UnauthorizedException(string message) : PopaBaseException(message, 401);
