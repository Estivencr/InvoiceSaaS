namespace InvoiceSaaS.Application.Common;

public class BusinessException : Exception
{
    public int StatusCode { get; }

    public BusinessException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : BusinessException
{
    public NotFoundException(string entity, Guid id)
        : base($"{entity} with ID '{id}' was not found.", 404) { }

    public NotFoundException(string message) : base(message, 404) { }
}

public class UnauthorizedException : BusinessException
{
    public UnauthorizedException(string message = "Unauthorized") : base(message, 401) { }
}

public class ForbiddenException : BusinessException
{
    public ForbiddenException(string message = "Access denied") : base(message, 403) { }
}
