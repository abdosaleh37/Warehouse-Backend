using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Entities.Shared.ResponseHandling;
using Warehouse.DataAccess.Services.CategoryService;

namespace Warehouse.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ResponseHandler _responseHandler;
        private readonly ILogger<SectionsController> _logger;
        private readonly ICategoryService categoryService;

        public CategoriesController(
            ResponseHandler responseHandler,
            ILogger<SectionsController> logger,
            ICategoryService categoryService)
        {
            _responseHandler = responseHandler;
            _logger = logger;
            this.categoryService = categoryService;
        }
    }
}
