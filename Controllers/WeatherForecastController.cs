using Microsoft.AspNetCore.Mvc;

namespace Traviam.Controllers;

[ApiController]
[Route("[controller]")]
public class BackOfficeController : ControllerBase
{


    private readonly ILogger<BackOfficeController> _logger;

    public BackOfficeController(ILogger<BackOfficeController> logger)
    {
        _logger = logger;
    }
}
