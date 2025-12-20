using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.DataAccess.Services.SectionService;

namespace Warehouse.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SectionsController : ControllerBase
{
    private readonly ISectionService _sectionService;

    public SectionsController(ISectionService sectionService)
    {
        _sectionService = sectionService;
    }

}
