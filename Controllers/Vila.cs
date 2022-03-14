using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Data.SQLite;
using System.Text.Json;
using Swashbuckle.AspNetCore.Annotations;

using Traviam.Utils;
using Traviam.GameLogic;


namespace Traviam.Controllers;

[ApiController]
[Route("[controller]")]
public class VilaController : ControllerBase
{
    private string CONNECTION_STRING = Utils.ConnectionStrings.GetDBConnString();
    
    private Vila vila;

    private Edificio edificio;

    private readonly ILogger<VilaController> _logger;


    public VilaController(ILogger<VilaController> logger)
    {
        _logger = logger;
    }


    [HttpGet("XY")]
    [Produces("application/json")]
    public IActionResult GetByPos(int x, int y)
    {
        vila = new Vila();
        if(vila.LoadFromXY(x,y))
        {
            Console.WriteLine("-> /Vila/Get: Após Load Vila");
            return Ok(vila.UpdateVila());
        }
        return NotFound();
    }  
    

    [HttpGet("id")]
    [Produces("application/json")]
    public IActionResult GetById(int id)
    {
        vila = new Vila();
        if(vila.LoadFromId(id)){
            Console.WriteLine("-> /Vila/Get: Após Load Vila");
            return Ok(vila.UpdateVila());
        }
        return NotFound();
    }  


    [HttpGet("BuildingTileInfo")]
    [Produces("application/json")]
    public IActionResult BuildingTileInfo(int vilaId, int posVilaX, int posVilaY)
    {
        edificio = new Edificio();
        if(edificio.LoadFromIdXY(vilaId, posVilaX, posVilaY))
        {
           Console.WriteLine(":::: Foi Encontrado um edificios.");
           return Ok(edificio);
        }
        
        return  Ok(JsonSerializer.Serialize(DbHelper.GetPossiveisEdificios(vilaId, posVilaX, posVilaY)));
    }


    [HttpGet("Recursos")]
    [Produces("application/json")]
    public IActionResult Recursos(int vilaId)
    {
        vila = new Vila();
        if(vila.LoadFromId(vilaId)==false)
        {
            return NotFound();
        }
        Console.WriteLine("-> /Vila/Get: Após Load Vila (/VILA/RECURSOS)");
        vila.UpdateVila();
        
        var recursos = new 
        {
            madeira = vila.madeira,
            pedra = vila.pedra,
            trigo = vila.trigo
        };
        return Ok(recursos);
    }
}
