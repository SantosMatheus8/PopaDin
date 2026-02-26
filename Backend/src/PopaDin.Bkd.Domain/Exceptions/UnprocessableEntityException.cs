namespace PopaDin.Bkd.Domain.Exceptions;

public class UnprocessableEntityException(string message) : PopaBaseException(message, 422);
