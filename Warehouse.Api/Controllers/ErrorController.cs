using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.Api.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : ControllerBase
{
    private readonly ILogger<ErrorController> _logger;
    private readonly ResponseHandler _responseHandler;

    public ErrorController(
        ILogger<ErrorController> logger,
        ResponseHandler responseHandler)
    {
        _logger = logger;
        _responseHandler = responseHandler;
    }

    [Route("/api/error")]
    public IActionResult HandleError()
    {
        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandlerFeature?.Error;

        if (exception != null)
        {
            if (IsCancellation(exception))
            {
                _logger.LogInformation("Request cancelled: {Message}", exception.Message);
                return new EmptyResult();
            }

            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        }

        return StatusCode(500, _responseHandler.ServerError<object>("An unexpected error occurred. Please try again later."));
    }

    private bool IsCancellation(Exception? ex)
    {
        if (ex == null) return false;
        if (ex is OperationCanceledException) return true;
        if (ex is TaskCanceledException) return true;
        return IsCancellation(ex.InnerException);
    }
}
