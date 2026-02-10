using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.AuthService;
using Warehouse.Entities.DTO.Auth;
using Warehouse.Entities.Shared.Helpers;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly ResponseHandler _responseHandler;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        ResponseHandler responseHandler,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _responseHandler = responseHandler;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<Response<RegisterResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid registration request: {Errors}", validationResult.Errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }

        var response = await _authService.RegisterAsync(request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<Response<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = validationResult.Errors.FlattenErrors();
            _logger.LogWarning("Invalid login request: {Errors}", validationResult.Errors);
            return StatusCode((int)_responseHandler.BadRequest<object>(errors).StatusCode,
                _responseHandler.BadRequest<object>(errors));
        }

        var response = await _authService.LoginAsync(request, cancellationToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<Response<RefreshTokenResponse>>> RefreshToken(
        [FromBody] RefreshTokenRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return StatusCode((int)_responseHandler.BadRequest<object>("RefreshToken is required").StatusCode,
                _responseHandler.BadRequest<object>("RefreshToken is required"));
        }

        var response = await _authService.RefreshTokenAsync(request.RefreshToken);
        return StatusCode((int)response.StatusCode, response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var response = await _authService.LogoutAsync(User);
        return StatusCode((int)response.StatusCode, response);
    }
}
