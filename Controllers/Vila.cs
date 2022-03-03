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


    [HttpGet("GetByPos")]
    [Produces("application/json")]
    public IActionResult GetByPos(int x, int y)
    {
        vila = new Vila();
        vila.LoadFromXY(x,y);
        Console.WriteLine("-> /Vila/Get: Após Load Vila");
        return Ok(vila.UpdateVila());
    }  
    
    [HttpGet("GetById")]
    [Produces("application/json")]
    public IActionResult GetById(int id)
    {
        vila = new Vila();
        vila.LoadFromId(id);
        Console.WriteLine("-> /Vila/Get: Após Load Vila");
        return Ok(vila.UpdateVila());
    }  
}
