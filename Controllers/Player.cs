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

    private string CONNECTION_STRING = Utils.ConnectionStrings.GetDBConnString();

    private Player player;
    private Vila vila;
    private Edificio edificio;

    private readonly ILogger<PlayerController> _logger;

    public PlayerController(ILogger<PlayerController> logger)
    {
        _logger = logger;
    }


    [HttpGet("Get")]
    [Produces("application/json")]
    public IActionResult Get(String nome)
    {         
        player = new Player();
        player.LoadFromNome(nome);
        Console.WriteLine();
        Console.WriteLine(player.nome);
        Console.WriteLine(player.DtCriacao);
        Console.WriteLine(JsonSerializer.Serialize(player));
        Console.WriteLine();
        return Ok(JsonSerializer.Serialize(player));
    }
    

    [HttpPut("Cria")]
    [Produces("application/json")]
    public IActionResult Cria(String nome)
    {
        player = new Player();
        player.Init(nome);
        player.GeraCoordenada(1f, "x");
        player.GeraCoordenada(1f, "y");
        player.GravaDados();
        player.AtribuiTile();

        vila      = new Vila();
        vila.LoadFromXY(player.CapitalX, player.CapitalY);

        edificio  = new Edificio();
        edificio.CriaEdificio("Centro da Vila", vila.id, player.id, 0, 0);
        edificio.CriaEdificio("Quinta", vila.id, player.id, 2, 2);
        edificio.CriaEdificio("Floresta", vila.id, player.id, 5, 4);
        edificio.CriaEdificio("Pedreira", vila.id, player.id, 1, 4);

        //player.LoadFromInt("id", player.id); // dar reload ao player antes de o enviar.

        return Ok(JsonSerializer.Serialize(player));
    }
}




