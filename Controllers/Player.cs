using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

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



    [HttpGet("")]
    [Produces("application/json")]
    [SwaggerOperation(Summary = "Retorna JSON com info do Jogador.", Description = "Retorna JSON construido com base no ID passado, não inclui as vilas do jogador.")]   
    public IActionResult Get(int id)
    {         
        player = new Player();
        if (player.LoadFromId(id) == false)
        {
            return NotFound();
        }
        Console.WriteLine();
        Console.WriteLine("->Player ID Query result: "+player.id.ToString());
        Console.WriteLine(player.DtCriacao);
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(player));
        Console.WriteLine();
        return Ok(JsonConvert.SerializeObject(player));
    }


    [HttpGet("Vilas")]
    [Produces("application/json")]
    [SwaggerOperation(Summary = "Retorna JSON com lista de vilas do Jogador.", Description = "Retorna JSON construido com base no ID do jogador passado, recursos de todas as vilas são atualizados e gravados.")]
    public IActionResult Vilas(int id)
    {         
        player = new Player();
        if (player.LoadFromId(id) == false)
        {
            return NotFound();
        }
        player.LoadVilas();
        Console.WriteLine();
        Console.WriteLine(player.nome);
        Console.WriteLine(player.DtCriacao);
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(player));
        Console.WriteLine();
        return Ok(JsonConvert.SerializeObject(player.vilas));
    }
    

    [HttpPost("Cria")]
    [Produces("application/json")]
    [SwaggerOperation(Summary = "Cria um novo jogador com o nome passado.", Description = "É atribuido um ID, o jogador é inicializado com uma vila, edificios e recursos.")]
    public IActionResult Cria(String nome) //TODO: Validar se coordenada gerada já existe.
    {
        player = new Player();
        player.Init(nome);
        player.GeraCoordenada(2f, "x"); //O gera coordenadas tem de passar para o Método INIT do player
        player.GeraCoordenada(2f, "y"); // de forma a ser mais fácil de detetar as posições dos jogadores.
        player.GravaDados();
        player.AtribuiTile();

        vila      = new Vila();
        vila.LoadFromXY(player.CapitalX, player.CapitalY);

        edificio  = new Edificio();
        edificio.CriaEdificio("Centro da Vila", vila.id, 0, 0, 1, player.id);
        edificio.CriaEdificio("Quinta", vila.id, 2, 2, 1, player.id);
        edificio.CriaEdificio("Floresta", vila.id, 5, 4, 1, player.id);
        edificio.CriaEdificio("Pedreira", vila.id, 1, 4, 1, player.id);

        //player.LoadFromInt("id", player.id); // dar reload ao player antes de o enviar.

        return Ok(System.Text.Json.JsonSerializer.Serialize(player));
    }
}




