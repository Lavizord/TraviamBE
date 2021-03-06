using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Linq;
using System.Data;
using System.Data.SQLite;
using System.Text.Json;
using Swashbuckle.AspNetCore.Annotations;

using Traviam.GameLogic;
using Traviam.Utils;

namespace Traviam.Controllers;

[ApiController]
[Route("[controller]")]
public class EdificiosController : ControllerBase
{
    private string CONNECTION_STRING = Utils.ConnectionStrings.GetDBConnString();
    

    private Edificio edificio;
    private Vila vila;

    private readonly ILogger<EdificiosController> _logger;


    public EdificiosController(ILogger<EdificiosController> logger)
    {
        _logger = logger;
    }


    [HttpGet("")]
    [Produces("application/json")]
    public IActionResult Get(int id)
    {
        edificio = new Edificio();
        edificio.LoadFromId(id);
        return Ok(JsonSerializer.Serialize(edificio));
    }


    [HttpPatch("Upgrade")]
    public IActionResult Upgrade(int id)
    {
        edificio = new Edificio();
        edificio.LoadFromId(id);
        if(edificio.Upgrade()){
            return Ok();
        }
        
        return NoContent();
    }


    [HttpPost("Cria")]
    [Produces("application/json")]
    public IActionResult Cria(int tileId, int playerID, String nome, int x, int y)
    {
        edificio = new Edificio();
        edificio.CriaEdificio(nome, tileId, x, y, 1, playerID); 
        return Ok(JsonSerializer.Serialize(edificio));
    }

}
