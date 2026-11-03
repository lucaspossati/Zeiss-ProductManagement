using FluentValidation;
using System.Net;
using Zeiss.ProductManagement.Api.Contracts;
using Zeiss.ProductManagement.Domain.Products;

namespace Zeiss.ProductManagement.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred while processing request {RequestPath}.", context.Request.Path);

            var response = CreateResponse(exception);
            context.Response.StatusCode = GetStatusCode(exception);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(response);
        }
    }

    private static BaseResponse<object> CreateResponse(Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            return BaseResponse<object>.Fail(
                "One or more validation errors occurred.",
                validationException.Errors.Select(error => new ApiValidationError(
                    "Product",
                    error.PropertyName,
                    error.ErrorCode,
                    error.ErrorMessage)).ToArray());
        }

        if (exception is ProductNotFoundException)
        {
            return BaseResponse<object>.Fail(
                exception.Message,
                new ApiValidationError("Product", null, "product.not_found", exception.Message));
        }

        if (exception is DuplicateProductIdentifierException)
        {
            return BaseResponse<object>.Fail(
                exception.Message,
                new ApiValidationError("Product", "ProductIdentifier", "product.identifier.duplicate", exception.Message));
        }

        if (exception is ProductDomainException domainException)
        {
            return BaseResponse<object>.Fail(
                exception.Message,
                new ApiValidationError("Product", domainException.PropertyName, "product.invalid", exception.Message));
        }

        if (exception is ArgumentException argumentException)
        {
            return BaseResponse<object>.Fail(
                exception.Message,
                new ApiValidationError("Request", argumentException.ParamName, "request.invalid", exception.Message));
        }

        return BaseResponse<object>.Fail(
            "An unexpected internal error occurred. Please try again later.",
            new ApiValidationError("Server", null, "server.unexpected", "Unexpected server error."));
    }

    private static int GetStatusCode(Exception exception)
    {
        if (exception is ProductNotFoundException)
        {
            return (int)HttpStatusCode.NotFound;
        }

        if (exception is DuplicateProductIdentifierException)
        {
            return (int)HttpStatusCode.Conflict;
        }

        if (exception is ValidationException || exception is InvalidStockOperationException || exception is InsufficientStockException || exception is ProductDomainException || exception is ArgumentException)
        {
            return (int)HttpStatusCode.BadRequest;
        }

        return (int)HttpStatusCode.InternalServerError;
    }
}
