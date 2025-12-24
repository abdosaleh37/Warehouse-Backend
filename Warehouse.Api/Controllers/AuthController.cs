using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.AuthService;
using Warehouse.Entities.DTO.Auth;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly ResponseHandler _responseHandler;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, IValidator<RegisterRequest> registerValidator, ResponseHandler responseHandler, ILogger<AuthController> logger)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _responseHandler = responseHandler;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<Response<RegisterResponse>>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = ValidationHelper.FlattenErrors(validationResult.Errors);
            _logger.LogWarning("Invalid registration request: {Errors}", validationResult.Errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }

        var response = await _authService.RegisterAsync(request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }
}
