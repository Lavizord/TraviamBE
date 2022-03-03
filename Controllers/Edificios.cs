using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Linq;
using System.Data;
using System.Data.SQLite;
using System.Text.Json;

using Traviam.GameLogic;
using Traviam.Utils;

namespace Traviam.Controllers;

[ApiController]
[Route("[controller]")]
public class EdificiosController : ControllerBase
{
    private string CONNECTION_STRING = Utils.ConnectionStrings.GetDBConnString();
    

    private Edificio edificio;


    private readonly ILogger<EdificiosController> _logger;


    public EdificiosController(ILogger<EdificiosController> logger)
    {
        _logger = logger;
    }


    [HttpGet("Get")]
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
        return Ok();
    }


    [HttpPatch("Cria")]
        [Produces("application/json")]
    public IActionResult Cria(int tileId, int playerID, String nome, int x, int y)
    {
        edificio = new Edificio();
        edificio.CriaEdificio(nome, tileId, x, y, playerID); 
        return Ok(JsonSerializer.Serialize(edificio));
    }

}
