using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Data.SQLite;
using System.Text.Json;
using Newtonsoft.Json;
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
    [SwaggerOperation(Summary = "Retorna JSON com info da Vila.", Description = "Retorna JSON da vila construido com base nas coordenadas passados, não inclui os edificios da vila")]   
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
    [SwaggerOperation(Summary = "Retorna JSON com info da Vila.", Description = "Retorna JSON da vila construido com base no id passado, não inclui os edificios da vila")]
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
    [SwaggerOperation(Summary = "Retorna JSON com info dos edificios que jogador pode construir.", Description = "Lista de edificios enviada tem como base a coordenada X e Y passada ao endpoint, bem como o ID da vila em que o jogador está a carregar.")]
    public IActionResult BuildingTileInfo(int vilaId, int posVilaX, int posVilaY)
    {
        edificio = new Edificio();
        if(edificio.LoadFromIdXY(vilaId, posVilaX, posVilaY))
        {
           Console.WriteLine(":::: Foi Encontrado um edificios.");
           return Ok(edificio);
        }
        
        return  Ok(System.Text.Json.JsonSerializer.Serialize(DbHelper.GetPossiveisEdificios(vilaId, posVilaX, posVilaY)));
    }


    [HttpGet("Recursos")]
    [Produces("application/json")]
    [SwaggerOperation(Summary = "Retorna JSON com recursos atualizados da Vila.", Description = "Atualiza os recursos da vila pelo ID passado, e retorna o valor atualizado.")]
    public IActionResult Recursos(int vilaId)
    {
        vila = new Vila();
        vila.LoadFromId(vilaId);
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

    [HttpGet("Edificios")]
    [Produces("application/json")]
    [SwaggerOperation(Summary = "Retorna JSON com edificios construidos da vila.", Description = "Json criado com base no ID da vila. ")]
    public IActionResult Edificios(int vilaId)
    {
        vila = new Vila();
        if( vila.LoadFromId(vilaId) == false ){
            return NotFound();
        }
        Console.WriteLine("-> /Vila/Edificios: Após Load Vila (/VILA/Edificios)");
        vila.LoadEdificios();
        
        return Ok(JsonConvert.SerializeObject(vila.edificios));
    }
}
