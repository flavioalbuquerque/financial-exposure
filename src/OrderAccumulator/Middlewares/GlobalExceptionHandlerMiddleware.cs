using OrderAccumulator.Exceptions;
using System.Net;
using System.Text.Json;

namespace OrderAccumulator.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessValidationException bve)
        {
            await HandleBusinessValidationExceptionAsync(context, bve);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleBusinessValidationExceptionAsync(HttpContext context, BusinessValidationException exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";

        var problemDetails = new
        {
            type = "https://httpstatuses.com/400",
            title = "Bad Request",
            status = context.Response.StatusCode,
            detail = exception.Message,
            fieldName = exception.FieldName,
            instance = context.Request.Path
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
    
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var problemDetails = new
        {
            type = "https://httpstatuses.com/500",
            title = "Internal Server Error",
            status = context.Response.StatusCode,
            detail = exception.Message,
            instance = context.Request.Path
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}