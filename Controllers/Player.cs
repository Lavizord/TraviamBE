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
public class PlayerController : ControllerBase
{
    private const string CONNECTION_STRING = ConnectionStrings.CONNECTION_STRING;

    private Player player;
    private Vila vila;
    private Edificio edificio;

    private readonly ILogger<PlayerController> _logger;

    public PlayerController(ILogger<PlayerController> logger)
    {
        _logger = logger;
    }

    [HttpGet("Get")]
    public IActionResult Get(String nome)
    {   
        Console.WriteLine(CONNECTION_STRING);
        player = new Player();
        player.LoadFrom("nome", nome);
        
        string jsonString = JsonSerializer.Serialize(player);
        Console.WriteLine(jsonString);
        
        return Ok(JsonSerializer.Serialize(player));
    }
    
    [HttpPut("Cria")]
    public IActionResult Cria(String nome)
    {
        player = new Player();
        player.GeraId();
        player.GeraCoordenada(1f, "x");
        player.GeraCoordenada(1f, "y");
        Console.WriteLine(String.Format("X gerado:{0}, Y gerado:{1}", player.CapitalX, player.CapitalY));
        player.Grava();
        player.AtribuiTile();

        vila      = new Vila();
        vila.LoadFromXY(player.CapitalX, player.CapitalY);
        Console.WriteLine("Após load da Villa de X.Y");

        edificio  = new Edificio();
        
        edificio.CriaEdificio("Centro da Vila", vila.id, player.id);
        Console.WriteLine("Após Criar Edificio [Centro da Vila]");
        
        edificio.CriaEdificio("Quinta", vila.id, player.id);
        Console.WriteLine("Após Criar Edificio [Quinta]");
       
        edificio.CriaEdificio("Floresta", vila.id, player.id);

        edificio.CriaEdificio("Pedreira", vila.id, player.id);
        Console.WriteLine("Após Criar Edificio [Pedreira]");
        return Ok();
    }
}




