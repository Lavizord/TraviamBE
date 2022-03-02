using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Data.SQLite;
using Traviam.Utils;
using Traviam.GameLogic;


namespace Traviam.Controllers;

[ApiController]
[Route("[controller]")]
public class VilaController : ControllerBase
{
    private string CONNECTION_STRING = Utils.ConnectionStrings.GetDBConnString();
    
    private Vila vila;

    private readonly ILogger<VilaController> _logger;


    public VilaController(ILogger<VilaController> logger)
    {
        _logger = logger;
    }


    [HttpGet("Get")]
    [Produces("application/json")]
    public IActionResult Get(int x, int y)
    {
        vila = new Vila();
        vila.LoadFromXY(x,y);
        Console.WriteLine("-> /Vila/Get: Após Load Vila");
        return Ok(vila.UpdateVila(x, y));
    }  
}
