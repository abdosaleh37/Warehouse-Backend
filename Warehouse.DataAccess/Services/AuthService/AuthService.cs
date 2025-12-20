using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.Entities;
using Warehouse.Entities.Shared.ResponseHandling;

namespace Warehouse.DataAccess.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly WarehouseDbContext _context;
    private readonly IMapper _mapper;
    private readonly ResponseHandler _responseHandler;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        WarehouseDbContext context,
        IMapper mapper,
        ResponseHandler responseHandler)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _mapper = mapper;
        _responseHandler = responseHandler;
    }

}
