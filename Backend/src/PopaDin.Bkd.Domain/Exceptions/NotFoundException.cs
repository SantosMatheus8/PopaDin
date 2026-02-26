namespace PopaDin.Bkd.Domain.Exceptions;

public class NotFoundException(string message) : PopaBaseException(message, 404);
