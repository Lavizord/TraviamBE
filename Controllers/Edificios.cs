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
    private const string CONNECTION_STRING = ConnectionStrings.CONNECTION_STRING;
    

    private Edificio edificio;


    private readonly ILogger<EdificiosController> _logger;


    public EdificiosController(ILogger<EdificiosController> logger)
    {
        _logger = logger;
    }


    [HttpGet("Get")]
    public IActionResult Get(String id)
    {
        edificio = new Edificio();
        edificio.LoadFrom("id", id);
        return Ok(JsonSerializer.Serialize(edificio));
    }


    [HttpPatch("Upgrade")]
    public IActionResult Upgrade(int id)
    {
        return Ok();
    }


    [HttpPatch("Cria")]
    public IActionResult Cria(int tileId, int playerID, String nome)
    {
        edificio = new Edificio();
        edificio.CriaEdificio(nome, tileId, playerID);
        return Ok();
    }

}
